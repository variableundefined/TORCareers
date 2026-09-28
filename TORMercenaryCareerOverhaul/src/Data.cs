using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.AbilitySystem;
using TOR_Core.AbilitySystem.Crosshairs;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul
{
    internal static class Data
    {
        internal const string AbilityId = "LetThemHaveIt";
        internal const string EffectId = "apply_let_them_have_it";
        internal const string CommanderKeystone = "CommanderKeystone";

        internal const int CoolDown = 60;
        internal const float Duration = 30f;
        internal const float Radius = 6f;
        internal const float CastRange = 30f;
        internal const float RadiusPerLeadership = 0.05f;
        internal const float DurationPerLeadership = 0.05f;
        internal const int KillCooldown = 3;
        internal const float Magnitude = 0.15f;

        private const string UnstoppableId = "let_them_have_it_unstoppable";
        private const string UnstoppableParticle = "berzerk_effect";

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
            }
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
            var radius = Radius;
            if (hero.HasCareerChoice(CommanderKeystone)) radius *= 2f;
            return radius + LeadershipRadius(hero.CharacterObject);
        }

        internal static bool IsLetThemHaveIt(AbilityTemplate template)
        {
            return template?.StringID != null && template.StringID.StartsWith(AbilityId);
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
