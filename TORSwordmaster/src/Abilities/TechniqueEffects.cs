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
        internal const string PhoenixBleed = "sm_phoenix_bleed";
        internal const string PhoenixDismount = "sm_phoenix_dismount";
        internal const string FallingWaterBurst = "sm_falling_water";
        internal const string SunAura = "sm_sun_aura";
        internal const string SunHoeth = "sm_sun_hoeth";
        internal const string LoecKnockdown = "sm_loec_knockdown";
        internal const string MasterPhysical = "sm_master_physical";
        internal const string MasterMagical = "sm_master_magical";

        private const float DamagePerMeleeSkill = 0.002f;

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
                case Technique.Master:
                    MastersStrike.Arm(caster);
                    break;
            }
        }

        internal static void OnLoecCast(Agent caster)
        {
            Trigger(LoecKnockdown, Technique.Loec, caster.Position, caster);
        }

        internal static void OnSunCast(Agent caster)
        {
            Trigger(SunAura, Technique.Sun, caster.Position, caster);
            if (G.Has(G.Keystone(G.SwordOfHoeth)))
                Trigger(SunHoeth, Technique.Sun, caster.Position, caster);
        }

        internal static void OnFallingWaterCast(Agent caster, string burstId = FallingWaterBurst)
        {
            Trigger(burstId, Technique.FallingWater, caster.Position, caster);
        }

        private static string PlayerFallingWater()
        {
            if (!G.Has(G.Passive(G.Bladelord, 4))) return FallingWaterBurst;

            var effects = Reflection.TriggeredEffectTemplates();
            if (!effects.TryGetValue(FallingWaterBurst, out var burst)) return FallingWaterBurst;

            var scaledId = FallingWaterBurst + "_bladelord";
            var scaled = (TriggeredEffectTemplate)burst.Clone(scaledId);
            scaled.DamageAmount = (int)(burst.DamageAmount * (1f + DamagePerMeleeSkill * Focus.HighestMeleeSkill(Hero.MainHero)));
            effects[scaledId] = scaled;
            return scaledId;
        }

        // Called for every enemy the blade passes through; the damage itself is the missile's own.
        internal static void PhoenixHit(Agent caster, Agent victim)
        {
            var hit = new MBList<Agent>();
            hit.Add(victim);

            if (G.Has(G.Keystone(G.Heirloom)))
                Trigger(PhoenixBleed, Technique.Phoenix, victim.Position, caster, hit);

            if (G.Has(G.Passive(G.Heirloom, 2)) && victim.HasMount)
                Trigger(PhoenixDismount, Technique.Phoenix, victim.Position, caster, hit);
        }

        internal static void MastersStrikeHit(Agent caster, Agent victim, int armorLoss, int magicalDamage)
        {
            var targets = new MBList<Agent>();
            targets.Add(victim);
            if (armorLoss > 0) Trigger(WithDamage(MasterPhysical, armorLoss), Technique.Master, victim.Position, caster, targets);
            if (magicalDamage > 0) Trigger(WithDamage(MasterMagical, magicalDamage), Technique.Master, victim.Position, caster, targets);
        }

        private static string WithDamage(string id, int damage)
        {
            var effects = Reflection.TriggeredEffectTemplates();
            if (!effects.TryGetValue(id, out var effect)) return id;

            var scaledId = id + "_hit";
            var scaled = (TriggeredEffectTemplate)effect.Clone(scaledId);
            scaled.DamageAmount = damage;
            effects[scaledId] = scaled;
            return scaledId;
        }

        private static void Trigger(string id, string technique, Vec3 position, Agent caster, MBList<Agent> targets = null)
        {
            if (!Reflection.TriggeredEffectTemplates().TryGetValue(id, out var template)) return;
            new TriggeredEffect(template).Trigger(position, Vec3.Up, caster, AbilityFactory.GetTemplate(technique), targets);
        }
    }
}
