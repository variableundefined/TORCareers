using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Trance;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Abilities
{
    internal class Technique : Prayer
    {
        internal const string Phoenix = "FlightOfThePhoenix";
        internal const string Loec = "ShadowsOfLoec";
        internal const string Sun = "PathOfTheSun";
        internal const string FallingWater = "PathOfFallingWater";
        internal const string Master = "MastersStrike";

        private const float RitualCooldownFactor = 0.8f;

        internal static readonly Dictionary<string, float> Costs = new Dictionary<string, float>
        {
            { Phoenix, 100f },
            { Loec, 100f },
            { Sun, 400f },
            { FallingWater, 500f },
            { Master, 500f },
        };

        private readonly float _cost;

        internal Technique(AbilityTemplate template, float cost) : base(template)
        {
            _cost = cost;
        }

        public override bool IsDisabled(Agent casterAgent, out TextObject disabledReason)
        {
            if (base.IsDisabled(casterAgent, out disabledReason)) return true;
            if (casterAgent == null || !casterAgent.IsMainAgent) return false;

            if (StringID == Master && !G.Has(G.Keystone(G.Bladelord)))
            {
                disabledReason = new TextObject("{=sm_requires_bladelord}Needs the Bladelord keystone!");
                return true;
            }

            if ((StringID == Loec || StringID == FallingWater) && casterAgent.HasMount)
            {
                disabledReason = new TextObject("{=tor_career_ability_not_usable_mounted}Not usable mounted");
                return true;
            }

            if (!WayOfTheSwordScript.IsActiveFor(casterAgent))
            {
                disabledReason = new TextObject("{=sm_requires_trance}Needs Way of the Sword!");
                return true;
            }

            var ability = Focus.Of(casterAgent);
            if (ability == null || Focus.Get(ability) < _cost)
            {
                disabledReason = new TextObject("{=sm_not_enough_charge}Not enough charge!");
                return true;
            }

            return false;
        }

        public override void ActivateAbility(Agent casterAgent)
        {
            var others = casterAgent?.GetComponent<AbilityComponent>()?.KnownAbilitySystem
                .Where(a => a != this && a is Prayer)
                .ToDictionary(a => a, a => Reflection.CooldownEndTime.GetValue(a));

            base.ActivateAbility(casterAgent);

            if (G.Has(G.Keystone(G.Ritual)))
            {
                var now = Mission.Current.CurrentTime;
                var end = (float)Reflection.CooldownEndTime.GetValue(this);
                Reflection.CooldownEndTime.SetValue(this, now + (end - now) * RitualCooldownFactor);
            }

            if (others != null)
                foreach (var pair in others)
                    Reflection.CooldownEndTime.SetValue(pair.Key, pair.Value);

            var ability = Focus.Of(casterAgent);
            if (ability != null) Focus.Add(ability, -_cost);

            TechniqueEffects.OnPlayerCast(StringID, casterAgent);
        }
    }
}
