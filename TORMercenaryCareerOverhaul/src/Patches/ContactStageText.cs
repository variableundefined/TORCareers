using System;
using System.Reflection;
using HarmonyLib;
using TOR_Core.CampaignMechanics.CharacterCreation;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class ContactStageText
    {
        private const string Title = "Choose Your Contact";

        internal const string Effect =
            "Mercenaries from your contacts show up in addition to normal mercenary hire at the tavern "
            + "frequently. Each tier in your career improves the quality of the recruits you are sent.";

        private const string Description =
            "Every mercenary keeps contacts in at least one company they are on good terms with:";

        private static FieldInfo _dataSource;
        private static PropertyInfo _titleText;
        private static PropertyInfo _descriptionText;

        internal static void Apply(Harmony harmony)
        {
            var view = typeof(TORSpecializationStageView);

            _dataSource = AccessTools.Field(view, "_dataSource")
                          ?? throw new MissingFieldException(view.Name, "_dataSource");

            var vm = typeof(TORSpecializationStageVM);
            _titleText = AccessTools.Property(vm, "TitleText")
                         ?? throw new MissingFieldException(vm.Name, "TitleText");
            _descriptionText = AccessTools.Property(vm, "DescriptionText")
                               ?? throw new MissingFieldException(vm.Name, "DescriptionText");

            var initialize = AccessTools.Method(view, "InitializeUI")
                             ?? throw new MissingMethodException(view.Name, "InitializeUI");

            harmony.Patch(initialize, postfix: new HarmonyMethod(AccessTools.Method(typeof(ContactStageText), nameof(Retitle))));

        }

        private static void Retitle(object __instance, TORCharacterCreationContentHandler handler)
        {
            try
            {
                if (handler == null) return;

                var profession = handler.GetSelectedProfessionId();
                if (string.IsNullOrEmpty(profession)) return;
                if (!MercenaryOrigins.MercenaryProfessions.Contains(profession)) return;

                var source = _dataSource.GetValue(__instance);
                if (source == null) return;

                _titleText.SetValue(source, Title);
                _descriptionText.SetValue(source, Description);
            }
            catch (Exception ex)
            {
                Log.Write("ContactStageText: " + ex.Message);
            }
        }
    }
}
