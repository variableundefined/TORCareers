using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Models;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class LetThemHaveItScaling
    {
        internal static void Apply(Harmony harmony)
        {
            var radius = AccessTools.Method(typeof(TORAbilityModel), nameof(TORAbilityModel.CalculateRadiusForAbility))
                         ?? throw new MissingMethodException("TORAbilityModel.CalculateRadiusForAbility not found.");
            var duration = AccessTools.Method(typeof(TORAbilityModel), nameof(TORAbilityModel.CalculateStatusEffectDurationForAbility))
                           ?? throw new MissingMethodException("TORAbilityModel.CalculateStatusEffectDurationForAbility not found.");
            var ctor = AccessTools.Constructor(typeof(CareerAbility), new[] { typeof(AbilityTemplate), typeof(Agent) })
                       ?? throw new MissingMethodException("CareerAbility(AbilityTemplate, Agent) not found.");

            harmony.Patch(radius, postfix: new HarmonyMethod(AccessTools.Method(typeof(LetThemHaveItScaling), nameof(ScaleRadius))));
            harmony.Patch(duration, postfix: new HarmonyMethod(AccessTools.Method(typeof(LetThemHaveItScaling), nameof(ScaleDuration))));
            harmony.Patch(ctor, postfix: new HarmonyMethod(AccessTools.Method(typeof(LetThemHaveItScaling), nameof(SizeRing))));
        }

        private static void ScaleRadius(CharacterObject character, AbilityTemplate originAbilityTemplate, ref float __result)
        {
            try
            {
                if (!IsPlayersRally(character, originAbilityTemplate)) return;
                __result += Data.LeadershipRadius(character);
            }
            catch (Exception ex)
            {
                Log.Write("LetThemHaveItScaling radius: " + ex.Message);
            }
        }

        private static void ScaleDuration(CharacterObject character, AbilityTemplate originAbilityTemplate, ref float __result)
        {
            try
            {
                if (!IsPlayersRally(character, originAbilityTemplate)) return;
                __result += Data.LeadershipDuration(character);
            }
            catch (Exception ex)
            {
                Log.Write("LetThemHaveItScaling duration: " + ex.Message);
            }
        }

        private static void SizeRing(CareerAbility __instance, Agent agent)
        {
            try
            {
                if (!Data.IsLetThemHaveIt(__instance?.Template)) return;

                var hero = agent?.GetHero();
                if (hero == null || hero != Hero.MainHero) return;

                __instance.Template.TargetCapturingRadius = Data.RingRadius(hero);
            }
            catch (Exception ex)
            {
                Log.Write("LetThemHaveItScaling ring: " + ex.Message);
            }
        }

        private static bool IsPlayersRally(CharacterObject character, AbilityTemplate template)
        {
            if (!Data.IsLetThemHaveIt(template)) return false;
            if (character == null || !character.IsHero || character.HeroObject != Hero.MainHero) return false;
            return Hero.MainHero.HasCareer(TORCareers.Mercenary);
        }
    }
}
