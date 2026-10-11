using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TORMiscTweaks.Settings
{
    public class MiscTweaksSettings : AttributeGlobalSettings<MiscTweaksSettings>
    {
        public override string Id => "TORMiscTweaks_v1";
        public override string DisplayName => "TOR Misc Tweaks";
        public override string FolderName => "TORMiscTweaks";
        public override string FormatType => "json2";

        internal static MiscTweaksSettings Current => Instance ?? Defaults;
        private static readonly MiscTweaksSettings Defaults = new MiscTweaksSettings();

        [SettingPropertyBool("Improved Companion Search", IsToggle = true, Order = 0, RequireRestart = false,
            HintText = "Improved Companion Search with bigger radius for TOR companions.")]
        [SettingPropertyGroup("Companion Search", GroupOrder = 0)]
        public bool CompanionSearchEnabled { get; set; } = true;

        [SettingPropertyBool("Toggle Witch King Envoy menu in Town", IsToggle = true, Order = 0, RequireRestart = false,
            HintText = "Adds an option to access the Witch King prisoner sell menu immediately from the town menu.")]
        [SettingPropertyGroup("Eonir", GroupOrder = 1)]
        public bool WitchKingEnvoyMenuEnabled { get; set; } = true;

        [SettingPropertyBool("Sell all prisoners to Witch King Envoy", Order = 1, RequireRestart = false,
            HintText = "Quick access for people who are already grinding.")]
        [SettingPropertyGroup("Eonir", GroupOrder = 1)]
        public bool WitchKingEnvoySellAllEnabled { get; set; } = true;
    }
}
