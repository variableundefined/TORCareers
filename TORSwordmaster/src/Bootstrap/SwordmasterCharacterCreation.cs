using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TOR_Core.CampaignMechanics.CharacterCreation;
using TOR_Core.Extensions;
using TORSwordmaster.Career;

namespace TORSwordmaster.Bootstrap
{
    internal static class SwordmasterCharacterCreation
    {
        internal const string OptionId = "option_3_eo_swordmaster";
        private const string FileName = "swordmaster_cc_options.xml";
        private static readonly Vec2 TorLithanel = new Vec2(1216.198f, 1345.101f);

        private static AccessTools.FieldRef<TORCharacterCreationContentHandler, List<CharacterCreationOption>> _options;
        private static List<CharacterCreationOption> _loaded;

        internal static void Apply(Harmony harmony)
        {
            _options = AccessTools.FieldRefAccess<TORCharacterCreationContentHandler, List<CharacterCreationOption>>("_options");
            if (_options == null)
                throw new MissingFieldException("TORCharacterCreationContentHandler._options not found.");

            _loaded = LoadOptions();
            if (_loaded == null || _loaded.Count == 0)
                throw new InvalidOperationException(FileName + " produced no options.");

            var target = AccessTools.Method(typeof(TORCharacterCreationContentHandler), "InitializeContent",
                new[] { typeof(CharacterCreationManager) })
                ?? throw new MissingMethodException("TORCharacterCreationContentHandler.InitializeContent not found.");

            harmony.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(SwordmasterCharacterCreation), nameof(AddOption))));
        }

        private static List<CharacterCreationOption> LoadOptions()
        {
            var path = Path.Combine(ModuleHelper.GetModuleFullPath("TORSwordmaster"), "ModuleData", FileName);
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
                if (!options.Any(x => x?.Id == option.Id))
                    options.Add(option);
        }

        internal static void OnCharacterCreationIsOver()
        {
            try
            {
                var handler = TORCharacterCreationContentHandler.Instance;
                if (handler == null || handler.GetSelectedProfessionId() != OptionId) return;

                var career = SwordmasterCareer.Career;
                if (career == null)
                {
                    Log.Error("Swordmaster career was never registered; this start keeps the default career.");
                    return;
                }

                Hero.MainHero.AddCareer(career);
                MobileParty.MainParty.Position = new CampaignVec2(TorLithanel, true);
                if (GameStateManager.Current.ActiveState is MapState mapState)
                {
                    mapState.Handler.ResetCamera(true, true);
                    mapState.Handler.TeleportCameraToMainParty();
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not assign the Swordmaster career: " + e.Message);
            }
        }
    }
}
