using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORSwordmaster.Abilities;

namespace TORSwordmaster.Career
{
    internal static class SwordmasterCareer
    {
        internal const string Id = "Swordmaster";
        internal const string AbilityId = "WayOfTheSword";

        internal const int MaxCharge = 1000;

        internal static CareerObject Career { get; private set; }

        internal static CareerObject Create()
        {
            var career = Game.Current.ObjectManager
                .RegisterPresumedObject(new CareerObject(Id));

            career.Initialize(
                name: "Swordmaster",
                condition: null,
                abilityID: AbilityId,
                function: NoCharge,
                maxCharge: MaxCharge,
                abilityScriptType: typeof(WayOfTheSwordScript));

            Career = career;
            return career;
        }

        private static float NoCharge(Agent affectorAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue,
                                      AttackTypeMask mask, CareerHelper.ChargeCollisionFlag collisionFlag)
        {
            return 0f;
        }
    }
}
