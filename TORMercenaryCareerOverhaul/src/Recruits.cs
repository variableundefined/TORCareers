using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TORMercenaryCareerOverhaul.CampaignMechanics;

namespace TORMercenaryCareerOverhaul
{
    internal sealed class Promotion
    {
        internal CharacterObject Target;
        internal int Gold;
        internal int Resource;
    }

    internal static class Recruits
    {
        internal const string Empire = "empire";
        internal const string Dawi = "sturgia";
        internal const string Eonir = "eonir";

        internal const int MaxBatch = 5;
        internal const int EliteCareerTier = 2;
        internal const int CrossCultureCareerTier = 3;

        internal const string TooHighTier = "Unit is too high tier to be inducted into a mercenary company.";

        private const string DawiRanger = "tor_dw_ranger";
        private const string SeaElfGuard = "tor_he_seaelf_spearman";
        private const string BugmansRanger = "tor_ror_bugman_ranger";
        private const string SeaElfSentinel = "tor_he_seaelf_sentinel";
        private const string EmpireElite = "tor_merc_paymaster_bodyguard";

        internal static bool IsMercenary()
        {
            var hero = Hero.MainHero;
            return hero != null && hero.HasCareer(TORCareers.Mercenary);
        }

        internal static bool Serves(string culture)
        {
            return culture == Empire || culture == Dawi || culture == Eonir;
        }

        internal static bool IsCrossCulturePair(string player, string troop)
        {
            if (player == Empire) return troop == Dawi || troop == Eonir;
            if (player == Dawi || player == Eonir) return troop == Empire;
            return false;
        }

        internal static bool ShowsFor(CharacterObject troop)
        {
            var player = Hero.MainHero?.Culture?.StringId;
            var culture = troop?.Culture?.StringId;
            if (!Serves(player) || culture == null) return false;
            return culture == player || IsCrossCulturePair(player, culture);
        }

        internal static Promotion For(CharacterObject troop, out string reason)
        {
            reason = null;
            var hero = Hero.MainHero;
            var player = hero.Culture.StringId;
            var culture = troop.Culture?.StringId;

            if (troop.Tier <= 2)
            {
                if (culture != player)
                {
                    if (!IsCrossCulturePair(player, culture))
                    {
                        reason = "Only recruits of your own culture can be inducted.";
                        return null;
                    }

                    if (!hero.HasUnlockedCareerChoiceTier(CrossCultureCareerTier))
                    {
                        reason = "Requires the Commander tier to induct recruits of another culture.";
                        return null;
                    }
                }

                var basic = Basic(culture);
                if (basic == null)
                    reason = culture == Empire ? "You have no mercenary contact." : "No mercenary company takes this troop.";
                return basic;
            }

            if (troop.Tier >= 3 && culture == player && hero.HasUnlockedCareerChoiceTier(EliteCareerTier))
            {
                var elite = Elite(player);
                if (elite == null)
                {
                    reason = "No elite recruit is available for your culture.";
                    return null;
                }

                if (troop == elite.Target || troop.Tier >= elite.Target.Tier)
                {
                    reason = "Already at or above the tier of " + elite.Target.Name + ".";
                    return null;
                }

                return elite;
            }

            reason = TooHighTier;
            return null;
        }

        private static Promotion Basic(string culture)
        {
            switch (culture)
            {
                case Empire:
                    var company = MercenaryContacts.SignedCompany();
                    return company == null ? null : Make(company.Tiers[0], 750, 4);
                case Dawi:
                    return Make(DawiRanger, 150, 0);
                case Eonir:
                    return Make(SeaElfGuard, 300, 0);
                default:
                    return null;
            }
        }

        private static Promotion Elite(string culture)
        {
            switch (culture)
            {
                case Empire:
                    return Make(EmpireElite, 750, 10);
                case Dawi:
                    return Make(BugmansRanger, 750, 10);
                case Eonir:
                    return Make(SeaElfSentinel, 450, 0);
                default:
                    return null;
            }
        }

        private static Promotion Make(string troopId, int gold, int resource)
        {
            var target = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
            if (target == null)
            {
                Log.Write("Recruits: troop not found: " + troopId);
                return null;
            }

            return new Promotion { Target = target, Gold = gold, Resource = resource };
        }
    }
}
