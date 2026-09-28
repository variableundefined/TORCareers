using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TOR_Core.CampaignMechanics.CharacterCreation;
using TORMercenaryCareerOverhaul.CampaignMechanics;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryOrigins
    {
        private const string MercenaryEffectText = "Mercenary Career";
        private const int ProfessionStage = 3;

        private static FieldInfo _optionsField;
        private static FieldInfo _specializationsField;

        internal static readonly HashSet<string> MercenaryProfessions = new HashSet<string>(StringComparer.Ordinal);

        internal static void Apply(Harmony harmony)
        {
            var handler = typeof(TORCharacterCreationContentHandler);

            _optionsField = AccessTools.Field(handler, "_options")
                            ?? throw new MissingFieldException(handler.Name, "_options");
            _specializationsField = AccessTools.Field(handler, "_specializationOptions")
                                    ?? throw new MissingFieldException(handler.Name, "_specializationOptions");

            var has = AccessTools.Method(handler, "HasSpecializationOptions")
                      ?? throw new MissingMethodException(handler.Name, "HasSpecializationOptions");
            var get = AccessTools.Method(handler, "GetSpecializationOptions")
                      ?? throw new MissingMethodException(handler.Name, "GetSpecializationOptions");
            var set = AccessTools.Method(handler, "SetSelectedSpecializationOptionId")
                      ?? throw new MissingMethodException(handler.Name, "SetSelectedSpecializationOptionId");

            var ensure = new HarmonyMethod(AccessTools.Method(typeof(MercenaryOrigins), nameof(Ensure)));
            harmony.Patch(has, prefix: ensure);
            harmony.Patch(get, prefix: ensure);
            harmony.Patch(set, postfix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryOrigins), nameof(Record))));

        }

        private static void Ensure(object __instance)
        {
            try
            {
                var specializations = _specializationsField.GetValue(__instance) as List<SpecializationOption>;
                if (specializations == null) return;
                if (specializations.Any(s => s != null && s.Id != null && s.Id.StartsWith(Contacts.OptionPrefix))) return;

                var options = _optionsField.GetValue(__instance) as List<CharacterCreationOption>;
                if (options == null) return;

                var professions = options
                    .Where(o => o != null
                                && o.StageNumber == ProfessionStage
                                && !string.IsNullOrEmpty(o.Id)
                                && o.PositiveEffectText != null
                                && o.PositiveEffectText.Contains(MercenaryEffectText))
                    .ToList();

                if (professions.Count == 0)
                {
                    Log.Write("MercenaryContactOrigins: no mercenary professions found.");
                    return;
                }

                var added = 0;
                foreach (var profession in professions)
                {
                    MercenaryProfessions.Add(profession.Id);

                    foreach (var company in Contacts.All)
                    {
                        if (!Contacts.AllowedFor(company, profession.Culture)) continue;
                        added++;

                        specializations.Add(new SpecializationOption
                        {
                            Id = Contacts.OptionIdFor(company),
                            ProfessionRequirement = profession.Id,
                            Name = company.Name,
                            Description = company.Blurb,
                            PositiveEffect = ContactStageText.Effect,
                            NegativeEffect = "",
                            SkillsToIncrease = new string[0],
                            AttributesToIncrease = new string[0],
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Write("MercenaryContactOrigins: " + ex.Message);
            }
        }

        private static void Record(string optionId)
        {
            try
            {
                var company = Contacts.ByOptionId(optionId);
                if (company == null) return;

                MercenaryContacts.Sign(company.Id);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryContactOrigins: " + ex.Message);
            }
        }
    }
}
