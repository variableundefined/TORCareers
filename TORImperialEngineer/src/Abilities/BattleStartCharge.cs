using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    [HarmonyPatch(typeof(CareerAbility), MethodType.Constructor, new[] { typeof(AbilityTemplate), typeof(Agent) })]
    internal static class BattleStartCharge
    {
        private static readonly FieldInfo CurrentCharge = AccessTools.Field(typeof(CareerAbility), "_currentCharge");
        private static readonly FieldInfo MaxCharge = AccessTools.Field(typeof(CareerAbility), "_maxCharge");

        [HarmonyPostfix]
        private static void Postfix(CareerAbility __instance, Agent agent)
        {
            try
            {
                if (agent == null || agent.GetHero() != Hero.MainHero) return;
                if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Logistics + "Keystone")) return;
                if (__instance.Template == null || !__instance.Template.StringID.StartsWith(ImperialEngineerCareer.AbilityId)) return;
                if (CurrentCharge == null || MaxCharge == null) return;

                CurrentCharge.SetValue(__instance, (float)(int)MaxCharge.GetValue(__instance));
                __instance.SetCoolDown(0);
            }
            catch (Exception e)
            {
                Log.Warn("Could not start Experimental Munition charged: " + e.Message);
            }
        }
    }
}
