using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.Firearms;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TORWaywatcherOverhaul.Arrows
{
    internal class EnchantedArrowsMissionLogic : MissionLogic
    {
        private const float MissileMemory = 15f;
        private const float TrueflightVerticalGap = 0.003f;
        private const float ShardSpread = 0.015f;
        private const float LethalShardSpread = 0.025f;
        private const float BodkinBonusPerArmour = 0.01f;
        private const float BodkinMaxBonus = 0.5f;
        private const float TrueflightRangeStart = 20f;
        private const float TrueflightBonusPerMetre = 0.01f;
        private const float TrueflightMaxBonus = 0.5f;
        private const float CompanionLethalDuration = 20f;
        private const float SweepInterval = 0.5f;

        private readonly Dictionary<int, (Agent Shooter, ArrowShot Shot)> _burstShots = new Dictionary<int, (Agent, ArrowShot)>();
        private readonly Dictionary<int, float> _trueflightShots = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _bodkinShots = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _shardShots = new Dictionary<int, float>();
        private readonly HashSet<Agent> _band = new HashSet<Agent>();
        private readonly Dictionary<Agent, string> _bandTraits = new Dictionary<Agent, string>();
        private readonly Dictionary<Agent, (int Arrows, float Until)> _bandLethal = new Dictionary<Agent, (int, float)>();
        private bool _selectorsAdded;
        private bool _playerWasLethal;
        private bool _traitsDirty = true;
        private int _seenQuiverVersion = -1;
        private float _sweep;
        private string _appliedTraits;
        private bool? _isArena;

        public override void AfterStart()
        {
            base.AfterStart();
            Quiver.EndLethalShot();
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (!_selectorsAdded && IsPlayerWaywatcher(agent))
                _selectorsAdded = Safe("arrow selectors", () => ArrowSelectors.Add(agent));
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            if (IsBandMember(agent))
            {
                _band.Add(agent);
                _bandTraits.Remove(agent);
                _traitsDirty = true;
            }
            if (!IsPlayerWaywatcher(agent)) return;
            if (!_selectorsAdded)
                _selectorsAdded = Safe("arrow selectors", () => ArrowSelectors.Add(agent));
            _appliedTraits = null;
            _traitsDirty = true;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Quiver.Version != _seenQuiverVersion)
            {
                _seenQuiverVersion = Quiver.Version;
                ShareLethalShot();
                _traitsDirty = true;
            }

            _sweep -= dt;
            if (_sweep <= 0f)
            {
                _sweep = SweepInterval;
                ForgetOldShots();
                ExpireBandLethal();
                _traitsDirty = true;
            }

            if (!_traitsDirty) return;
            _traitsDirty = false;
            SyncTraits();
            SyncBandTraits();
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position,
            Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            var isPlayer = IsPlayerWaywatcher(shooterAgent);
            if ((!isPlayer && !_band.Contains(shooterAgent)) || weaponIndex == EquipmentIndex.None) return;
            if (!ArrowTraits.IsBow(shooterAgent.Equipment[weaponIndex])) return;

            var hero = Hero.MainHero;
            var arrow = Quiver.Loaded;
            var shot = new ArrowShot
            {
                Arrow = arrow,
                Tier = EnchantedArrow.Tier(hero),
                Lethal = isPlayer ? Quiver.IsLethal : IsBandLethal(shooterAgent),
                Essence = hero.HasCareerChoice(EnchantedArrow.StarfireEssence),
                FiredAt = Mission.CurrentTime
            };

            Safe("enchanted arrow", () =>
            {
                if (arrow == ArrowType.SwiftshiverShards)
                    FireShards(shooterAgent, shot, position, velocity, orientation);
                else if (arrow == ArrowType.TrueflightArrow)
                    FireStacked(shooterAgent, shot, position, velocity, orientation, 1f + EnchantedArrow.TrueflightSpeed(shot.Tier), _trueflightShots);
                else if (arrow == ArrowType.ArcaneBodkin && shot.Lethal)
                    FireStacked(shooterAgent, shot, position, velocity, orientation, 1f, _bodkinShots);
                else if (arrow == ArrowType.ArcaneBodkin)
                    RegisterBodkin(shooterAgent, shot);
                else if (EnchantedArrow.Bursts(arrow))
                    RegisterBurst(shooterAgent, shot);
                return true;
            });

            if (!shot.Lethal) return;
            if (isPlayer) Quiver.ConsumeLethalArrow();
            else if (_bandLethal.TryGetValue(shooterAgent, out var lethal))
            {
                _bandLethal[shooterAgent] = (lethal.Arrows - 1, lethal.Until);
                if (lethal.Arrows - 1 <= 0) _traitsDirty = true;
            }
        }

        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            base.OnMissileHit(attacker, victim, isCanceled, collisionData);
            var index = collisionData.AffectorWeaponSlotOrMissileIndex;
            if (!_burstShots.TryGetValue(index, out var entry) || entry.Shooter != attacker) return;

            _burstShots.Remove(index);
            var position = collisionData.CollisionGlobalPosition;
            Safe("arrow burst", () =>
            {
                ArrowImpact.Burst(entry.Shooter, entry.Shot, position, victim);
                return true;
            });
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon,
            in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (affectorAgent == null || !affectorAgent.IsMainAgent || !attackCollisionData.IsMissile) return;
            if (affectedAgent == null || blow.InflictedDamage <= 0) return;
            if (!Hero.MainHero.HasCareerChoice(EnchantedArrow.Hawkeyed)) return;

            var index = attackCollisionData.AffectorWeaponSlotOrMissileIndex;
            if (_shardShots.ContainsKey(index))
            {
                if (CareerHelper.IsValidCareerMissionInteractionBetweenAgents(affectorAgent, affectedAgent))
                    CareerHelper.ApplyCareerAbilityCharge(blow.InflictedDamage, ChargeType.DamageDone,
                        TORDamageHelper.DetermineMask(blow), affectorAgent, affectedAgent, attackCollisionData);
                return;
            }

            if (!affectedAgent.IsActive()) return;
            var extra = 0;
            if (_trueflightShots.ContainsKey(index))
            {
                var distance = affectorAgent.Position.Distance(affectedAgent.Position);
                var bonus = MBMath.ClampFloat((distance - TrueflightRangeStart) * TrueflightBonusPerMetre, 0f, TrueflightMaxBonus);
                extra = (int)(blow.InflictedDamage * bonus);
            }
            else if (_bodkinShots.ContainsKey(index))
            {
                var armour = affectedAgent.GetBaseArmorEffectivenessForBodyPart(attackCollisionData.VictimHitBodyPart);
                extra = (int)(blow.InflictedDamage * MathF.Min(armour * BodkinBonusPerArmour, BodkinMaxBonus));
            }
            if (extra <= 0) return;

            affectedAgent.ApplyDamage(extra, attackCollisionData.CollisionGlobalPosition, affectorAgent,
                doBlow: false, hasShockWave: false, originatesFromAbility: false);
        }

        private void FireStacked(Agent shooter, ArrowShot shot, Vec3 position, Vec3 velocity, Mat3 orientation,
            float speedFactor, Dictionary<int, float> registry)
        {
            var fired = Mission.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (fired == null) return;

            var ammo = fired.Weapon;
            Mission.RemoveMissileAsClient(fired.Index);

            var speed = velocity.Length * speedFactor;
            var direction = velocity.NormalizedCopy();
            var count = shot.Lethal ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                var aim = count > 1 ? Stacked(direction, i == 0 ? -TrueflightVerticalGap : TrueflightVerticalGap) : direction;

                var missile = Mission.AddCustomMissileWithWeaponDamage(shooter, ammo, position, aim, orientation, speed, speed, false);
                if (missile == null) continue;
                FirearmsMissionLogic.ApplyWeaponTraitParticles(missile, shooter);
                registry[missile.Index] = shot.FiredAt;
            }
        }

        private void FireShards(Agent shooter, ArrowShot shot, Vec3 position, Vec3 velocity, Mat3 orientation)
        {
            var fired = Mission.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (fired == null) return;

            var ammo = fired.Weapon;
            Mission.RemoveMissileAsClient(fired.Index);

            var speed = velocity.Length;
            var direction = velocity.NormalizedCopy();
            var side = Vec3.CrossProduct(direction, Vec3.Up);
            side.Normalize();
            var up = Vec3.CrossProduct(side, direction);
            up.Normalize();

            var spread = shot.Lethal ? LethalShardSpread : ShardSpread;
            var count = EnchantedArrow.Shards(shot.Tier) * (shot.Lethal ? 2 : 1);
            for (var i = 0; i < count; i++)
            {
                var aim = (direction
                    + side * MBRandom.RandomFloatRanged(-spread, spread)
                    + up * MBRandom.RandomFloatRanged(-spread, spread)).NormalizedCopy();

                var missile = Mission.AddCustomMissileWithWeaponDamage(shooter, ammo, position, aim, orientation, speed, speed, false);
                if (missile == null) continue;
                FirearmsMissionLogic.ApplyWeaponTraitParticles(missile, shooter);
                _shardShots[missile.Index] = shot.FiredAt;
            }
        }

        private static Vec3 Stacked(Vec3 direction, float gap)
        {
            var side = Vec3.CrossProduct(direction, Vec3.Up);
            side.Normalize();
            var up = Vec3.CrossProduct(side, direction);
            up.Normalize();
            return (direction + up * gap).NormalizedCopy();
        }

        private void RegisterBurst(Agent shooter, ArrowShot shot)
        {
            var missile = Mission.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (missile != null)
                _burstShots[missile.Index] = (shooter, shot);
        }

        private void RegisterBodkin(Agent shooter, ArrowShot shot)
        {
            var missile = Mission.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (missile != null)
                _bodkinShots[missile.Index] = shot.FiredAt;
        }

        private void SyncTraits()
        {
            var agent = Agent.Main;
            if (!IsPlayerWaywatcher(agent)) return;

            var hero = Hero.MainHero;
            var traits = ArrowTraits.For(Quiver.Loaded, EnchantedArrow.Tier(hero), Quiver.IsLethal, hero);
            var signature = agent.Index + "|" + Bows(agent) + "|" + string.Join(",", traits);
            if (signature == _appliedTraits) return;

            if (Safe("arrow traits", () => ArrowTraits.Apply(agent, traits)))
                _appliedTraits = signature;
            else
                _traitsDirty = true;
        }

        private void ShareLethalShot()
        {
            var lethal = Quiver.IsLethal;
            if (lethal && !_playerWasLethal && _band.Count > 0 && Companions.SharesLethalShot(Hero.MainHero))
            {
                var until = Mission.CurrentTime + CompanionLethalDuration;
                foreach (var agent in _band)
                    if (agent.IsActive()) _bandLethal[agent] = (Quiver.LethalArrows, until);
            }
            _playerWasLethal = lethal;
        }

        private void ExpireBandLethal()
        {
            if (_bandLethal.Count == 0) return;
            var now = Mission.CurrentTime;
            foreach (var agent in _bandLethal.Where(x => x.Value.Arrows <= 0 || x.Value.Until <= now || !x.Key.IsActive()).Select(x => x.Key).ToList())
                _bandLethal.Remove(agent);
        }

        private bool IsBandLethal(Agent agent) =>
            _bandLethal.TryGetValue(agent, out var lethal) && lethal.Arrows > 0 && lethal.Until > Mission.CurrentTime;

        private void SyncBandTraits()
        {
            if (_band.Count == 0) return;
            var hero = Hero.MainHero;
            var tier = EnchantedArrow.Tier(hero);
            foreach (var agent in _band)
            {
                if (!agent.IsActive()) continue;
                var traits = ArrowTraits.For(Quiver.Loaded, tier, IsBandLethal(agent), hero);
                var signature = Bows(agent) + "|" + string.Join(",", traits);
                if (_bandTraits.TryGetValue(agent, out var applied) && applied == signature) continue;
                if (Safe("companion arrow traits", () => ArrowTraits.Apply(agent, traits)))
                    _bandTraits[agent] = signature;
            }
        }

        private bool IsBandMember(Agent agent) =>
            agent != null && Campaign.Current != null && Hero.MainHero != null
            && Hero.MainHero.HasCareer(TORCareers.Waywatcher) && !IsArena
            && Companions.IsWaywatcherCompanion(agent) && Companions.SharesArrows(Hero.MainHero);

        private static string Bows(Agent agent)
        {
            var ids = new List<string>();
            for (var slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
                if (ArrowTraits.IsBow(agent.Equipment[slot])) ids.Add(agent.Equipment[slot].Item.StringId);
            return string.Join(",", ids);
        }

        private void ForgetOldShots()
        {
            var now = Mission.CurrentTime;
            if (_burstShots.Count > 0)
                foreach (var index in _burstShots.Where(x => now - x.Value.Shot.FiredAt > MissileMemory).Select(x => x.Key).ToList())
                    _burstShots.Remove(index);
            if (_trueflightShots.Count > 0)
                foreach (var index in _trueflightShots.Where(x => now - x.Value > MissileMemory).Select(x => x.Key).ToList())
                    _trueflightShots.Remove(index);
            if (_bodkinShots.Count > 0)
                foreach (var index in _bodkinShots.Where(x => now - x.Value > MissileMemory).Select(x => x.Key).ToList())
                    _bodkinShots.Remove(index);
            if (_shardShots.Count > 0)
                foreach (var index in _shardShots.Where(x => now - x.Value > MissileMemory).Select(x => x.Key).ToList())
                    _shardShots.Remove(index);
        }

        private bool IsPlayerWaywatcher(Agent agent) =>
            agent != null && Campaign.Current != null && Hero.MainHero != null
            && agent.GetHero() == Hero.MainHero && Hero.MainHero.HasCareer(TORCareers.Waywatcher)
            && !IsArena && agent.GetComponent<AbilityComponent>() != null;

        private bool IsArena => _isArena ??= Mission.IsArenaMission();

        private static bool Safe(string what, Func<bool> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                Log.Error(what + " failed: " + e.Message);
                return false;
            }
        }
    }
}
