using TaleWorlds.MountAndBlade;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Trance
{
    internal static class Riposte
    {
        private const float DamageBonus = 0.5f;
        private const float Duration = 10f;

        internal const string DamageEffect = "sm_riposte_dmg";

        private static bool _armed;

        internal static void Reset()
        {
            _armed = false;
        }

        internal static void Arm(Agent agent)
        {
            if (agent == null || !G.Has(G.Passive(G.Storm, 1))) return;

            _armed = true;
            Effects.Apply(agent, DamageEffect, DamageBonus, Duration, agent);
        }

        internal static void OnStrike(Agent agent)
        {
            if (!_armed) return;

            _armed = false;
            Effects.Remove(agent, DamageEffect);
        }
    }
}
