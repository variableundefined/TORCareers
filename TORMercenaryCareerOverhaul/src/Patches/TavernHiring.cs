using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Models;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class TavernHiring
    {
        internal const string Card = "PaymasterPassive4";

        private const float RollFactor = 1.5f;
        private const float DailyFactor = 1.5f;
        private const float Discount = 0.25f;

        private static readonly TextObject Reason = new TextObject("Mercenary contacts");

        internal static void Apply(Harmony harmony)
        {
            var count = AccessTools.Method(typeof(RecruitmentCampaignBehavior), "FindNumberOfMercenariesWillBeAdded")
                        ?? throw new MissingMethodException("RecruitmentCampaignBehavior.FindNumberOfMercenariesWillBeAdded not found.");

            var cost = AccessTools.Method(typeof(TORPartyWageModel), nameof(TORPartyWageModel.GetTroopRecruitmentCost))
                       ?? throw new MissingMethodException("TORPartyWageModel.GetTroopRecruitmentCost not found.");

            harmony.Patch(count, postfix: new HarmonyMethod(AccessTools.Method(typeof(TavernHiring), nameof(MoreOfThem))));
            harmony.Patch(cost, postfix: new HarmonyMethod(AccessTools.Method(typeof(TavernHiring), nameof(Cheaper))));
        }

        private static bool Active()
        {
            var hero = Hero.MainHero;
            return hero != null && hero.HasCareer(TORCareers.Mercenary) && hero.HasCareerChoice(Card);
        }

        private static void MoreOfThem(ref int __result, bool dailyUpdate)
        {
            if (__result <= 0 || !Active()) return;
            __result = (int)Math.Round(__result * (dailyUpdate ? DailyFactor : RollFactor));
        }

        private static void Cheaper(CharacterObject troop, ref ExplainedNumber __result)
        {
            try
            {
                if (troop == null || !Active()) return;
                if (!IsCurrentTavernOffer(troop)) return;
                __result.AddFactor(0f - Discount, Reason);
            }
            catch (Exception ex)
            {
                Log.Write("TavernHiring: " + ex.Message);
            }
        }

        private static bool IsCurrentTavernOffer(CharacterObject troop)
        {
            var town = Settlement.CurrentSettlement?.Town;
            if (town == null) return false;

            var recruitment = Campaign.Current?.GetCampaignBehavior<RecruitmentCampaignBehavior>();
            var data = recruitment?.GetMercenaryData(town);
            return data != null && data.TroopType == troop;
        }
    }
}
