using HarmonyLib;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch]
    internal static class CouncilGuardBootstrap
    {
        private static Game _registeredFor;

        internal static bool Registered => _registeredFor != null && _registeredFor == Game.Current;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TORCareerChoices), MethodType.Constructor)]
        private static void RegisterCouncilGuard(TORCareerChoices __instance)
        {
            if (Registered) return;

            var career = CouncilGuardCareer.Create();
            CouncilGuardChoiceGroups.Register(career);
            var choices = new CouncilGuardCareerChoices(career);

            Reflection.AllCareers().Add(career);
            Reflection.AllCareerChoices(__instance).Add(choices);

            _registeredFor = Game.Current;
            Log.Write("Career registered for this campaign. Careers: " + Reflection.AllCareers().Count + ", choice sets: " + Reflection.AllCareerChoices(__instance).Count + ".");
        }
    }
}
