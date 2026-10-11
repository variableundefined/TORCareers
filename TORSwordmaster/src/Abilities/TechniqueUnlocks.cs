using TaleWorlds.CampaignSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Abilities
{
    internal static class TechniqueUnlocks
    {
        internal static void Grant(Hero hero)
        {
            if (hero == null) return;

            hero.AddAbility(Technique.Phoenix);
            if (hero.GetPerkValue(TORPerks.Faith.NovicePrayers)) hero.AddAbility(Technique.Loec);
            if (hero.GetPerkValue(TORPerks.Faith.AdeptPrayers)) hero.AddAbility(Technique.Sun);
            if (hero.GetPerkValue(TORPerks.Faith.GrandPrayers)) hero.AddAbility(Technique.FallingWater);
            if (hero.HasCareerChoice(G.Keystone(G.Bladelord))) hero.AddAbility(Technique.Master);
        }
    }
}
