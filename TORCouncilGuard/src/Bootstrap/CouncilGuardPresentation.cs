using System;
using HarmonyLib;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch(typeof(CareerObjectVM), MethodType.Constructor, new[] { typeof(CareerObject) })]
    internal static class CouncilGuardIllustration
    {
        private const string Borrowed = "CareerSystem\\Illustrations\\GreyLord";

        [HarmonyPostfix]
        private static void Postfix(CareerObjectVM __instance, CareerObject career)
        {
            try
            {
                if (career == null || career.StringId != CouncilGuardCareer.Id) return;
                __instance.SpriteName = Borrowed;
            }
            catch (Exception e)
            {
                Log.Warn("Could not set career illustration: " + e.Message);
            }
        }
    }
}
