using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.CampaignMechanics
{
    internal class MercenaryMissionLogic : MissionLogic
    {
        internal const string MercenaryLordKeystone = "MercenaryLordKeystone";
        internal const string SurvivalistKeystone = "SurvivalistKeystone";
        internal const string DuelistCard = "DuelistPassive4";
        internal const int XpPerVictimLevel = 10;

        // OnAgentBuild: TOR's AbilityManagerMissionLogic creates the career ability in OnAgentCreated.
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            try
            {
                if (agent == null || !agent.IsMainAgent) return;

                var hero = Hero.MainHero;
                if (hero == null || agent.GetHero() != hero) return;
                if (!hero.HasCareer(TORCareers.Mercenary)) return;
                if (!hero.HasCareerChoice(SurvivalistKeystone)) return;

                agent.GetComponent<AbilityComponent>()?.CareerAbility?.SetCoolDown(0);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryMissionLogic build: " + ex.Message);
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            try
            {
                if (affectorAgent == null || !affectorAgent.IsMainAgent) return;
                if (!CareerHelper.IsValidCareerMissionInteractionBetweenAgents(affectorAgent, affectedAgent)) return;

                var hero = Hero.MainHero;
                if (hero == null || !hero.HasCareer(TORCareers.Mercenary)) return;

                if (hero.HasCareerChoice(MercenaryLordKeystone)) CutCooldown(affectorAgent);
                if (hero.HasCareerChoice(DuelistCard)) GrantLeadership(affectorAgent, affectedAgent);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryMissionLogic kill: " + ex.Message);
            }
        }

        private static void CutCooldown(Agent killer)
        {
            var ability = killer.GetComponent<AbilityComponent>()?.CareerAbility;
            if (ability == null) return;

            var left = ability.GetCoolDownLeft();
            if (left <= 0) return;

            ability.SetCoolDown(Math.Max(0, left - Data.KillCooldown));
        }

        private static void GrantLeadership(Agent killer, Agent victim)
        {
            if (victim?.Character == null || !victim.IsEnemyOf(killer)) return;

            Hero.MainHero.AddSkillXp(DefaultSkills.Leadership, XpPerVictimLevel * victim.Character.Level);
        }
    }
}
