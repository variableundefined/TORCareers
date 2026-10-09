using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Economy;
using TORSwordmaster.Career;
using TORSwordmaster.Trance;

namespace TORSwordmaster
{
    public class SubModule : MBSubModuleBase
    {
        private Game _careerRegisteredFor;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new HarmonyLib.Harmony("TORSwordmaster");
            Feature("Swordmaster origin", () => SwordmasterCharacterCreation.Apply(harmony));
            Feature("Way of the Sword weapon check", () => Abilities.WayOfTheSwordRestriction.Apply(harmony));
            Feature("Swordmaster technique book", () => TechniqueBook.Apply(harmony));
            Feature("Swordmaster prayer route", () => PriestRoute.Apply(harmony));
            Feature("Swordmaster hireling activities", () => harmony.CreateClassProcessor(typeof(SwordmasterHireling)).Patch());
            Feature("Swordmaster templates", TemplateInjector.Inject);
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            if (game.GameType is Campaign)
                Feature("Swordmaster career", () => RegisterCareer(game));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (!(game.GameType is Campaign)) return;

            if (gameStarterObject is CampaignGameStarter starter)
                starter.AddBehavior(new SwordmasterCampaignBehavior());
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Campaign.Current != null)
                mission.AddMissionBehavior(new SwordmasterBattleLogic());
        }

        private void RegisterCareer(Game game)
        {
            if (_careerRegisteredFor == game) return;

            var choiceSets = TORCareerChoices.Instance
                ?? throw new InvalidOperationException("TORCareerChoices.Instance is null; TOR_Core has not built its careers yet.");

            var career = SwordmasterCareer.Create();
            SwordmasterChoiceGroups.Register(career);
            var choices = new SwordmasterCareerChoices(career);

            Reflection.AllCareers().Add(career);
            Reflection.AllCareerChoices(choiceSets).Add(choices);

            Reflection.RegisterCareerButton(SwordmasterCareer.Id, MartialTraining.Instance);

            _careerRegisteredFor = game;
            Log.Write("Career registered for this campaign.");
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
