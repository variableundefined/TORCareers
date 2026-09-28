using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;

namespace TORMercenaryCareerOverhaul
{
    internal static class MercenaryUnlockText
    {
        internal const string Tier2Text = "Elite recruits and better offers";
        internal const string Tier3Text = "Cross-culture recruits, best offers";

        internal static void Apply()
        {
            var career = TORCareers.Mercenary;
            if (career == null)
            {
                Log.Write("MercenaryUnlockText: Mercenary career not found.");
                return;
            }

            var unlockDelegate = AccessTools.Field(typeof(CareerChoiceGroupObject), "_unlockDelegate")
                                 ?? throw new MissingFieldException("CareerChoiceGroupObject", "_unlockDelegate");

            foreach (var group in career.ChoiceGroups) Describe(group, unlockDelegate);
        }

        private static void Describe(CareerChoiceGroupObject group, FieldInfo unlockDelegate)
        {
            try
            {
                if (group == null) return;

                string text;
                int renownTier;
                if (group.Tier == 2) { text = Tier2Text; renownTier = 2; }
                else if (group.Tier == 3) { text = Tier3Text; renownTier = 4; }
                else return;

                CareerChoiceGroupObject.UnlockDelegate reward = delegate (Hero hero, out string unlockText)
                {
                    unlockText = text;
                    return hero != null && hero.Clan != null && hero.Clan.Tier >= renownTier;
                };

                unlockDelegate.SetValue(group, reward);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryUnlockText: " + ex.Message);
            }
        }
    }
}
