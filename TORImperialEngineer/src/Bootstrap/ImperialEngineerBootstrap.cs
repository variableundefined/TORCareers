using HarmonyLib;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Bootstrap
{
    [HarmonyPatch]
    internal static class ImperialEngineerBootstrap
    {
        private static Game _registeredFor;

        internal static bool Registered => _registeredFor != null && _registeredFor == Game.Current;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TORCareerChoices), MethodType.Constructor)]
        private static void RegisterImperialEngineer(TORCareerChoices __instance)
        {
            if (Registered) return;

            var career = ImperialEngineerCareer.Create();
            ImperialEngineerChoiceGroups.Register(career);
            var choices = new ImperialEngineerCareerChoices(career);

            Reflection.AllCareers().Add(career);
            Reflection.AllCareerChoices(__instance).Add(choices);

            _registeredFor = Game.Current;
            Log.Write("Career registered for this campaign. Careers: " + Reflection.AllCareers().Count + ", choice sets: " + Reflection.AllCareerChoices(__instance).Count + ".");
        }
    }
}
