using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    internal static class CouncilGuardFaith
    {
        internal const int XpPerVictimLevel = 10;
        private const string Choice = "GuardianOfTorLithanelPassive4";

        internal static void OnKill(Agent affector, Agent affected)
        {
            try
            {
                if (affector == null || !affector.IsMainAgent) return;
                if (affected?.Character == null || !affected.IsEnemyOf(affector)) return;

                var hero = Hero.MainHero;
                if (hero == null || !hero.HasCareer(CouncilGuardCareer.Career)) return;
                if (!hero.HasCareerChoice(Choice)) return;

                hero.AddSkillXp(TORSkills.Faith, XpPerVictimLevel * affected.Character.Level);
            }
            catch (Exception e)
            {
                Log.Warn("Faith grant failed: " + e.Message);
            }
        }
    }
}
