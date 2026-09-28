using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal static class Munition
    {
        internal const string Trait = "ie_munition";
        internal const string PenetratingTrait = "ie_munition_penetrating";
        internal const string HeavyTrait = "ie_ammo_heavy";
        internal const string CavalcadeTrait = "ie_munition_cavalcade";
        internal const string CavalcadeHeavyTrait = "ie_munition_cavalcade_heavy";
        internal const float CavalcadeExplosionFactor = 1.25f;
        internal const string ShotEffect = "ie_munition_shot";
        internal const string ReloadEffect = "ie_munition_reload";
        internal const string ShotAttribute = "ExperimentalMunition";
        internal const string MisfireEffect = "ie_misfire";
        internal const string SteadyHandsChoice = G.School + "Keystone";

        private static readonly string[] AllTraits = { Trait, PenetratingTrait, HeavyTrait, CavalcadeTrait, CavalcadeHeavyTrait };

        private const int BaseShots = 3;
        private const int BonusShots = 2;
        private const int SkillPerShot = 50;
        private const float HeavyMisfireChance = 0.03f;
        private const float ScatterMisfireChance = 0.05f;
        private const float ExplosiveMisfireChance = 0.08f;
        private const float CavalcadeMisfireFactor = 1.5f;

        private static float _duration = 60f;

        internal static int Shots(Hero hero)
        {
            var skillPerShot = hero.HasCareerChoice(G.Cannons + "Keystone") ? SkillPerShot / 2 : SkillPerShot;

            var shots = BaseShots + hero.GetSkillValue(TORSkills.GunPowder) / skillPerShot;
            if (hero.HasCareerChoice(G.Logistics + "Keystone"))
                shots += hero.GetSkillValue(DefaultSkills.Engineering) / skillPerShot;
            if (hero.HasCareerChoice(G.Customized + "Keystone"))
                shots += BonusShots;
            return shots;
        }

        internal static int MisfireSeverity(Hero hero)
        {
            var severity = 0;
            if (hero.HasCareerChoice(G.Cavalcade + "Keystone")) severity++;
            return severity;
        }

        internal static float MisfireChance(Hero hero)
        {
            var chance = Ammo.Selected == AmmoType.Explosive ? ExplosiveMisfireChance
                : Ammo.Selected == AmmoType.Scatter ? ScatterMisfireChance
                : HeavyMisfireChance;
            if (hero.HasCareerChoice(G.Cavalcade + "Keystone")) chance *= CavalcadeMisfireFactor;
            return chance;
        }

        private static List<ItemTrait> Traits(Hero hero)
        {
            var ids = new List<string> { Trait };
            if (Ammo.Selected == AmmoType.Heavy) ids.Add(HeavyTrait);
            if (hero.HasCareerChoice(G.Cavalcade + "Keystone")) ids.Add(Ammo.Selected == AmmoType.Heavy ? CavalcadeHeavyTrait : CavalcadeTrait);
            if (Ammo.Selected == AmmoType.Heavy && hero.HasCareerChoice(G.School + "Keystone")) ids.Add(PenetratingTrait);

            var traits = new List<ItemTrait>();
            foreach (var id in ids)
            {
                var trait = Find(id);
                if (trait != null) traits.Add(trait);
            }
            return traits;
        }

        internal static ItemTrait Find(string id)
        {
            var trait = ItemTrait.All.FirstOrDefault(x => x.ItemTraitStringId == id);
            if (trait == null) Log.Warn("Item trait " + id + " is missing.");
            return trait;
        }

        internal static int Remaining(Agent agent)
        {
            var component = agent?.GetComponent<StatusEffectComponent>();
            if (component == null) return 0;
            return component.GetTemporaryAttributes(retrieveDuplicates: true).Count(x => x == ShotAttribute);
        }

        internal static void Load(Agent agent, Hero hero, int shots, float duration)
        {
            _duration = duration;
            Unload(agent);

            for (var i = 0; i < shots; i++)
                agent.ApplyStatusEffect(ShotEffect, agent, duration, append: false, isMutated: false, stack: true);

            Empower(agent, hero, duration);
        }

        internal static void Reload(Agent agent, Hero hero)
        {
            Strip(agent);
            Empower(agent, hero, _duration);
        }

        internal static int Consume(Agent agent)
        {
            agent.GetComponent<StatusEffectComponent>()?.RemoveStatusEffect(ShotEffect);
            return Remaining(agent);
        }

        internal static void Strip(Agent agent)
        {
            var traits = agent.GetComponent<ItemTraitAgentComponent>();
            try
            {
                if (traits != null)
                    Reflection.RemoveWeaponTraits(traits, AllTraits);
            }
            catch (Exception e)
            {
                Log.Error("Could not remove Experimental Munition traits: " + e.Message);
            }

            agent.GetComponent<StatusEffectComponent>()?.RemoveStatusEffect(ReloadEffect);
        }

        internal static void StripLeftovers(Agent agent)
        {
            var traits = agent.GetComponent<ItemTraitAgentComponent>();
            if (traits == null) return;
            try
            {
                if (Reflection.RemoveWeaponTraits(traits, AllTraits))
                    agent.GetComponent<StatusEffectComponent>()?.RemoveStatusEffect(ReloadEffect);
            }
            catch (Exception e)
            {
                Log.Error("Could not remove Experimental Munition traits: " + e.Message);
            }
        }

        internal static void Unload(Agent agent)
        {
            var component = agent.GetComponent<StatusEffectComponent>();
            if (component != null)
            {
                var guard = Remaining(agent);
                while (guard-- > 0 && Remaining(agent) > 0)
                    component.RemoveStatusEffect(ShotEffect);
            }
            Strip(agent);
        }

        internal static void Misfire(Agent shooter)
        {
            try
            {
                var effect = Reflection.CreateTriggeredEffect(MisfireEffect);
                if (effect == null)
                {
                    Log.Warn("Triggered effect " + MisfireEffect + " is missing; misfire skipped.");
                    return;
                }
                if (Hero.MainHero.HasCareerChoice(SteadyHandsChoice))
                    SteadyMisfire(effect, shooter);
                else
                    effect.Trigger(shooter.Position, Vec3.Up, shooter);
            }
            catch (Exception e)
            {
                Log.Error("Misfire failed: " + e.Message);
            }
        }

        private static void SteadyMisfire(TOR_Core.BattleMechanics.TriggeredEffect.TriggeredEffect effect, Agent shooter)
        {
            var position = shooter.Position;
            Reflection.TriggeredEffectTemplates().TryGetValue(MisfireEffect, out var template);
            var radius = template?.Radius ?? 2.5f;

            var others = Mission.Current.GetNearbyAgents(position.AsVec2, radius, new MBList<Agent>());
            others.Remove(shooter);
            effect.Trigger(position, Vec3.Up, shooter, null, others);

            if (template == null || template.DamageAmount <= 0) return;

            var damage = template.DamageAmount;
            try
            {
                damage = TaleWorlds.CampaignSystem.Campaign.Current.Models.GetAbilityModel()
                    .CalculateAbilityDamage(shooter, shooter, damage, template.DamageType, null);
            }
            catch (Exception e)
            {
                Log.Warn("Misfire resistance calculation failed, using base damage: " + e.Message);
            }

            if (damage > 0)
                shooter.ApplyDamage(damage, position, shooter, doBlow: true, hasShockWave: false);
        }

        private static void Empower(Agent agent, Hero hero, float duration)
        {
            var component = agent.GetComponent<ItemTraitAgentComponent>();
            var gun = Firearms.FindGun(agent);
            if (component != null && !gun.IsEmpty)
                foreach (var trait in Traits(hero))
                    component.AddTraitToWeapon(gun, trait, duration);

            if (hero.HasCareerChoice(G.Customized + "Keystone"))
                agent.ApplyStatusEffect(ReloadEffect, agent, duration, append: false);
        }
    }
}
