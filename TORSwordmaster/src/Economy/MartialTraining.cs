using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Items;
using static Helpers.PartyScreenHelper;

namespace TORSwordmaster.Economy
{
    internal class MartialTraining : CareerButtonBehaviorBase
    {
        internal const int FavorCost = 25;

        internal const string Discipline = "SmDisciplineOfTheTower";
        internal const string Flame = "SmFlameOfAsuryan";

        private const string FlameTrait = "sm_flame_of_asuryan";
        private const float FlameFireDamage = 0.20f;
        private const float Permanent = 99999f;

        internal sealed class Path
        {
            internal string Id { get; }
            internal string Name { get; }
            internal string Description { get; }
            internal string TriggeredEffect { get; }
            internal int Tier { get; }

            internal Path(string id, string name, string description, string triggeredEffect, int tier = 1)
            {
                Id = id;
                Name = name;
                Description = description;
                TriggeredEffect = triggeredEffect;
                Tier = tier;
            }
        }

        internal static readonly Path[] Paths =
        {
            new Path("SmPathOfTheRain", "Path of the Rain", "25% Physical Ranged Resistance, 10% Movement Speed", "sm_mt_path_of_the_rain"),
            new Path("SmPathOfTheHawk", "Path of the Hawk", "10% Swing Speed", "sm_mt_path_of_the_hawk"),
            new Path("SmPathOfFrost", "Path of Frost", "15% Physical Damage", "sm_mt_path_of_frost"),
            new Path("SmPathOfTheMountain", "Path of the Mountain", "15% Physical Melee Resistance", "sm_mt_path_of_the_mountain"),
            new Path(Discipline, "Discipline of the Tower", "25% Wage reduction", null, 2),
            new Path("SmWardOfHoeth", "Ward of Hoeth", "30% Magical, Fire and Lightning Resistance", "sm_mt_ward_of_hoeth", 2),
            new Path("SmMartialProwess", "Martial Prowess", "Gains 'Cleave' on Attack", "sm_mt_martial_prowess", 3),
            new Path("SmUnyieldingForm", "Unyielding Form", "Unstoppable", "sm_mt_unyielding_form", 3),
            new Path("SmGraceOfAsuryan", "Grace of Asuryan", "15% Resistance to every damage type", "sm_mt_grace_of_asuryan", 3),
            new Path(Flame, "Flame of Asuryan", "+20% 'Fire' damage", null, 3),
        };

        internal static MartialTraining Instance { get; } = new MartialTraining();

        private CharacterObject _character;

        private MartialTraining() : base(null) { }

        public override string CareerButtonIcon => "favor_icon_45";

        private static List<Path> Current(CharacterObject character) =>
            CareerButtonHelper.GetCurrentActiveItems(character, Paths, p => p.Id);

        internal static bool Has(CharacterObject character, string pathId) =>
            character != null && CareerButtonHelper.GetTroopAttributeIds(character).Contains(pathId);

        private static bool IsUnlocked(Path path) =>
            path.Tier <= 1 || Hero.MainHero.HasUnlockedCareerChoiceTier(path.Tier);

        public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false, bool shiftClick = false)
        {
            _character = characterObject;
            var canPay = Hero.MainHero.GetCultureSpecificCustomResourceValue() >= FavorCost;

            var elements = Paths
                .Where(IsUnlocked)
                .Select(p =>
                {
                    var eligible = characterObject.IsHero || characterObject.Tier >= p.Tier;
                    var hint = eligible ? p.Description : p.Description + " (requires tier " + p.Tier + " troops)";
                    return new InquiryElement(p, p.Name, (ImageIdentifier)null, canPay && eligible, hint);
                })
                .ToList();

            var current = Current(characterObject);
            if (current != null && current.Any())
                elements.Add(CareerButtonHelper.CreateRemoveOption(string.Join(", ", current.Select(p => p.Name))));

            var title = TORTextHelper.GetText("sm_martial_training_title", "Martial Training");
            var description = new TextObject("{=sm_martial_training_description}Train this unit for {COST} Favor.");
            description.SetTextVariable("COST", FavorCost);

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                title, description.ToString(), elements, true, 1, 1,
                TORTextHelper.GetText("tor_inquiry_accept_text", "Accept"),
                TORTextHelper.GetText("tor_inquiry_cancel_text", "Cancel"),
                OnSelected, _ => { }, "", false), false, false);
        }

        private void OnSelected(List<InquiryElement> elements)
        {
            CareerButtonHelper.ProcessSelection(_character, elements, Current(_character), p => p.Id,
                _ => Hero.MainHero.AddCultureSpecificCustomResource(-FavorCost));
        }

        public override bool ShouldButtonBeVisible(CharacterObject characterObject, bool isPrisoner = false)
        {
            if (GetActivePartyState().PartyScreenMode != PartyScreenMode.Normal) return false;
            if (characterObject == null || isPrisoner) return false;
            if (characterObject.IsHero)
                return characterObject.HeroObject != Hero.MainHero && characterObject.HeroObject.PartyBelongedTo == MobileParty.MainParty;
            return !characterObject.IsRanged;
        }

        public override bool ShouldButtonBeActive(CharacterObject characterObject, out TextObject displayText, bool isPrisoner = false)
        {
            _character = characterObject;

            var current = Current(characterObject);
            if (current != null && current.Any())
            {
                displayText = new TextObject(string.Join("\n", current.Select(p => p.Name + ": " + p.Description)));
                return true;
            }

            displayText = new TextObject("{=sm_martial_training_accept}Train this unit for {COST} Favor.");
            displayText.SetTextVariable("COST", FavorCost);
            return Hero.MainHero.GetCultureSpecificCustomResourceValue() >= FavorCost;
        }

        internal static void ApplyInBattle(Agent agent)
        {
            if (!(agent.Character is CharacterObject character)) return;

            var ids = CareerButtonHelper.GetTroopAttributeIds(character);
            if (ids.Count == 0) return;

            foreach (var path in Paths)
            {
                if (!ids.Contains(path.Id)) continue;
                if (path.TriggeredEffect != null) CareerHelper.AddDefaultPermanentMissionEffect(agent, path.TriggeredEffect);
                if (path.Id == Flame) ApplyFlame(agent);
            }
        }

        private static void ApplyFlame(Agent agent)
        {
            var component = agent.GetComponent<ItemTraitAgentComponent>();
            var trait = GetFlameTrait();
            if (component != null && trait != null) component.AddTraitToWieldedWeapon(trait, Permanent);
        }

        private static ItemTrait GetFlameTrait()
        {
            var traits = ItemTrait.All;
            var trait = traits.FirstOrDefault(t => t.ItemTraitStringId == FlameTrait);
            if (trait != null) return trait;

            var template = traits.FirstOrDefault(t => t.ItemTraitStringId == "flaming_weapon");
            if (template == null) return null;

            trait = (ItemTrait)Activator.CreateInstance(typeof(ItemTrait), true);
            trait.ItemTraitStringId = FlameTrait;
            trait.ItemTraitName = "Flame of Asuryan";
            trait.ItemTraitDescription = "Adds extra 20% fire damage.";
            trait.IconName = template.IconName;
            trait.ImbuedStatusEffectId = "none";
            trait.AdditionalDamageTuple = new DamageProportionTuple(DamageType.Fire, FlameFireDamage);
            trait.WeaponParticlePreset = template.WeaponParticlePreset;
            traits.Add(trait);
            return trait;
        }
    }
}
