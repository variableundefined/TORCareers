using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.ModuleManager;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.Items;

namespace TORImperialEngineer.Bootstrap
{
    internal static class TemplateInjector
    {
        private static List<T> Load<T>(string fileName, string rootElement)
        {
            var path = Path.Combine(
                ModuleHelper.GetModuleFullPath("TORImperialEngineer"), "ModuleData", fileName);
            if (!File.Exists(path))
            {
                Log.Warn(fileName + " not found at " + path);
                return null;
            }

            var serializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(rootElement));
            using (var stream = File.OpenRead(path))
            {
                return serializer.Deserialize(stream) as List<T>;
            }
        }

        internal static void Inject()
        {
            InjectAbilities();
            InjectStatusEffects();
            InjectItemTraits();
            InjectTriggeredEffects();
        }

        private static void InjectAbilities()
        {
            var parsed = Load<AbilityTemplate>("imperial_engineer_abilities.xml", "AbilityTemplates");
            if (parsed == null) return;

            var templates = Reflection.AbilityTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectStatusEffects()
        {
            var parsed = Load<StatusEffectTemplate>("imperial_engineer_statuseffects.xml", "StatusEffects");
            if (parsed == null) return;

            var templates = Reflection.StatusEffectTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectItemTraits()
        {
            var parsed = Load<ItemTrait>("imperial_engineer_itemtraits.xml", "ItemTraits");
            if (parsed == null) return;

            var traits = Reflection.ItemTraits();
            foreach (var trait in parsed)
                if (!traits.Any(x => x.ItemTraitStringId == trait.ItemTraitStringId))
                    traits.Add(trait);
        }

        private static void InjectTriggeredEffects()
        {
            var parsed = Load<TriggeredEffectTemplate>("imperial_engineer_triggeredeffects.xml", "TriggeredEffectTemplates");
            if (parsed == null) return;

            var templates = Reflection.TriggeredEffectTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        [HarmonyPatch(typeof(ItemTraitManager), nameof(ItemTraitManager.LoadItemTraits))]
        internal static class ItemTraitsBackstop
        {
            [HarmonyPostfix]
            private static void Postfix() => InjectItemTraits();
        }

        [HarmonyPatch(typeof(AbilityFactory), nameof(AbilityFactory.LoadTemplates))]
        internal static class AbilitiesBackstop
        {
            [HarmonyPostfix]
            private static void Postfix() => InjectAbilities();
        }

        [HarmonyPatch(typeof(StatusEffectManager), nameof(StatusEffectManager.LoadStatusEffects))]
        internal static class StatusEffectsBackstop
        {
            [HarmonyPostfix]
            private static void Postfix() => InjectStatusEffects();
        }

        [HarmonyPatch(typeof(TriggeredEffectManager), nameof(TriggeredEffectManager.LoadTemplates))]
        internal static class TriggeredEffectsBackstop
        {
            [HarmonyPostfix]
            private static void Postfix() => InjectTriggeredEffects();
        }
    }
}
