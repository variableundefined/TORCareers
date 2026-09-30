using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TriggeredEffectInstance = TOR_Core.BattleMechanics.TriggeredEffect.TriggeredEffect;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.CampaignMechanics;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal class MunitionMissionLogic : MissionLogic
    {
        private const float LastShotGrace = 2.5f;
        private const float MissileMemory = 10f;
        private const float GroupMemory = 3f;
        private const string PersonalReloadEffect = "ie_personal_reload";
        private const string Explosion = "ie_explosive_shot";
        private const float SingleShotExplosionFactor = 0.6f;
        private const float RepeaterExplosionFactor = 0.4f;
        private const string ScaledSuffix = "*ie_scaled";
        private const float CannonsRadiusBonus = 1f;
        private const float SweepInterval = 0.5f;

        private class LoadedShot
        {
            internal Agent Shooter;
            internal float FiredAt;
            internal float? Explosion;
        }

        private Agent _awaitingPermanentEffects;
        private float _sweep;

        private readonly Misfires _misfires;

        public MunitionMissionLogic()
        {
            _misfires = new Misfires(RegisterLoadedMissiles);
        }

        private readonly Dictionary<Agent, float> _spent = new Dictionary<Agent, float>();
        private readonly Dictionary<int, LoadedShot> _loadedMissiles = new Dictionary<int, LoadedShot>();
        private readonly Dictionary<Agent, LoadedShot> _loadedGroups = new Dictionary<Agent, LoadedShot>();

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position,
            Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            if (!IsLoader(shooterAgent)) return;

            var weapon = shooterAgent.Equipment[weaponIndex];
            var twin = TwinShot.Fires(shooterAgent, weapon);

            if (_spent.ContainsKey(shooterAgent) && Munition.Remaining(shooterAgent) <= 0)
            {
                Munition.Strip(shooterAgent);
                _spent.Remove(shooterAgent);
            }

            if (Munition.Remaining(shooterAgent) <= 0 || Firearms.IsFiringGrenade(shooterAgent))
            {
                if (twin) TwinShot.Fire(shooterAgent, weapon, position, orientation, velocity);
                return;
            }

            var chance = Munition.MisfireChance(Hero.MainHero);
            var suppressed = MBRandom.RandomFloat < chance
                && _misfires.Trigger(shooterAgent, weaponIndex, position, orientation, velocity, twin);

            if (!suppressed)
                FireLoadedShot(shooterAgent, weapon, position, orientation, velocity, twin);

            ExperimentalMunitionScript.Pulse(shooterAgent);

            if (Munition.Consume(shooterAgent) > 0) return;

            _spent[shooterAgent] = LastShotGrace;
        }

        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            var shot = FindShot(attacker, collisionData.AffectorWeaponSlotOrMissileIndex);
            if (shot?.Explosion == null) return;

            var damage = shot.Explosion.Value;
            shot.Explosion = null;
            Explode(shot.Shooter, collisionData.CollisionGlobalPosition, damage);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (agent == null || !ImperialEngineerCareer.IsPlayer) return;

            BattleStartCharge.Apply(agent);
            ExtraResistances.Apply(agent);
            if (agent.IsMainAgent)
                _awaitingPermanentEffects = agent;
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            EngineeringExperience.OnKill(affectorAgent, affectedAgent, blow);
        }

        public override void OnMissionTick(float dt)
        {
            ApplyPermanentEffects();
            _misfires.Tick(dt);
            TickSpent(dt);
            SweepEmptyGuns(dt);
            ForgetOldShots();
        }

        private void FireLoadedShot(Agent shooter, MissionWeapon weapon, Vec3 position, Mat3 orientation, Vec3 velocity, bool twin)
        {
            var now = Mission.CurrentTime;
            var scatterAmmo = Firearms.FiresScatter(weapon);

            if (Ammo.Selected == AmmoType.Scatter && !scatterAmmo)
            {
                ScatterShot.Fire(shooter, weapon, position, orientation, velocity, twin: twin);
                _loadedGroups[shooter] = new LoadedShot { Shooter = shooter, FiredAt = now };
                return;
            }

            float? explosion = null;
            if (Ammo.Selected == AmmoType.Explosive && !scatterAmmo)
                explosion = ExplosionDamage(weapon);

            if (twin)
                TwinShot.Fire(shooter, weapon, position, orientation, velocity);

            if (scatterAmmo || twin)
            {
                _loadedGroups[shooter] = new LoadedShot { Shooter = shooter, FiredAt = now, Explosion = explosion };
                return;
            }

            var missile = Mission.MissilesList.LastOrDefault(m => m.ShooterAgent == shooter);
            if (missile != null)
                _loadedMissiles[missile.Index] = new LoadedShot { Shooter = shooter, FiredAt = now, Explosion = explosion };
        }

        private static float ExplosionDamage(MissionWeapon weapon)
        {
            var factor = weapon.MaxAmmo > 1 ? RepeaterExplosionFactor : SingleShotExplosionFactor;
            return MissileDamage.Shot(weapon) * factor;
        }

        private void RegisterLoadedMissiles(Agent shooter, MissionWeapon weapon, List<Mission.Missile> missiles, AmmoType loaded, bool grouped)
        {
            if (loaded != AmmoType.Explosive || Firearms.FiresScatter(weapon)) return;
            var explosion = ExplosionDamage(weapon);
            if (grouped)
            {
                _loadedGroups[shooter] = new LoadedShot { Shooter = shooter, FiredAt = Mission.CurrentTime, Explosion = explosion };
                return;
            }
            foreach (var missile in missiles)
                if (missile != null)
                    _loadedMissiles[missile.Index] = new LoadedShot { Shooter = shooter, FiredAt = Mission.CurrentTime, Explosion = explosion };
        }

        private LoadedShot FindShot(Agent shooter, int missileIndex)
        {
            if (_loadedMissiles.TryGetValue(missileIndex, out var shot)) return shot;
            if (shooter != null && _loadedGroups.TryGetValue(shooter, out var group)) return group;
            return null;
        }

        private static void Explode(Agent shooter, Vec3 position, float damage)
        {
            if (shooter == null || !shooter.IsActive()) return;

            try
            {
                if (!Reflection.TriggeredEffectTemplates().TryGetValue(Explosion, out var template)) return;

                var hero = Hero.MainHero;
                var radius = template.Radius;
                if (hero.HasCareerChoice(G.Cannons + "Keystone")) radius += CannonsRadiusBonus;
                if (hero.HasCareerChoice(G.Cavalcade + "Keystone")) damage *= Munition.CavalcadeExplosionFactor;

                var visuals = (TriggeredEffectTemplate)template.Clone(Explosion + ScaledSuffix);
                visuals.Radius = radius;
                visuals.DamageAmount = 0;
                new TriggeredEffectInstance(visuals, true).Trigger(position, Vec3.Up, shooter);

                DamageWithFalloff(shooter, position, radius, damage, template.DamageVariance, template.DamageType);
            }
            catch (System.Exception e)
            {
                Log.Error("Explosive shot failed: " + e.Message);
            }
        }

        private static void DamageWithFalloff(Agent shooter, Vec3 position, float radius, float damage, float variance, DamageType type)
        {
            var model = TaleWorlds.CampaignSystem.Campaign.Current?.Models.GetAbilityModel();
            var targets = Mission.Current.GetNearbyAgents(position.AsVec2, radius, new MBList<Agent>());

            foreach (var agent in targets.ToList())
            {
                if (agent == null || !agent.IsHuman || !agent.IsActive() || agent.Health < 1f) continue;

                var falloff = (radius - agent.Position.Distance(position)) / radius;
                if (falloff <= 0f) continue;

                var raw = (int)(damage * falloff * (1f + MBRandom.RandomFloatRanged(-variance, variance)));
                if (raw <= 0) continue;

                var final = model?.CalculateAbilityDamage(shooter, agent, raw, type, null) ?? raw;
                if (final > 0)
                    agent.ApplyDamage(final, position, shooter, doBlow: true, hasShockWave: false);
            }
        }

        private void ApplyPermanentEffects()
        {
            var agent = _awaitingPermanentEffects;
            if (agent == null) return;
            if (!agent.IsActive())
            {
                _awaitingPermanentEffects = null;
                return;
            }
            if (agent.GetComponent<StatusEffectComponent>() == null || agent.GetComponent<ItemTraitAgentComponent>() == null) return;

            if (Hero.MainHero.HasCareerChoice(G.Cannons + "Passive1"))
                CareerHelper.AddDefaultPermanentMissionEffect(agent, PersonalReloadEffect);
            _awaitingPermanentEffects = null;
        }

        private void TickSpent(float dt)
        {
            if (_spent.Count == 0) return;

            foreach (var agent in _spent.Keys.ToList())
            {
                var left = _spent[agent] - dt;
                if (left > 0f)
                {
                    _spent[agent] = left;
                    continue;
                }

                if (agent.IsActive() && Munition.Remaining(agent) <= 0)
                    Munition.Strip(agent);
                _spent.Remove(agent);
            }
        }

        private void SweepEmptyGuns(float dt)
        {
            _sweep -= dt;
            if (_sweep > 0f) return;
            _sweep = SweepInterval;

            foreach (var agent in Mission.Agents)
            {
                if (!IsLoader(agent) || !agent.IsActive() || _spent.ContainsKey(agent)) continue;
                if (Munition.Remaining(agent) <= 0)
                    Munition.StripLeftovers(agent);
            }
        }

        private void ForgetOldShots()
        {
            var now = Mission.CurrentTime;

            if (_loadedMissiles.Count > 0)
                foreach (var index in _loadedMissiles.Where(x => now - x.Value.FiredAt > MissileMemory).Select(x => x.Key).ToList())
                    _loadedMissiles.Remove(index);

            if (_loadedGroups.Count > 0)
                foreach (var agent in _loadedGroups.Where(x => now - x.Value.FiredAt > GroupMemory).Select(x => x.Key).ToList())
                    _loadedGroups.Remove(agent);
        }

        private static bool IsLoader(Agent agent)
        {
            if (agent == null || !ImperialEngineerCareer.IsPlayer) return false;
            if (agent.IsMainAgent) return true;
            return agent.IsHero && agent.BelongsToMainParty();
        }
    }
}
