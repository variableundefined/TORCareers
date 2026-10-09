using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;
using TOR_Core.Extensions;
using static Helpers.PartyScreenHelper;

namespace TORSwordmaster.Economy
{
    internal class MartialTraining : CareerButtonBehaviorBase
    {
        internal const int FavorCost = 125;

        internal sealed class Path
        {
            internal string Id { get; }
            internal string Name { get; }
            internal string Description { get; }
            internal string TriggeredEffect { get; }

            internal Path(string id, string name, string description, string triggeredEffect)
            {
                Id = id;
                Name = name;
                Description = description;
                TriggeredEffect = triggeredEffect;
            }
        }

        internal static readonly Path[] Paths =
        {
            new Path("SmPathRain", "Path of the Rain", "25% Physical Ranged Resistance, 10% Movement Speed", "sm_mt_rain"),
            new Path("SmPathHawk", "Path of the Hawk", "10% Swing Speed", "sm_mt_hawk"),
            new Path("SmPathFrost", "Path of Frost", "15% Physical Damage", "sm_mt_frost"),
        };

        internal static MartialTraining Instance { get; } = new MartialTraining();

        private CharacterObject _character;

        private MartialTraining() : base(null) { }

        private static List<Path> Current(CharacterObject character) =>
            CareerButtonHelper.GetCurrentActiveItems(character, Paths, p => p.Id);

        public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false, bool shiftClick = false)
        {
            _character = characterObject;
            var canPay = Hero.MainHero.GetCultureSpecificCustomResourceValue() >= FavorCost;

            var elements = Paths
                .Select(p => new InquiryElement(p, p.Name, (ImageIdentifier)null, canPay, p.Description))
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
            return characterObject != null && !isPrisoner && !characterObject.IsHero && !characterObject.IsRanged;
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
                if (ids.Contains(path.Id))
                    CareerHelper.AddDefaultPermanentMissionEffect(agent, path.TriggeredEffect);
        }
    }
}
