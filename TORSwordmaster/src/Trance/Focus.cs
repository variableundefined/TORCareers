using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment;
using TORSwordmaster.Bootstrap;
using TORSwordmaster.Career;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Trance
{
    internal static class Focus
    {
        internal const float PerDamage = 1f;
        internal const float MinHitGain = 20f;
        internal const float MaxHitGain = 100f;
        internal const float SwingGainCap = 120f;
        internal const float SwingWindow = 0.3f;
        internal const float Blocked = 80f;
        internal const float ShieldBlocked = 50f;
        internal const float Parried = 150f;
        internal const float Deflected = 300f;
        internal const float Chamber = 300f;
        internal const float StartingCharge = 500f;
        internal const float BlockCooldown = 1f;
        internal const float MissileHitCost = 30f;
        private const float StormBonus = 1.5f;

        private const float Drain = 25f;

        internal const float BaseBonus = 0.15f;
        private const float BonusPerSkillPoint = 0.0003f;

        internal static CareerAbility Of(Agent agent) =>
            agent?.GetComponent<AbilityComponent>()?.CareerAbility;

        internal static float Get(CareerAbility ability) =>
            (float)Reflection.CurrentCharge.GetValue(ability);

        internal static void Set(CareerAbility ability, float value) =>
            Reflection.CurrentCharge.SetValue(ability, Math.Max(0f, Math.Min(SwordmasterCareer.MaxCharge, value)));

        internal static void Add(CareerAbility ability, float amount) =>
            Set(ability, Get(ability) + amount);

        internal static void Fill(CareerAbility ability) =>
            Set(ability, SwordmasterCareer.MaxCharge);

        internal static float ParryGain(float amount) =>
            G.Has(G.Passive(G.Storm, 4)) ? amount * StormBonus : amount;

        internal static float DrainPerSecond() => Drain;

        internal static float HitGain(float damage) =>
            Math.Max(MinHitGain, Math.Min(MaxHitGain, damage * PerDamage));

        internal static float DamageBonus()
        {
            var hero = Hero.MainHero;
            if (hero == null) return BaseBonus;

            var bonus = BaseBonus + BonusPerSkillPoint * HighestMeleeSkill(hero);
            if (G.Has(G.Keystone(G.Heirloom)))
                bonus += BonusPerSkillPoint * hero.GetSkillValue(DefaultSkills.Leadership);
            return bonus;
        }

        internal static int HighestMeleeSkill(Hero hero) =>
            hero == null ? 0 : Math.Max(hero.GetSkillValue(DefaultSkills.OneHanded),
                Math.Max(hero.GetSkillValue(DefaultSkills.TwoHanded), hero.GetSkillValue(DefaultSkills.Polearm)));

        internal static float SwingBonus()
        {
            var hero = Hero.MainHero;
            if (hero == null || !G.Has(G.Keystone(G.ThirtyForms))) return BaseBonus;
            return BaseBonus + BonusPerSkillPoint * hero.GetSkillValue(TORSkills.Faith);
        }

        internal static bool HasMeleeWeapon(Agent agent)
        {
            if (agent == null) return false;
            var weapon = agent.WieldedWeapon;
            var usage = weapon.CurrentUsageItem;
            return !weapon.IsEmpty && usage != null && !usage.IsRangedWeapon && !usage.IsConsumable && !usage.IsShield;
        }

        internal static bool UsesShield(Agent agent)
        {
            if (agent == null) return false;
            var offhand = agent.GetOffhandWieldedItemIndex();
            return offhand != EquipmentIndex.None && agent.Equipment[offhand].CurrentUsageItem?.IsShield == true;
        }
    }
}
