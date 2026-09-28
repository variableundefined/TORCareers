using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    [HarmonyPatch]
    internal static class EngineeringExperience
    {
        private const int XpPerVictimLevel = 10;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CareerHelper), nameof(CareerHelper.ApplyCareerAbilityCharge))]
        private static void OnKill(ChargeType chargeType, AttackTypeMask attackTypeMask, Agent affector, Agent affected)
        {
            try
            {
                if (chargeType != ChargeType.NumberOfKills || attackTypeMask != AttackTypeMask.Ranged) return;
                if (affector == null || !affector.IsMainAgent) return;
                if (affected?.Character == null || !affected.IsEnemyOf(affector)) return;
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
