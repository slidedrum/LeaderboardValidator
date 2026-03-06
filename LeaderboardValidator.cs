using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LeaderboardValidator
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LeaderboardValidator : BaseUnityPlugin
    {
        public static SceneConsole sceneConsole 
        {
            get            
            {
                return SceneConsole.instance;
            }
        }
        public static AgentBehaviourSpawner spawnManager 
        {
            get             
            {
                return Traverse.Create(sceneConsole).Field("spawnManager").GetValue<AgentBehaviourSpawner>();
            }
        }
        public static List<LogicRunner> activeRunners 
        {
            get             
            {
                return Traverse.Create(spawnManager).Field("activeRunners").GetValue<List<LogicRunner>>();
            }
        }
        public void Setup()
        {
            
        }

        public static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static new ManualLogSource Logger;
        public static List<GameState> gameStates = new List<GameState>();
        public static float LastUpdateTime = 0f;
        public static float UpdateInterval = 5f;
        public static void Update(Clock clock)
        {
            var currentTime = Traverse.Create(clock).Field("currentTime").GetValue<float>();
            if (currentTime - LastUpdateTime >= UpdateInterval)
            {
                LastUpdateTime = currentTime;
                SaveGameState();
            }
        }
        //TODO Actually make a method to validate the event states.
        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            harmony.PatchAll();
        }
        public static void SaveGameState()
        {
            Logger.LogInfo("Saving game state");
            gameStates.Add(new GameState());
        }
        public static void SaveEventState(string eventName)
        {
            Logger.LogInfo($"Saving event state for {eventName}");
            //EventState eventState = new EventState()
            //{
            //    eventName = eventName,
            //    gameState = new GameState()
            //};
            Logger.LogInfo($"Saved event state for {eventName}");
            //eventStates.Add(eventState);
        }


    }
}
