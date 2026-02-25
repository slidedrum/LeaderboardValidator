using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LeaderboardValidator
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LeaderboardValidator : BaseUnityPlugin
    {
        public static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static new ManualLogSource Logger;
        public static List<EventState> eventStates = new List<EventState>();

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            harmony.PatchAll();
        }
        public static GameState GetGameState()
        {
            Logger.LogInfo("Getting gamestate");
            GameState currentState = new GameState();
            Logger.LogInfo("Created new gamestate instance");
            var playerList = TargetingListener.instance.playerList;
            Logger.LogInfo($"Got playerlist {playerList.Count}");
            foreach (Player player in playerList)
            {
                Logger.LogInfo($"Getting gamestate for player {player.playerName}");
                var equipmentManager = player.equipmentManager;
                Gun gun = Traverse.Create(equipmentManager)
                .Field("gun")
                .GetValue<Gun>();
                Logger.LogInfo($"Got gun for player {player.playerName}");
                if (player.gameObject == null)
                {
                    Logger.LogWarning($"player.gameObject is null for player {player.playerName}");
                    continue;
                }
                Logger.LogInfo($"player.gameObject {player.gameObject}");
                Logger.LogInfo($"player.gameObject.BloodBucksBank {player.gameObject.GetComponent<BloodBucksBank>()}");
                var points = player.gameObject.GetComponent<BloodBucksBank>()?.NetworkbloodBucks ?? -1;
                Logger.LogInfo($"Got points for player {player.playerName}: {points}");
                var weaponAmmo = player.gameObject.GetComponent<PlayerEquipmentStateEngine>()?.CurrentActiveGun?.currentAmmo ?? -1;
                Logger.LogInfo($"Got weapon ammo for player {player.playerName}: {weaponAmmo}");
                var reserveAmmo = player.gameObject.GetComponent<PlayerEquipmentStateEngine>()?.CurrentActiveGun?.stashedAmmo ?? -1;
                Logger.LogInfo($"Got reserve ammo for player {player.playerName}: {reserveAmmo}");
                if (gun == null)
                {
                    Logger.LogWarning($"gun is null for player {player.playerName}");
                    continue;
                }
                var currentWeapons = gun.gunData.Select(g => g.equipmentName).ToArray();
                Logger.LogInfo($"Got current weapons for player {player.playerName}: {string.Join(", ", currentWeapons)}");
                var currentWeaponIndex = gun.activeGunIndex;
                Logger.LogInfo($"Got current weapon index for player {player.playerName}: {currentWeaponIndex}");
                currentState.playerList.Add(new PlayerState()
                {
                    playerName = player.playerName,
                    position = player.vmPos.transform.position,
                    points = (int)points,
                    weaponAmmo = weaponAmmo,
                    reserveAmmo = reserveAmmo,
                    currentWeapons = currentWeapons,
                    currentWeaponIndex = currentWeaponIndex
                });
                Logger.LogInfo($"Added playerstate for player {player.playerName}");
            }
            Logger.LogInfo("Getting current round");
            currentState.currentRound = WaveManager.instance.GetCurWave();
            Logger.LogInfo($"Current round is {currentState.currentRound}");
            AgentBehaviourSpawner spawner = GameObject.Find("1.CORE/Setup/5.AIManagment/1.SpawnManager").GetComponent<AgentBehaviourSpawner>();
            Logger.LogInfo("Got spawner");
            currentState.zombiesLeft = Traverse.Create(spawner)
                .Field("curSpawnedDemons")
                .GetValue<int>();
            Logger.LogInfo($"Zombies left: {currentState.zombiesLeft}");
            return currentState;
        }
        public static void SaveEventState(string eventName)
        {
            Logger.LogInfo($"Saving event state for {eventName}");
            EventState eventState = new EventState()
            {
                eventName = eventName,
                gameState = GetGameState()
            };
            Logger.LogInfo($"Saved event state for {eventName}");
            eventStates.Add(eventState);
        }
        public class GameState
        {
            public List<PlayerState> playerList = new();
            public int currentRound = -1;
            public int zombiesLeft = -1;
        }
        public class PlayerState
        {
            public string playerName;
            public Vector3 position;
            public int points;
            public int? weaponAmmo;
            public int? reserveAmmo;
            public string[] currentWeapons;
            public int currentWeaponIndex;
        }
        public class EventState
        {
            public string eventName;
            public GameState gameState;
        }
    }
    [HarmonyPatch(typeof(WaveManager), nameof(WaveManager.NewWave))]
    public class WaveManager_NewWave_Patch
    {
        public static void Postfix(AgentBehaviourSpawner __instance)
        {
            LeaderboardValidator.SaveEventState("NewWave");
        }
    }
    [HarmonyPatch(typeof(AbilitySlotBase), nameof(AbilitySlotBase.ModifyBlood))]
    public class AbilitySlotBase_ModifyBlood_Patch
    {
        public static void Postfix(AbilitySlotBase __instance)
        {
            LeaderboardValidator.SaveEventState("ModifyBlood");
        }
    }
    [HarmonyPatch(typeof(BloodBucksBank), "set_NetworkbloodBucks")]
    class Patch_NetworkbloodBucks
    {
        static void Postfix(float value)
        {
            LeaderboardValidator.SaveEventState("Bucks");
        }
    }
}
