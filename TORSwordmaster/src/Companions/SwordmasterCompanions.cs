using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOR_Core.Extensions;
using TORSwordmaster.Abilities;
using TORSwordmaster.Career;

namespace TORSwordmaster.Companions
{
    internal static class SwordmasterCompanions
    {
        internal const string TemplateId = "tor_sm_swordmaster_companion";
        internal const int GoldCost = 50000;
        internal const int FavorCost = 500;

        private static readonly string[] Abilities =
        {
            Technique.Phoenix, Technique.Loec, Technique.Sun, Technique.FallingWater,
        };

        private static readonly EquipmentIndex[] ArmourSlots =
        {
            EquipmentIndex.Head, EquipmentIndex.Body, EquipmentIndex.Leg, EquipmentIndex.Gloves, EquipmentIndex.Cape,
        };

        internal static bool CanAfford() =>
            Hero.MainHero.Gold >= GoldCost && Hero.MainHero.GetCultureSpecificCustomResourceValue() >= FavorCost;

        internal static bool HasCompanionRoom() =>
            Clan.PlayerClan.Companions.Count < Clan.PlayerClan.CompanionLimit;

        internal static Hero CreateCandidate()
        {
            var template = MBObjectManager.Instance.GetObject<CharacterObject>(TemplateId);
            if (template == null)
            {
                Log.Error(TemplateId + " not found; no companion created.");
                return null;
            }

            var settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;
            var hero = HeroCreator.CreateSpecialHero(template, settlement, null, null, 30);

            hero.AddAttribute("AbilityUser");
            hero.AddAttribute("AICompanion");
            hero.AddAttribute(SwordmasterCareerChoices.CompanionAttribute);
            foreach (var ability in Abilities)
                hero.AddAbility(ability);

            Rust(hero.BattleEquipment);
            Rust(hero.CivilianEquipment);
            return hero;
        }

        internal static void Hire(Hero hero)
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, GoldCost, true);
            Hero.MainHero.AddCultureSpecificCustomResource(-FavorCost);

            hero.SetNewOccupation((Occupation)31);
            AddCompanionAction.Apply(Clan.PlayerClan, hero);
            AddHeroToPartyAction.Apply(hero, MobileParty.MainParty, true);
        }

        internal static void Discard(Hero hero)
        {
            if (hero != null && hero.IsAlive)
                KillCharacterAction.ApplyByRemove(hero);
        }

        private static void Rust(Equipment equipment)
        {
            var objects = MBObjectManager.Instance;
            var sword = objects.GetObject<ItemModifier>("rusty_sword");
            var plate = objects.GetObject<ItemModifier>("rusty_plate");

            var weapon = equipment[EquipmentIndex.Weapon0];
            if (!weapon.IsEmpty && sword != null)
                equipment[EquipmentIndex.Weapon0] = new EquipmentElement(weapon.Item, sword);

            if (plate == null) return;
            foreach (var slot in ArmourSlots)
            {
                var element = equipment[slot];
                if (!element.IsEmpty)
                    equipment[slot] = new EquipmentElement(element.Item, plate);
            }
        }
    }
}
