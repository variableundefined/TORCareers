using System;
using HarmonyLib;
using TOR_Core.CampaignMechanics;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class SurvivalistHunt
    {
        internal static void Apply(Harmony harmony)
        {
            var hunt = AccessTools.Method(typeof(TORCareerPerkCampaignBehavior), "LaunchHuntingEvent")
                       ?? throw new MissingMethodException("TORCareerPerkCampaignBehavior", "LaunchHuntingEvent");

            harmony.Patch(hunt, prefix: new HarmonyMethod(AccessTools.Method(typeof(SurvivalistHunt), nameof(Skip))));

        }

        private static bool Skip()
        {
            try
            {
                return !Recruits.IsMercenary();
            }
            catch (Exception ex)
            {
                Log.Write("SurvivalistHunt: " + ex.Message);
                return true;
            }
        }
    }
}
