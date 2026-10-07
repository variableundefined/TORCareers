using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TOR_Core.Models;
using TORWaywatcherOverhaul.Arrows;

namespace TORWaywatcherOverhaul.Patches
{
    [HarmonyPatch(typeof(TORAgentApplyDamageModel), nameof(TORAgentApplyDamageModel.DecideMissileWeaponFlags))]
    internal static class StarfireEssenceShieldPiercing
    {
        [HarmonyPostfix]
        private static void Postfix(Agent attackerAgent, in MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
        {
            if ((missileWeaponFlags & WeaponFlags.CanPenetrateShield) == 0) return;
            if (attackerAgent == null || !attackerAgent.IsMainAgent || Campaign.Current == null) return;

            var hero = Hero.MainHero;
            if (hero == null || !hero.HasCareer(TORCareers.Waywatcher) || !hero.HasCareerChoice(Companions.SharedQuiver)) return;
            if (HasOtherShieldPiercing(attackerAgent, missileWeapon)) return;

            missileWeaponFlags &= ~WeaponFlags.CanPenetrateShield;
        }

        private static bool HasOtherShieldPiercing(Agent agent, MissionWeapon missile)
        {
            if (missile.IsEmpty || missile.CurrentUsageItem == null) return false;
            if ((missile.CurrentUsageItem.WeaponFlags & WeaponFlags.CanPenetrateShield) != 0) return true;
            if (agent.HasAttribute("ShieldPenetration")) return true;

            var traits = new List<ItemTrait>();
            if (!agent.WieldedWeapon.IsEmpty && agent.WieldedWeapon.Item != null)
                traits.AddRange(agent.WieldedWeapon.Item.GetTraits(agent));
            if (missile.Item != null)
                traits.AddRange(missile.Item.GetTraits(agent));
            return traits.Any(t => t.StatsTuple?.StatType == ItemTraitStatType.ShieldPenetration);
        }
    }
}
