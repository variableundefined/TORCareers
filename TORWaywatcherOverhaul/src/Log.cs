using System;
using System.IO;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace TORWaywatcherOverhaul
{
    internal static class Log
    {
        private const string Prefix = "[Waywatcher] ";

        private static string _path;
        private static bool _started;

        internal static void Info(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(Prefix + message));
            Write("INFO  " + message);
        }

        internal static void Warn(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(Prefix + message, Colors.Yellow));
            Write("WARN  " + message);
        }

        internal static void Error(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(Prefix + message, Colors.Red));
            Write("ERROR " + message);
        }

        internal static void Write(string line)
        {
            try
            {
                _path ??= ModuleHelper.GetModuleFullPath("TORWaywatcherOverhaul") + "waywatcher_overhaul.log";
                if (!_started)
                {
                    _started = true;
                    File.WriteAllText(_path, "TOR Waywatcher Overhaul - " + DateTime.Now + Environment.NewLine);
                }
                File.AppendAllText(_path, DateTime.Now.ToString("HH:mm:ss") + " " + line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
