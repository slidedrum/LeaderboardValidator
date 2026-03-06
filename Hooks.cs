using HarmonyLib;

namespace LeaderboardValidator
{
    //Different hooks that trigger saving the gamestate.
    //Obviously need a lot more hooks to make it viable.  but this is more a proof of concept.  Would be difficult to do this as a mod.
    [HarmonyPatch(typeof(AbilitySlotBase), nameof(AbilitySlotBase.ModifyBlood))]
    public class AbilitySlotBase_ModifyBlood_Patch //This triggers way more than it needs to, but it's good enough for this proof of concept.
    {
        public static void Postfix(AbilitySlotBase __instance, float amount)
        {
            Player player = __instance.caster.gameObject.GetComponent<Player>();
            bloodChangedEvent logworthyEvent = new((int)amount, player);
            LeaderboardValidator.addEventToList(logworthyEvent);
        }
    }
    [HarmonyPatch(typeof(BloodBucksBank), "set_NetworkbloodBucks")]
    class NetworkbloodBucks_set_NetworkbloodBucks_Patch
    {
        static void Prefix(BloodBucksBank __instance, float value)
        {
            int currentPoints = (int)__instance.NetworkbloodBucks;
            int newPoints = (int)value;
            int pointsGained = newPoints - currentPoints;
            Player player = __instance.gameObject.GetComponent<Player>();
            gainPointsEvent logworthyEvent = new(pointsGained, player);
            LeaderboardValidator.addEventToList(logworthyEvent);
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
