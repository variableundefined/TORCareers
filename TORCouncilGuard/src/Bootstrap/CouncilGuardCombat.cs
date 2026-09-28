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
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORCouncilGuard.Abilities;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch(typeof(CareerHelper), nameof(CareerHelper.AddCareerPassivesForDamageValues))]
    internal static class BladesMagicalResistance
    {
        private const string Choice = "BladesOfElthinArvanPassive4";
        private const float Resistance = 0.30f;

        [HarmonyPostfix]
        private static void Postfix(Agent victim, PropertyMask mask, float[] __result)
        {
            if (__result == null || mask != PropertyMask.Defense) return;
            if (victim == null || !victim.IsMainAgent) return;

            var hero = Hero.MainHero;
            if (hero == null || !hero.HasCareerChoice(Choice)) return;

            __result[(int)DamageType.Magical] += Resistance;
        }
    }

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

    [HarmonyPatch(typeof(CareerAbility), MethodType.Constructor, new[] { typeof(AbilityTemplate), typeof(Agent) })]
    internal static class ToriourKeystoneStartCharged
    {
        [HarmonyPostfix]
        private static void Postfix(CareerAbility __instance, Agent agent)
        {
            var hero = Hero.MainHero;
            if (agent == null || agent.GetHero() != hero) return;
            var career = CouncilGuardCareer.Career;
            if (hero == null || career == null || !hero.HasCareer(career)) return;
            if (!hero.HasCareerChoice("ToriourKeystone")) return;

            __instance.AddCharge(career.MaxCharge);
            __instance.SetCoolDown(0);
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

    internal class JudgementOfAsuryanKillLogic : MissionLogic
    {
        private const int CooldownPerKill = 2;

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectorAgent == null || !affectorAgent.IsMainAgent) return;
            if (agentState != AgentState.Killed && agentState != AgentState.Unconscious) return;
            if (affectedAgent == null || !affectedAgent.IsHuman || !affectedAgent.IsEnemyOf(affectorAgent)) return;

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
