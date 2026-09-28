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
    internal class ImperialEngineerAgentStatCalculateModel : TORAgentStatCalculateModel
    {
        private const float PersonalMissileSpeed = 1.15f;
        private const float TroopInaccuracy = 0.90f;
        private const int PersonalPouchBonus = 6;
        private const int PersonalGrenadeBonus = 3;
        private const int TroopPouchBonus = 6;

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);

            if (agent == null || !agent.IsMainAgent || agentDrivenProperties == null) return;
            if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Customized + "Passive3")) return;

            agentDrivenProperties.MissileSpeedMultiplier *= PersonalMissileSpeed;
        }

        public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill)
        {
            var result = base.GetWeaponInaccuracy(agent, weapon, weaponSkill);

            if (agent == null || agent.IsMainAgent || weapon == null || !weapon.IsGunPowderWeapon()) return result;
            if (!ImperialEngineerCareer.IsPlayer || !Hero.MainHero.HasCareerChoice(G.Gunnery + "Passive3")) return result;
            if (!agent.BelongsToMainParty() || !Firearms.IsGunpowderTroop(agent)) return result;

            return result * TroopInaccuracy;
        }

        public override void InitializeMissionEquipment(Agent agent)
        {
            base.InitializeMissionEquipment(agent);

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
