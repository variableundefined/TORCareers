using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORMercenaryCareerOverhaul.CampaignMechanics;
using TORMercenaryCareerOverhaul.Patches;

namespace TORMercenaryCareerOverhaul
{
    internal static class MercenaryCards
    {
        internal static readonly string RootDescription =
            "Never retreat! Never surrender! Inspire troops in an area. Allies within become 'Unbreakable' and "
            + "'Unstoppable' for " + Trim(Data.Duration) + "s. Gains " + Trim(Data.RadiusPerLeadership) + "m radius and "
            + Trim(Data.DurationPerLeadership) + "s duration per point of Leadership. Always affects yourself. "
            + Percent(Data.NonCasterDuration) + " duration if you are not a spellcaster. (" + Data.CoolDown + "s cooldown.)";

        private static readonly Dictionary<string, string> Descriptions =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MercenaryRoot"] = RootDescription,

            [MercenaryMissionLogic.MercenaryLordKeystone] =
                "Each of your kills shortens Let Them Have It!'s cooldown by " + Data.KillCooldown
                + "s. It also provides " + Percent(Data.Magnitude) + " reload speed.",

            [MercenaryMissionLogic.SurvivalistKeystone] =
                "Let Them Have It! also provides +15% 'Physical Resistance', and begins the battle ready.",

            [PartySpeed] =
                "+1 party move speed on campaign map.",

            [SurvivalistHealing] =
                "Personal healing rate increased by +1.",

            [MercenaryMissionLogic.DuelistCard] =
                "Kills in combat grant 'Leadership' experience.",

            [FactionResource] =
                "+100% 'Faction Resource' from battles. Gain another 100% if Leadership reaches 300.",

            [Data.CommanderKeystone] =
                "Let Them Have It!: +" + Trim(Data.CasterRadius) + "m radius for spellcasters, "
                + Percent(Data.CommanderBonus) + " (additive) to each bonus otherwise.",

            [Data.PaymasterKeystone] =
                "Let Them Have It! effects doubled. Non-spellcasters also gain "
                + Percent(Data.MagicResistance) + " spell resistance.",

            [TavernHiring.Card] =
                "Mercenaries in town taverns appear in greater numbers and cost 25% less.",
        };

        private const string PartySpeed = "SurvivalistPassive3";
        private const string SurvivalistHealing = "SurvivalistPassive4";
        private const string FactionResource = "MercenaryLordPassive4";

        internal static void Apply()
        {
            var career = TORCareers.Mercenary;
            if (career == null)
            {
                Log.Write("MercenaryCards: Mercenary career not found.");
                return;
            }

            var passive = AccessTools.Property(typeof(CareerChoiceObject), "Passive")
                          ?? throw new MissingMemberException("CareerChoiceObject", "Passive");

            var choices = career.AllChoices;
            if (career.RootNode != null) choices.Add(career.RootNode);

            foreach (var choice in choices) Fix(choice, passive);
        }

        private static void Fix(CareerChoiceObject choice, PropertyInfo passiveProperty)
        {
            var id = choice?.StringId;
            if (string.IsNullOrEmpty(id)) return;

            string description;
            if (!Descriptions.TryGetValue(id, out description)) return;

            try
            {
                ((PropertyObject)choice).Initialize(new TextObject(id), new TextObject(description));

                var passive = PassiveFor(id);
                if (passive != null) passiveProperty.SetValue(choice, passive, null);
            }
            catch (Exception ex)
            {
                Log.Write("MercenaryCards: " + id + ": " + ex.Message);
            }
        }

        private static CareerChoiceObject.PassiveEffect PassiveFor(string id)
        {
            switch (id)
            {
                case PartySpeed:
                    return new CareerChoiceObject.PassiveEffect(1f, PassiveEffectType.PartyMovementSpeed);
                case SurvivalistHealing:
                    return new CareerChoiceObject.PassiveEffect(1f, PassiveEffectType.HealthRegeneration);
                case MercenaryMissionLogic.DuelistCard:
                    return new CareerChoiceObject.PassiveEffect();
                default:
                    return null;
            }
        }

        private static string Trim(float value)
        {
            return value.ToString("0.##");
        }

        private static string Percent(float value)
        {
            return "+" + Math.Round(value * 100f) + "%";
        }
    }
}
