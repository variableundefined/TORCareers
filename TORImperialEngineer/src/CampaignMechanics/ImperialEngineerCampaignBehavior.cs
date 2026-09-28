using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    internal class ImperialEngineerCampaignBehavior : CampaignBehaviorBase
    {
        private const int DailyTroopXp = 25;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
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
