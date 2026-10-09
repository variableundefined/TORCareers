using TaleWorlds.MountAndBlade;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Trance
{
    internal static class FinalStroke
    {
        private const int Strikes = 2;
        private const float DamageBonus = 1.5f;
        private const float Duration = 600f;

        internal const string UnblockableEffect = "doom_seeking_unblockable";
        internal const string DamageEffect = "sm_final_dmg";

        private static int _remaining;

        internal static void Reset()
        {
            _remaining = 0;
        }

        internal static void Arm(Agent agent)
        {
            if (agent == null || !G.Has(G.Keystone(G.Bladelord))) return;

            _remaining = Strikes;
            Effects.Apply(agent, UnblockableEffect, Duration, agent);
            Effects.Apply(agent, DamageEffect, DamageBonus, Duration, agent);
        }

        internal static void OnStrike(Agent agent)
        {
            if (_remaining <= 0) return;

            _remaining--;
            if (_remaining > 0) return;

            Effects.Remove(agent, UnblockableEffect);
            Effects.Remove(agent, DamageEffect);
        }
    }
}
