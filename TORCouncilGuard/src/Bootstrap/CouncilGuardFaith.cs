using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch]
    internal static class CouncilGuardFaith
    {
        internal const int XpPerVictimLevel = 10;
        private const string Choice = "GuardianOfTorLithanelPassive4";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CareerHelper), nameof(CareerHelper.ApplyCareerAbilityCharge))]
        private static void OnKill(ChargeType chargeType, Agent affector, Agent affected)
        {
            try
            {
                if (chargeType != ChargeType.NumberOfKills) return;
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
