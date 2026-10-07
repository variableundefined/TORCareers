using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;

namespace TORWaywatcherOverhaul.Arrows
{
    internal static class Companions
    {
        internal const string SharedQuiver = "StarfireEssencePassive4";
        internal const string SharedLethalShot = "EyeOfTheHunterPassive1";

        private static readonly HashSet<string> Templates = new HashSet<string>
        {
            "tor_wanderer_woodelf_2",
            "tor_wanderer_eonir_0"
        };

        internal static bool IsWaywatcherCompanion(Agent agent)
        {
            var hero = agent?.GetHero();
            if (hero == null || hero == Hero.MainHero || !hero.IsPlayerCompanion) return false;
            var template = hero.Template?.StringId;
            return template != null && Templates.Contains(template);
        }

        internal static bool SharesArrows(Hero player) =>
            player != null && (player.HasCareerChoice(SharedQuiver) || player.HasCareerChoice(SharedLethalShot));

        internal static bool SharesLethalShot(Hero player) =>
            player != null && player.HasCareerChoice(SharedLethalShot);
    }
}
