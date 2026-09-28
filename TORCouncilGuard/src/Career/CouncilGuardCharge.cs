using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TaleWorlds.CampaignSystem;
using TOR_Core.Extensions;

namespace TORCouncilGuard.Career
{
    internal static class CouncilGuardCharge
    {
        internal static float Charge(
            Agent affectorAgent,
            Agent affectedAgent,
            ChargeType chargeType,
            int chargeValue,
            AttackTypeMask mask,
            CareerHelper.ChargeCollisionFlag collisionFlag)
        {
            if (affectorAgent == null || !affectorAgent.IsMainAgent) return 0f;
            if (mask != AttackTypeMask.Melee) return 0f;

            if (chargeType != ChargeType.DamageDone) return 0f;

            var rate = Hero.MainHero.HasCareerChoice("ToriourKeystone") ? 2f : 1f;
            return chargeValue * rate;
        }
    }
}
