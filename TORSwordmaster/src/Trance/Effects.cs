using System;
using System.Collections;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.Extensions;

namespace TORSwordmaster.Trance
{
    internal static class Effects
    {
        private static readonly System.Reflection.FieldInfo CurrentEffects =
            AccessTools.Field(typeof(StatusEffectComponent), "_currentEffects");

        internal static void Apply(Agent agent, string id, float duration, Agent applier)
        {
            agent?.ApplyStatusEffect(id, applier ?? agent, duration, false, true);
        }

        internal static void Apply(Agent agent, string id, float value, float duration, Agent applier)
        {
            if (agent == null) return;
            Apply(agent, id, duration, applier);
            SetValue(agent, id, value);
        }

        internal static void SetValue(Agent agent, string id, float value)
        {
            var component = agent.GetComponent<StatusEffectComponent>();
            if (component == null || !(CurrentEffects?.GetValue(component) is IDictionary effects)) return;

            foreach (var key in effects.Keys)
            {
                var template = (key as StatusEffect)?.Template;
                if (template != null && template.StringID.StartsWith(id, StringComparison.Ordinal))
                    template.BaseEffectValue = value;
            }
        }

        internal static void Remove(Agent agent, string id)
        {
            agent?.GetComponent<StatusEffectComponent>()?.RemoveStatusEffect(id, StatusEffectComponent.EffectFlag.All);
        }

        internal static void Cleanse(Agent agent)
        {
            agent?.GetComponent<StatusEffectComponent>()?.RemoveStatusEffect("", StatusEffectComponent.EffectFlag.Enemy);
        }
    }
}
