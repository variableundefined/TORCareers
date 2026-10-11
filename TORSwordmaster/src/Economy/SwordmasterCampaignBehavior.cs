using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
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

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
            CampaignEvents.PerkOpenedEvent.AddNonSerializedListener(this, OnPerkOpened);
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
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

        private static void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            if (SwordmasterCompanions.IsSwordmaster(hero)) SwordmasterCompanions.Setup(hero);
        }
    }
}
