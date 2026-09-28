using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOR_Core.Extensions;

namespace TORCouncilGuard.Career
{
    internal static class CitybornFilter
    {
        private const string CitybornPrefix = "tor_eo_cityborn";
        private const string QueensGuardPrefix = "tor_eo_queens_guard";
        private const string AsurPrefix = "tor_he_";

        internal static bool IsCityborn(CharacterObject character)
        {
            if (character == null || character.IsHero) return false;
            var id = character.StringId;
            return id.StartsWith(CitybornPrefix) || id.StartsWith(QueensGuardPrefix);
        }

        internal static bool IsAsur(CharacterObject character)
        {
            if (character == null || character.IsHero) return false;
            return character.StringId.StartsWith(AsurPrefix);
        }

        internal static bool IsBuffTarget(CharacterObject character)
        {
            if (IsCityborn(character)) return true;
            if (!IsAsur(character)) return false;
            var hero = Hero.MainHero;
            return hero != null && hero.HasCareerChoice("TempleOfAsuryanPassive2");
        }

        internal static bool IsBuffTarget(Agent agent) =>
            agent?.Character is CharacterObject c && IsBuffTarget(c);
    }
}
