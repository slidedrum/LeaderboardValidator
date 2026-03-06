using HarmonyLib;

namespace LeaderboardValidator
{
    //Different hooks that trigger saving the gamestate.
    [HarmonyPatch(typeof(LobbyPingReceiver), "Update")]
    public class WaveManager_NewWave_Patch
    {
        public static void Postfix()
        {
            //LeaderboardValidator.SaveEventState("NewWave");
        }
    }
    [HarmonyPatch(typeof(AbilitySlotBase), nameof(AbilitySlotBase.ModifyBlood))]
    public class AbilitySlotBase_ModifyBlood_Patch
    {
        public static void Postfix()
        {
            //LeaderboardValidator.SaveEventState("ModifyBlood");
        }
    }
    [HarmonyPatch(typeof(BloodBucksBank), "set_NetworkbloodBucks")]
    class NetworkbloodBucks_set_NetworkbloodBucks_Patch
    {
        static void Postfix()
        {
            //LeaderboardValidator.SaveEventState("Bucks");
        }
    }
    [HarmonyPatch(typeof(Clock), "Update")]
    class Clock_Update_Patch
    {
        static void Postfix(Clock __instance)
        {
            //This is probably a bad way to detect when we're in game, but it should work?
            LeaderboardValidator.Update(__instance);
        }
    }
}
