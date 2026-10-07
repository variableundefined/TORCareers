using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TORWaywatcherOverhaul.Bootstrap;

namespace TORWaywatcherOverhaul.Arrows
{
    internal sealed class ArrowShot
    {
        internal ArrowType Arrow;
        internal int Tier;
        internal bool Lethal;
        internal bool Essence;
        internal float FiredAt;
    }

    internal static class ArrowImpact
    {
        private const float EssenceDamageFactor = 1.25f;
        private const int MoonfireDamage = 25;
        private const float TargetOnlyEffectRadius = 0.5f;
        private const float MinVisualScale = 0.2f;
        private const float MaxVisualScale = 2.5f;

        internal static void Burst(Agent shooter, ArrowShot shot, Vec3 position, Agent victim)
        {
            if (shooter == null || !shooter.IsActive() || !EnchantedArrow.Bursts(shot.Arrow)) return;

            var id = TemplateId(shot.Arrow);
            if (!Reflection.TriggeredEffectTemplates().TryGetValue(id, out var template))
            {
                Log.Warn("Missing triggered effect " + id + ".");
                return;
            }

            var radius = EnchantedArrow.Radius(shot.Arrow, shot.Tier)
                + (shot.Lethal ? EnchantedArrow.LethalRadiusBonus(shot.Tier) : 0f);
            var victimHit = victim != null && victim.IsActive() && victim != shooter;
            var ground = new Vec3(position.x, position.y, victimHit ? victim.Position.z : position.z);

            if (shot.Arrow == ArrowType.MoonfireShot)
            {
                var damage = (int)(MoonfireDamage * (shot.Essence ? EssenceDamageFactor : 1f));
                var everyone = Nearby(position, radius, shooter, null);
                Fire(template, id, AreaStatusEffects(shot), template.ImbuedStatusEffectDuration, damage, radius, everyone, ground, shooter, true);
                SpawnBurstVisual(template.BurstParticleEffectPrefab, position, radius / NominalPrefabRadius(shot.Arrow), template.SoundEffectLength);
                return;
            }

            if (radius <= 0f)
            {
                if (victimHit)
                    Fire(template, id, TargetStatusEffects(shot, false), TargetDuration(shot), 0, TargetOnlyEffectRadius,
                        new MBList<Agent> { victim }, ground, shooter, false);
                return;
            }

            var everyoneHit = Nearby(position, radius, shooter, null);
            if (victimHit && !everyoneHit.Contains(victim)) everyoneHit.Add(victim);
            if (everyoneHit.Count > 0)
                Fire(template, id, TargetStatusEffects(shot, shot.Essence), TargetDuration(shot), 0, radius, everyoneHit, ground, shooter, true);
            SpawnBurstVisual(template.BurstParticleEffectPrefab, position, radius / NominalPrefabRadius(shot.Arrow), template.SoundEffectLength);
        }

        private static MBList<Agent> Nearby(Vec3 position, float radius, Agent shooter, Agent excluded)
        {
            var agents = Mission.Current.GetNearbyAgents(position.AsVec2, radius, new MBList<Agent>());
            agents.Remove(shooter);
            if (excluded != null) agents.Remove(excluded);
            return agents;
        }

        private static void Fire(TriggeredEffectTemplate template, string id, List<string> statusEffects, float duration,
            int damage, float radius, MBList<Agent> targets, Vec3 position, Agent shooter, bool withSound)
        {
            var effect = (TriggeredEffectTemplate)template.Clone(id + "*wwo");
            effect.Radius = radius;
            effect.DamageAmount = damage;
            effect.ImbuedStatusEffects = statusEffects;
            effect.ImbuedStatusEffectDuration = duration;
            effect.BurstParticleEffectPrefab = "none";
            if (!withSound) effect.SoundEffectId = "none";
            new TriggeredEffect(effect, true).Trigger(position, Vec3.Up, shooter, null, targets);
        }

        private static void SpawnBurstVisual(string prefab, Vec3 position, float scale, float lifetime)
        {
            prefab = prefab?.Trim();
            if (string.IsNullOrEmpty(prefab) || prefab.Equals("none", StringComparison.OrdinalIgnoreCase)) return;
            var scene = Mission.Current?.Scene;
            if (scene == null) return;

            scale = MBMath.ClampFloat(scale, MinVisualScale, MaxVisualScale);
            var entity = GameEntity.CreateEmpty(scene);
            var local = MatrixFrame.Identity;
            var particles = ParticleSystem.CreateParticleSystemAttachedToEntity(prefab, entity, ref local);
            if (particles == null)
            {
                Log.Warn("Missing burst particle " + prefab + ".");
                entity.Remove(0);
                return;
            }

            var up = Vec3.Up;
            var frame = new MatrixFrame(Mat3.CreateMat3WithForward(up), position);
            frame.rotation.ApplyScaleLocal(scale);
            entity.SetGlobalFrame(frame);
            entity.FadeOut(lifetime, true);
        }

        private static float NominalPrefabRadius(ArrowType arrow)
        {
            switch (arrow)
            {
                case ArrowType.StarfireShaft: return 2f;
                case ArrowType.MoonfireShot: return 2f;
                default: return 4f;
            }
        }

        private static string TemplateId(ArrowType arrow)
        {
            switch (arrow)
            {
                case ArrowType.StarfireShaft: return "wwo_starfire_burst";
                case ArrowType.MoonfireShot: return "wwo_moonfire_burst";
                default: return "wwo_hagbane_burst";
            }
        }

        private static List<string> TargetStatusEffects(ArrowShot shot, bool essence)
        {
            var effects = new List<string>();
            var bonus = essence ? 1 : 0;
            switch (shot.Arrow)
            {
                case ArrowType.StarfireShaft:
                    effects.Add("wwo_burn" + (4 + bonus));
                    if (shot.Tier >= 3) effects.Add("starfire_fire_vulnerability");
                    break;
                case ArrowType.HagbaneTips:
                    effects.Add("wwo_poison" + ((shot.Tier >= 2 ? 5 : 4) + bonus));
                    effects.Add(shot.Tier >= 3 ? "wwo_slow50" : shot.Tier == 2 ? "wwo_slow40" : "wwo_slow30");
                    break;
            }
            return effects;
        }

        private static float TargetDuration(ArrowShot shot)
        {
            if (shot.Arrow == ArrowType.StarfireShaft) return shot.Tier >= 3 ? 10f : shot.Tier == 2 ? 8f : 6f;
            return shot.Tier >= 3 ? 8f : shot.Tier == 2 ? 7f : 6f;
        }

        private static List<string> AreaStatusEffects(ArrowShot shot)
        {
            var effects = new List<string>();
            if (shot.Arrow == ArrowType.MoonfireShot && shot.Tier >= 3) effects.Add("wwo_magic_vulnerability");
            return effects;
        }
    }
}
