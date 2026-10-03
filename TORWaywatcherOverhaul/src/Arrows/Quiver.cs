using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Extensions;

namespace TORWaywatcherOverhaul.Arrows
{
    internal static class Quiver
    {
        private const int BaseLethalArrows = 3;

        internal static ArrowType Loaded { get; private set; } = ArrowType.TrueflightArrow;

        internal static int LethalArrows { get; private set; }

        internal static bool IsLethal => LethalArrows > 0;

        internal static void Load(ArrowType arrow)
        {
            Loaded = arrow;
            var text = new TextObject("{=wwo_arrow_loaded}{ARROW} loaded.");
            text.SetTextVariable("ARROW", Name(arrow));
            InformationManager.DisplayMessage(new InformationMessage(text.ToString()));
        }

        internal static int LethalShotArrows(Hero hero)
        {
            var arrows = BaseLethalArrows;
            if (hero == null) return arrows;
            arrows += hero.GetAllCareerChoices().Count(x => x.Contains("Keystone") && !x.Contains("Root"));
            if (hero.HasCareerChoice(EnchantedArrow.ProtectorOfTheWoods)) arrows++;
            return arrows;
        }

        internal static void StartLethalShot(int arrows) => LethalArrows = arrows;

        internal static void ConsumeLethalArrow()
        {
            if (LethalArrows > 0) LethalArrows--;
        }

        internal static void EndLethalShot() => LethalArrows = 0;

        internal static TextObject Name(ArrowType arrow)
        {
            switch (arrow)
            {
                case ArrowType.SwiftshiverShards: return new TextObject("{=wwo_arrow_swiftshiver_name}Swiftshiver Shards");
                case ArrowType.TrueflightArrow: return new TextObject("{=wwo_arrow_trueflight_name}Trueflight Arrow");
                case ArrowType.ArcaneBodkin: return new TextObject("{=wwo_arrow_bodkin_name}Arcane Bodkin");
                case ArrowType.StarfireShaft: return new TextObject("{=wwo_arrow_starfire_name}Starfire Shaft");
                case ArrowType.MoonfireShot: return new TextObject("{=wwo_arrow_moonfire_name}Moonfire Shot");
                default: return new TextObject("{=wwo_arrow_hagbane_name}Hagbane Tips");
            }
        }
    }
}
