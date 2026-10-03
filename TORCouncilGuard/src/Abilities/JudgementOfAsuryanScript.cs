using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem.Scripts;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Items;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Abilities
{
    public class JudgementOfAsuryanScript : CareerAbilityScript
    {
        internal const string PersonalFlame = "cg_flame_personal";
        internal const string SharedFlame = "cg_flame_shared";
        internal const string CleaveEffect = "cg_judgement_cleave";
        internal const string ExplosionTrait = "cg_flame_explosion";
        internal const string SwingSpeedEffect = "cg_judgement_swing_speed";
        internal const string BurnEffect = "cg_asuryan_burn";
        internal const float BurnDuration = 10f;

        private const string AbilityName = "Judgement of Asuryan";
        private const float BaseFireBonus = 0.30f;
        private const float BaseSwingSpeed = 0.10f;
        private const float BonusPerLevel = 0.001f;
        private const float BonusPerLeadership = 0.0005f;
        private const float DurationPerCharm = 0.05f;
        private const float ShareRadius = 5f;
        private const float TempleRadiusMultiplier = 2f;
        private const string ParticlePrefab = "psys_flaming_weapon";

        private static readonly System.Reflection.FieldInfo CurrentEffects =
            AccessTools.Field(typeof(StatusEffectComponent), "_currentEffects");

        protected override void OnInit()
        {
            base.OnInit();

            var caster = CasterAgent;
            var hero = Hero.MainHero;
            if (caster == null || hero == null || Mission.Current == null) return;

            var duration = Ability.Template.Duration;
            if (hero.HasCareerChoice("GuardOfTheSenateKeystone"))
                duration += DurationPerCharm * hero.GetSkillValue(DefaultSkills.Charm);

            var share = hero.HasCareerChoice("ProtectorOfTheSilverTowerKeystone");
            var leadershipBonus = share ? BonusPerLeadership * hero.GetSkillValue(DefaultSkills.Leadership) : 0f;

            var fire = FireBonus(hero) + leadershipBonus;
            var swingSpeed = BaseSwingSpeed + leadershipBonus;

            Ignite(caster, PersonalTraits(hero, fire), duration);
            Hasten(caster, swingSpeed, duration);

            if (hero.HasCareerChoice("GuardianOfTorLithanelKeystone"))
                caster.ApplyStatusEffect(CleaveEffect, caster, duration, false, true);

            if (!share) return;

            var radius = ShareRadius;
            if (hero.HasCareerChoice("TempleOfAsuryanKeystone")) radius *= TempleRadiusMultiplier;

            var sharedTraits = new List<ItemTrait> { FlameTrait(SharedFlame, fire) };
            var allies = Mission.Current.GetNearbyAllyAgents(caster.Position.AsVec2, radius, caster.Team, new MBList<Agent>());

            foreach (var ally in (List<Agent>)(object)allies)
            {
                if (ally == null || ally == caster || !ally.IsActive()) continue;
                if (!CitybornFilter.IsBuffTarget(ally)) continue;

                Ignite(ally, sharedTraits, duration);
                Hasten(ally, swingSpeed, duration);
            }
        }

        private static int BestMeleeSkill(Hero hero)
        {
            return Math.Max(hero.GetSkillValue(DefaultSkills.OneHanded),
                   Math.Max(hero.GetSkillValue(DefaultSkills.TwoHanded),
                            hero.GetSkillValue(DefaultSkills.Polearm)));
        }

        private static float FireBonus(Hero hero)
        {
            var bonus = BaseFireBonus + BonusPerLevel * BestMeleeSkill(hero);
            if (hero.HasCareerChoice("ForgeOfRainbowFallsKeystone"))
                bonus += BonusPerLevel * hero.GetSkillValue(TORSkills.Faith);
            return bonus;
        }

        private static List<ItemTrait> PersonalTraits(Hero hero, float fire)
        {
            var traits = new List<ItemTrait>
            {
                FlameTrait(PersonalFlame, fire),
            };

            if (hero.HasCareerChoice("TempleOfAsuryanKeystone"))
            {
                var explosion = ItemTrait.All.FirstOrDefault(x => x.ItemTraitStringId == ExplosionTrait);
                if (explosion != null) traits.Add(explosion);
            }

            return traits;
        }

        private static ItemTrait NewTrait(string id)
        {
            var trait = (ItemTrait)Activator.CreateInstance(typeof(ItemTrait), nonPublic: true);
            trait.ItemTraitStringId = id;
            trait.ItemTraitName = AbilityName;
            return trait;
        }

        private static ItemTrait FlameTrait(string id, float fire)
        {
            var trait = NewTrait(id);
            trait.AdditionalDamageTuple = new DamageProportionTuple(DamageType.Fire, fire);
            trait.ImbuedStatusEffectId = "none";
            trait.ImbuedEffectChance = 0f;
            trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = ParticlePrefab };
            return trait;
        }

        private static void Ignite(Agent agent, List<ItemTrait> traits, float duration)
        {
            var component = agent.GetComponent<ItemTraitAgentComponent>();
            if (component == null) return;

            foreach (var trait in traits)
                component.AddTraitToWieldedWeapon(trait, duration);
        }

        private static void Hasten(Agent agent, float swingSpeed, float duration)
        {
            agent.ApplyStatusEffect(SwingSpeedEffect, agent, duration, false, true);

            var component = agent.GetComponent<StatusEffectComponent>();
            if (component == null || !(CurrentEffects?.GetValue(component) is IDictionary effects)) return;

            foreach (var key in effects.Keys)
            {
                var template = (key as StatusEffect)?.Template;
                if (template != null && template.StringID.StartsWith(SwingSpeedEffect, StringComparison.Ordinal))
                    template.BaseEffectValue = swingSpeed;
            }
        }
    }
}
