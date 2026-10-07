using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TORWaywatcherOverhaul.Arrows;
using TORWaywatcherOverhaul.Bootstrap;
using TORWaywatcherOverhaul.Career;

namespace TORWaywatcherOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new Harmony("TORWaywatcherOverhaul");
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Any()))
                Feature(type.Name, () => harmony.CreateClassProcessor(type).Patch());

            Feature("Enchanted Arrow templates", TemplateInjector.Inject);
            Feature("Lethal Shot tooltip", WaywatcherCareerSetup.UpdateLethalShotTooltip);
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            if (!(game.GameType is Campaign)) return;

            Feature("Lethal Shot script", WaywatcherCareerSetup.ReplaceLethalShot);
            Feature("Waywatcher career texts", WaywatcherTexts.Apply);
            Feature("Waywatcher companion passives", WaywatcherCareerSetup.ClearReplacedPassives);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Campaign.Current != null)
                mission.AddMissionBehavior(new EnchantedArrowsMissionLogic());
        }

        private static void Feature(string name, Action apply)
        {
            try
            {
                apply();
            }
            catch (Exception e)
            {
                Log.Error("FAILED: " + name + " not applied: " + e.Message);
            }
        }
    }
}
