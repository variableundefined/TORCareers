using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    internal static class ExtraResistances
    {
        private const string PersonalFireEffect = "ie_personal_fire_resistance";
        private const string GunpowderTroopFireEffect = "ie_troop_fire_resistance";
        private const float BattleLong = 99999f;

        internal static void Apply(Agent agent)
        {
            if (agent == null || !agent.IsHuman || !ImperialEngineerCareer.IsPlayer) return;

            var hero = Hero.MainHero;

            if (agent.IsMainAgent)
            {
                if (hero.HasCareerChoice(G.Leonardo + "Passive2"))
                    agent.ApplyStatusEffect(PersonalFireEffect, agent, BattleLong);
                return;
            }

            if (hero.HasCareerChoice(G.Cannons + "Passive4") && agent.BelongsToMainParty() && Firearms.IsGunpowderTroop(agent))
                agent.ApplyStatusEffect(GunpowderTroopFireEffect, agent, BattleLong);
        }
    }
}
