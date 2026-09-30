using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal static class TwinShot
    {
        internal const float DamageFactor = 0.75f;
        internal const float VerticalGap = 0.004f;

        private static bool Active =>
            ImperialEngineerCareer.IsPlayer && Hero.MainHero.HasCareerChoice(G.Cavalcade + "Passive1");

        internal static bool Fires(Agent agent, MissionWeapon weapon) =>
            agent != null && agent.IsMainAgent && Active
            && Firearms.IsSingleShotGun(weapon) && !Firearms.FiresScatter(weapon)
            && !Firearms.IsGrenadeItem(weapon.AmmoWeapon.Item);

        internal static IEnumerable<Mat3> Barrels(Mat3 orientation)
        {
            var upper = orientation;
            upper.RotateAboutSide(VerticalGap);
            var lower = orientation;
            lower.RotateAboutSide(-VerticalGap);
            return new[] { upper, lower };
        }

        internal static void Fire(Agent shooter, MissionWeapon weapon, Vec3 position, Mat3 orientation, Vec3 velocity)
        {
            var fired = Mission.Current.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (fired != null)
                Mission.Current.RemoveMissileAsClient(fired.Index);

            var ammo = MissileDamage.Ammo(weapon.AmmoWeapon);
            Spawn(shooter, weapon.AmmoWeapon, position, orientation, velocity.Length,
                MissileDamage.Bonus(MissileDamage.Gun(weapon) + ammo, DamageFactor, ammo));
        }

        internal static List<Mission.Missile> Spawn(Agent shooter, MissionWeapon ammo, Vec3 position, Mat3 orientation, float speed, float bonus)
        {
            var missiles = new List<Mission.Missile>();
            if (ammo.IsEmpty) return missiles;

            foreach (var barrel in Barrels(orientation))
            {
                var missile = MissileDamage.With(bonus, () =>
                    Mission.Current.AddCustomMissileWithWeaponDamage(shooter, ammo, position, barrel.f, barrel, speed, speed, false));
                if (missile == null) continue;
                TOR_Core.BattleMechanics.Firearms.FirearmsMissionLogic.ApplyWeaponTraitParticles(missile, shooter);
                missiles.Add(missile);
            }
            return missiles;
        }
    }
}
