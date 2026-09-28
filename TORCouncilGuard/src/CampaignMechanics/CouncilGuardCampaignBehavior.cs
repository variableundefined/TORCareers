using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TOR_Core.CampaignMechanics.CharacterCreation;
using TOR_Core.Extensions;
using TORCouncilGuard.Bootstrap;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.CampaignMechanics
{
    internal class CouncilGuardCampaignBehavior : CampaignBehaviorBase
    {
        private static readonly Vec2 TorLithanel = new Vec2(1216.198f, 1345.101f);

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        // Fires after TOR's finalize, which has already fallen back to Mercenary and the Eonir spawn.
        private static void OnCharacterCreationIsOver()
        {
            try
            {
                var handler = TORCharacterCreationContentHandler.Instance;
                if (handler == null || handler.GetSelectedProfessionId() != CouncilGuardCharacterCreation.OptionId) return;

                var career = CouncilGuardCareer.Career;
                if (career == null)
                {
                    Log.Error("Council Guard career was never registered; this start stays Mercenary.");
                    return;
                }

                Hero.MainHero.AddCareer(career);
                MobileParty.MainParty.Position = new CampaignVec2(TorLithanel, true);
                if (GameStateManager.Current.ActiveState is MapState mapState)
                {
                    mapState.Handler.ResetCamera(true, true);
                    mapState.Handler.TeleportCameraToMainParty();
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not assign the Council Guard career: " + e.Message);
            }
        }
    }
}
