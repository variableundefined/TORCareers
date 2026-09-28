using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TORImperialEngineer.Abilities;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.CampaignMechanics;

namespace TORImperialEngineer
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORImperialEngineer";
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony(HarmonyId);

            var patchClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Any());

            foreach (var type in patchClasses)
                Feature(type.Name, () => _harmony.CreateClassProcessor(type).Patch());

            Feature("Imperial Engineer origin", () => ImperialEngineerCharacterCreation.Apply(_harmony));

            Feature("Imperial Engineer templates", TemplateInjector.Inject);
        }

        protected override void OnGameStart(TaleWorlds.Core.Game game, TaleWorlds.Core.IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is TaleWorlds.CampaignSystem.Campaign && gameStarterObject is TaleWorlds.CampaignSystem.CampaignGameStarter starter)
                starter.AddBehavior(new ImperialEngineerCampaignBehavior());
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (TaleWorlds.CampaignSystem.Campaign.Current != null)
                mission.AddMissionBehavior(new MunitionMissionLogic());
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
