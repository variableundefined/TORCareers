using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.ModuleManager;
using TOR_Core.Extensions.ExtendedInfoSystem;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryUnitProperties
    {
        private const string TorFile = "tor_extendedunitproperties.xml";
        private const string OwnFile = "ModuleData/merc_unitproperties.xml";

        internal static void Apply(Harmony harmony)
        {
            var load = AccessTools.Method(typeof(ExtendedInfoManager), nameof(ExtendedInfoManager.TryLoadCharacters))
                       ?? throw new MissingMethodException("ExtendedInfoManager.TryLoadCharacters not found.");

            harmony.Patch(load, postfix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryUnitProperties), nameof(Merge))));
        }

        private static void Merge(ref string filepath, ref Dictionary<string, CharacterExtendedInfo> infos)
        {
            try
            {
                if (infos == null || filepath == null || !filepath.EndsWith(TorFile, StringComparison.OrdinalIgnoreCase)) return;

                var path = ModuleHelper.GetModuleFullPath("TORMercenaryCareerOverhaul") + OwnFile;
                if (!File.Exists(path))
                {
                    Log.Write("MercenaryUnitProperties: missing " + path);
                    return;
                }

                List<CharacterExtendedInfo> own;
                using (var stream = File.OpenRead(path))
                {
                    own = new XmlSerializer(typeof(List<CharacterExtendedInfo>)).Deserialize(stream) as List<CharacterExtendedInfo>;
                }

                if (own == null) return;

                foreach (var info in own)
                {
                    if (info?.CharacterStringId == null) continue;
                    infos[info.CharacterStringId] = info;
                }
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryUnitProperties: " + ex.Message);
            }
        }
    }
}
