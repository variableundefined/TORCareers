using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Items;

namespace TORWaywatcherOverhaul.Bootstrap
{
    internal static class Reflection
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        private static FieldInfo Field(Type owner, string name, BindingFlags flags) =>
            owner.GetField(name, flags)
            ?? throw new MissingFieldException(owner.FullName + "." + name + " not found. TOR_Core layout changed.");

        private static T StaticField<T>(Type owner, string name) where T : class =>
            Field(owner, name, Static).GetValue(null) as T
            ?? throw new InvalidOperationException(owner.FullName + "." + name + " was null.");

        internal static Dictionary<string, AbilityTemplate> AbilityTemplates() =>
            StaticField<Dictionary<string, AbilityTemplate>>(typeof(AbilityFactory), "_templates");

        internal static Dictionary<string, StatusEffectTemplate> StatusEffectTemplates() =>
            StaticField<Dictionary<string, StatusEffectTemplate>>(typeof(StatusEffectManager), "_idToStatusEffect");

        internal static Dictionary<string, TriggeredEffectTemplate> TriggeredEffectTemplates() =>
            StaticField<Dictionary<string, TriggeredEffectTemplate>>(typeof(TriggeredEffectManager), "_dictionary");

        internal static void SetAbilityScriptType(CareerObject career, Type scriptType)
        {
            var property = typeof(CareerObject).GetProperty(nameof(CareerObject.AbilityScriptType), Instance)
                ?? throw new MissingMemberException("CareerObject.AbilityScriptType not found. TOR_Core layout changed.");
            property.SetValue(career, scriptType);
        }

        internal static void ClearPassive(CareerChoiceObject choice)
        {
            var property = typeof(CareerChoiceObject).GetProperty(nameof(CareerChoiceObject.Passive), Instance)
                ?? throw new MissingMemberException("CareerChoiceObject.Passive not found. TOR_Core layout changed.");
            property.SetValue(choice, null);
        }

        internal static List<Tuple<MissionWeapon, ItemTrait, float>> DynamicTraits(ItemTraitAgentComponent component) =>
            Field(typeof(ItemTraitAgentComponent), "_dynamicTraits", Instance).GetValue(component) as List<Tuple<MissionWeapon, ItemTrait, float>>
            ?? throw new InvalidCastException("ItemTraitAgentComponent._dynamicTraits is not the expected list.");

        internal static void WireCastEvents(AbilityComponent component, Ability ability)
        {
            var onCastStart = typeof(AbilityComponent).GetMethod("OnCastStart", Instance);
            var onCastComplete = typeof(AbilityComponent).GetMethod("OnCastComplete", Instance);
            if (onCastStart == null || onCastComplete == null)
                throw new MissingMethodException("AbilityComponent cast handlers not found. TOR_Core layout changed.");

            ability.OnCastStart += (Ability.OnCastStartHandler)Delegate.CreateDelegate(typeof(Ability.OnCastStartHandler), component, onCastStart);
            ability.OnCastComplete += (Ability.OnCastCompleteHandler)Delegate.CreateDelegate(typeof(Ability.OnCastCompleteHandler), component, onCastComplete);
        }
    }
}
