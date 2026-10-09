using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.AbilitySystem.Scripts;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Trance;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Abilities
{
    public class WayOfTheSwordScript : CareerAbilityScript
    {
        internal const string DamageEffect = "sm_wots_dmg";
        internal const string SwingEffect = "sm_wots_swing";
        internal const string CleaveEffect = "sm_wots_cleave";
        internal const string PhysicalResistEffect = "sm_wots_physres";
        internal const string RangedResistEffect = "sm_wots_rangeres";
        internal const string SpeedEffect = "sm_wots_speed";
        internal const string AllyDamageEffect = "sm_ally_dmg";
        internal const string AllySwingEffect = "sm_ally_swing";

        private const float BuffDuration = 2f;
        private const float RefreshInterval = 0.5f;
        private const float AllyRadius = 5f;
        private const float RitualPhysicalResist = 0.10f;
        private const float StormRangedResist = 0.50f;
        private const float CooldownResetInterval = 60f;

        internal static WayOfTheSwordScript Active { get; set; }
        internal static float LastCooldownReset { get; set; } = float.MinValue;

        private bool _started;
        private float _refresh;

        internal static bool IsActiveFor(Agent agent) =>
            Active != null && !Active.IsFading && Active.CasterAgent == agent;

        protected override void OnBeforeTick(float dt)
        {
            base.OnBeforeTick(dt);
            if (IsFading) return;

            var caster = CasterAgent;
            var ability = Ability as CareerAbility;
            if (caster == null || ability == null) return;

            if (!_started)
            {
                _started = true;
                if (!Begin(caster, ability)) return;
            }

            Focus.Add(ability, -Focus.DrainPerSecond() * dt);
            if (Focus.Get(ability) <= 0f)
            {
                End(caster);
                return;
            }

            _refresh -= dt;
            if (_refresh <= 0f)
            {
                _refresh = RefreshInterval;
                Buff(caster);
            }
        }

        private bool Begin(Agent caster, CareerAbility ability)
        {
            if (IsActiveFor(caster) && Active != this)
            {
                Active.End(caster);
                Focus.Fill(ability);
                Stop();
                return false;
            }

            if (!Focus.HasMeleeWeapon(caster))
            {
                Focus.Fill(ability);
                Stop();
                if (caster.IsMainAgent)
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject(WayOfTheSwordRestriction.NeedsMeleeWeapon).ToString(), Colors.Red));
                return false;
            }

            Active = this;
            Focus.Fill(ability);

            if (G.Has(G.Passive(G.Ritual, 1)))
                Effects.Cleanse(caster);

            if (G.Has(G.Keystone(G.Ritual)))
                ResetTechniqueCooldowns(caster);

            Buff(caster);
            return true;
        }

        private void End(Agent caster)
        {
            foreach (var id in new[] { DamageEffect, SwingEffect, CleaveEffect, PhysicalResistEffect, RangedResistEffect, SpeedEffect })
                Effects.Remove(caster, id);

            if (Active == this) Active = null;
            Stop();
            FinalStroke.Arm(caster);
        }

        private static void Buff(Agent caster)
        {
            var damage = Focus.DamageBonus();
            var swing = Focus.SwingBonus();
            Effects.Apply(caster, DamageEffect, damage, BuffDuration, caster);
            Effects.Apply(caster, SwingEffect, swing, BuffDuration, caster);

            if (!caster.HasMount)
                Effects.Apply(caster, SpeedEffect, BuffDuration, caster);

            if (G.Has(G.Keystone(G.SwordDancing)))
                Effects.Apply(caster, CleaveEffect, BuffDuration, caster);

            if (G.Has(G.Passive(G.Ritual, 4)))
                Effects.Apply(caster, PhysicalResistEffect, RitualPhysicalResist, BuffDuration, caster);

            if (G.Has(G.Keystone(G.Storm)))
                Effects.Apply(caster, RangedResistEffect, StormRangedResist, BuffDuration, caster);

            if (G.Has(G.Keystone(G.ThirtyForms)))
                BuffAllies(caster, damage, swing);
        }

        private static void BuffAllies(Agent caster, float damage, float swing)
        {
            var allies = Mission.Current.GetNearbyAllyAgents(caster.Position.AsVec2, AllyRadius, caster.Team, new MBList<Agent>());
            foreach (var ally in (List<Agent>)(object)allies)
            {
                if (ally == null || ally == caster || !ally.IsHuman || !ally.IsActive()) continue;
                Effects.Apply(ally, AllyDamageEffect, damage, BuffDuration, caster);
                Effects.Apply(ally, AllySwingEffect, swing, BuffDuration, caster);
            }
        }

        private static void ResetTechniqueCooldowns(Agent caster)
        {
            var now = Mission.Current.CurrentTime;
            if (now - LastCooldownReset < CooldownResetInterval) return;
            LastCooldownReset = now;

            var component = caster.GetComponent<AbilityComponent>();
            if (component == null) return;

            foreach (var technique in component.KnownAbilitySystem)
                if (technique is Technique)
                    Reflection.CooldownEndTime.SetValue(technique, 0f);
        }
    }
}
