using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TORCouncilGuard.Bootstrap;

namespace TORCouncilGuard
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORCouncilGuard";
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony(HarmonyId);

            var patchClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Any());

            foreach (var type in patchClasses)
                Feature(type.Name, () => _harmony.CreateClassProcessor(type).Patch());

            Feature("Council Guard origin", () => CouncilGuardCharacterCreation.Apply(_harmony));

            Feature("Council Guard templates", TemplateInjector.Inject);
        }

        protected override void OnGameStart(TaleWorlds.Core.Game game, TaleWorlds.Core.IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is TaleWorlds.CampaignSystem.Campaign)
                Feature("Council Guard favor", () => CouncilGuardFavor.Apply(_harmony));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (TaleWorlds.CampaignSystem.Campaign.Current != null)
                {
                mission.AddMissionBehavior(new JudgementOfAsuryanBurnLogic());
                mission.AddMissionBehavior(new JudgementOfAsuryanKillLogic());
            }
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
