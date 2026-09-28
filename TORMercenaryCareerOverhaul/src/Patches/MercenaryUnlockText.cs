using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryUnlockText
    {
        internal const string Tier2Text = "Elite recruits and better offers";
        internal const string Tier3Text = "Cross-culture recruits, best offers";

        private static FieldInfo _unlockDelegate;

        internal static void Apply(Harmony harmony)
        {
            _unlockDelegate = AccessTools.Field(typeof(CareerChoiceGroupObject), "_unlockDelegate")
                              ?? throw new MissingFieldException("CareerChoiceGroupObject", "_unlockDelegate");

            harmony.Patch(CareerReflection.GroupInitialize(),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryUnlockText), nameof(Describe))));

        }

        private static void Describe(CareerChoiceGroupObject __instance)
        {
            try
            {
                if (__instance == null || __instance.OwnerCareer != TORCareers.Mercenary) return;

                string text;
                int renownTier;
                if (__instance.Tier == 2) { text = Tier2Text; renownTier = 2; }
                else if (__instance.Tier == 3) { text = Tier3Text; renownTier = 4; }
                else return;

                CareerChoiceGroupObject.UnlockDelegate reward = delegate (Hero hero, out string unlockText)
                {
                    unlockText = text;
                    return hero != null && hero.Clan != null && hero.Clan.Tier >= renownTier;
                };

                _unlockDelegate.SetValue(__instance, reward);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryUnlockText: " + ex.Message);
            }
        }
    }
}
