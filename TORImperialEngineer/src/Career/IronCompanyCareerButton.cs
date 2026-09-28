using System;
using Helpers;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;
using TOR_Core.Extensions;
using TOR_Core.Utilities;
using static Helpers.PartyScreenHelper;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Career
{
    internal class IronCompanyCareerButton : CareerButtonBehaviorBase
    {
        private const string GuardId = "tor_empire_iron_company_guard";
        private const int PrestigeCost = 5;
        private const int GoldCost = 500;
        private const int MinimumTier = 3;
        private const int MaximumBatch = 5;
        private const float LogisticsDiscount = 0.75f;


        internal static IronCompanyCareerButton Instance { get; } = new IronCompanyCareerButton();

        private IronCompanyCareerButton() : base(null) { }

        private static int ExchangeCost()
        {
            if (!Hero.MainHero.HasCareerChoice(G.Logistics + "Passive3")) return PrestigeCost;
            return (int)Math.Round(PrestigeCost * LogisticsDiscount);
        }

        public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false, bool shiftClick = false)
        {
            var guard = MBObjectManager.Instance.GetObject<CharacterObject>(GuardId);
            if (guard == null)
            {
                Log.Error(GuardId + " not found; Iron Company conversion skipped.");
                return;
            }

            var cost = ExchangeCost();
            var count = shiftClick
                ? CareerButtonHelper.GetMaximumExchangeTroops(characterObject, false, MaximumBatch, GoldCost, cost)
                : 1;

            for (var i = 0; i < count; i++)
            {
                CustomResourceManager.AddResourceChanges(Hero.MainHero.GetCultureSpecificCustomResource(), cost);
                PartyScreenHelper.GetActivePartyState().PartyScreenLogic.CurrentData.PartyGoldChangeAmount -= GoldCost;
                CareerButtonHelper.ExchangeUnitForNewUnit(characterObject, guard, true);
            }
        }

        public override bool ShouldButtonBeVisible(CharacterObject characterObject, bool isPrisoner)
        {
            if (PartyScreenHelper.GetActivePartyState().PartyScreenMode != PartyScreenMode.Normal) return false;
            if (characterObject == null || characterObject.IsHero || isPrisoner) return false;
            return !IsIronCompany(characterObject);
        }

        public override bool ShouldButtonBeActive(CharacterObject characterObject, out TextObject displayText, bool isPrisoner = false)
        {
            displayText = TextObject.GetEmpty();
            var roster = Hero.MainHero.PartyBelongedTo.MemberRoster;
            var index = roster.FindIndexOfTroop(characterObject);
            if (index == -1) return false;

            var cost = ExchangeCost();
            var prestigeIcon = CustomResourceManager.GetResourceObject("Prestige").GetCustomResourceIconAsText();

            var enlist = TORTextHelper.GetTextObject("imperial_engineer_iron_company_upgrade_text",
                "Enlist into the Iron Company{newline}Turns them into an Iron Company Guard. Costs {EXCHANGE_COST} {PRESTIGE_ICON} and {GOLD_COST} gold.");
            enlist.SetTextVariable("EXCHANGE_COST", cost);
            enlist.SetTextVariable("PRESTIGE_ICON", prestigeIcon);
            enlist.SetTextVariable("GOLD_COST", GoldCost);
            displayText = enlist;

            if (!InEmpireTown())
            {
                displayText = TORTextHelper.GetTextObject("imperial_engineer_iron_company_town_text", "Must be in an Empire town");
                return false;
            }

            if (characterObject.Tier < MinimumTier)
            {
                displayText = TORTextHelper.GetTextObject("imperial_engineer_iron_company_tier_text", "Tier of troops is too low");
                return false;
            }

            if (IsKnight(characterObject))
            {
                displayText = TORTextHelper.GetTextObject("imperial_engineer_iron_company_knight_text", "Knights cannot be converted");
                return false;
            }

            if (characterObject.Culture?.StringId == TORConstants.Cultures.BRETONNIA || characterObject.Race != 0)
            {
                displayText = TORTextHelper.GetTextObject("imperial_engineer_iron_company_culture_text", "Bretonnian and non-human troops cannot be converted");
                return false;
            }

            if (roster.GetElementNumber(index) - roster.GetElementWoundedNumber(index) <= 0)
            {
                displayText = TORTextHelper.GetTextObject("imperial_engineer_iron_company_wounded_text", "Not enough healthy troops available");
                return false;
            }

            var prestige = Hero.MainHero.GetCultureSpecificCustomResource();
            if (CustomResourceManager.GetPendingFor(prestige.StringId) + cost > Hero.MainHero.GetCultureSpecificCustomResourceValue())
            {
                var text = TORTextHelper.GetTextObject("imperial_engineer_iron_company_prestige_text", "Requires at least {EXCHANGE_COST} {PRESTIGE_ICON}");
                text.SetTextVariable("EXCHANGE_COST", cost);
                text.SetTextVariable("PRESTIGE_ICON", prestigeIcon);
                displayText = text;
                return false;
            }

            var goldChange = PartyScreenHelper.GetActivePartyState().PartyScreenLogic.CurrentData.PartyGoldChangeAmount;
            if (Hero.MainHero.Gold + goldChange < GoldCost)
            {
                var text = TORTextHelper.GetTextObject("imperial_engineer_iron_company_gold_text", "Requires at least {GOLD_COST} gold");
                text.SetTextVariable("GOLD_COST", GoldCost);
                displayText = text;
                return false;
            }

            return true;
        }

        private static bool InEmpireTown()
        {
            var settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty?.CurrentSettlement;
            return settlement != null && settlement.IsTown && settlement.Culture?.StringId == TORConstants.Cultures.EMPIRE;
        }

        private static bool IsKnight(CharacterObject character) =>
            character.IsEliteTroop() || character.IsKnightUnit();

        private static bool IsIronCompany(CharacterObject character) =>
            character.StringId.Contains("iron_company") || character.StringId.Contains("ironsider");
    }

    [HarmonyPatch(typeof(CareerButtons), nameof(CareerButtons.GetCareerButton))]
    internal static class IronCompanyCareerButtonLookup
    {
        [HarmonyPostfix]
        private static void Postfix(CareerObject careerObject, ref CareerButtonBehaviorBase __result)
        {
            if (__result == null && careerObject != null && careerObject.StringId == ImperialEngineerCareer.Id)
                __result = IronCompanyCareerButton.Instance;
        }
    }
}
