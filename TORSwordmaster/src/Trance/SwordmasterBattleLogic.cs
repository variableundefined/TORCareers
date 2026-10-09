using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Economy;
using TORSwordmaster.Career;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Trance
{
    internal class SwordmasterBattleLogic : MissionLogic
    {
        internal const int FaithPerVictimLevel = 10;
        internal const int HealthPerEnchantedItem = 5;
        private const float ResistPerEnchantedItem = 0.02f;
        private const float CompanionWard = 0.10f;
        private const float Permanent = 99999f;

        internal const string HeirloomResistEffect = "sm_heirloom_physres";
        internal const string CompanionWardEffect = "sm_companion_ward";

        private float _lastBlockGain = float.MinValue;
        private float _swingStart = float.MinValue;
        private float _swingGain;

        private static bool IsSwordmaster()
        {
            var career = SwordmasterCareer.Career;
            return career != null && Hero.MainHero != null && Hero.MainHero.HasCareer(career);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (agent == null || !agent.IsHuman) return;

            try
            {
                var hero = agent.GetHero();
                if (hero != null && hero == Hero.MainHero)
                {
                    if (IsSwordmaster()) SetUpPlayer(agent);
                }
                else if (hero != null && hero.HasAttribute(SwordmasterCareerChoices.CompanionAttribute))
                {
                    SetUpCompanion(agent);
                }
                else if (hero == null && agent.GetOriginMobileParty() == MobileParty.MainParty)
                {
                    MartialTraining.ApplyInBattle(agent);
                }
            }
            catch (Exception e)
            {
                Log.Error("Agent setup failed: " + e.Message);
            }
        }

        private static void SetUpPlayer(Agent agent)
        {
            WayOfTheSwordScript.Active = null;
            WayOfTheSwordScript.LastCleanse = float.MinValue;
            Riposte.Reset();
            MastersStrike.Reset();

            TechniqueUnlocks.Grant(Hero.MainHero);
            InstallTechniques(agent);

            if (G.Has(G.Passive(G.Heirloom, 3)))
                ApplyHeirloom(agent);
            if (G.Has(G.Passive(G.Ritual, 3)))
            {
                var ability = Focus.Of(agent);
                if (ability != null) Focus.Set(ability, Focus.StartingCharge);
            }
        }

        private static void InstallTechniques(Agent agent)
        {
            var component = agent.GetComponent<AbilityComponent>();
            if (component == null) return;

            foreach (var entry in Technique.Costs)
            {
                if (!Hero.MainHero.HasAbility(entry.Key)) continue;

                var template = AbilityFactory.GetTemplate(entry.Key);
                if (template == null)
                {
                    Log.Warn("Technique template missing: " + entry.Key);
                    continue;
                }

                var technique = new Technique(template, entry.Value);
                technique.SetCrosshair(AbilityFactory.InitializeCrosshair(template));
                Reflection.WireCastEvents(component, technique);

                var known = component.KnownAbilitySystem;
                var index = known.FindIndex(a => a.StringID == entry.Key);
                if (index >= 0)
                {
                    var wasCurrent = component.CurrentAbility == known[index];
                    known[index] = technique;
                    if (wasCurrent) component.SelectAbility(technique);
                }
                else
                {
                    known.Add(technique);
                }
            }
        }

        private static void ApplyHeirloom(Agent agent)
        {
            var equipment = Hero.MainHero.BattleEquipment;
            var count = 0;
            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.Horse; i++)
            {
                var item = equipment[i].Item;
                if (item != null && item.HasAnyTrait()) count++;
            }
            if (count == 0) return;

            var health = count * HealthPerEnchantedItem;
            agent.BaseHealthLimit += health;
            agent.HealthLimit += health;
            agent.Health += health;
            Effects.Apply(agent, HeirloomResistEffect, count * ResistPerEnchantedItem, Permanent, agent);
        }

        private static void SetUpCompanion(Agent agent)
        {
            if (agent.GetOriginMobileParty() != MobileParty.MainParty) return;

            if (G.Has(G.Passive(G.SwordOfHoeth, 3)))
                Effects.Apply(agent, CompanionWardEffect, CompanionWard, Permanent, agent);

            var component = agent.GetComponent<AbilityComponent>();
            if (component == null) return;

            foreach (var ability in component.KnownAbilitySystem.Where(a => a.StringID == Technique.Sun))
                ability.OnCastComplete += cast => TechniqueEffects.OnSunCast(agent);
            foreach (var ability in component.KnownAbilitySystem.Where(a => a.StringID == Technique.Loec))
                ability.OnCastComplete += cast => TechniqueEffects.OnLoecCast(agent);
            foreach (var ability in component.KnownAbilitySystem.Where(a => a.StringID == Technique.FallingWater))
                ability.OnCastComplete += cast => TechniqueEffects.OnFallingWaterCast(agent);
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon,
                                        in Blow blow, in AttackCollisionData attackCollisionData)
        {
            if (affectorAgent == null || affectedAgent == null || affectedAgent.IsMount || !affectedAgent.IsEnemyOf(affectorAgent)) return;

            if (blow.IsMissile)
            {
                if (affectorWeapon.Item?.StringId != Technique.Phoenix) return;
                if (affectorAgent.IsMainAgent && IsSwordmaster())
                    TechniqueEffects.PhoenixHit(affectorAgent, affectedAgent, true);
                else if (affectorAgent.GetHero()?.HasAttribute(SwordmasterCareerChoices.CompanionAttribute) == true)
                    TechniqueEffects.PhoenixHit(affectorAgent, affectedAgent, false);
                return;
            }

            if (!affectorAgent.IsMainAgent || !IsSwordmaster()) return;

            if (attackCollisionData.CollisionResult != CombatCollisionResult.StrikeAgent || blow.InflictedDamage <= 0) return;
            if (!IsMeleeWeapon(affectorWeapon)) return;

            Riposte.OnStrike(affectorAgent);
            MastersStrike.OnStrike(affectorAgent, affectedAgent, blow);

            var ability = Focus.Of(affectorAgent);
            if (ability == null) return;

            var now = Mission.CurrentTime;
            if (now - _swingStart > Focus.SwingWindow)
            {
                _swingStart = now;
                _swingGain = 0f;
            }

            var gain = Math.Min(Focus.HitGain(blow.InflictedDamage), Focus.SwingGainCap - _swingGain);
            if (gain <= 0f) return;
            _swingGain += gain;
            Focus.Add(ability, gain);
        }

        public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            if (attacker == null || victim == null || collisionData.IsMissile || !attacker.IsEnemyOf(victim)) return;
            if (!IsSwordmaster()) return;

            if (!victim.IsMainAgent) return;

            var ability = Focus.Of(victim);
            if (ability == null) return;

            if (IsBlockResult(collisionData.CollisionResult)) Riposte.Arm(victim);

            if (collisionData.AttackBlockedWithShield)
            {
                if (IsBlockResult(collisionData.CollisionResult)) AddBlock(ability, Focus.ShieldBlocked);
                return;
            }

            switch (collisionData.CollisionResult)
            {
                case CombatCollisionResult.Blocked:
                    AddBlock(ability, Focus.Blocked);
                    break;
                case CombatCollisionResult.Parried:
                    Focus.Add(ability, Focus.ParryGain(Focus.Parried));
                    break;
                case CombatCollisionResult.ChamberBlocked:
                    Focus.Add(ability, Focus.ParryGain(Focus.Chamber));
                    break;
            }
        }

        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            if (isCanceled || victim == null || !victim.IsMainAgent || attacker == null || !attacker.IsEnemyOf(victim)) return;
            if (!IsSwordmaster()) return;

            var ability = Focus.Of(victim);
            if (ability == null) return;

            if (collisionData.MissileBlockedWithWeapon)
                Focus.Add(ability, Focus.ParryGain(Focus.Deflected));
            else if (!collisionData.AttackBlockedWithShield && G.Has(G.Keystone(G.Storm)) && WayOfTheSwordScript.IsActiveFor(victim))
                Focus.Add(ability, -Focus.MissileHitCost);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectorAgent == null || !affectorAgent.IsMainAgent || affectedAgent == null) return;
            if (!affectedAgent.IsHuman || !affectedAgent.IsEnemyOf(affectorAgent) || !IsSwordmaster()) return;
            if (agentState != AgentState.Killed && agentState != AgentState.Unconscious) return;
            if (blow.IsMissile || !blow.IsValid || !IsMeleeWeapon(affectorAgent.WieldedWeapon) || blow.InflictedDamage <= 0) return;

            if (G.Has(G.Passive(G.SwordDancing, 4)) && affectedAgent.Character != null
                && CareerHelper.IsValidCareerMissionInteractionBetweenAgents(affectorAgent, affectedAgent))
                Hero.MainHero.AddSkillXp(TORSkills.Faith, FaithPerVictimLevel * affectedAgent.Character.Level);

        }

        private void AddBlock(CareerAbility ability, float amount)
        {
            var now = Mission.CurrentTime;
            if (now - _lastBlockGain < Focus.BlockCooldown) return;
            _lastBlockGain = now;
            Focus.Add(ability, amount);
        }

        private static bool IsBlockResult(CombatCollisionResult result) =>
            result == CombatCollisionResult.Blocked || result == CombatCollisionResult.Parried || result == CombatCollisionResult.ChamberBlocked;

        private static bool IsMeleeWeapon(MissionWeapon weapon)
        {
            var usage = weapon.CurrentUsageItem;
            return !weapon.IsEmpty && usage != null && !usage.IsRangedWeapon && !usage.IsConsumable;
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            WayOfTheSwordScript.Active = null;
        }
    }
}
