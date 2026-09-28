using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Utilities;
using TaleWorlds.CampaignSystem;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal static class ScatterShot
    {
        private const int Pellets = 6;
        private const int GunneryPellets = 9;
        private const float PelletDamageFactor = 0.5f;
        private const string MusketBallId = "tor_neutral_weapon_ammo_musket_ball";
        private const float Spread = 0.05f;
        private const float PackItInFactor = 1.5f;

        internal static void Fire(Agent shooter, MissionWeapon weapon, Vec3 position, Mat3 orientation, Vec3 velocity, bool replaceFired = true)
        {
            var ball = MBObjectManager.Instance.GetObject<ItemObject>(MusketBallId);
            var usage = weapon.CurrentUsageItem;
            if (ball == null || usage == null)
            {
                Log.Warn("Scatter Shot ammunition not found; fired a normal shot.");
                return;
            }

            if (replaceFired)
            {
                var fired = Mission.Current.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
                if (fired != null)
                    Mission.Current.RemoveMissileAsClient(fired.Index);
            }

            var pellets = Hero.MainHero.HasCareerChoice(G.Gunnery + "Keystone") ? GunneryPellets : Pellets;
            if (shooter.Character is CharacterObject character && character.GetPerkValue(TORPerks.GunPowder.PackItIn))
                pellets = (int)(pellets * PackItInFactor);

            var pellet = new MissionWeapon(ball, null, null);
            var ballDamage = MissileDamage.Ammo(pellet);
            var bonus = MissileDamage.Bonus(MissileDamage.Gun(weapon) + ballDamage, PelletDamageFactor, ballDamage);

            var speed = velocity.Length;
            for (var i = 0; i < pellets; i++)
            {
                var deviation = TORCommon.GetRandomOrientation(orientation, Spread);
                MissileDamage.With(bonus, () =>
                    Mission.Current.AddCustomMissileWithWeaponDamage(shooter, pellet, position, deviation.f, deviation, speed, speed, false));
            }
        }
    }
}
