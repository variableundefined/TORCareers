using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TOR_Core.Utilities;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Abilities
{
    internal class Misfires
    {
        private const string FouledEffect = "ie_misfire_fouled";
        private const string DryFireClick = "event:/mission/combat/missile/foley/lightcrossbowrelease";
        private static readonly string[] DryFireAlerts = { "event:/alerts/naval/out_of_ammo", DryFireClick };
        private const string MuzzleFlash = "handgun_shoot_2";
        private const int MusketSounds = 5;
        private const float FouledDuration = 6f;
        private const float HangfireDelay = 1f;
        private const float ChainFireSpread = 0.15f;
        private const int MaxChainFireRounds = 3;
        private const float MuzzleOffset = 0.8f;

        private const float FlashWeight = 0.30f;
        private const float HangfireWeight = 0.30f;
        private const float FouledWeight = 0.25f;
        private const float FlashPerSeverity = 0.10f;
        private const float HangfirePerSeverity = 0.05f;
        private const float SeverityShift = FlashPerSeverity + HangfirePerSeverity;
        private const float ScatterFouledWeight = 0.50f;

        private enum Kind { FlashInThePan, Hangfire, Fouled, BurstBarrel, ChainFire }

        private readonly Action<Agent, MissionWeapon, List<Mission.Missile>, AmmoType, bool> _onLoadedMissiles;

        internal Misfires(Action<Agent, MissionWeapon, List<Mission.Missile>, AmmoType, bool> onLoadedMissiles)
        {
            _onLoadedMissiles = onLoadedMissiles;
        }

        private readonly List<(Agent Shooter, MissionWeapon Weapon, MissionWeapon Ammo, AmmoType Loaded, bool Twin, float Speed, float Delay)> _hangfires =
            new List<(Agent, MissionWeapon, MissionWeapon, AmmoType, bool, float, float)>();

        internal bool Trigger(Agent shooter, EquipmentIndex weaponIndex, Vec3 position, Mat3 orientation, Vec3 velocity, bool twin)
        {
            try
            {
                var kind = Roll(shooter.Equipment[weaponIndex], Munition.MisfireSeverity(Hero.MainHero));
                switch (kind)
                {
                    case Kind.FlashInThePan:
                        RemoveFiredMissile(shooter);
                        DryFire(shooter, position);
                        break;
                    case Kind.Hangfire:
                        var weapon = shooter.Equipment[weaponIndex];
                        RemoveFiredMissile(shooter);
                        _hangfires.Add((shooter, weapon, weapon.AmmoWeapon, Ammo.Selected, twin, velocity.Length, HangfireDelay));
                        break;
                    case Kind.Fouled:
                        shooter.ApplyStatusEffect(FouledEffect, shooter, FouledDuration, append: false);
                        break;
                    case Kind.ChainFire:
                        Munition.Misfire(shooter);
                        ChainFire(shooter, weaponIndex, position, orientation, velocity);
                        break;
                    default:
                        Munition.Misfire(shooter);
                        break;
                }

                if (shooter.IsMainAgent)
                    InformationManager.DisplayMessage(new InformationMessage(Describe(kind).ToString(), Colors.Red));

                return kind == Kind.FlashInThePan || kind == Kind.Hangfire;
            }
            catch (Exception e)
            {
                Log.Error("Misfire failed: " + e.Message);
                return false;
            }
        }

        internal void Tick(float dt)
        {
            for (var i = _hangfires.Count - 1; i >= 0; i--)
            {
                var pending = _hangfires[i];
                pending.Delay -= dt;
                if (pending.Delay > 0f)
                {
                    _hangfires[i] = pending;
                    continue;
                }

                _hangfires.RemoveAt(i);
                try
                {
                    if (pending.Shooter.IsActive() && !pending.Ammo.IsEmpty)
                        Hangfire(pending.Shooter, pending.Weapon, pending.Ammo, pending.Loaded, pending.Twin, pending.Speed);
                }
                catch (Exception e)
                {
                    Log.Error("Hangfire failed: " + e.Message);
                }
            }
        }

        private static Kind Roll(MissionWeapon weapon, int severity)
        {
            var flash = FlashWeight - FlashPerSeverity * severity;
            var hangfire = HangfireWeight - HangfirePerSeverity * severity;

            var ammo = weapon.AmmoWeapon;
            var scatter = !ammo.IsEmpty && ammo.Item != null && Firearms.IsScatterAmmo(ammo.Item);
            var lastRound = weapon.MaxAmmo <= 1 || weapon.Ammo <= 1;
            var repeaterWithRounds = weapon.MaxAmmo > 1 && weapon.Ammo > 1 && !scatter;

            if (scatter)
                return lastRound && MBRandom.RandomFloat < ScatterFouledWeight - SeverityShift * severity ? Kind.Fouled : Kind.BurstBarrel;

            var roll = MBRandom.RandomFloat;
            if (roll < flash) return Kind.FlashInThePan;
            roll -= flash;
            if (roll < hangfire) return Kind.Hangfire;
            roll -= hangfire;
            if (roll < FouledWeight) return lastRound ? Kind.Fouled : Kind.FlashInThePan;
            return repeaterWithRounds ? Kind.ChainFire : Kind.BurstBarrel;
        }

        private static void RemoveFiredMissile(Agent shooter)
        {
            var missile = Mission.Current.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (missile != null)
                Mission.Current.RemoveMissileAsClient(missile.Index);
        }

        private void Hangfire(Agent shooter, MissionWeapon weapon, MissionWeapon ammo, AmmoType loaded, bool twin, float speed)
        {
            var orientation = shooter.LookRotation;
            var position = shooter.GetEyeGlobalPosition() + orientation.f * MuzzleOffset;
            MuzzleEffects(position, orientation);

            if (loaded == AmmoType.Scatter && !Firearms.FiresScatter(weapon))
            {
                ScatterShot.Fire(shooter, weapon, position, orientation, orientation.f * speed, replaceFired: false, twin: twin);
                return;
            }

            if (twin)
            {
                var ammoDamage = MissileDamage.Ammo(ammo);
                var bonus = MissileDamage.Bonus(MissileDamage.Gun(weapon) + ammoDamage, TwinShot.DamageFactor, ammoDamage);
                _onLoadedMissiles?.Invoke(shooter, weapon, TwinShot.Spawn(shooter, ammo, position, orientation, speed, bonus), loaded, true);
                return;
            }

            var missile = MissileDamage.With(MissileDamage.Gun(weapon), () =>
                Mission.Current.AddCustomMissileWithWeaponDamage(shooter, ammo, position, orientation.f, orientation, speed, speed, false));
            _onLoadedMissiles?.Invoke(shooter, weapon, new List<Mission.Missile> { missile }, loaded, false);
        }

        internal static void MuzzleEffects(Vec3 position, Mat3 orientation)
        {
            Mission.Current.AddParticleSystemBurstByName(MuzzleFlash, new MatrixFrame(orientation, position), false);
            PlaySound("musket_fire_sound_" + (MBRandom.RandomInt(MusketSounds) + 1), position);
        }

        private void ChainFire(Agent shooter, EquipmentIndex weaponIndex, Vec3 position, Mat3 orientation, Vec3 velocity)
        {
            var weapon = shooter.Equipment[weaponIndex];
            var ammo = weapon.AmmoWeapon;
            if (weapon.IsEmpty || ammo.IsEmpty) return;

            var loaded = (int)weapon.Ammo;
            var rounds = Math.Min(loaded, MaxChainFireRounds);
            if (rounds <= 0) return;

            var speed = velocity.Length;
            var missiles = new List<Mission.Missile>();
            for (var i = 0; i < rounds; i++)
            {
                var deviation = TORCommon.GetRandomOrientation(orientation, ChainFireSpread);
                missiles.Add(Mission.Current.AddCustomMissileWithWeaponDamage(shooter, ammo, position, deviation.f, deviation, speed, speed, false));
            }

            for (var slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
            {
                var pouch = shooter.Equipment[slot];
                if (pouch.IsEmpty || pouch.Item != ammo.Item) continue;
                shooter.SetReloadAmmoInSlot(weaponIndex, slot, (short)(loaded - rounds));
                break;
            }

            _onLoadedMissiles?.Invoke(shooter, weapon, missiles, Ammo.Selected, false);
        }

        private static void DryFire(Agent shooter, Vec3 position)
        {
            if (shooter.IsMainAgent)
            {
                foreach (var name in DryFireAlerts)
                    if (SoundEvent.GetEventIdFromString(name) >= 0 && SoundEvent.PlaySound2D(name))
                        return;
                return;
            }
            PlaySound(DryFireClick, position);
        }

        private static void PlaySound(string name, Vec3 position)
        {
            var id = SoundEvent.GetEventIdFromString(name);
            if (id >= 0)
                Mission.Current.MakeSound(id, position, false, false, -1, -1);
        }

        private static TextObject Describe(Kind kind)
        {
            switch (kind)
            {
                case Kind.FlashInThePan: return new TextObject("{=imperial_engineer_misfire_flash}Misfire - Flash in the Pan");
                case Kind.Hangfire: return new TextObject("{=imperial_engineer_misfire_hangfire}Misfire - Hangfire");
                case Kind.Fouled: return new TextObject("{=imperial_engineer_misfire_fouled}Misfire - Fouled");
                case Kind.ChainFire: return new TextObject("{=imperial_engineer_misfire_chain}Misfire - Chain Fire");
                default: return new TextObject("{=imperial_engineer_misfire_burst}Misfire - Burst Barrel");
            }
        }
    }
}
