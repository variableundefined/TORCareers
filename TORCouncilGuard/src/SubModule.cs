using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment;
using TORCouncilGuard.Bootstrap;
using TORCouncilGuard.CampaignMechanics;
using TORCouncilGuard.Career;

namespace TORCouncilGuard
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORCouncilGuard";
        private Harmony _harmony;
        private Game _careerRegisteredFor;

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

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            if (game.GameType is Campaign)
                Feature("Council Guard career", () => RegisterCareer(game));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (!(game.GameType is Campaign)) return;

            Feature("Council Guard favor", () => CouncilGuardFavor.Apply(_harmony));
            if (gameStarterObject is CampaignGameStarter starter)
                starter.AddBehavior(new CouncilGuardCampaignBehavior());
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Campaign.Current != null)
            {
                mission.AddMissionBehavior(new CouncilGuardMainAgentLogic());
                mission.AddMissionBehavior(new JudgementOfAsuryanBurnLogic());
                mission.AddMissionBehavior(new CouncilGuardKillLogic());
            }
        }

        private void RegisterCareer(Game game)
        {
            if (_careerRegisteredFor == game) return;

            var choiceSets = TORCareerChoices.Instance
                ?? throw new InvalidOperationException("TORCareerChoices.Instance is null; TOR_Core has not built its careers yet.");

            var career = CouncilGuardCareer.Create();
            CouncilGuardChoiceGroups.Register(career);
            var choices = new CouncilGuardCareerChoices(career);

            Reflection.AllCareers().Add(career);
            Reflection.AllCareerChoices(choiceSets).Add(choices);

            _careerRegisteredFor = game;
            Log.Write("Career registered for this campaign. Careers: " + Reflection.AllCareers().Count + ", choice sets: " + Reflection.AllCareerChoices(choiceSets).Count + ".");
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
