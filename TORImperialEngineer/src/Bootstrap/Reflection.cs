using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.Items;

namespace TORImperialEngineer.Bootstrap
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

        internal static Dictionary<string, TriggeredEffectTemplate> TriggeredEffectTemplates()
        {
            return Field<Dictionary<string, TriggeredEffectTemplate>>(null, typeof(TriggeredEffectManager), "_dictionary")
                ?? throw new InvalidOperationException("TriggeredEffectManager._dictionary was null.");
        }

        private static readonly FieldInfo WeaponTraitsField =
            AccessTools.Field(typeof(ItemTraitAgentComponent), "_dynamicTraits");

        internal static bool RemoveWeaponTraits(ItemTraitAgentComponent component, ICollection<string> traitIds)
        {
            if (!(WeaponTraitsField?.GetValue(component) is List<Tuple<MissionWeapon, ItemTrait, float>> entries))
                throw new MissingFieldException("ItemTraitAgentComponent._dynamicTraits not found. TOR_Core layout changed.");

            if (entries.RemoveAll(x => x.Item2 != null && traitIds.Contains(x.Item2.ItemTraitStringId)) <= 0)
                return false;
            component.OnWieldedItemChanged();
            return true;
        }
    }
}
