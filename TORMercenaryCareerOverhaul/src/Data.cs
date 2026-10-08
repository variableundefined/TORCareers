using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.AbilitySystem;
using TOR_Core.AbilitySystem.Crosshairs;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Utilities;

namespace TORMercenaryCareerOverhaul
{
    internal static class Data
    {
        internal const string AbilityId = "LetThemHaveIt";
        internal const string EffectId = "apply_let_them_have_it";
        internal const string CommanderKeystone = "CommanderKeystone";
        internal const string PaymasterKeystone = "PaymasterKeystone";
        internal const string MagicResistanceId = "let_them_have_it_magic_res";

        internal const int CoolDown = 60;
        internal const float Duration = 30f;
        internal const float Radius = 6f;
        internal const float CastRange = 30f;
        internal const float RadiusPerLeadership = 0.05f;
        internal const float DurationPerLeadership = 0.05f;
        internal const int KillCooldown = 3;
        internal const float Magnitude = 0.15f;
        internal const float NonCasterDuration = 0.2f;
        internal const float MagicResistance = 0.3f;
        internal const float PaymasterFactor = 2f;
        internal const float CommanderBonus = 0.10f;
        internal const float CasterRadius = 6f;

        internal static readonly string[] Bonuses =
        {
            "let_them_have_it_melee_dmg", "let_them_have_it_range_dmg", "let_them_have_it_melee_res",
            "let_them_have_it_range_res", "let_them_have_it_melee_ats", "let_them_have_it_melee_rls",
            MagicResistanceId,
        };

        private static readonly Dictionary<string, float> BaseValues = new Dictionary<string, float>();

        private const string UnstoppableId = "let_them_have_it_unstoppable";
        private const string UnstoppableParticle = "psys_energy_sphere";
        private const string ResistanceTemplateId = "let_them_have_it_melee_res";

        internal static void Apply()
        {
            var ability = AbilityFactory.GetTemplate(AbilityId);
            if (ability == null)
            {
                Log.Write("Ability not found: " + AbilityId);
                return;
            }

            ability.TriggerType = TriggerType.TickOnce;
            ability.TickInterval = -1f;
            ability.AbilityTargetType = AbilityTargetType.GroundAtPosition;
            ability.CrosshairType = CrosshairType.TargetedAOE;
            ability.MaxDistance = CastRange;
            ability.TargetCapturingRadius = Radius;
            ability.CoolDown = CoolDown;
            ability.Duration = 1f;

            var effect = Effect(EffectId);
            if (effect == null)
            {
                Log.Write("Effect not found: " + EffectId);
            }
            else
            {
                effect.TargetType = TargetType.Friendly;
                effect.Radius = Radius;
                effect.ImbuedStatusEffectDuration = Duration;
            }

            var unstoppable = Status(UnstoppableId);
            if (unstoppable != null)
            {
                unstoppable.ParticleId = UnstoppableParticle;
                unstoppable.ParticleIntensity = TORParticleSystem.ParticleIntensity.Low;
            }

            RegisterMagicResistance();

            foreach (var id in Bonuses)
            {
                var template = Status(id);
                if (template != null) BaseValues[id] = template.BaseEffectValue;
            }
        }

        private static void RegisterMagicResistance()
        {
            if (Status(MagicResistanceId) != null) return;

            var source = Status(ResistanceTemplateId);
            var registry = AccessTools.StaticFieldRefAccess<Dictionary<string, StatusEffectTemplate>>(
                typeof(StatusEffectManager), "_idToStatusEffect");
            if (source == null || registry == null)
            {
                Log.Write("Could not register " + MagicResistanceId);
                return;
            }

            var template = (StatusEffectTemplate)source.Clone(MagicResistanceId);
            template.DamageType = DamageType.All;
            template.AttackTypeMask = AttackTypeMask.Spell;
            template.BaseEffectValue = MagicResistance;
            registry[MagicResistanceId] = template;
        }

        internal static bool IsCaster(Hero hero)
        {
            return hero != null && hero.IsSpellCaster();
        }

        internal static float CommanderRadius(Hero hero)
        {
            return hero.HasCareerChoice(CommanderKeystone) && IsCaster(hero) ? CasterRadius : 0f;
        }

        internal static float BonusValue(string id, Hero hero)
        {
            float value;
            if (!BaseValues.TryGetValue(id, out value)) value = Magnitude;

            if (hero == null) return value;
            if (hero.HasCareerChoice(PaymasterKeystone) && id != MagicResistanceId) value *= PaymasterFactor;
            if (hero.HasCareerChoice(CommanderKeystone) && !IsCaster(hero)) value += CommanderBonus;
            return value;
        }

        internal static float LeadershipRadius(CharacterObject character)
        {
            return RadiusPerLeadership * character.GetSkillValue(DefaultSkills.Leadership);
        }

        internal static float LeadershipDuration(CharacterObject character)
        {
            return DurationPerLeadership * character.GetSkillValue(DefaultSkills.Leadership);
        }

        internal static float RingRadius(Hero hero)
        {
            return Radius + CommanderRadius(hero) + LeadershipRadius(hero.CharacterObject);
        }

        private static TriggeredEffectTemplate Effect(string id)
        {
            return TriggeredEffectManager.GetTemplatesWithIds(new List<string> { id })?.FirstOrDefault();
        }

        private static StatusEffectTemplate Status(string id)
        {
            return StatusEffectManager.GetStatusEffectTemplatesWithIds(new List<string> { id })?.FirstOrDefault();
        }
    }
}
