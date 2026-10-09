using HarmonyLib;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TORSwordmaster.Career;
using TORSwordmaster.Trance;

namespace TORSwordmaster.Abilities
{
    internal static class WayOfTheSwordRestriction
    {
        internal const string NeedsMeleeWeapon = "{=sm_needs_melee_weapon}Needs a melee weapon!";

        internal static void Apply(Harmony harmony)
        {
            var target = AccessTools.Method(typeof(CareerAbility), nameof(CareerAbility.IsDisabled))
                ?? throw new System.MissingMethodException("CareerAbility.IsDisabled not found.");
            harmony.Patch(target, postfix: new HarmonyMethod(AccessTools.Method(typeof(WayOfTheSwordRestriction), nameof(Postfix))));
        }

        private static void Postfix(CareerAbility __instance, Agent casterAgent, ref TextObject disabledReason, ref bool __result)
        {
            if (__result || casterAgent == null) return;
            if (__instance?.Template?.StringID != SwordmasterCareer.AbilityId) return;
            if (Focus.HasMeleeWeapon(casterAgent)) return;

            disabledReason = new TextObject(NeedsMeleeWeapon);
            __result = true;
        }
    }
}
