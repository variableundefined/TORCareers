using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Career;

namespace TORSwordmaster.Bootstrap
{
    internal static class PriestRoute
    {
        private static readonly List<(string PrayerID, int Rank)> PlayerPrayers = new List<(string, int)>
        {
            (Technique.Phoenix, 1),
            (Technique.Loec, 2),
            (Technique.Sun, 3),
            (Technique.FallingWater, 4),
        };

        private static readonly List<(string PrayerID, int Rank)> CompanionPrayers = new List<(string, int)>
        {
            (Technique.Phoenix, 1),
            (Technique.Loec, 1),
            (Technique.Sun, 1),
            (Technique.FallingWater, 1),
        };

        internal static void Apply(Harmony harmony)
        {
            var isPriest = AccessTools.Method(typeof(HeroExtensions), nameof(HeroExtensions.IsPriest))
                ?? throw new MissingMethodException("HeroExtensions.IsPriest not found.");
            var prayerList = AccessTools.Method(typeof(CareerHelper), nameof(CareerHelper.GetPriestPrayerList))
                ?? throw new MissingMethodException("CareerHelper.GetPriestPrayerList not found.");

            harmony.Patch(isPriest, postfix: new HarmonyMethod(AccessTools.Method(typeof(PriestRoute), nameof(IsPriest))));
            harmony.Patch(prayerList, postfix: new HarmonyMethod(AccessTools.Method(typeof(PriestRoute), nameof(PrayerList))));
        }

        private static bool IsSwordmaster(Hero hero)
        {
            var career = SwordmasterCareer.Career;
            return hero != null && career != null && hero == Hero.MainHero && hero.HasCareer(career);
        }

        private static bool IsCompanion(Hero hero) =>
            hero != null && hero.HasAttribute(SwordmasterCareerChoices.CompanionAttribute);

        private static void IsPriest(Hero hero, ref bool __result)
        {
            if (!__result && (IsSwordmaster(hero) || IsCompanion(hero))) __result = true;
        }

        private static void PrayerList(Hero priestHero, ref List<(string PrayerID, int Rank)> __result)
        {
            if (__result == null || __result.Count > 0) return;

            if (IsSwordmaster(priestHero)) __result.AddRange(PlayerPrayers);
            else if (IsCompanion(priestHero)) __result.AddRange(CompanionPrayers);
        }
    }
}
