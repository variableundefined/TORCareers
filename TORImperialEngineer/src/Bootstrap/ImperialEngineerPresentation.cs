using System;
using HarmonyLib;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Bootstrap
{
    [HarmonyPatch(typeof(CareerObjectVM), MethodType.Constructor, new[] { typeof(CareerObject) })]
    internal static class ImperialEngineerIllustration
    {
        private const string Borrowed = "CareerSystem\\Illustrations\\Mercenary";

        [HarmonyPostfix]
        private static void Postfix(CareerObjectVM __instance, CareerObject career)
        {
            try
            {
                if (career == null || career.StringId != ImperialEngineerCareer.Id) return;
                __instance.SpriteName = Borrowed;
            }
            catch (Exception e)
            {
                Log.Warn("Could not set career illustration: " + e.Message);
            }
        }
    }
}
