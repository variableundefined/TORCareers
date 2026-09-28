using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class TavernHiring
    {
        internal const string Card = "PaymasterPassive4";

        private const float RollFactor = 1.5f;
        private const float DailyFactor = 1.5f;

        internal static void Apply(Harmony harmony)
        {
            var count = AccessTools.Method(typeof(RecruitmentCampaignBehavior), "FindNumberOfMercenariesWillBeAdded")
                        ?? throw new MissingMethodException("RecruitmentCampaignBehavior.FindNumberOfMercenariesWillBeAdded not found.");

            harmony.Patch(count, postfix: new HarmonyMethod(AccessTools.Method(typeof(TavernHiring), nameof(MoreOfThem))));
        }

        internal static bool Active()
        {
            var hero = Hero.MainHero;
            return hero != null && hero.HasCareer(TORCareers.Mercenary) && hero.HasCareerChoice(Card);
        }

        private static void MoreOfThem(ref int __result, bool dailyUpdate)
        {
            if (__result <= 0 || !Active()) return;
            __result = (int)Math.Round(__result * (dailyUpdate ? DailyFactor : RollFactor));
        }
    }
}
