using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORCouncilGuard.Abilities;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch(typeof(CareerAbility), nameof(CareerAbility.IsDisabled))]
    internal static class JudgementOfAsuryanRestrictions
    {
        [HarmonyPostfix]
        private static void Postfix(CareerAbility __instance, Agent casterAgent,
                                    ref TaleWorlds.Localization.TextObject disabledReason, ref bool __result)
        {
            if (__result) return;
            if (__instance?.Template?.StringID != CouncilGuardCareer.AbilityId) return;
            if (casterAgent == null) return;

            var weapon = casterAgent.WieldedWeapon;
            if (weapon.IsEmpty || weapon.CurrentUsageItem == null)
            {
                disabledReason = TORTextHelper.GetTextObject("tor_career_ability_not_usable_without_weapon", "Not usable without weapon");
                __result = true;
            }
            else if (weapon.CurrentUsageItem.IsRangedWeapon || weapon.CurrentUsageItem.IsConsumable)
            {
                disabledReason = new TaleWorlds.Localization.TextObject("{=tor_ability_requires_melee_weapon}You must wield a melee weapon to use this ability");
                __result = true;
            }
        }
    }

    internal class CouncilGuardMainAgentLogic : MissionLogic
    {
        private const string MagicalResistanceChoice = "BladesOfElthinArvanPassive4";
        private const string MagicalResistanceEffect = "cg_blades_magical_resistance";
        private const float PermanentDuration = 99999f;

        // TOR builds the StatusEffectComponent and CareerAbility in OnAgentCreated, so both exist by now.
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            var hero = Hero.MainHero;
            if (agent == null || hero == null || agent.GetHero() != hero) return;

            if (hero.HasCareerChoice(MagicalResistanceChoice))
                agent.ApplyStatusEffect(MagicalResistanceEffect, agent, PermanentDuration, false);

            var career = CouncilGuardCareer.Career;
            if (career == null || !hero.HasCareer(career) || !hero.HasCareerChoice("ToriourKeystone")) return;

            var ability = agent.GetComponent<AbilityComponent>()?.CareerAbility;
            if (ability == null) return;

            ability.AddCharge(career.MaxCharge);
            ability.SetCoolDown(0);
        }
    }

    internal class JudgementOfAsuryanBurnLogic : MissionLogic
    {
        private const float SpreadRadius = 3f;

        private static readonly System.Reflection.FieldInfo CurrentEffects =
            AccessTools.Field(typeof(StatusEffectComponent), "_currentEffects");

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon,
                                        in Blow blow, in AttackCollisionData attackCollisionData)
        {
            if (affectorAgent == null || affectedAgent == null || blow.IsMissile) return;
            if (!affectedAgent.IsHuman || !affectedAgent.IsEnemyOf(affectorAgent)) return;

            var weapon = affectorWeapon.CurrentUsageItem;
            if (affectorWeapon.IsEmpty || weapon == null || weapon.IsRangedWeapon || weapon.IsConsumable) return;

            if (attackCollisionData.CollisionResult != CombatCollisionResult.StrikeAgent
                || attackCollisionData.AttackBlockedWithShield
                || attackCollisionData.CollidedWithShieldOnBack) return;

            var hero = Hero.MainHero;
            if (hero == null) return;

            if (!affectorAgent.IsMainAgent) return;

            var flaming = HasTrait(affectorWeapon, affectorAgent, JudgementOfAsuryanScript.PersonalFlame);

            if (affectedAgent.Health <= 0f)
            {
                if (hero.HasCareerChoice("BladesOfElthinArvanKeystone") && IsBurning(affectedAgent))
                    Spread(affectedAgent, affectorAgent);
                return;
            }

            if (flaming && hero.HasCareerChoice("ForgeOfRainbowFallsKeystone"))
                Burn(affectedAgent, affectorAgent);
        }

        private static void Burn(Agent target, Agent applier)
        {
            var logic = Mission.Current?.GetMissionBehavior<AbilityManagerMissionLogic>();
            if (logic != null)
                logic.QueueTriggeredStatusEffect(target, JudgementOfAsuryanScript.BurnEffect, applier,
                    JudgementOfAsuryanScript.BurnDuration, false, false);
            else
                target.ApplyStatusEffect(JudgementOfAsuryanScript.BurnEffect, applier, JudgementOfAsuryanScript.BurnDuration, false);
        }

        private static bool HasTrait(MissionWeapon weapon, Agent agent, string traitId)
        {
            var item = weapon.Item;
            return item != null && item.GetTraits(agent).Any(t => t.ItemTraitStringId == traitId);
        }

        private static bool IsBurning(Agent agent)
        {
            var component = agent.GetComponent<StatusEffectComponent>();
            if (component == null) return false;

            if (!(CurrentEffects?.GetValue(component) is System.Collections.IDictionary effects)) return false;
            return effects.Keys.OfType<StatusEffect>().Any(e =>
                e.Template != null
                && e.Template.Type == StatusEffectTemplate.EffectType.DamageOverTime
                && e.Template.DamageType == DamageType.Fire);
        }

        private static void Spread(Agent victim, Agent killer)
        {
            var enemies = Mission.Current.GetNearbyEnemyAgents(victim.Position.AsVec2, SpreadRadius, killer.Team, new MBList<Agent>());
            foreach (var enemy in (List<Agent>)(object)enemies)
            {
                if (enemy == null || enemy == victim || !enemy.IsHuman || !enemy.IsActive() || enemy.Health <= 0f) continue;
                Burn(enemy, killer);
            }
        }
    }

    internal class CouncilGuardKillLogic : MissionLogic
    {
        private const int CooldownPerKill = 2;

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectorAgent == null || !affectorAgent.IsMainAgent) return;
            if (affectedAgent == null || !affectedAgent.IsEnemyOf(affectorAgent)) return;

            if (CareerHelper.IsValidCareerMissionInteractionBetweenAgents(affectorAgent, affectedAgent))
                CouncilGuardFaith.OnKill(affectorAgent, affectedAgent);

            if (agentState != AgentState.Killed && agentState != AgentState.Unconscious) return;
            if (!affectedAgent.IsHuman) return;

            var hero = Hero.MainHero;
            if (hero == null || !hero.HasCareerChoice("GuardOfTheSenateKeystone")) return;

            var item = affectorAgent.WieldedWeapon.Item;
            if (item == null || !item.GetTraits(affectorAgent).Any(t => t.ItemTraitStringId == JudgementOfAsuryanScript.PersonalFlame)) return;

            var ability = affectorAgent.GetComponent<AbilityComponent>()?.CareerAbility;
            if (ability == null || !ability.IsOnCooldown()) return;

            ability.SetCoolDown(Math.Max(0, ability.GetCoolDownLeft() - CooldownPerKill));
        }
    }
}
