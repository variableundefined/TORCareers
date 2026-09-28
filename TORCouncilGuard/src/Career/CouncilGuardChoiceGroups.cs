using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORCouncilGuard.Career
{
    internal static class CouncilGuardChoiceGroups
    {
        internal const string Toriour = "Toriour";
        internal const string Guardian = "GuardianOfTorLithanel";
        internal const string SilverTower = "ProtectorOfTheSilverTower";
        internal const string Forge = "ForgeOfRainbowFalls";
        internal const string Senate = "GuardOfTheSenate";
        internal const string Temple = "TempleOfAsuryan";
        internal const string Blades = "BladesOfElthinArvan";

        private static readonly (string Id, string Name, int Tier)[] Groups =
        {
            (Toriour,     "Toriour",                       1),
            (Guardian,    "Guardian of Tor Lithanel",      1),
            (SilverTower, "Protector of the Silver Tower", 2),
            (Forge,       "Forge of Rainbow Falls",        2),
            (Senate,      "Guard of the Senate",           2),
            (Temple,     "Temple of Asuryan",          3),
            (Blades,      "Blades of Elthin Arvan",        3),
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
                        text = TORTextHelper.GetText("tor_careerunlock_level_" + rank, CouncilGuardCareer.Id, fallback);
                        return hero?.Clan == null || hero.Clan.Tier >= required;
                    },
                    null);
            }
        }
    }
}
