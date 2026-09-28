using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class DuelistLeadership
    {
        internal const string Card = "DuelistPassive4";
        internal const int XpPerVictimLevel = 10;

        internal static void Apply(Harmony harmony)
        {
            var charge = AccessTools.Method(typeof(CareerHelper), nameof(CareerHelper.ApplyCareerAbilityCharge))
                         ?? throw new MissingMethodException("CareerHelper.ApplyCareerAbilityCharge not found.");

            harmony.Patch(charge, postfix: new HarmonyMethod(AccessTools.Method(typeof(DuelistLeadership), nameof(OnKill))));
        }

        private static void OnKill(ChargeType chargeType, Agent affector, Agent affected)
        {
            try
            {
                if (chargeType != ChargeType.NumberOfKills) return;
                if (affector == null || !affector.IsMainAgent) return;
                if (affected?.Character == null || !affected.IsEnemyOf(affector)) return;
                if (Hero.MainHero == null || !Hero.MainHero.HasCareer(TORCareers.Mercenary)) return;
                if (!Hero.MainHero.HasCareerChoice(Card)) return;

                Hero.MainHero.AddSkillXp(DefaultSkills.Leadership, XpPerVictimLevel * affected.Character.Level);
            }
            catch (Exception ex)
            {
                Log.Write("DuelistLeadership: " + ex.Message);
            }
        }
    }
}
