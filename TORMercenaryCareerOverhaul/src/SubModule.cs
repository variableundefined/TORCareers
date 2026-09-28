using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
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
                Apply(harmony, nameof(MercenaryLetThemHaveIt), MercenaryLetThemHaveIt.Apply);
                Apply(harmony, nameof(LetThemHaveItScaling), LetThemHaveItScaling.Apply);
                Apply(harmony, nameof(MercenaryCards), MercenaryCards.Apply);
                Apply(harmony, nameof(TavernHiring), TavernHiring.Apply);
                Apply(harmony, nameof(MercenaryOrigins), MercenaryOrigins.Apply);
                Apply(harmony, nameof(MercenaryUnlockText), MercenaryUnlockText.Apply);
                Apply(harmony, nameof(ContactStageText), ContactStageText.Apply);
                Apply(harmony, nameof(SurvivalistHunt), SurvivalistHunt.Apply);
                Apply(harmony, nameof(MercenaryRecruitButton), MercenaryRecruitButton.Apply);
                Apply(harmony, nameof(DuelistLeadership), DuelistLeadership.Apply);
                Apply(harmony, nameof(MercenaryUnitProperties), MercenaryUnitProperties.Apply);
                Apply(harmony, nameof(OpeningRally), OpeningRally.Apply);
            }
            catch (Exception ex)
            {
                Log.Write("SubModule: " + ex);
            }
        }

        private static void Apply(Harmony harmony, string name, Action<Harmony> apply)
        {
            try
            {
                apply(harmony);
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
                }
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
