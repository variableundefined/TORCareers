using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Career;
using TORSwordmaster.Companions;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Economy
{
    internal class SwordmasterCampaignBehavior : CampaignBehaviorBase
    {
        internal const int MeleeTroopDailyXp = 25;
        internal const int CompanionDailyXp = 100;

        // Puts the Companion option ABOVE everything else since TOR's priority are all 200.
        private const int EnvoyHubPriority = 201;

        private Hero _candidate;
        private bool _meetCandidate;
        private bool _openCandidateConversation;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
            CampaignEvents.PerkOpenedEvent.AddNonSerializedListener(this, OnPerkOpened);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private static bool IsSwordmaster()
        {
            var career = SwordmasterCareer.Career;
            return career != null && Hero.MainHero != null && Hero.MainHero.HasCareer(career);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (IsSwordmaster()) TechniqueUnlocks.Grant(Hero.MainHero);
            AddDialogs(starter);
        }

        private static void OnCharacterCreationIsOver()
        {
            SwordmasterCharacterCreation.OnCharacterCreationIsOver();
            if (IsSwordmaster()) TechniqueUnlocks.Grant(Hero.MainHero);
        }

        private static void OnPerkOpened(Hero hero, PerkObject perk)
        {
            if (hero == Hero.MainHero && IsSwordmaster()) TechniqueUnlocks.Grant(hero);
        }

        private static void OnDailyTick()
        {
            if (!IsSwordmaster()) return;
            TechniqueUnlocks.Grant(Hero.MainHero);

            var party = MobileParty.MainParty;
            if (party == null) return;

            if (G.Has(G.Passive(G.ThirtyForms, 2)))
            {
                var roster = party.MemberRoster;
                for (var i = 0; i < roster.Count; i++)
                {
                    var troop = roster.GetCharacterAtIndex(i);
                    if (troop != null && !troop.IsHero && !troop.IsRanged)
                        roster.AddXpToTroopAtIndex(MeleeTroopDailyXp * roster.GetElementNumber(i), i);
                }
            }

            if (G.Has(G.Passive(G.SwordOfHoeth, 4)))
            {
                foreach (var hero in party.GetMemberHeroes().Where(h => h.HasAttribute(SwordmasterCareerChoices.CompanionAttribute)))
                {
                    hero.AddSkillXp(DefaultSkills.TwoHanded, CompanionDailyXp);
                    hero.AddSkillXp(TORSkills.Faith, CompanionDailyXp);
                }
            }
        }

        private static bool IsAsurEnvoy() =>
            Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.HasAttribute("AsurEnvoy");

        private bool IsCandidate() =>
            _candidate != null && Hero.OneToOneConversationHero == _candidate;

        private static void SetCostVariables()
        {
            GameTexts.SetVariable("SM_COMPANION_GOLD", SwordmasterCompanions.GoldCost);
            GameTexts.SetVariable("SM_COMPANION_FAVOR", SwordmasterCompanions.FavorCost);
            GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            var favor = CustomResourceManager.GetResourceObject("CouncilFavor");
            if (favor != null) GameTexts.SetVariable("FAVOR_ICON", favor.GetCustomResourceIconAsText());
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("sm_envoy_companion", "asur_envoy_main_hub", "sm_envoy_companion_offer",
                "{=sm_envoy_companion_ask}I seek a companion trained in the way of the sword",
                () => IsAsurEnvoy() && IsSwordmaster(), SetCostVariables, EnvoyHubPriority);

            starter.AddDialogLine("sm_envoy_companion_offer", "sm_envoy_companion_offer", "sm_envoy_companion_choice",
                "{=sm_envoy_companion_offer}Very well. I know of a student of the sword whose training is nearly done. It will cost you {SM_COMPANION_FAVOR}{FAVOR_ICON} and {SM_COMPANION_GOLD}{GOLD_ICON}.",
                IsAsurEnvoy, null, 150);

            starter.AddPlayerLine("sm_envoy_companion_meet", "sm_envoy_companion_choice", "close_window",
                "{=sm_envoy_companion_meet}Let me meet them.",
                () => IsAsurEnvoy() && SwordmasterCompanions.CanAfford(), () => _meetCandidate = true, 150);

            starter.AddPlayerLine("sm_envoy_companion_decline", "sm_envoy_companion_choice", "back_to_main_hub_asur",
                "{=sm_envoy_not_now}Not now.", () => IsAsurEnvoy() && SwordmasterCompanions.CanAfford(), null, 150);

            starter.AddPlayerLine("sm_envoy_companion_poor", "sm_envoy_companion_choice", "back_to_main_hub_asur",
                "{=sm_envoy_cannot_afford_one}I do not have enough to hire one.",
                () => IsAsurEnvoy() && !SwordmasterCompanions.CanAfford(), null, 150);

            starter.AddDialogLine("sm_candidate_start", "start", "sm_candidate_offer",
                "{=sm_candidate_intro}You wished to see me?",
                IsCandidate, SetCostVariables, 300);

            starter.AddPlayerLine("sm_candidate_offer", "sm_candidate_offer", "sm_candidate_reply",
                "{=sm_candidate_offer}I'm looking to take on an apprentice of my own, and train them in the Way of the Sword.",
                IsCandidate, null, 150);

            starter.AddDialogLine("sm_candidate_reply", "sm_candidate_reply", "sm_candidate_choice",
                "{=sm_candidate_reply}I would accept, but it means leaving my master before my training is finished. They will not let me go for nothing.",
                IsCandidate, null, 150);
            starter.AddPlayerLine("sm_candidate_hire", "sm_candidate_choice", "close_window",
                "{=sm_candidate_hire}Then I will settle it with your master. ({SM_COMPANION_GOLD}{GOLD_ICON} {SM_COMPANION_FAVOR}{FAVOR_ICON})",
                IsCandidate, Hire, 150,
                (out TextObject explanation) =>
                {
                    if (!SwordmasterCompanions.HasCompanionRoom())
                    {
                        explanation = new TextObject("{=sm_envoy_companion_limit}You have reached your companion limit.");
                        return false;
                    }
                    if (!SwordmasterCompanions.CanAfford())
                    {
                        explanation = new TextObject("{=sm_envoy_cannot_afford}You cannot afford this.");
                        return false;
                    }
                    explanation = TextObject.GetEmpty();
                    return true;
                });

            starter.AddPlayerLine("sm_candidate_leave", "sm_candidate_choice", "close_window",
                "{=sm_candidate_decline}Then stay with your master, for now.", IsCandidate, null, 150);
        }

        private void Hire()
        {
            try
            {
                SwordmasterCompanions.Hire(_candidate);
                Log.Write("Swordmaster companion hired: " + _candidate.Name);
                _candidate = null;
            }
            catch (Exception e)
            {
                Log.Error("Swordmaster companion not hired: " + e.Message);
            }
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> characters)
        {
            if (_candidate != null && characters != null && characters.Contains(_candidate.CharacterObject))
            {
                SwordmasterCompanions.Discard(_candidate);
                _candidate = null;
            }

            if (_meetCandidate)
            {
                _meetCandidate = false;
                _openCandidateConversation = true;
            }
        }

        private void OnTick(float dt)
        {
            if (!_openCandidateConversation) return;
            _openCandidateConversation = false;

            try
            {
                _candidate = SwordmasterCompanions.CreateCandidate();
                if (_candidate == null) return;

                var player = new ConversationCharacterData(Hero.MainHero.CharacterObject, Hero.MainHero.PartyBelongedTo?.Party);
                var candidate = new ConversationCharacterData(_candidate.CharacterObject);
                Campaign.Current.CurrentConversationContext = ConversationContext.Default;
                Campaign.Current.ConversationManager.OpenMapConversation(player, candidate);
            }
            catch (Exception e)
            {
                Log.Error("Could not introduce the Swordmaster: " + e.Message);
                SwordmasterCompanions.Discard(_candidate);
                _candidate = null;
            }
        }
    }
}
