using EnemyComponent;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LeaderboardValidator
{
    public class TimeStamp
    {
        public float absoluteTimestamp;
        public float gameTimestamp;
        public float systemTimestamp;
        public TimeStamp()
        {
            gameTimestamp = Traverse.Create(UIManager.Instance.clock).Field("currentTime").GetValue<float>();
            absoluteTimestamp = Time.time;
            systemTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
    public class GameState 
    {
        //The whole data structure should probably be obfuscated somehow.  It'll never be impossible to reverse engineer, but it should't be this easy.
        //The plan is to encode this into a binary format.  Send that over the network to the server.  The server decodes it, and validates if it makes sense.
        //Could also use this data to make a replay viewer for moderaters to see what happend durring the game.  Similar to a theatre mode replay.
        public List<PlayerState> playerStateList = new();
        public List<EnemyState> enemyStateList = new();
        public List<LogworthyEvent> eventsList = new();
        public int currentRound = -1;
        public int zombiesLeft = -1;
        public TimeStamp timeStamp;

        public GameState()
        {
            timeStamp = new();
            eventsList = LeaderboardValidator.popEventList();

            var playerList = TargetingListener.instance.playerList;
            foreach (Player player in playerList)
            {
                playerStateList.Add(new PlayerState(player));
            }
            foreach (LogicRunner runner in LeaderboardValidator.activeRunners)
            {
                List<BehaviourTreeRunner> activeAgents = Traverse.Create(runner).Field("activeAgents").GetValue<List<BehaviourTreeRunner>>();
                foreach (BehaviourTreeRunner agent in activeAgents)
                {
                    enemyStateList.Add(new EnemyState(agent));
                }
            }
            currentRound = WaveManager.instance.GetCurWave();
            zombiesLeft = Traverse.Create(LeaderboardValidator.spawnManager).Field("curSpawnedDemons").GetValue<int>();
        }
    }
    public class PlayerState
    {
        public string playerName;
        public Vector3 position;
        public Quaternion rotation;
        public AbilityState[] abilityStates = new AbilityState[4];
        public int points;
        public int weaponAmmo;
        public int reserveAmmo;
        public string[] currentWeapons = new string[3];
        public int currentWeaponIndex;

        public PlayerState(Player player)
        {
            if (player.gameObject == null)
                return;
            PlayerComponentManager playerComponentManager = player.gameObject.GetComponent<PlayerComponentManager>();
            var equipmentManager = player.equipmentManager;
            Gun gun = Traverse.Create(equipmentManager).Field("gun").GetValue<Gun>();
            if (gun == null)
                return;
            points = (int)(playerComponentManager.playerComponents.OfType<BloodBucksBank>().FirstOrDefault().bloodBucks);
            weaponAmmo = player.equipmentManager.CurrentActiveGun?.currentAmmo ?? -1;
            reserveAmmo = player.equipmentManager.CurrentActiveGun?.stashedAmmo ?? -1;
            currentWeapons = gun.gunData.Select(g => g.equipmentName).ToArray();
            currentWeaponIndex = gun.activeGunIndex;
            playerName = player.playerName;
            position = player.vmPos.transform.position;
            AbilityCaster caster = Traverse.Create(equipmentManager).Field("baseCaster").GetValue<AbilityCaster>();
            for(int i = 0; i < 4; i++)
            {
                AbilitySlot slot = caster.slots[i];
                if (slot == null)
                    continue;
                abilityStates[i] = new AbilityState(slot);
            }
        }
    }
    public class AbilityState
    {
        public int blood;
        public string abilityPath;
        public AbilityState(AbilitySlot slot)
        {
            abilityPath = slot.Ability?.assetPath ?? "None";
            blood = (int)slot.Blood;
        }
    }
    public class EnemyState
    {
        public Health target;
        public Vector3 position;
        public Quaternion rotation;
        public int currentHealth;

        public EnemyState(BehaviourTreeRunner agent)
        {
            target = Traverse.Create(agent).Field("target").GetValue<Health>();
            position = agent.transform.position;
            rotation = agent.transform.rotation;
            currentHealth = (int)agent.GetHealth().CurrentHealth;
        }
    }

}
