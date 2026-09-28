using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORImperialEngineer.Career
{
    internal static class ImperialEngineerChoiceGroups
    {
        internal const string Logistics = "ImperialLogistics";
        internal const string Customized = "CustomizedWeapons";
        internal const string School = "ImperialSchoolOfEngineers";
        internal const string Gunnery = "ImperialGunnerySchool";
        internal const string Cannons = "CannonsOfTheEmpire";
        internal const string Leonardo = "LeonardosLegacy";
        internal const string Cavalcade = "WhirlingCavalcadeOfDeath";

        internal static readonly string[] All =
        {
            Logistics, Customized, School, Gunnery, Cannons, Leonardo, Cavalcade,
        };

        private static readonly (string Id, string Name, int Tier)[] Groups =
        {
            (Logistics,  "Imperial Logistics",           1),
            (Customized, "Customized Weapons",           1),
            (School,     "Imperial School of Engineers", 2),
            (Gunnery,    "Imperial Gunnery School",      2),
            (Cannons,    "Cannons of the Empire",        2),
            (Leonardo,   "Leonardo's Legacy",            3),
            (Cavalcade,  "Whirling Cavalcade of Death",  3),
        };

        private static readonly int[] RequiredClanTier = { 0, 0, 2, 4 };

        internal static void Register(CareerObject career)
        {
            foreach (var (id, name, tier) in Groups)
            {
                var group = Game.Current.ObjectManager
                    .RegisterPresumedObject(new CareerChoiceGroupObject(id));

                var required = RequiredClanTier[tier];
                var rank = tier;
                group.Initialize(name, career, tier,
                    (Hero hero, out string text) =>
                    {
                        var fallback = required > 0 ? "Required clan renown: " + required : string.Empty;
                        text = TORTextHelper.GetText("tor_careerunlock_level_" + rank, ImperialEngineerCareer.Id, fallback);
                        return hero?.Clan == null || hero.Clan.Tier >= required;
                    },
                    null);
            }
        }
    }
}
