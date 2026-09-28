using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal static class BattleStartCharge
    {
        internal static void Apply(Agent agent)
        {
            try
            {
                if (agent == null || agent.GetHero() != Hero.MainHero) return;
                if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Logistics + "Keystone")) return;

                var ability = agent.GetComponent<AbilityComponent>()?.CareerAbility;
                if (ability?.Template == null || !ability.Template.StringID.StartsWith(ImperialEngineerCareer.AbilityId)) return;

                ability.AddCharge(ImperialEngineerCareer.Career.MaxCharge);
                ability.SetCoolDown(0);
            }
            catch (Exception e)
            {
                Log.Warn("Could not start Experimental Munition charged: " + e.Message);
            }
        }
    }
}
