using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TOR_Core.Items;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;

namespace TORCouncilGuard.Bootstrap
{
    internal static class Reflection
    {
        private static T Field<T>(object instance, Type owner, string name) where T : class
        {
            var f = AccessTools.Field(owner, name);
            if (f == null)
                throw new MissingFieldException(owner.FullName + "." + name + " not found. TOR_Core layout changed.");
            return f.GetValue(instance) as T;
        }

        internal static List<CareerObject> AllCareers()
        {
            var instance = TORCareers.Instance
                ?? throw new InvalidOperationException("TORCareers.Instance is null.");
            var list = Field<object>(instance, typeof(TORCareers), "_allCareers") as IList;
            if (list is List<CareerObject> typed) return typed;
            throw new InvalidCastException("TORCareers._allCareers is not a List<CareerObject>.");
        }

        internal static List<TORCareerChoicesBase> AllCareerChoices(TORCareerChoices instance)
        {
            return Field<List<TORCareerChoicesBase>>(instance, typeof(TORCareerChoices), "_allCareerChoices")
                ?? throw new InvalidOperationException("TORCareerChoices._allCareerChoices was null.");
        }

        internal static Dictionary<string, AbilityTemplate> AbilityTemplates()
        {
            return Field<Dictionary<string, AbilityTemplate>>(null, typeof(AbilityFactory), "_templates")
                ?? throw new InvalidOperationException("AbilityFactory._templates was null.");
        }

        internal static Dictionary<string, StatusEffectTemplate> StatusEffectTemplates()
        {
            return Field<Dictionary<string, StatusEffectTemplate>>(null, typeof(StatusEffectManager), "_idToStatusEffect")
                ?? throw new InvalidOperationException("StatusEffectManager._idToStatusEffect was null.");
        }

        internal static List<ItemTrait> ItemTraits()
        {
            return (List<ItemTrait>)AccessTools.Field(typeof(ItemTraitManager), "_itemTraits")
                .GetValue(ItemTraitManager.Instance);
        }
    }
}
