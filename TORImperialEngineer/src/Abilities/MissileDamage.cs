using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.HarmonyPatches;

namespace TORImperialEngineer.Abilities
{
    internal static class MissileDamage
    {
        private static float? _override;

        internal static float Ammo(MissionWeapon ammo) =>
            !ammo.IsEmpty && ammo.CurrentUsageItem != null ? ammo.GetModifiedThrustDamageForCurrentUsage() : 0f;

        internal static float Gun(MissionWeapon gun) =>
            !gun.IsEmpty && gun.CurrentUsageItem != null ? gun.GetModifiedThrustDamageForCurrentUsage() : 0f;

        internal static float Shot(MissionWeapon gun) => Gun(gun) + Ammo(gun.AmmoWeapon);

        internal static float Bonus(float shotDamage, float factor, float ammoDamage) =>
            MathF.Max(0f, shotDamage * factor - ammoDamage);

        internal static T With<T>(float bonus, Func<T> spawn)
        {
            _override = bonus;
            try
            {
                return spawn();
            }
            finally
            {
                _override = null;
            }
        }

        private static void Adjust(Agent shooterAgent, ref float damageBonus)
        {
            if (_override.HasValue)
            {
                damageBonus = _override.Value;
                return;
            }

            if (!MissionPatches.UseWeaponDamageForCustomMissile || shooterAgent == null) return;

            var gun = shooterAgent.WieldedWeapon;
            if (!TwinShot.Fires(shooterAgent, gun)) return;

            var ammo = Ammo(gun.AmmoWeapon);
            damageBonus = Bonus(Gun(gun) + ammo, TwinShot.DamageFactor, ammo);
        }

        [HarmonyPatch(typeof(Mission), "AddMissileAux")]
        internal static class CustomMissile
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Agent shooterAgent, ref float damageBonus) => Adjust(shooterAgent, ref damageBonus);
        }

        [HarmonyPatch(typeof(Mission), "AddMissileSingleUsageAux")]
        internal static class CustomMissileSingleUsage
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Agent shooterAgent, ref float damageBonus) => Adjust(shooterAgent, ref damageBonus);
        }
    }
}
