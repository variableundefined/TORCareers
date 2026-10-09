using System;
using System.Collections.ObjectModel;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.AbilitySystem.SpellBook;
using TOR_Core.AbilitySystem.Spells;
using TOR_Core.AbilitySystem.Spells.Prayers;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Career;

namespace TORSwordmaster.Bootstrap
{
    internal static class TechniqueBook
    {
        private static readonly FieldInfo LoreObject = AccessTools.Field(typeof(BattlePrayersVM), "_loreObjectVm");

        private static readonly string[] Techniques =
        {
            Technique.Phoenix, Technique.Loec, Technique.Sun, Technique.FallingWater,
        };

        internal static void Apply(Harmony harmony)
        {
            var screen = AccessTools.Constructor(typeof(CareerScreenVM), new[] { typeof(Action) })
                ?? throw new MissingMethodException("CareerScreenVM(Action) not found.");
            var initialize = AccessTools.Method(typeof(BattlePrayersVM), "Initialize")
                ?? throw new MissingMethodException("BattlePrayersVM.Initialize not found.");
            if (LoreObject == null)
                throw new MissingFieldException("BattlePrayersVM._loreObjectVm not found.");

            harmony.Patch(screen, postfix: new HarmonyMethod(AccessTools.Method(typeof(TechniqueBook), nameof(ShowButton))));
            harmony.Patch(initialize, prefix: new HarmonyMethod(AccessTools.Method(typeof(TechniqueBook), nameof(FillBook))));
        }

        private static bool IsSwordmaster()
        {
            var career = SwordmasterCareer.Career;
            return career != null && Hero.MainHero != null && Hero.MainHero.HasCareer(career);
        }

        private static void ShowButton(CareerScreenVM __instance)
        {
            if (IsSwordmaster()) __instance.HasBattlePrayers = true;
        }

        private static bool FillBook(BattlePrayersVM __instance)
        {
            if (!IsSwordmaster()) return true;

            var hero = Hero.MainHero;
            var level = hero.GetPerkValue(TORPerks.Faith.GrandPrayers) ? PrayerLevel.Grand
                : hero.GetPerkValue(TORPerks.Faith.AdeptPrayers) ? PrayerLevel.Adept
                : hero.GetPerkValue(TORPerks.Faith.NovicePrayers) ? PrayerLevel.Novice
                : PrayerLevel.Minor;

            ((Collection<StatItemVM>)(object)__instance.StatItems).Clear();
            ((Collection<StatItemVM>)(object)__instance.StatItems).Add(new StatItemVM(
                TORTextHelper.GetText("tor_prayerbook_prayer_level", "Prayer level: "),
                GameTexts.FindText("tor_prayer_level", level.ToString()).ToString()));

            var lore = new PrayerLoreObjectVM(__instance, new System.Collections.Generic.List<string>(Techniques), hero);
            foreach (var item in lore.PrayerList)
                if (!item.IsKnown) item.IsDisabled = true;

            LoreObject.SetValue(__instance, lore);
            return false;
        }
    }
}
