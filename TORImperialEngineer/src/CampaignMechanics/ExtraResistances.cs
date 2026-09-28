using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.CampaignMechanics
{
    [HarmonyPatch(typeof(CareerHelper), nameof(CareerHelper.AddCareerPassivesForDamageValues))]
    internal static class ExtraResistances
    {
        private const float GunpowderTroopFire = 0.10f;
        private const float PersonalFire = 0.20f;

        [HarmonyPostfix]
        private static void Postfix(Agent victim, PropertyMask mask, float[] __result)
        {
            if (mask != PropertyMask.Defense || __result == null || victim == null) return;
            if (!ImperialEngineerCareer.IsPlayer) return;

            var hero = Hero.MainHero;

            if (victim.IsMainAgent)
            {
                if (hero.HasCareerChoice(G.Leonardo + "Passive2"))
                    __result[(int)DamageType.Fire] += PersonalFire;
                return;
            }

            if (hero.HasCareerChoice(G.Cannons + "Passive4") && Firearms.IsGunpowderTroop(victim))
                __result[(int)DamageType.Fire] += GunpowderTroopFire;
        }
    }
}
