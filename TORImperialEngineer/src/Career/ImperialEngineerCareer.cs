using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TORImperialEngineer.Abilities;

namespace TORImperialEngineer.Career
{
    internal static class ImperialEngineerCareer
    {
        internal const string Id = "ImperialEngineer";
        internal const string AbilityId = "ExperimentalMunition";

        private const int MaxCharge = 800;

        internal static CareerObject Career { get; private set; }

        internal static bool IsPlayer =>
            Career != null && Hero.MainHero != null && Hero.MainHero.HasCareer(Career);

        internal static CareerObject Create()
        {
            var career = Game.Current.ObjectManager
                .RegisterPresumedObject(new CareerObject(Id));

            career.Initialize(
                name: "Imperial Engineer",
                condition: null,
                abilityID: AbilityId,
                function: ImperialEngineerCharge.Charge,
                maxCharge: MaxCharge,
                abilityScriptType: typeof(ExperimentalMunitionScript));

            Career = career;
            return career;
        }
    }
}
