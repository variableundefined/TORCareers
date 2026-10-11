using System;
using System.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.CampaignMechanics.Menagery;
using TOR_Core.Extensions;
using TORMiscTweaks.Settings;

namespace TORMiscTweaks.Eonir
{
    internal class WitchKingEnvoyMenu : CampaignBehaviorBase
    {
        private const string EnvoyAttribute = "DruchiiEnvoy";

        private static AccessTools.FieldRef<EonirFavorEnvoyTownBehavior, bool> _tradeFlag;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            try
            {
                _tradeFlag ??= AccessTools.FieldRefAccess<EonirFavorEnvoyTownBehavior, bool>("_isDruchiiEnvoyTrade");
            }
            catch (Exception e)
            {
                Log.Error("FAILED: Witch King Envoy menu disabled, TOR envoy trade flag not found: " + e.Message);
                return;
            }

            starter.AddGameMenuOption("town", "tmt_witch_king_envoy",
                "{=tmt_witch_king_envoy}Trade prisoners with the Witch King Envoy",
                CanTrade, Trade);
            starter.AddGameMenuOption("town", "tmt_witch_king_envoy_sell_all",
                "{=tmt_witch_king_envoy_sell_all}Sell all prisoners to the Witch King Envoy",
                CanSellAll, SellAll);
        }

        private static EonirFavorEnvoyTownBehavior EnvoyBehavior =>
            Campaign.Current?.GetCampaignBehavior<EonirFavorEnvoyTownBehavior>();

        private static bool IsAvailable(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;

            var settlement = Settlement.CurrentSettlement;
            if (settlement == null || !settlement.IsTorLithanel()) return false;
            if (Hero.MainHero.Culture?.StringId != "eonir") return false;
            if (EnvoyBehavior == null) return false;
            if (!Hero.AllAliveHeroes.Any(h => h.HasAttribute(EnvoyAttribute))) return false;

            if (Clan.PlayerClan.Tier < 2)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=tmt_witch_king_envoy_tier}You do not have the privilege to serve the council. (Low Clan Tier)");
            }
            else if (MobileParty.MainParty.PrisonRoster.TotalManCount == 0)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=tmt_witch_king_envoy_empty}You have no prisoners.");
            }
            return true;
        }

        private static bool CanTrade(MenuCallbackArgs args)
        {
            return MiscTweaksSettings.Current.WitchKingEnvoyMenuEnabled && IsAvailable(args);
        }

        private static bool CanSellAll(MenuCallbackArgs args)
        {
            var settings = MiscTweaksSettings.Current;
            if (!settings.WitchKingEnvoyMenuEnabled || !settings.WitchKingEnvoySellAllEnabled) return false;
            if (!IsAvailable(args)) return false;

            if (args.IsEnabled)
                args.Tooltip = new TextObject("{=tmt_witch_king_envoy_sell_all_hint}{COUNT} prisoners for {FAVOR} favor.")
                    .SetTextVariable("COUNT", MobileParty.MainParty.PrisonRoster.TotalManCount)
                    .SetTextVariable("FAVOR", PreviewFavor());
            return true;
        }

        private static void Trade(MenuCallbackArgs args)
        {
            var envoy = EnvoyBehavior;
            if (envoy == null) return;

            _tradeFlag(envoy) = true;
            PartyScreenHelper.OpenScreenAsDonatePrisoners();

            if (Game.Current.GameStateManager.ActiveState is PartyState state && state.PartyScreenLogic != null)
                state.PartyScreenLogic.PartyScreenClosedEvent += (_, _, _, _, _, _, _) => _tradeFlag(envoy) = false;
            else
                _tradeFlag(envoy) = false;
        }

        private static void SellAll(MenuCallbackArgs args)
        {
            var text = new TextObject("{=tmt_witch_king_envoy_sell_all_confirm}Hand over all {COUNT} prisoners to the Witch King Envoy for {FAVOR} favor?")
                .SetTextVariable("COUNT", MobileParty.MainParty.PrisonRoster.TotalManCount)
                .SetTextVariable("FAVOR", PreviewFavor());

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=tmt_witch_king_envoy_sell_all_title}Witch King Envoy").ToString(),
                text.ToString(), true, true,
                GameTexts.FindText("str_ok").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                DoSellAll, null));
        }

        private static void DoSellAll()
        {
            var envoy = EnvoyBehavior;
            var settlement = Settlement.CurrentSettlement;
            var party = MobileParty.MainParty;
            if (envoy == null || settlement == null || party.PrisonRoster.TotalManCount == 0) return;

            var sold = party.PrisonRoster.ToFlattenedRoster();
            var prison = settlement.Party.PrisonRoster;

            foreach (var element in party.PrisonRoster.GetTroopRoster().ToList())
            {
                party.PrisonRoster.AddToCounts(element.Character, -element.Number, false, -element.WoundedNumber);
                prison.AddToCounts(element.Character, element.Number, false, element.WoundedNumber);
                if (element.Character.IsHero)
                    EnterSettlementAction.ApplyForPrisoner(element.Character.HeroObject, settlement);
            }

            _tradeFlag(envoy) = true;
            try
            {
                CampaignEventDispatcher.Instance.OnPrisonerDonatedToSettlement(party, sold, settlement);
            }
            finally
            {
                _tradeFlag(envoy) = false;
            }

            Campaign.Current.GameMenuManager.RefreshMenuOptions(Campaign.Current.CurrentMenuContext);
        }

        private static int PreviewFavor()
        {
            var favor = 0;
            foreach (var element in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
            {
                if (element.Character.IsHero)
                    favor += element.Character.HeroObject.Level / 5 - 1;
                else
                    favor += element.Character.Tier * element.Number;
            }
            return favor;
        }
    }
}
