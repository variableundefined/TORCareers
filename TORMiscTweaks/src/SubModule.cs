using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TORMiscTweaks
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORMiscTweaks";
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony(HarmonyId);

            var patchClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Any());

            foreach (var type in patchClasses)
                Feature(type.Name, () => _harmony.CreateClassProcessor(type).Patch());
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (!(game.GameType is Campaign)) return;
            if (!(gameStarterObject is CampaignGameStarter starter)) return;
        }

        private static void Feature(string name, Action patch)
        {
            try
            {
                patch();
            }
            catch (Exception e)
            {
                Log.Error("FAILED: " + name + " not patched, stock behaviour stands: " + e.Message);
            }
        }
    }
}
