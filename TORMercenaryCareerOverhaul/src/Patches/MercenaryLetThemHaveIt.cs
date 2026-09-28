using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryLetThemHaveIt
    {
        internal const string MercenaryLordKeystone = "MercenaryLordKeystone";

        internal static void Apply(Harmony harmony)
        {
            var charge = AccessTools.Method(typeof(CareerHelper), nameof(CareerHelper.ApplyCareerAbilityCharge));
            if (charge == null)
                throw new MissingMethodException("CareerHelper.ApplyCareerAbilityCharge not found.");

            harmony.Patch(charge, postfix: new HarmonyMethod(
                AccessTools.Method(typeof(MercenaryLetThemHaveIt), nameof(CutCooldownOnKill))));
        }

        private static void CutCooldownOnKill(ChargeType chargeType, Agent affector)
        {
            try
            {
                if (chargeType != ChargeType.NumberOfKills) return;
                if (affector == null || !affector.IsMainAgent) return;
                if (Mission.Current == null || Agent.Main == null) return;
                if (Hero.MainHero == null || !Hero.MainHero.HasCareer(TORCareers.Mercenary)) return;
                if (!Hero.MainHero.HasCareerChoice(MercenaryLordKeystone)) return;

                var ability = Agent.Main.GetComponent<AbilityComponent>()?.CareerAbility;
                if (ability == null) return;

                var left = ability.GetCoolDownLeft();
                if (left <= 0) return;

                ability.SetCoolDown(Math.Max(0, left - Data.KillCooldown));
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryLetThemHaveIt: " + ex.Message);
            }
        }
    }
}
