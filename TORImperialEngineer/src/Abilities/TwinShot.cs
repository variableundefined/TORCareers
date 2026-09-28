using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TORImperialEngineer.Bootstrap;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    internal static class TwinShot
    {
        private const string TraitId = "ie_twin_shot";
        private const float BattleLong = 100000f;
        internal const float DamageFactor = 0.75f;

        private static bool Active =>
            ImperialEngineerCareer.IsPlayer && Hero.MainHero.HasCareerChoice(G.Cavalcade + "Passive1");

        internal static bool Fires(Agent agent, MissionWeapon weapon) =>
            agent != null && agent.IsMainAgent && Active && Ammo.Selected != AmmoType.Scatter
            && Firearms.IsSingleShotGun(weapon) && !Firearms.FiresScatter(weapon);

        internal static void Suspend(Agent agent)
        {
            var component = agent?.GetComponent<ItemTraitAgentComponent>();
            if (component == null) return;
            try
            {
                Reflection.RemoveWeaponTraits(component, new[] { TraitId });
            }
            catch (System.Exception e)
            {
                Log.Error("Could not suspend twin shot: " + e.Message);
            }
        }

        internal static void Arm(Agent agent)
        {
            if (!Active || agent == null || Ammo.Selected == AmmoType.Scatter) return;
            Suspend(agent);

            var component = agent.GetComponent<ItemTraitAgentComponent>();
            var trait = Munition.Find(TraitId);
            if (component == null || trait == null) return;

            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var weapon = agent.Equipment[i];
                if (Firearms.IsSingleShotGun(weapon))
                    component.AddTraitToWeapon(weapon, trait, BattleLong);
            }
        }
    }
}
