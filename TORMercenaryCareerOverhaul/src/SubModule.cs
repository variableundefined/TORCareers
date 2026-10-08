using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORMercenaryCareerOverhaul.CampaignMechanics;
using TORMercenaryCareerOverhaul.Patches;

namespace TORMercenaryCareerOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "TORMercenaryCareerOverhaul";

        private bool _applied;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                var harmony = new Harmony(HarmonyId);
                Apply(harmony, nameof(TavernHiring), TavernHiring.Apply);
                Apply(harmony, nameof(MercenaryOrigins), MercenaryOrigins.Apply);
                Apply(harmony, nameof(ContactStageText), ContactStageText.Apply);
                Apply(harmony, nameof(SurvivalistHunt), SurvivalistHunt.Apply);
                Apply(harmony, nameof(MercenaryRecruitButton), MercenaryRecruitButton.Apply);
                Apply(harmony, nameof(MercenaryUnitProperties), MercenaryUnitProperties.Apply);
                Apply(harmony, nameof(LetThemHaveItSelf), LetThemHaveItSelf.Apply);
            }
            catch (Exception ex)
            {
                Log.Write("SubModule: " + ex);
            }
        }

        private static void Apply(Harmony harmony, string name, Action<Harmony> apply)
        {
            Apply(name, () => apply(harmony));
        }

        private static void Apply(string name, Action apply)
        {
            try
            {
                apply();
            }
            catch (Exception ex)
            {
                Log.Write(name + ": " + ex.Message);
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            try
            {
                if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter starter)
                {
                    starter.AddBehavior(new MercenaryContacts());
                    starter.AddModel(new MercenaryPartyWageModel());
                }
            }
            catch (Exception ex)
            {
                Log.Write("SubModule: " + ex);
            }
        }

        // TOR_Core builds every career object in its BeginGameStart; this module loads after it.
        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);

            try
            {
                if (!(game.GameType is Campaign)) return;

                Apply(nameof(LetThemHaveItScaling), LetThemHaveItScaling.Apply);
                Apply(nameof(MercenaryCards), MercenaryCards.Apply);
                Apply(nameof(MercenaryUnlockText), MercenaryUnlockText.Apply);
                CareerHelper.RefreshCareerChoicesCache();
            }
            catch (Exception ex)
            {
                Log.Write("SubModule: " + ex);
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            try
            {
                if (Campaign.Current != null) mission.AddMissionBehavior(new MercenaryMissionLogic());
            }
            catch (Exception ex)
            {
                Log.Write("SubModule: " + ex);
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (_applied) return;
            _applied = true;

            try
            {
                Data.Apply();
            }
            catch (Exception ex)
            {
                Log.Write("Data: " + ex);
            }
        }
    }
}
