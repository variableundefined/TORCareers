using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORWaywatcherOverhaul.Bootstrap;
using TORWaywatcherOverhaul.LethalShot;

namespace TORWaywatcherOverhaul.Career
{
    internal static class WaywatcherCareerSetup
    {
        internal static void ReplaceLethalShot()
        {
            var career = TORCareers.Waywatcher
                ?? throw new System.InvalidOperationException("TORCareers.Waywatcher is null.");
            Reflection.SetAbilityScriptType(career, typeof(WaywatcherLethalShotScript));
        }

        internal static void UpdateLethalShotTooltip()
        {
            if (Reflection.AbilityTemplates().TryGetValue("LethalShot", out var template))
                template.TooltipDescription = WaywatcherTexts.LethalShotTooltip;
        }
    }
}
