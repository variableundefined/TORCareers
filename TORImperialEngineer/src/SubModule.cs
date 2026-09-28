using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment;
using TORImperialEngineer.Abilities;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.CampaignMechanics;
using TORImperialEngineer.Career;
using TORImperialEngineer.Combat;

namespace TORImperialEngineer
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORImperialEngineer";
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

            Feature("Imperial Engineer origin", () => ImperialEngineerCharacterCreation.Apply(_harmony));

            Feature("Imperial Engineer templates", TemplateInjector.Inject);
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            if (game.GameType is Campaign)
                Feature("Imperial Engineer career", () => RegisterCareer(game));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddModel(new ImperialEngineerAgentStatCalculateModel());
                starter.AddBehavior(new ImperialEngineerCampaignBehavior());
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Campaign.Current != null)
                mission.AddMissionBehavior(new MunitionMissionLogic());
        }

        private void RegisterCareer(Game game)
        {
            if (_careerRegisteredFor == game) return;

            var choiceSets = TORCareerChoices.Instance
                ?? throw new InvalidOperationException("TORCareerChoices.Instance is null.");

            var career = ImperialEngineerCareer.Create();
            ImperialEngineerChoiceGroups.Register(career);
            var choices = new ImperialEngineerCareerChoices(career);

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
