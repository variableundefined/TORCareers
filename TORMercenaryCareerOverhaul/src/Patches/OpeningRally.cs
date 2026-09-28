using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class OpeningRally
    {
        internal const string Card = "SurvivalistKeystone";

        internal static void Apply(Harmony harmony)
        {
            var constructor = AccessTools.Constructor(typeof(CareerAbility), new[] { typeof(AbilityTemplate), typeof(Agent) })
                              ?? throw new MissingMethodException("CareerAbility", "ctor");

            harmony.Patch(constructor, postfix: new HarmonyMethod(AccessTools.Method(typeof(OpeningRally), nameof(Ready))));
        }

        private static void Ready(CareerAbility __instance, Agent agent)
        {
            try
            {
                if (__instance == null || agent == null) return;

                var hero = Hero.MainHero;
                if (hero == null || agent.GetHero() != hero) return;
                if (!hero.HasCareer(TORCareers.Mercenary)) return;
                if (!hero.HasCareerChoice(Card)) return;

                __instance.SetCoolDown(0);
            }
            catch (Exception ex)
            {
                Log.Write("OpeningRally: " + ex.Message);
            }
        }
    }
}
