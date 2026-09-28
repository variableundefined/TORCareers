using HarmonyLib;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Abilities
{
    [HarmonyPatch(typeof(CareerAbility), nameof(CareerAbility.CanCast))]
    internal static class MunitionCastCondition
    {
        [HarmonyPostfix]
        private static void Postfix(Agent casterAgent, ref TextObject failureReason, ref bool __result)
        {
            if (!__result || !ImperialEngineerCareer.IsPlayer) return;
            if (Firearms.CanLoad(casterAgent)) return;

            failureReason = new TextObject("{=imperial_engineer_requires_firearm}Ability can only be used with a gun not loaded with grenades");
            __result = false;
        }
    }
}
