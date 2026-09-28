using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    internal static class EngineeringExperience
    {
        private const int XpPerVictimLevel = 10;

        internal static void OnKill(Agent affector, Agent affected, KillingBlow blow)
        {
            try
            {
                if (affector == null || !affector.IsMainAgent || affected == null) return;
                if (!CareerHelper.IsValidCareerMissionInteractionBetweenAgents(affector, affected)) return;
                if (TORDamageHelper.DetermineMask(blow) != AttackTypeMask.Ranged) return;
                if (affected.Character == null || !affected.IsEnemyOf(affector)) return;
                if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Customized + "Passive4")) return;

                Hero.MainHero.AddSkillXp(DefaultSkills.Engineering, XpPerVictimLevel * affected.Character.Level);
            }
            catch (Exception e)
            {
                Log.Warn("Engineering experience grant failed: " + e.Message);
            }
        }
    }
}
