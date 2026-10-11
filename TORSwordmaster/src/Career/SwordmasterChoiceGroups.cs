using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORSwordmaster.Career
{
    internal static class SwordmasterChoiceGroups
    {
        internal const string SwordDancing = "SmSwordDancing";
        internal const string Heirloom = "SmHeirloomOfAenarion";
        internal const string ThirtyForms = "SmThirtyForms";
        internal const string Storm = "SmPathOfTheStorm";
        internal const string SwordOfHoeth = "SmSwordOfHoeth";
        internal const string Bladelord = "SmBladelord";
        internal const string Ritual = "SmRitualOfCleansing";

        internal static readonly (string Id, string Name, int Tier)[] Groups =
        {
            (SwordDancing, "Sword-Dancing",        1),
            (Heirloom,     "Heirloom of Aenarion", 2),
            (ThirtyForms,  "Thirty Forms",         2),
            (Storm,        "Path of the Storm",    2),
            (SwordOfHoeth, "Sword of Hoeth",       3),
            (Bladelord,    "Bladelord",            3),
            (Ritual,       "Ritual of Cleansing",  3),
        };

        private static readonly int[] RequiredClanTier = { 0, 0, 2, 4 };

        internal static string Passive(string group, int n) => group + "Passive" + n;
        internal static string Keystone(string group) => group + "Keystone";

        internal static bool Has(string choiceId) =>
            Hero.MainHero != null && Hero.MainHero.HasCareerChoice(choiceId);

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
                        text = TORTextHelper.GetText("tor_careerunlock_level_" + rank, SwordmasterCareer.Id, fallback);
                        return hero?.Clan == null || hero.Clan.Tier >= required;
                    },
                    null);
            }
        }
    }
}
