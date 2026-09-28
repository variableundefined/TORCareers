using System.Collections.Generic;
using System.Linq;

namespace TORMercenaryCareerOverhaul
{
    internal sealed class Company
    {
        internal string Id;
        internal string Name;
        internal string Blurb;
        internal string[] Tiers;
        internal string[] Restricted = new string[0];
    }

    internal static class Contacts
    {
        internal const string OptionPrefix = "tor_merc_contact_";

        internal static readonly List<Company> All = new List<Company>
        {
            new Company
            {
                Id = "braganza",
                Name = "Braganza's Besiegers",
                Blurb = "Lead by Luka Braganza the Braganza's Besiegers are a famous band of Dogs of War "
                        + "mercenaries that hails from the Tilean city-state of Miragliano, trained and "
                        + "maintained as heavily armored Crossbowmen.",
                Tiers = new[] { "tor_dog_braganza_01", "tor_dog_braganza_02", "tor_dog_braganza_03" },
                Restricted = new[] { "vlandia" },
            },
            new Company
            {
                Id = "miragliano",
                Name = "Marksmen of Miragliano",
                Blurb = "The Marksmen of Miragliano are a famous band of Dogs of War mercenaries whose "
                        + "origins hail from the Tilean city-state of Miragliano, as their name implies.",
                Tiers = new[] { "tor_dog_miragliano_01", "tor_dog_miragliano_02", "tor_dog_miragliano_03" },
                Restricted = new[] { "vlandia" },
            },
            new Company
            {
                Id = "leopard",
                Name = "Leopold's Leopard Company",
                Blurb = "Leopold's Leopard Company is a famous band of Dogs of War mercenaries that has its "
                        + "origins within the great city-state of Luccini, and takes its name from the "
                        + "legendary Leopard of Luccini.",
                Tiers = new[] { "tor_dog_leopard_01", "tor_dog_leopard_02", "tor_dog_leopard_03" },
                Restricted = new[] { "vlandia" },
            },
            new Company
            {
                Id = "ricco",
                Name = "Ricco's Republican Guard",
                Blurb = "Ricco's Republican Guard are a famous band of Dogs of War mercenaries hailing from "
                        + "the great Republican city-state of Remas, raised by a rebellious group of merchants.",
                Tiers = new[] { "tor_dog_ricco_01", "tor_dog_ricco_02", "tor_dog_ricco_03" },
                Restricted = new[] { "vlandia" },
            },
            new Company
            {
                Id = "voland",
                Name = "Voland's Venators",
                Blurb = "Voland's Venators are a famous band of Dogs of War mercenaries whose reputation as "
                        + "the best cavalrymen in all of Tilea exceeds that of even veteran knights of the "
                        + "Empire, or some say, even Bretonnia itself. These are renegades, exiles, and "
                        + "disposed sons of nobles and lords.",
                Tiers = new[] { "tor_dog_voland_01", "tor_dog_voland_02", "tor_dog_voland_03" },
                Restricted = new[] { "vlandia" },
            },
            new Company
            {
                Id = "muktar",
                Name = "Al Muktar's Desert Dogs",
                Blurb = "Al Muktar's Desert Dogs are a famous band of Dogs of War mercenaries that hail from "
                        + "the arid deserts of great Araby, experienced raiders and skirmishers who ride as "
                        + "light cavalry.",
                Tiers = new[] { "tor_dog_muktar_01", "tor_dog_muktar_02", "tor_dog_muktar_03" },
                Restricted = new[] { "vlandia" },
            },
        };

        internal static Company ById(string id)
        {
            return All.FirstOrDefault(c => c.Id == id);
        }

        internal static Company ByOptionId(string optionId)
        {
            if (string.IsNullOrEmpty(optionId) || !optionId.StartsWith(OptionPrefix)) return null;
            return ById(optionId.Substring(OptionPrefix.Length));
        }

        internal static string OptionIdFor(Company company)
        {
            return OptionPrefix + company.Id;
        }

        internal static bool AllowedFor(Company company, string culture)
        {
            if (company == null) return false;
            if (string.IsNullOrEmpty(culture)) return true;
            return !company.Restricted.Contains(culture);
        }
    }
}
