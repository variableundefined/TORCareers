using System;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions.ExtendedInfoSystem;

namespace TORImperialEngineer.Career
{
    internal static class ImperialEngineerCharge
    {
        private const int MaxChargePerHit = 120;

        internal static float Charge(
            Agent affectorAgent,
            Agent affectedAgent,
            ChargeType chargeType,
            int chargeValue,
            AttackTypeMask mask,
            CareerHelper.ChargeCollisionFlag collisionFlag)
        {
            if (affectorAgent == null || affectedAgent == null || !affectorAgent.IsMainAgent) return 0f;
            if (mask != AttackTypeMask.Ranged && mask != AttackTypeMask.Melee) return 0f;
            if (chargeType != ChargeType.DamageDone) return 0f;
            if (Has(collisionFlag, CareerHelper.ChargeCollisionFlag.HitShield)) return 0f;
            if (affectorAgent.Team == affectedAgent.Team) return 0f;

            return Math.Min(MaxChargePerHit, chargeValue);
        }

        private static bool Has(CareerHelper.ChargeCollisionFlag value, CareerHelper.ChargeCollisionFlag flag) =>
            (value & flag) == flag;
    }
}
