using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;

namespace TORMercenaryCareerOverhaul.Patches
{
    internal static class MercenaryCards
    {
        private static readonly Dictionary<string, string> Descriptions =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MercenaryRoot"] =
                "Never retreat! Never surrender! Inspire troops in an area. Allies within become 'Unbreakable' and "
                + "'Unstoppable' for " + Trim(Data.Duration) + "s. Gains " + Trim(Data.RadiusPerLeadership) + "m radius and "
                + Trim(Data.DurationPerLeadership) + "s duration per point of Leadership. (" + Data.CoolDown + "s cooldown.)",

            [MercenaryLetThemHaveIt.MercenaryLordKeystone] =
                "Each of your kills shortens Let Them Have It!'s cooldown by " + Data.KillCooldown
                + "s. It also provides " + Percent(Data.Magnitude) + " reload speed.",

            [OpeningRally.Card] =
                "Let Them Have It! also provides +15% 'Physical Resistance', and begins the battle ready.",

            [PartySpeed] =
                "+1 party move speed on campaign map.",

            [SurvivalistHealing] =
                "Personal healing rate increased by +1.",

            [DuelistLeadership.Card] =
                "Kills in combat grant 'Leadership' experience.",

            [FactionResource] =
                "+100% 'Faction Resource' from battles. Gain another 100% if Leadership reaches 300.",

            [Data.CommanderKeystone] =
                "The base radius of Let Them Have It! is doubled.",

            [TavernHiring.Card] =
                "Mercenaries in town taverns appear in greater numbers and cost 25% less.",
        };

        private const string PartySpeed = "SurvivalistPassive3";
        private const string SurvivalistHealing = "SurvivalistPassive4";
        private const string FactionResource = "MercenaryLordPassive4";

        private static PropertyInfo _passive;

        internal static void Apply(Harmony harmony)
        {
            _passive = AccessTools.Property(typeof(CareerChoiceObject), "Passive")
                       ?? throw new MissingMemberException("CareerChoiceObject", "Passive");

            harmony.Patch(CareerReflection.ChoiceInitialize(),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(MercenaryCards), nameof(Fix))));
        }

        private static void Fix(CareerChoiceObject __instance)
        {
            if (__instance == null) return;

            var id = ((MBObjectBase)__instance).StringId;
            if (string.IsNullOrEmpty(id)) return;

            string description;
            if (!Descriptions.TryGetValue(id, out description)) return;

            try
            {
                ((PropertyObject)__instance).Initialize(new TextObject(id), new TextObject(description));

                var passive = PassiveFor(id);
                if (passive != null) _passive.SetValue(__instance, passive, null);
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
                case DuelistLeadership.Card:
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
