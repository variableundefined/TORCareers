using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.ModuleManager;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.Items;

namespace TORSwordmaster.Bootstrap
{
    internal static class TemplateInjector
    {
        private static List<T> Load<T>(string fileName, string rootElement)
        {
            var path = Path.Combine(ModuleHelper.GetModuleFullPath("TORSwordmaster"), "ModuleData", fileName);
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
            Add(Load<AbilityTemplate>("swordmaster_abilities.xml", "AbilityTemplates"),
                Reflection.AbilityTemplates(), t => t.StringID);
            Add(Load<StatusEffectTemplate>("swordmaster_statuseffects.xml", "StatusEffects"),
                Reflection.StatusEffectTemplates(), t => t.StringID);
            Add(Load<TriggeredEffectTemplate>("swordmaster_triggeredeffects.xml", "TriggeredEffectTemplates"),
                Reflection.TriggeredEffectTemplates(), t => t.StringID);

            var traits = Load<ItemTrait>("swordmaster_itemtraits.xml", "ItemTraits");
            if (traits != null)
                ItemTrait.All.AddRange(traits.Where(t => ItemTraitManager.Instance.GetItemTraitByStringId(t.ItemTraitStringId) == null).ToList());

            var itemProperties = Path.Combine(ModuleHelper.GetModuleFullPath("TORSwordmaster"), "ModuleData", "swordmaster_extendeditemproperties.xml");
            ExtendedItemObjectManager.LoadXML(itemProperties);
        }

        private static void Add<T>(List<T> parsed, Dictionary<string, T> target, System.Func<T, string> id)
        {
            if (parsed == null) return;
            foreach (var template in parsed)
                if (!target.ContainsKey(id(template)))
                    target.Add(id(template), template);
        }
    }
}
