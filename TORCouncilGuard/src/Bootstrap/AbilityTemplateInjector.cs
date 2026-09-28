using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.ModuleManager;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.Items;

namespace TORCouncilGuard.Bootstrap
{
    internal static class TemplateInjector
    {
        private static List<T> Load<T>(string fileName, string rootElement)
        {
            var path = Path.Combine(
                ModuleHelper.GetModuleFullPath("TORCouncilGuard"), "ModuleData", fileName);
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

        // TOR_Core loads its own templates in its OnSubModuleLoad, which runs before ours.
        internal static void Inject()
        {
            InjectAbilities();
            InjectStatusEffects();
            InjectItemTraits();
        }

        private static void InjectAbilities()
        {
            var parsed = Load<AbilityTemplate>("council_guard_abilities.xml", "AbilityTemplates");
            if (parsed == null) return;

            var templates = Reflection.AbilityTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectStatusEffects()
        {
            var parsed = Load<StatusEffectTemplate>("council_guard_statuseffects.xml", "StatusEffects");
            if (parsed == null) return;

            var templates = Reflection.StatusEffectTemplates();
            foreach (var template in parsed)
                if (!templates.ContainsKey(template.StringID))
                    templates.Add(template.StringID, template);
        }

        private static void InjectItemTraits()
        {
            var parsed = Load<ItemTrait>("council_guard_itemtraits.xml", "ItemTraits");
            if (parsed == null) return;

            var traits = Reflection.ItemTraits();
            foreach (var trait in parsed)
                if (!traits.Any(x => x.ItemTraitStringId == trait.ItemTraitStringId))
                    traits.Add(trait);
        }
    }
}
