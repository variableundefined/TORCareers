using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TOR_Core.Models;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Combat
{
    [HarmonyPatch(typeof(TORAgentStatCalculateModel), nameof(TORAgentStatCalculateModel.InitializeMissionEquipment))]
    internal static class AmmoPatches
    {
        private const int PersonalPouchBonus = 6;
        private const int PersonalGrenadeBonus = 3;
        private const int TroopPouchBonus = 6;

        [HarmonyPostfix]
        private static void Postfix(Agent agent)
        {
            if (agent == null || !agent.IsHuman || agent.Origin is SummonedAgentOrigin) return;
            if (Mission.Current == null || Mission.Current.IsArenaMission()) return;
            if (!ImperialEngineerCareer.IsPlayer) return;

            var hero = Hero.MainHero;
            var personal = agent.IsMainAgent && hero.HasCareerChoice(G.School + "Passive3");
            var troop = !agent.IsMainAgent && hero.HasCareerChoice(G.Gunnery + "Passive4")
                && agent.BelongsToMainParty() && Firearms.IsGunpowderTroop(agent);
            if (!personal && !troop) return;

            var equipment = agent.Equipment;
            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var slot = equipment[i];
                if (slot.IsEmpty || slot.Item == null) continue;

                var usage = slot.CurrentUsageItem;
                if (usage == null || usage.RelevantSkill == null) continue;
                if (!usage.IsAmmo && usage.AmmoClass != WeaponClass.Stone) continue;

                var grenade = Firearms.IsGrenadeItem(slot.Item);
                int bonus;
                if (personal) bonus = grenade ? PersonalGrenadeBonus : PersonalPouchBonus;
                else if (!grenade) bonus = TroopPouchBonus;
                else continue;

                equipment.SetAmountOfSlot(i, (short)(slot.Amount + bonus), true);
            }
        }
    }
}
