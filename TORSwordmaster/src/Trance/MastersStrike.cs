using System;
using TaleWorlds.MountAndBlade;
using TORSwordmaster.Abilities;

namespace TORSwordmaster.Trance
{
    internal static class MastersStrike
    {
        private const int Strikes = 2;
        private const float Duration = 600f;

        internal const string UnblockableEffect = "doom_seeking_unblockable";

        private static int _remaining;

        internal static void Reset()
        {
            _remaining = 0;
        }

        internal static void Arm(Agent agent)
        {
            if (agent == null) return;

            _remaining = Strikes;
            Effects.Apply(agent, UnblockableEffect, Duration, agent);
        }

        internal static void OnStrike(Agent agent, Agent victim, in Blow blow)
        {
            if (_remaining <= 0) return;

            var magnitude = (int)blow.BaseMagnitude;
            var armorLoss = Math.Max(0, magnitude - blow.InflictedDamage);
            TechniqueEffects.MastersStrikeHit(agent, victim, armorLoss, magnitude);

            _remaining--;
            if (_remaining <= 0) Effects.Remove(agent, UnblockableEffect);
        }
    }
}
