using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.ModuleManager;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.Items;

namespace TORWaywatcherOverhaul.Bootstrap
{
    internal static class TemplateInjector
    {
        private static List<T> Load<T>(string fileName, string rootElement)
        {
            var path = Path.Combine(ModuleHelper.GetModuleFullPath("TORWaywatcherOverhaul"), "ModuleData", fileName);
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
            var parsed = Load<AbilityTemplate>("waywatcher_abilities.xml", "AbilityTemplates");
            if (parsed == null) return;

            var templates = Reflection.AbilityTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectStatusEffects()
        {
            var parsed = Load<StatusEffectTemplate>("waywatcher_statuseffects.xml", "StatusEffects");
            if (parsed == null) return;

            var templates = Reflection.StatusEffectTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectItemTraits()
        {
            var parsed = Load<ItemTrait>("waywatcher_itemtraits.xml", "ItemTraits");
            if (parsed == null) return;

            var traits = ItemTraitManager.Instance.GetItemTraits();
            foreach (var trait in parsed)
                if (!traits.Any(x => x.ItemTraitStringId == trait.ItemTraitStringId))
                    traits.Add(trait);
        }

        private static void InjectTriggeredEffects()
        {
            var parsed = Load<TriggeredEffectTemplate>("waywatcher_triggeredeffects.xml", "TriggeredEffectTemplates");
            if (parsed == null) return;

            var templates = Reflection.TriggeredEffectTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }
    }
}
