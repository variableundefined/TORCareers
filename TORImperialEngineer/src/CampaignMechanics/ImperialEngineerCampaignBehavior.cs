using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TOR_Core.CampaignMechanics.CharacterCreation;
using TOR_Core.Extensions;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    internal class ImperialEngineerCampaignBehavior : CampaignBehaviorBase
    {
        private const int DailyTroopXp = 25;

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        // Swap career on start
        private static void OnCharacterCreationIsOver()
        {
            try
            {
                var handler = TORCharacterCreationContentHandler.Instance;
                if (handler == null || handler.GetSelectedProfessionId() != ImperialEngineerCharacterCreation.OptionId) return;

                var career = ImperialEngineerCareer.Career;
                if (career == null)
                {
                    Log.Error("Imperial Engineer career was never registered; this start will default to Mercenary.");
                    return;
                }

                Hero.MainHero.AddCareer(career);

                MobileParty.MainParty.Position = Hero.MainHero.Culture.StartingPoint;
                if (GameStateManager.Current.ActiveState is MapState mapState)
                {
                    mapState.Handler.ResetCamera(true, true);
                    mapState.Handler.TeleportCameraToMainParty();
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not assign the Imperial Engineer career: " + e.Message);
            }
        }

        private static void OnDailyTick()
        {
            if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.School + "Passive4")) return;

            var roster = MobileParty.MainParty?.MemberRoster;
            if (roster == null) return;

            var troops = roster.GetTroopRoster();
            for (var index = 0; index < troops.Count; index++)
            {
                var character = troops[index].Character;
                if (character == null || character.IsHero) continue;
                if (!character.IsRanged && !Firearms.IsEngineerTroop(character)) continue;

                roster.AddXpToTroopAtIndex(DailyTroopXp * troops[index].Number, index);
            }
        }
    }
}
