using System;
using System.IO;
using TaleWorlds.ModuleManager;

namespace TORMercenaryCareerOverhaul
{
    internal static class Log
    {
        private static readonly string Path =
            ModuleHelper.GetModuleFullPath("TORMercenaryCareerOverhaul") + "tweaks.log";

        private static bool _started;

        internal static void Write(string line)
        {
            try
            {
                if (!_started)
                {
                    _started = true;
                    File.WriteAllText(Path, DateTime.Now + Environment.NewLine);
                }
                File.AppendAllText(Path, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
