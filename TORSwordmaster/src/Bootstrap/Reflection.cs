using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

namespace TORSwordmaster.Bootstrap
{
    internal static class Reflection
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static FieldInfo Required(Type owner, string name)
        {
            return AccessTools.Field(owner, name)
                ?? throw new MissingFieldException(owner.FullName + "." + name + " not found. TOR_Core layout changed.");
        }

        private static T Field<T>(object instance, Type owner, string name) where T : class
        {
            return Required(owner, name).GetValue(instance) as T
                ?? throw new InvalidOperationException(owner.FullName + "." + name + " was null or of an unexpected type.");
        }

        internal static readonly FieldInfo CurrentCharge = Required(typeof(CareerAbility), "_currentCharge");
        internal static readonly FieldInfo CooldownEndTime = Required(typeof(Ability), "_cooldownEndTime");

        internal static List<CareerObject> AllCareers()
        {
            var instance = TORCareers.Instance
                ?? throw new InvalidOperationException("TORCareers.Instance is null.");
            if (Field<IList>(instance, typeof(TORCareers), "_allCareers") is List<CareerObject> typed) return typed;
            throw new InvalidCastException("TORCareers._allCareers is not a List<CareerObject>.");
        }

        internal static List<TORCareerChoicesBase> AllCareerChoices(TORCareerChoices instance) =>
            Field<List<TORCareerChoicesBase>>(instance, typeof(TORCareerChoices), "_allCareerChoices");

        internal static Dictionary<string, AbilityTemplate> AbilityTemplates() =>
            Field<Dictionary<string, AbilityTemplate>>(null, typeof(AbilityFactory), "_templates");

        internal static Dictionary<string, StatusEffectTemplate> StatusEffectTemplates() =>
            Field<Dictionary<string, StatusEffectTemplate>>(null, typeof(StatusEffectManager), "_idToStatusEffect");

        internal static Dictionary<string, TriggeredEffectTemplate> TriggeredEffectTemplates() =>
            Field<Dictionary<string, TriggeredEffectTemplate>>(null, typeof(TriggeredEffectManager), "_dictionary");

        internal static void RegisterCareerButton(string careerId, CareerButtonBehaviorBase button)
        {
            var buttons = Field<Dictionary<string, CareerButtonBehaviorBase>>(CareerButtons.Instance, typeof(CareerButtons), "_careerButtons");
            buttons[careerId] = button;
        }

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
