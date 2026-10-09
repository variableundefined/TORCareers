using System;
using TaleWorlds.MountAndBlade;
using TORSwordmaster.Abilities;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Trance
{
    internal static class StormStacks
    {
        internal const float SwingPerStack = 0.03f;
        private const float ResistPerStack = 0.02f;
        private const float StackDuration = 10f;
        private const int MaxStacks = 5;

        internal const string SwingEffect = "sm_storm_swing";
        internal const string ResistEffect = "sm_storm_physres";

        private static int _stacks;
        private static float _expires;

        internal static void Reset()
        {
            _stacks = 0;
            _expires = 0f;
        }

        internal static void OnPerfectParry(Agent agent)
        {
            if (!G.Has(G.Passive(G.Storm, 1))) return;

            _stacks = Math.Min(MaxStacks, _stacks + 1);
            _expires = Mission.Current.CurrentTime + StackDuration;
            Refresh(agent);
        }

        internal static void Tick(Agent agent)
        {
            if (_stacks <= 0 || agent == null) return;

            var now = Mission.Current.CurrentTime;
            if (G.Has(G.Keystone(G.Storm)) && WayOfTheSwordScript.IsActiveFor(agent))
            {
                _expires = now + StackDuration;
                Refresh(agent);
                return;
            }

            if (now < _expires) return;

            _stacks = 0;
            Effects.Remove(agent, SwingEffect);
            Effects.Remove(agent, ResistEffect);
        }

        private static void Refresh(Agent agent)
        {
            var remaining = Math.Max(1f, _expires - Mission.Current.CurrentTime + 1f);
            Effects.Apply(agent, SwingEffect, _stacks * SwingPerStack, remaining, agent);
            Effects.Apply(agent, ResistEffect, _stacks * ResistPerStack, remaining, agent);
        }
    }
}
