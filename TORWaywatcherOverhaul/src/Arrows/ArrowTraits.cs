using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TORWaywatcherOverhaul.Bootstrap;

namespace TORWaywatcherOverhaul.Arrows
{
    internal static class ArrowTraits
    {
        private const string Prefix = "wwo_";
        private const float Permanent = 100000f;

        internal static List<string> For(ArrowType arrow, int tier, bool lethal, Hero hero)
        {
            var traits = new List<string>();
            switch (arrow)
            {
                case ArrowType.SwiftshiverShards:
                    traits.Add("wwo_shard_damage");
                    break;
                case ArrowType.TrueflightArrow:
                    var damage = EnchantedArrow.TrueflightDamagePercent(tier);
                    if (damage > 0) traits.Add("wwo_trueflight_" + damage);
                    traits.Add("wwo_shield_pierce");
                    break;
                case ArrowType.ArcaneBodkin:
                    traits.Add("wwo_bodkin_fx");
                    traits.Add("wwo_bodkin_ap_" + EnchantedArrow.BodkinArmourPenetration(tier));
                    var bodkinDamage = EnchantedArrow.BodkinDamagePercent(tier);
                    if (bodkinDamage > 0) traits.Add("wwo_bodkin_damage_" + bodkinDamage);
                    break;
                case ArrowType.StarfireShaft:
                    traits.Add("wwo_strip_physical");
                    traits.Add("wwo_starfire_fire");
                    traits.Add("wwo_starfire_bonus_" + EnchantedArrow.StarfireFireBonusPercent(tier));
                    break;
                case ArrowType.MoonfireShot:
                    traits.Add("wwo_strip_physical");
                    traits.Add("wwo_moonfire_magic");
                    traits.Add("wwo_moonfire_bonus_" + EnchantedArrow.MoonfireMagicBonusPercent(tier));
                    break;
                case ArrowType.HagbaneTips:
                    traits.Add("wwo_hagbane_fx");
                    break;
            }
            if (lethal && EnchantedArrow.IsPiercing(arrow))
            {
                traits.Add("wwo_lethal_glow");
                if (tier >= 2 && arrow != ArrowType.SwiftshiverShards) traits.Add("wwo_target_pierce");
            }
            return traits;
        }

        internal static bool Apply(Agent agent, IReadOnlyCollection<string> traitIds)
        {
            var component = agent?.GetComponent<ItemTraitAgentComponent>();
            if (component == null) return false;

            var bows = Bows(agent).ToList();
            var bowItems = new HashSet<ItemObject>(bows.Select(x => x.Item));

            Reflection.DynamicTraits(component).RemoveAll(x =>
                x.Item2 != null && x.Item2.ItemTraitStringId.StartsWith(Prefix) && bowItems.Contains(x.Item1.Item));

            foreach (var id in traitIds)
            {
                var trait = ItemTrait.All.FirstOrDefault(x => x.ItemTraitStringId == id);
                if (trait == null)
                {
                    Log.Warn("Missing item trait " + id + ".");
                    continue;
                }
                foreach (var bow in bows)
                    component.AddTraitToWeapon(bow, trait, Permanent);
            }

            component.OnWieldedItemChanged();
            return true;
        }

        internal static bool IsBow(MissionWeapon weapon) =>
            !weapon.IsEmpty && weapon.CurrentUsageItem != null && weapon.CurrentUsageItem.WeaponClass == WeaponClass.Bow;

        private static IEnumerable<MissionWeapon> Bows(Agent agent)
        {
            for (var slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
            {
                var weapon = agent.Equipment[slot];
                if (IsBow(weapon)) yield return weapon;
            }
        }
    }
}
