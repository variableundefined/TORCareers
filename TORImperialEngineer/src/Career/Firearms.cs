using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;

namespace TORImperialEngineer.Career
{
    internal static class Firearms
    {
        private static readonly Dictionary<string, bool> GunpowderTroops = new Dictionary<string, bool>();

        internal static bool CanLoad(Agent agent)
        {
            if (agent == null) return false;

            var wielded = agent.WieldedWeapon;
            if (wielded.IsEmpty || wielded.Item == null) return false;

            var usage = wielded.CurrentUsageItem;
            if (usage == null || !usage.IsRangedWeapon || !usage.IsGunPowderWeapon()) return false;
            if (wielded.Item.IsExplosiveAmmunition()) return false;

            var loaded = wielded.AmmoWeapon;
            if (!loaded.IsEmpty && loaded.Item != null && loaded.Item.IsGrenadeAmmo()) return false;

            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var slot = agent.Equipment[i];
                if (slot.IsEmpty || slot.Item == null || slot.CurrentUsageItem == null) continue;
                if (!slot.CurrentUsageItem.IsAmmo || slot.CurrentUsageItem.WeaponClass != usage.AmmoClass) continue;
                if (slot.Amount > 0 && !slot.Item.IsGrenadeAmmo()) return true;
            }

            return !loaded.IsEmpty && loaded.Item != null;
        }

        internal static bool CarriesGun(Agent agent)
        {
            if (agent == null) return false;
            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var usage = agent.Equipment[i].CurrentUsageItem;
                if (usage != null && usage.IsRangedWeapon && usage.IsGunPowderWeapon()) return true;
            }
            return false;
        }

        internal static MissionWeapon FindGun(Agent agent)
        {
            if (CanLoad(agent)) return agent.WieldedWeapon;

            for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var weapon = agent.Equipment[i];
                if (weapon.IsEmpty || weapon.Item == null || weapon.CurrentUsageItem == null) continue;
                if (weapon.CurrentUsageItem.IsRangedWeapon && weapon.CurrentUsageItem.IsGunPowderWeapon()
                    && !weapon.Item.IsExplosiveAmmunition())
                    return weapon;
            }
            return MissionWeapon.Invalid;
        }

        internal static bool IsFiringGrenade(Agent shooter)
        {
            var wielded = shooter.WieldedWeapon;
            if (wielded.IsEmpty || wielded.Item == null) return false;
            if (wielded.Item.IsExplosiveAmmunition()) return true;

            var ammo = wielded.AmmoWeapon;
            return !ammo.IsEmpty && ammo.Item != null && ammo.Item.StringId.Contains("grenade");
        }

        internal static bool IsScatterAmmo(ItemObject item) =>
            item.StringId.Contains("scatter") || item.StringId.Contains("buckshot");

        internal static bool FiresScatter(MissionWeapon weapon)
        {
            var ammo = weapon.AmmoWeapon;
            return !ammo.IsEmpty && ammo.Item != null && IsScatterAmmo(ammo.Item);
        }

        internal static bool IsSingleShotGun(MissionWeapon weapon)
        {
            if (weapon.IsEmpty || weapon.Item == null || weapon.MaxAmmo != 1) return false;

            var usage = weapon.CurrentUsageItem;
            if (usage == null || !usage.IsRangedWeapon || !usage.IsGunPowderWeapon()) return false;

            return !weapon.Item.StringId.Contains("blunderbuss") && !IsGrenadeItem(weapon.Item);
        }

        internal static bool IsGrenadeItem(ItemObject item)
        {
            if (item == null) return false;
            return item.IsGrenadeAmmo() || item.IsExplosiveAmmunition() || item.StringId.Contains("grenade");
        }

        internal static bool IsEngineerTroop(CharacterObject character) =>
            character != null && !character.IsHero && character.StringId.Contains("engineer");

        internal static bool IsGunpowderTroop(CharacterObject character)
        {
            if (character == null || character.IsHero) return false;
            if (GunpowderTroops.TryGetValue(character.StringId, out var known)) return known;

            var result = character.BattleEquipments.Any(equipment =>
            {
                for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
                    if (equipment[i].Item != null && equipment[i].Item.IsGunPowderWeapon()) return true;
                return false;
            });

            GunpowderTroops[character.StringId] = result;
            return result;
        }

        internal static bool IsGunpowderTroop(Agent agent) =>
            agent?.Character is CharacterObject character && IsGunpowderTroop(character);
    }
}
