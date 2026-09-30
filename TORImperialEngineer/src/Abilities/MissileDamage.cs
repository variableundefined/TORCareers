using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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

        private static void Adjust(ref float damageBonus)
        {
            if (_override.HasValue)
                damageBonus = _override.Value;
        }

        [HarmonyPatch(typeof(Mission), "AddMissileAux")]
        internal static class CustomMissile
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(ref float damageBonus) => Adjust(ref damageBonus);
        }

        [HarmonyPatch(typeof(Mission), "AddMissileSingleUsageAux")]
        internal static class CustomMissileSingleUsage
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(ref float damageBonus) => Adjust(ref damageBonus);
        }
    }
}
