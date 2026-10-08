using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class LetThemHaveItSelf
    {
        private static readonly AccessTools.FieldRef<TriggeredEffect, TriggeredEffectTemplate> TemplateOf =
            AccessTools.FieldRefAccess<TriggeredEffect, TriggeredEffectTemplate>("_template");

        internal static void Apply(Harmony harmony)
        {
            var trigger = AccessTools.Method(typeof(TriggeredEffect), nameof(TriggeredEffect.Trigger))
                          ?? throw new MissingMethodException("TriggeredEffect", "Trigger");

            harmony.Patch(trigger, prefix: new HarmonyMethod(AccessTools.Method(typeof(LetThemHaveItSelf), nameof(IncludeCaster))));
        }

        private static void IncludeCaster(TriggeredEffect __instance, Vec3 position, Agent triggererAgent,
            AbilityTemplate originAbilityTemplate, ref MBList<Agent> targets)
        {
            try
            {
                if (triggererAgent == null || !triggererAgent.IsActive()) return;

                var template = TemplateOf(__instance);
                if (template?.StringID == null || !template.StringID.StartsWith(Data.EffectId, StringComparison.Ordinal)) return;

                var hero = triggererAgent.GetHero();
                if (hero == null || hero != Hero.MainHero || !Recruits.IsMercenary()) return;

                if (targets == null) targets = AlliesInRadius(template, position, triggererAgent, originAbilityTemplate);
                else targets = new MBList<Agent>(targets);

                if (!targets.Contains(triggererAgent)) targets.Add(triggererAgent);
            }
            catch (Exception ex)
            {
                Log.Write("LetThemHaveItSelf: " + ex.Message);
            }
        }

        private static MBList<Agent> AlliesInRadius(TriggeredEffectTemplate template, Vec3 position, Agent caster, AbilityTemplate origin)
        {
            var radius = template.Radius;
            var model = Campaign.Current?.Models?.GetAbilityModel();
            if (model != null && origin != null && caster.Character is CharacterObject character)
                radius = model.CalculateRadiusForAbility(character, origin, radius);

            return Mission.Current.GetNearbyAllyAgents(position.AsVec2, radius, caster.Team, new MBList<Agent>());
        }
    }
}
