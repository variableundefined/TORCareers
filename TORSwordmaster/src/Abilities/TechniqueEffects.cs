using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Trance;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Abilities
{
    internal static class TechniqueEffects
    {
        internal const string PhoenixStrike = "sm_phoenix_strike";
        internal const string PhoenixLine = "sm_phoenix_line";
        internal const string PhoenixBleed = "sm_phoenix_bleed";
        internal const string PhoenixDismount = "sm_phoenix_dismount";
        internal const string FallingWaterBurst = "sm_falling_water";
        internal const string SunAura = "sm_sun_aura";
        internal const string SunHoeth = "sm_sun_hoeth";
        internal const string LoecKnockdown = "sm_loec_knockdown";

        private const float DamagePerTwoHanded = 0.002f;

        private const float LineLength = 6f;
        private const float LineHalfWidth = 0.75f;

        internal static void OnPlayerCast(string techniqueId, Agent caster)
        {
            switch (techniqueId)
            {
                case Technique.Loec:
                    OnLoecCast(caster);
                    if (G.Has(G.Keystone(G.Ritual))) Effects.Cleanse(caster);
                    break;
                case Technique.Sun:
                    OnSunCast(caster);
                    break;
                case Technique.FallingWater:
                    OnFallingWaterCast(caster, PlayerFallingWater());
                    break;
            }
        }

        internal static void OnLoecCast(Agent caster)
        {
            Trigger(LoecKnockdown, caster.Position, caster);
        }

        internal static void OnSunCast(Agent caster)
        {
            Trigger(SunAura, caster.Position, caster);
            if (G.Has(G.Keystone(G.SwordOfHoeth)))
                Trigger(SunHoeth, caster.Position, caster);
        }

        internal static void OnFallingWaterCast(Agent caster, string burstId = FallingWaterBurst)
        {
            Trigger(burstId, caster.Position, caster);
        }

        private static string PlayerFallingWater()
        {
            if (!G.Has(G.Passive(G.Bladelord, 4))) return FallingWaterBurst;

            var effects = Reflection.TriggeredEffectTemplates();
            if (!effects.TryGetValue(FallingWaterBurst, out var burst)) return FallingWaterBurst;

            var scaledId = FallingWaterBurst + "_bladelord";
            var scaled = (TriggeredEffectTemplate)burst.Clone(scaledId);
            scaled.DamageAmount = (int)(burst.DamageAmount * (1f + DamagePerTwoHanded * (Hero.MainHero?.GetSkillValue(DefaultSkills.TwoHanded) ?? 0)));
            effects[scaledId] = scaled;
            return scaledId;
        }

        internal static void PhoenixHit(Agent caster, Agent victim, bool player)
        {
            var hit = new MBList<Agent>();
            hit.Add(victim);
            Trigger(PhoenixStrike, victim.Position, caster, hit);

            var behind = AgentsBehind(caster, victim);
            if (behind.Count > 0) Trigger(PhoenixLine, victim.Position, caster, behind);

            if (!player) return;

            var all = new MBList<Agent>();
            all.Add(victim);
            all.AddRange(behind);

            if (G.Has(G.Keystone(G.Heirloom)))
                Trigger(PhoenixBleed, victim.Position, caster, all);

            if (G.Has(G.Passive(G.Heirloom, 2)))
            {
                var riders = new MBList<Agent>();
                riders.AddRange(all.Where(a => a.HasMount));
                if (riders.Count > 0) Trigger(PhoenixDismount, victim.Position, caster, riders);
            }
        }

        private static MBList<Agent> AgentsBehind(Agent caster, Agent victim)
        {
            var origin = victim.Position.AsVec2;
            var direction = origin - caster.Position.AsVec2;
            var result = new MBList<Agent>();
            if (direction.LengthSquared < 0.0001f) return result;
            direction.Normalize();

            var nearby = Mission.Current.GetNearbyEnemyAgents(origin + direction * (LineLength / 2f), LineLength / 2f + LineHalfWidth, caster.Team, new MBList<Agent>());
            foreach (var agent in nearby)
            {
                if (agent == victim || !agent.IsHuman || !agent.IsActive()) continue;
                var offset = agent.Position.AsVec2 - origin;
                var along = Vec2.DotProduct(offset, direction);
                if (along <= 0f || along > LineLength) continue;
                if (Math.Abs(offset.x * direction.y - offset.y * direction.x) > LineHalfWidth) continue;
                result.Add(agent);
            }
            return result;
        }

        private static void Trigger(string id, Vec3 position, Agent caster, MBList<Agent> targets = null)
        {
            if (!Reflection.TriggeredEffectTemplates().TryGetValue(id, out var template)) return;
            new TriggeredEffect(template).Trigger(position, Vec3.Up, caster, null, targets);
        }
    }
}
