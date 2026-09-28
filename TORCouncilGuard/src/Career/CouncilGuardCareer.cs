using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TORCouncilGuard.Abilities;

namespace TORCouncilGuard.Career
{
    internal static class CouncilGuardCareer
    {
        internal const string Id = "CouncilGuard";
        internal const string AbilityId = "JudgementOfAsuryan";

        private const int MaxCharge = 1200;

        internal static CareerObject Career { get; private set; }

        internal static CareerObject Create()
        {
            var career = Game.Current.ObjectManager
                .RegisterPresumedObject(new CareerObject(Id));

            career.Initialize(
                name: "Council Guard",
                condition: null,
                abilityID: AbilityId,
                function: CouncilGuardCharge.Charge,
                maxCharge: MaxCharge,
                abilityScriptType: typeof(JudgementOfAsuryanScript));

            Career = career;
            return career;
        }
    }
}
