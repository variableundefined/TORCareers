using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.ModuleManager;
using TOR_Core.CampaignMechanics.CharacterCreation;

namespace TORImperialEngineer.Bootstrap
{
    internal static class ImperialEngineerCharacterCreation
    {
        internal const string OptionId = "option_3_empire_imperial_engineer";
        private const string FileName = "imperial_engineer_cc_options.xml";

        private static AccessTools.FieldRef<TORCharacterCreationContentHandler, List<CharacterCreationOption>> _options;

        private static List<CharacterCreationOption> _loaded;

        internal static void Apply(Harmony harmony)
        {
            _options = AccessTools.FieldRefAccess<TORCharacterCreationContentHandler,
                List<CharacterCreationOption>>("_options");
            if (_options == null)
                throw new MissingFieldException("TORCharacterCreationContentHandler fields not found.");

            _loaded = LoadOptions();
            if (_loaded == null || _loaded.Count == 0)
                throw new InvalidOperationException(FileName + " produced no options.");

            harmony.Patch(
                Required(typeof(TORCharacterCreationContentHandler), "InitializeContent",
                    new[] { typeof(CharacterCreationManager) }),
                prefix: Method(nameof(AddOption)));
        }

        private static List<CharacterCreationOption> LoadOptions()
        {
            var path = Path.Combine(
                ModuleHelper.GetModuleFullPath("TORImperialEngineer"), "ModuleData", FileName);
            if (!File.Exists(path))
                throw new FileNotFoundException(FileName + " not found.", path);

            var serializer = new XmlSerializer(typeof(List<CharacterCreationOption>));
            using (var stream = File.OpenRead(path))
            {
                return serializer.Deserialize(stream) as List<CharacterCreationOption>;
            }
        }

        private static void AddOption(TORCharacterCreationContentHandler __instance)
        {
            var options = _options(__instance);
            if (options == null) return;

            foreach (var option in _loaded)
            {
                if (options.Any(x => x?.Id == option.Id)) continue;
                options.Add(option);
            }
        }

        private static HarmonyMethod Method(string name) =>
            new HarmonyMethod(AccessTools.Method(typeof(ImperialEngineerCharacterCreation), name));

        private static MethodInfo Required(Type type, string name, Type[] parameters)
        {
            var found = AccessTools.Method(type, name, parameters);
            if (found == null)
                throw new MissingMethodException(type.Name + "." + name + " not found.");
            return found;
        }
    }
}
