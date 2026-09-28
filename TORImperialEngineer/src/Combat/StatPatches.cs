using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TOR_Core.Models;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Combat
{
    [HarmonyPatch(typeof(TORAgentStatCalculateModel), nameof(TORAgentStatCalculateModel.UpdateAgentStats))]
    internal static class MissileSpeedPatch
    {
        private const float PersonalMissileSpeed = 1.15f;

        [HarmonyPostfix]
        private static void Postfix(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (agent == null || !agent.IsMainAgent || agentDrivenProperties == null) return;
            if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Customized + "Passive3")) return;

            agentDrivenProperties.MissileSpeedMultiplier *= PersonalMissileSpeed;
        }
    }

    [HarmonyPatch(typeof(TORAgentStatCalculateModel), nameof(TORAgentStatCalculateModel.GetWeaponInaccuracy))]
    internal static class TroopAccuracyPatch
    {
        private const float TroopInaccuracy = 0.90f;

        [HarmonyPostfix]
        private static void Postfix(Agent agent, WeaponComponentData weapon, ref float __result)
        {
            if (agent == null || agent.IsMainAgent || weapon == null || !weapon.IsGunPowderWeapon()) return;
            if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Gunnery + "Passive3")) return;
            if (!agent.BelongsToMainParty() || !Firearms.IsGunpowderTroop(agent)) return;

            __result *= TroopInaccuracy;
        }
    }
}
