using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.CampaignMechanics
{
    internal class MercenaryContacts : CampaignBehaviorBase
    {
        internal static string PendingFromCreation;

        private const int VeteranTier = 2;
        private const int CommanderTier = 3;
        private const float ContractPremium = 3f;
        private const float PaymasterDiscount = 0.25f;

        private static readonly int[] TierWeight = { 55, 30, 15 };
        private static readonly int[] VeteranTierWeight = { 30, 40, 30 };
        private static readonly int[] CommanderTierWeight = { 15, 35, 50 };
        private static readonly int[] TierMin = { 6, 4, 2 };
        private static readonly int[] TierMax = { 10, 7, 4 };

        private static MercenaryContacts _instance;

        private List<string> _signed = new List<string>();
        private List<string> _offers = new List<string>();

        internal static Company SignedCompany()
        {
            if (_instance == null || _instance._signed.Count == 0) return null;
            return Contacts.ById(_instance._signed[0]);
        }

        internal static bool Sign(string companyId)
        {
            if (string.IsNullOrEmpty(companyId)) return false;
            if (_instance == null)
            {
                PendingFromCreation = companyId;
                return false;
            }
            if (_instance._signed.Contains(companyId)) return false;

            _instance._signed.Add(companyId);
            return true;
        }

        public override void RegisterEvents()
        {
            _instance = this;
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            _instance = this;
            dataStore.SyncData("tor_merc_contacts", ref _signed);
            dataStore.SyncData("tor_merc_offers", ref _offers);
            if (_signed == null) _signed = new List<string>();
            if (_offers == null) _offers = new List<string>();
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            FlushPending();

            starter.AddGameMenuOption("town_backstreet", "tor_merc_hire",
                "{=tor_merc_hire}Recruit {TOR_MERC_COUNT} {TOR_MERC_NAME} ({TOR_MERC_TOTAL}{GOLD_ICON})",
                CanHire, Hire, false, 2);
        }

        private void FlushPending()
        {
            if (string.IsNullOrEmpty(PendingFromCreation)) return;

            var pending = PendingFromCreation;
            PendingFromCreation = null;
            Sign(pending);
        }

        private bool CanHire(MenuCallbackArgs args)
        {
            FlushPending();
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            if (!IsMercenary() || _signed.Count == 0) return false;

            var offer = CurrentOffer();
            if (offer == null) return false;

            var troop = MBObjectManager.Instance.GetObject<CharacterObject>(offer.TroopId);
            if (troop == null) return false;

            var affordable = Math.Min(offer.Count, Hero.MainHero.Gold / offer.Price);
            var shown = Math.Max(1, affordable);

            MBTextManager.SetTextVariable("TOR_MERC_COUNT", shown);
            MBTextManager.SetTextVariable("TOR_MERC_NAME", troop.Name);
            MBTextManager.SetTextVariable("TOR_MERC_TOTAL", shown * offer.Price);

            args.Tooltip = new TextObject(offer.Company.Name + " - " + offer.Count
                                          + " available at " + offer.Price + "{GOLD_ICON} each.");
            args.IsEnabled = affordable > 0;
            if (!args.IsEnabled) args.Tooltip = new TextObject("You cannot afford a single one.");
            return true;
        }

        private void Hire(MenuCallbackArgs args)
        {
            var offer = CurrentOffer();
            if (offer == null) return;

            var troop = MBObjectManager.Instance.GetObject<CharacterObject>(offer.TroopId);
            if (troop == null)
            {
                Log.Write("Troop not found: " + offer.TroopId);
                return;
            }

            var affordable = Math.Min(offer.Count, Hero.MainHero.Gold / offer.Price);
            if (affordable <= 0) return;

            MobileParty.MainParty.MemberRoster.AddToCounts(troop, affordable);
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, affordable * offer.Price);
            Consume(offer, affordable);

            InformationManager.DisplayMessage(new InformationMessage(
                affordable + " " + troop.Name + " join you from " + offer.Company.Name + "."));
            GameMenu.SwitchToMenu("town_backstreet");
        }

        private sealed class Offer
        {
            internal Company Company;
            internal int Tier;
            internal int Count;
            internal int Price;
            internal string TroopId;
            internal string Key;
        }

        private static bool IsMercenary()
        {
            var hero = Hero.MainHero;
            return hero != null && hero.HasCareer(TORCareers.Mercenary);
        }

        private Offer CurrentOffer()
        {
            var settlement = Settlement.CurrentSettlement;
            if (settlement == null || !settlement.IsTown) return null;

            var day = (int)CampaignTime.Now.ToDays;
            var prefix = settlement.StringId + "|" + day + "|";

            var stored = _offers.FirstOrDefault(o => o.StartsWith(prefix, StringComparison.Ordinal));
            if (stored == null)
            {
                stored = Roll(settlement, day, prefix);
                if (stored == null) return null;
                _offers.RemoveAll(o => o.StartsWith(settlement.StringId + "|", StringComparison.Ordinal));
                _offers.Add(stored);
            }

            return Parse(stored);
        }

        private string Roll(Settlement settlement, int day, string prefix)
        {
            if (_signed.Count == 0) return null;

            var company = Contacts.ById(_signed[MBRandom.RandomInt(_signed.Count)]);
            if (company == null) return null;

            var weights = TierWeight;
            if (Hero.MainHero.HasUnlockedCareerChoiceTier(CommanderTier)) weights = CommanderTierWeight;
            else if (Hero.MainHero.HasUnlockedCareerChoiceTier(VeteranTier)) weights = VeteranTierWeight;

            var roll = MBRandom.RandomInt(weights.Sum());
            var tier = 0;
            for (var i = 0; i < weights.Length; i++)
            {
                if (roll < weights[i]) { tier = i; break; }
                roll -= weights[i];
            }

            var count = MBRandom.RandomInt(TierMin[tier], TierMax[tier] + 1);
            return prefix + company.Id + "|" + tier + "|" + count;
        }

        private Offer Parse(string stored)
        {
            var parts = stored.Split('|');
            if (parts.Length != 5) return null;

            var company = Contacts.ById(parts[2]);
            int tier, count;
            if (company == null || !int.TryParse(parts[3], out tier) || !int.TryParse(parts[4], out count)) return null;
            if (tier < 0 || tier >= company.Tiers.Length || count <= 0) return null;

            return new Offer
            {
                Company = company,
                Tier = tier,
                Count = count,
                Price = PriceFor(company.Tiers[tier]),
                TroopId = company.Tiers[tier],
                Key = stored,
            };
        }

        private static int PriceFor(string troopId)
        {
            var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
            if (troop == null) return int.MaxValue;

            var model = Campaign.Current.Models.PartyWageModel;
            var full = model.GetTroopRecruitmentCost(troop, Hero.MainHero).ResultNumber;
            var bare = model.GetTroopRecruitmentCost(troop, Hero.MainHero, true).ResultNumber;
            var mount = Math.Max(0f, full - bare);

            var price = bare * ContractPremium + mount;
            if (Hero.MainHero.HasCareerChoice(Patches.TavernHiring.Card)) price *= 1f - PaymasterDiscount;

            return Math.Max(1, (int)Math.Round(price));
        }

        private void Consume(Offer offer, int taken)
        {
            _offers.Remove(offer.Key);
            var left = offer.Count - taken;
            if (left <= 0) return;

            var parts = offer.Key.Split('|');
            _offers.Add(parts[0] + "|" + parts[1] + "|" + parts[2] + "|" + parts[3] + "|" + left);
        }
    }
}
