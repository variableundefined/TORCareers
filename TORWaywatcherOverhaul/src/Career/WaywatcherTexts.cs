using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOR_Core.CharacterDevelopment;

namespace TORWaywatcherOverhaul.Career
{
    internal static class WaywatcherTexts
    {
        internal const string LethalShotTooltip =
            "{=wwo_lethal_shot_tooltip}Next 3 arrows: 2x 'Piercing' arrows, +1m 'Bursting' radius. +1 arrow per 'Keystone'.";

        private static readonly (string ChoiceId, string Text)[] Choices =
        {
            ("WayWatcherRoot", "{=wwo_WayWatcherRoot}Soon, the Lumberfoots shall regret their trespass! Fire a Lethal Shot, with deadly precision! Your next 3 arrows fire double the 'Piercing' arrows, and 'Bursting' arrows gain +1m radius. Every 'Keystone' career perk unlocked increases arrows affected by Lethal Shot by +1, but decreases its recharge rate. (Ability is charged by dealing damage with bows.)"),
            ("ProtectorOfTheWoodsKeystone", "{=wwo_ProtectorOfTheWoodsKeystone}Lethal Shot can be used on battle start, reduces required damage to use, and affects +1 shot."),
            ("PathfinderKeystone", "{=wwo_PathfinderKeystone}Melee attacks also charge Lethal Shot."),
            ("ForestStalkerKeystone", "{=wwo_ForestStalkerKeystone}Troop damage also charges Lethal Shot."),
            ("HailOfArrowsKeystone", "{=wwo_HailOfArrowsKeystone}T2 Enchanted Arrows. Lethal Shot: +0.5m 'Bursting' radius, Trueflight and Bodkin pierce."),
            ("StarfireEssenceKeystone", "{=wwo_StarfireEssenceKeystone}'Bursting' arrows deal 25% more area damage."),
            ("HawkeyedKeystone", "{=wwo_HawkeyedKeystone}Shards: 2x Lethal Shot charge. Trueflight: +50% at range. Bodkin: +50% vs armour."),
            ("StarfireEssencePassive4", "{=wwo_StarfireEssencePassive4}Waywatcher and Ghost Strider companions share your Enchanted Arrows (no Lethal Shot)."),
            ("EyeOfTheHunterPassive1", "{=wwo_EyeOfTheHunterPassive1}Lethal Shot also empowers your Waywatcher and Ghost Strider companions."),
            ("EyeOfTheHunterKeystone", "{=wwo_EyeOfTheHunterKeystone}T3 Enchanted Arrows. Lethal Shot: +0.5m 'Bursting' radius.")
        };

        internal static void Apply()
        {
            foreach (var (choiceId, text) in Choices)
            {
                var choice = TORCareerChoices.GetChoice(choiceId);
                if (choice == null)
                {
                    Log.Warn("Career choice " + choiceId + " not found; its text was not replaced.");
                    continue;
                }
                ((PropertyObject)choice).Initialize(choice.Name, new TextObject(text));
            }
        }
    }
}
