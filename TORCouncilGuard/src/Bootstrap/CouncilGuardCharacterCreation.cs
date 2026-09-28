using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TOR_Core.CampaignMechanics.CharacterCreation;
using TOR_Core.Extensions;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    internal static class CouncilGuardCharacterCreation
    {
        internal const string OptionId = "option_3_eo_council_guard";
        private const string FileName = "council_guard_cc_options.xml";

        private static readonly Vec2 TorLithanel = new Vec2(1216.198f, 1345.101f);

        private static AccessTools.FieldRef<TORCharacterCreationContentHandler, List<CharacterCreationOption>> _options;
        private static AccessTools.FieldRef<TORCharacterCreationContentHandler, string> _selected;
        private static AccessTools.FieldRef<TORCharacterCreationContentHandler, CampaignVec2?> _spawn;

        private static List<CharacterCreationOption> _loaded;

        internal static void Apply(Harmony harmony)
        {
            _options = AccessTools.FieldRefAccess<TORCharacterCreationContentHandler,
                List<CharacterCreationOption>>("_options");
            _selected = AccessTools.FieldRefAccess<TORCharacterCreationContentHandler,
                string>("_selectedProfessionId");
            _spawn = AccessTools.FieldRefAccess<TORCharacterCreationContentHandler,
                CampaignVec2?>("_storedSpawnPosition");
            if (_options == null || _selected == null || _spawn == null)
                throw new MissingFieldException("TORCharacterCreationContentHandler fields not found.");

            _loaded = LoadOptions();
            if (_loaded == null || _loaded.Count == 0)
                throw new InvalidOperationException(FileName + " produced no options.");

            harmony.Patch(
                Required(typeof(TORCharacterCreationContentHandler), "ApplyProfessionBonuses", Type.EmptyTypes),
                prefix: Method(nameof(AssignCareer)));

            harmony.Patch(
                Required(typeof(TORCharacterCreationContentHandler), "InitializeContent",
                    new[] { typeof(CharacterCreationManager) }),
                prefix: Method(nameof(AddOption)));
        }

        private static List<CharacterCreationOption> LoadOptions()
        {
            var path = Path.Combine(
                ModuleHelper.GetModuleFullPath("TORCouncilGuard"), "ModuleData", FileName);
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

        private static void AssignCareer(TORCharacterCreationContentHandler __instance)
        {
            try
            {
                if (_selected(__instance) != OptionId) return;

                var career = CouncilGuardCareer.Career;
                if (career == null)
                {
                    Log.Error("Council Guard career was never registered; this start will default to Mercenary.");
                    return;
                }

                Hero.MainHero.AddCareer(career);
                _spawn(__instance) = new CampaignVec2(TorLithanel, true);
            }
            catch (Exception e)
            {
                Log.Error("Could not assign the Council Guard career: " + e.Message);
            }
        }

        private static HarmonyMethod Method(string name) =>
            new HarmonyMethod(AccessTools.Method(typeof(CouncilGuardCharacterCreation), name));

        private static MethodInfo Required(Type type, string name, Type[] parameters)
        {
            var found = AccessTools.Method(type, name, parameters);
            if (found == null)
                throw new MissingMethodException(type.Name + "." + name + " not found.");
            return found;
        }
    }
}
