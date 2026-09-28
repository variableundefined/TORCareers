using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Models;

namespace TORCouncilGuard.Bootstrap
{
    internal static class CouncilGuardFavor
    {
        private const string Choice = "ToriourPassive3";
        private const int CharmPerFavor = 50;

        private static bool _applied;

        internal static void Apply(Harmony harmony)
        {
            if (_applied) return;

            var target = AccessTools.Method(typeof(TORCustomResourceModel),
                nameof(TORCustomResourceModel.GetCultureSpecificCustomResourceChange));
            if (target == null)
                throw new System.MissingMethodException(nameof(TORCustomResourceModel),
                    nameof(TORCustomResourceModel.GetCultureSpecificCustomResourceChange));

            harmony.Patch(target, postfix: new HarmonyMethod(AccessTools.Method(typeof(CouncilGuardFavor), nameof(Postfix))));
            _applied = true;
        }

        private static void Postfix(Hero hero, ref ExplainedNumber __result)
        {
            if (hero == null || hero != Hero.MainHero || hero.PartyBelongedTo == null) return;
            if (hero.GetCultureSpecificCustomResource() == null) return;
            if (!hero.HasCareerChoice(Choice)) return;

            var bonus = hero.GetSkillValue(DefaultSkills.Charm) / CharmPerFavor;
            if (bonus <= 0) return;

            var choice = MBObjectManager.Instance.GetObject<CareerChoiceObject>(Choice);
            var label = choice?.BelongsToGroup?.Name?.ToString() ?? "Toriour";
            __result.Add(bonus, new TextObject(label));
        }
    }
}
