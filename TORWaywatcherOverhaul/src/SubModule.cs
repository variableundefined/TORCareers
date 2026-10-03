using System;
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
            Feature("Enchanted Arrow templates", TemplateInjector.Inject);
            Feature("Lethal Shot tooltip", WaywatcherCareerSetup.UpdateLethalShotTooltip);
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            if (!(game.GameType is Campaign)) return;

            Feature("Lethal Shot script", WaywatcherCareerSetup.ReplaceLethalShot);
            Feature("Waywatcher career texts", WaywatcherTexts.Apply);
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
