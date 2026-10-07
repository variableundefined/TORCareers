using TaleWorlds.CampaignSystem;
using TOR_Core.Extensions;

namespace TORWaywatcherOverhaul.Arrows
{
    internal enum ArrowType
    {
        SwiftshiverShards,
        TrueflightArrow,
        ArcaneBodkin,
        StarfireShaft,
        MoonfireShot,
        HagbaneTips
    }

    internal static class EnchantedArrow
    {
        internal const string HailOfArrows = "HailOfArrowsKeystone";
        internal const string EyeOfTheHunter = "EyeOfTheHunterKeystone";
        internal const string StarfireEssence = "StarfireEssenceKeystone";
        internal const string Hawkeyed = "HawkeyedKeystone";
        internal const string ProtectorOfTheWoods = "ProtectorOfTheWoodsKeystone";

        internal static readonly ArrowType[] All =
        {
            ArrowType.SwiftshiverShards,
            ArrowType.TrueflightArrow,
            ArrowType.ArcaneBodkin,
            ArrowType.StarfireShaft,
            ArrowType.MoonfireShot,
            ArrowType.HagbaneTips
        };

        internal static bool IsPiercing(ArrowType arrow) =>
            arrow == ArrowType.SwiftshiverShards || arrow == ArrowType.TrueflightArrow || arrow == ArrowType.ArcaneBodkin;

        internal static bool Bursts(ArrowType arrow) =>
            arrow == ArrowType.StarfireShaft || arrow == ArrowType.MoonfireShot || arrow == ArrowType.HagbaneTips;

        internal static string SelectorId(ArrowType arrow) => "Waywatcher" + arrow;

        internal static bool TryParseSelector(string abilityId, out ArrowType arrow)
        {
            foreach (var candidate in All)
            {
                if (SelectorId(candidate) != abilityId) continue;
                arrow = candidate;
                return true;
            }
            arrow = default;
            return false;
        }

        internal static int Tier(Hero hero)
        {
            if (hero == null) return 1;
            if (hero.HasCareerChoice(EyeOfTheHunter)) return 3;
            if (hero.HasCareerChoice(HailOfArrows)) return 2;
            return 1;
        }

        internal static int Shards(int tier) => tier >= 3 ? 6 : tier == 2 ? 4 : 2;

        internal static int ShardDamagePercent(int tier) => tier >= 3 ? 40 : tier == 2 ? 45 : 60;

        internal static float TrueflightSpeed(int tier) => tier >= 3 ? 1f : tier == 2 ? 0.5f : 0.25f;

        internal static int TrueflightDamagePercent(int tier) => tier >= 3 ? 50 : tier == 2 ? 25 : 0;

        internal static int BodkinArmourPenetration(int tier) => tier >= 2 ? 75 : 50;

        internal static int BodkinDamagePercent(int tier) => tier >= 3 ? 50 : tier == 2 ? 25 : 0;

        internal static int StarfireFireBonusPercent(int tier) => tier >= 3 ? 30 : tier == 2 ? 20 : 10;

        internal static int MoonfireMagicBonusPercent(int tier) => tier >= 3 ? 50 : tier == 2 ? 40 : 25;

        internal static float Radius(ArrowType arrow, int tier) =>
            arrow == ArrowType.MoonfireShot ? (tier >= 3 ? 1.5f : 1f) : 0f;

        internal static float LethalRadiusBonus(int tier) => tier >= 3 ? 2f : tier == 2 ? 1.5f : 1f;
    }
}
