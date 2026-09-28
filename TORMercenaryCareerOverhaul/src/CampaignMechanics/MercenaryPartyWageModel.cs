using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TOR_Core.Models;
using TORMercenaryCareerOverhaul.Patches;

namespace TORMercenaryCareerOverhaul.CampaignMechanics
{
    internal class MercenaryPartyWageModel : TORPartyWageModel
    {
        private const float Discount = 0.25f;

        private static readonly TextObject Reason = new TextObject("Mercenary contacts");

        public override ExplainedNumber GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            var cost = base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);

            try
            {
                if (troop == null || !TavernHiring.Active()) return cost;
                if (!IsCurrentTavernOffer(troop)) return cost;
                cost.AddFactor(0f - Discount, Reason);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryPartyWageModel: " + ex.Message);
            }

            return cost;
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
