using System;
using System.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CharacterDevelopment.CareerSystem.Button;
using TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;
using TOR_Core.Extensions;
using TOR_Core.Extensions.UI;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryRecruitButton
    {
        private const string Coin = @"General\Icons\Coin@2x";

        internal static void Apply(Harmony harmony)
        {
            var behaviour = typeof(MercenaryCareerButtonBehavior);

            var visible = AccessTools.Method(behaviour, "ShouldButtonBeVisible")
                          ?? throw new MissingMethodException(behaviour.Name, "ShouldButtonBeVisible");
            var active = AccessTools.Method(behaviour, "ShouldButtonBeActive")
                         ?? throw new MissingMethodException(behaviour.Name, "ShouldButtonBeActive");
            var clicked = AccessTools.Method(behaviour, "ButtonClickedEvent")
                          ?? throw new MissingMethodException(behaviour.Name, "ButtonClickedEvent");
            var icon = AccessTools.PropertyGetter(behaviour, "CareerButtonIcon")
                       ?? throw new MissingMethodException(behaviour.Name, "CareerButtonIcon");

            harmony.Patch(visible, prefix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryRecruitButton), nameof(Visible))));
            harmony.Patch(active, prefix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryRecruitButton), nameof(Active))));
            harmony.Patch(clicked, prefix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryRecruitButton), nameof(Clicked))));
            harmony.Patch(icon, prefix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryRecruitButton), nameof(Icon))));

            var row = AccessTools.Method(typeof(PartyCharacterVMExtension), "ExecuteButtonClick")
                      ?? throw new MissingMethodException("PartyCharacterVMExtension", "ExecuteButtonClick");

            harmony.Patch(row, postfix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryRecruitButton), nameof(Refresh))));
        }

        private static void Refresh(PartyCharacterVMExtension __instance)
        {
            try
            {
                if (!Recruits.IsMercenary() || __instance == null) return;
                __instance.RefreshValues();
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryRecruitButton: " + ex.Message);
            }
        }

        private static bool Icon(ref string __result)
        {
            if (!Recruits.IsMercenary()) return true;

            __result = Coin;
            return false;
        }

        private static bool Visible(CharacterObject characterObject, bool isPrisoner, ref bool __result)
        {
            if (!Recruits.IsMercenary()) return true;

            __result = false;
            try
            {
                if (characterObject == null || characterObject.IsHero || isPrisoner) return false;
                if (PartyScreenHelper.GetActivePartyState()?.PartyScreenMode != PartyScreenHelper.PartyScreenMode.Normal) return false;
                __result = Recruits.ShowsFor(characterObject);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryRecruitButton: " + ex.Message);
            }

            return false;
        }

        private static bool Active(CharacterObject characterObject, ref TextObject displayText, bool isPrisoner, ref bool __result)
        {
            if (!Recruits.IsMercenary()) return true;

            __result = false;
            try
            {
                if (characterObject == null || characterObject.IsHero || isPrisoner) return false;

                var promotion = Recruits.For(characterObject, out var reason);
                if (promotion == null)
                {
                    displayText = new TextObject(reason ?? Recruits.TooHighTier);
                    return false;
                }

                var resource = Hero.MainHero.GetCultureSpecificCustomResource();
                var costText = promotion.Resource > 0
                    ? promotion.Gold + " denars and " + promotion.Resource + " " + resource.Name
                    : promotion.Gold + " denars";

                if (Affordable(characterObject, promotion) < 1)
                {
                    displayText = new TextObject("Needs " + costText + ".");
                    return false;
                }

                displayText = new TextObject("Induct into " + promotion.Target.Name + " for " + costText
                                             + ". Shift-click for up to " + Recruits.MaxBatch + ".");
                __result = true;
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryRecruitButton: " + ex.Message);
            }

            return false;
        }

        private static bool Clicked(CharacterObject characterObject, bool isPrisoner, bool shiftClick)
        {
            if (!Recruits.IsMercenary()) return true;

            try
            {
                if (characterObject == null || characterObject.IsHero || isPrisoner) return false;

                var promotion = Recruits.For(characterObject, out _);
                if (promotion == null) return false;

                var count = Math.Min(shiftClick ? Recruits.MaxBatch : 1, Affordable(characterObject, promotion));
                if (count < 1) return false;

                var resource = Hero.MainHero.GetCultureSpecificCustomResource();
                var logic = PartyScreenHelper.GetActivePartyState().PartyScreenLogic;

                for (var i = 0; i < count; i++)
                {
                    logic.CurrentData.PartyGoldChangeAmount -= promotion.Gold;
                    if (promotion.Resource > 0) CustomResourceManager.AddResourceChanges(resource, promotion.Resource);
                    CareerButtonHelper.ExchangeUnitForNewUnit(characterObject, promotion.Target, true);
                }
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryRecruitButton: " + ex.Message);
            }

            return false;
        }

        private static int Affordable(CharacterObject troop, Promotion promotion)
        {
            var party = MobileParty.MainParty;
            if (party == null) return 0;

            var inParty = party.MemberRoster.GetElementNumber(troop);

            var gold = Hero.MainHero.Gold + PartyScreenHelper.GetActivePartyState().PartyScreenLogic.CurrentData.PartyGoldChangeAmount;
            var pending = CustomResourceManager.GetPendingResources().Values.Sum();
            var resource = Hero.MainHero.GetCultureSpecificCustomResourceValue() - pending;

            var byGold = promotion.Gold > 0 ? gold / promotion.Gold : int.MaxValue;
            var byResource = promotion.Resource > 0 ? (int)(resource / promotion.Resource) : int.MaxValue;

            return Math.Min(inParty, Math.Min(byGold, byResource));
        }
    }
}
