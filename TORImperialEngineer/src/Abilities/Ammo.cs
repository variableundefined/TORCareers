using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TOR_Core.Models;
using TaleWorlds.Core;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Abilities
{
    internal enum AmmoType { Heavy, Scatter, Explosive }

    internal static class Ammo
    {
        internal const string HeavyId = "HeavyShot";
        internal const string ScatterId = "ScatterShot";
        internal const string ExplosiveId = "ExplosiveShot";

        private static readonly string[] SelectorIds = { HeavyId, ScatterId, ExplosiveId };

        internal static AmmoType Selected { get; private set; } = AmmoType.Heavy;

        internal static bool IsSelector(AbilityTemplate template) =>
            template != null && SelectorIds.Contains(template.StringID);

        internal static void Select(AbilityTemplate template, Agent caster)
        {
            switch (template.StringID)
            {
                case ScatterId: Selected = AmmoType.Scatter; break;
                case ExplosiveId: Selected = AmmoType.Explosive; break;
                default: Selected = AmmoType.Heavy; break;
            }

            if (Selected == AmmoType.Scatter) TwinShot.Suspend(caster);
            else TwinShot.Arm(caster);

            foreach (var agent in Mission.Current.Agents)
            {
                if (agent == null || !agent.IsActive() || Munition.Remaining(agent) <= 0) continue;
                if (agent != caster && !(agent.IsHero && agent.BelongsToMainParty())) continue;
                Munition.Reload(agent, Hero.MainHero);
            }

            if (caster.IsMainAgent)
                InformationManager.DisplayMessage(new InformationMessage(Describe(template, caster).ToString()));
        }

        private static TextObject Describe(AbilityTemplate template, Agent caster)
        {
            var remaining = Munition.Remaining(caster);
            var text = remaining > 0
                ? new TextObject("{=imperial_engineer_ammo_loaded}{AMMO} loaded - {SHOTS} shots left")
                : new TextObject("{=imperial_engineer_ammo_next}Next load will be {AMMO}");
            text.SetTextVariable("AMMO", new TextObject(template.Name?.ToString() ?? string.Empty));
            text.SetTextVariable("SHOTS", remaining);
            return text;
        }

        private static void OnSelectorCast(Ability ability, Agent caster)
        {
            try
            {
                Select(ability.Template, caster);
            }
            catch (Exception e)
            {
                Log.Warn("Ammo selection failed: " + e.Message);
            }
        }

        [HarmonyPatch(typeof(AbilityHUD_VM), nameof(AbilityHUD_VM.RefreshValues))]
        internal static class SelectorLabel
        {
            [HarmonyPostfix]
            private static void Postfix(AbilityHUD_VM __instance)
            {
                var ability = Agent.Main?.GetCurrentAbility();
                if (ability != null && IsSelector(ability.Template))
                    __instance.AbilityType = new TextObject("{=imperial_engineer_ammo_type}(Ammunition)").ToString();
            }
        }

        [HarmonyPatch(typeof(TORAbilityModel), nameof(TORAbilityModel.GetRelevantSkillForAbility))]
        internal static class SelectorSkill
        {
            [HarmonyPostfix]
            private static void Postfix(AbilityTemplate ability, ref SkillObject __result)
            {
                if (IsSelector(ability)) __result = null;
            }
        }

        [HarmonyPatch(typeof(AbilityComponent), MethodType.Constructor, new[] { typeof(Agent) })]
        internal static class AddSelectors
        {
            private static readonly MethodInfo OnCastStart = AccessTools.Method(typeof(AbilityComponent), "OnCastStart");
            private static readonly MethodInfo OnCastComplete = AccessTools.Method(typeof(AbilityComponent), "OnCastComplete");

            [HarmonyPostfix]
            private static void Postfix(AbilityComponent __instance, Agent agent)
            {
                try
                {
                    if (agent == null || agent.GetHero() != Hero.MainHero || !ImperialEngineerCareer.IsPlayer) return;
                    if (OnCastStart == null || OnCastComplete == null) return;

                    var abilities = __instance.KnownAbilitySystem;
                    var wasEmpty = abilities.Count == 0;

                    foreach (var id in SelectorIds)
                    {
                        if (abilities.Any(a => a.Template?.StringID == id)) continue;
                        var ability = AbilityFactory.CreateNew(id, agent);
                        if (ability == null) continue;

                        ability.OnCastStart += (Ability.OnCastStartHandler)Delegate.CreateDelegate(typeof(Ability.OnCastStartHandler), __instance, OnCastStart);
                        ability.OnCastComplete += (Ability.OnCastCompleteHandler)Delegate.CreateDelegate(typeof(Ability.OnCastCompleteHandler), __instance, OnCastComplete);
                        ability.OnCastComplete += cast => OnSelectorCast(cast, agent);
                        abilities.Add(ability);
                    }

                    if (wasEmpty && abilities.Count > 0)
                        __instance.SelectAbility(0);
                }
                catch (Exception e)
                {
                    Log.Warn("Could not add ammo selection: " + e.Message);
                }
            }
        }
    }
}
