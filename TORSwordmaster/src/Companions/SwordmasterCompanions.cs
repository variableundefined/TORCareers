using TaleWorlds.CampaignSystem;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Career;

namespace TORSwordmaster.Companions
{
    internal static class SwordmasterCompanions
    {
        internal const string TemplateId = "tor_sm_swordmaster_companion";

        private static readonly string[] Abilities =
        {
            Technique.Phoenix, Technique.Loec, Technique.Sun, Technique.FallingWater,
        };

        internal static bool IsSwordmaster(Hero hero) => hero?.Template?.StringId == TemplateId;

        // TOR don't currently read extendedunitproperties from other modules, so the template's attributes and
        // techniques are given here, on hero creation, instead of through XML.
        internal static void Setup(Hero hero)
        {
            hero.AddAttribute("AbilityUser");
            hero.AddAttribute(SwordmasterCareerChoices.CompanionAttribute);
            foreach (var ability in Abilities)
                hero.AddAbility(ability);
        }
    }
}
