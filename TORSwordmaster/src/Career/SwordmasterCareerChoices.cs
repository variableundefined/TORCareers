using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORSwordmaster.Economy;
using TORSwordmaster.Trance;
using static TOR_Core.CharacterDevelopment.CareerSystem.CareerChoiceObject;
using G = TORSwordmaster.Career.SwordmasterChoiceGroups;

namespace TORSwordmaster.Career
{
    internal class SwordmasterCareerChoices : TORCareerChoicesBase
    {
        internal const string RootId = "SwordmasterRoot";
        internal const string CompanionAttribute = "SwordmasterCompanion";

        private readonly Dictionary<string, CareerChoiceObject> _choices =
            new Dictionary<string, CareerChoiceObject>();

        internal SwordmasterCareerChoices(CareerObject id) : base(id) { }

        private CareerChoiceObject C(string id) => _choices[id];

        protected override void RegisterAll()
        {
            Reg(RootId);
            foreach (var (id, _, _) in G.Groups)
            {
                for (var i = 1; i <= 4; i++) Reg(G.Passive(id, i));
                Reg(G.Keystone(id));
            }
        }

        private void Reg(string id)
        {
            _choices[id] = Game.Current.ObjectManager
                .RegisterPresumedObject(new CareerChoiceObject(id));
        }

        private void Passive(string group, int n, PassiveEffect effect)
        {
            C(G.Passive(group, n)).Initialize(
                CareerID, string.Empty, group, false, ChoiceType.Passive, null, effect);
        }

        private void Keystone(string group)
        {
            C(G.Keystone(group)).Initialize(
                CareerID, string.Empty, group, false, ChoiceType.Keystone, new List<MutationObject>(), null);
        }

        private static PassiveEffect Custom(float value) => new PassiveEffect(value);

        private static PassiveEffect SelfDamage(DamageType type, float pct, AttackTypeMask mask = AttackTypeMask.Melee) =>
            new PassiveEffect(PassiveEffectType.Damage, new DamageProportionTuple(type, pct), mask,
                (a, v, m) => a != null && a.IsMainAgent);

        private static PassiveEffect SelfResist(DamageType type, float pct, AttackTypeMask mask = AttackTypeMask.All,
                                                PassiveEffect.SpecialCombatInteractionFunction extra = null) =>
            new PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(type, pct), mask,
                (a, v, m) => v != null && v.IsMainAgent && (extra == null || extra(a, v, m)));

        private static bool IsMeleeTroop(Agent agent) =>
            agent != null && agent.BelongsToMainParty() && !agent.IsMainAgent && !agent.IsHero
            && agent.Character != null && !agent.Character.IsRanged;

        private static bool IsSwordmasterCompanion(Agent agent) =>
            agent != null && agent.IsHero && !agent.IsMainAgent && agent.BelongsToMainParty()
            && agent.GetHero() != null && agent.GetHero().HasAttribute(CompanionAttribute);

        private static PassiveEffect TroopCharacter(float value, PassiveEffectType type, bool meleeOnly) =>
            new PassiveEffect(value, type, true,
                c => c != null && !c.IsHero && (!meleeOnly || !c.IsRanged));

        protected override void InitializePassives()
        {
            C(RootId).Initialize(CareerID, string.Empty, string.Empty, true, ChoiceType.Passive, null,
                new PassiveEffect(-25f, PassiveEffectType.TroopWages, true,
                    c => c != null && !c.IsHero && MartialTraining.Has(c, MartialTraining.Discipline)));

            var d = G.SwordDancing;
            Passive(d, 1, new PassiveEffect(20f, PassiveEffectType.Health));
            Passive(d, 2, new PassiveEffect(10f, PassiveEffectType.SwingSpeed, true));
            Passive(d, 3, SelfDamage(DamageType.Physical, 15f));
            Passive(d, 4, Custom(SwordmasterBattleLogic.FaithPerVictimLevel));

            var h = G.Heirloom;
            Passive(h, 1, new PassiveEffect(-20f, PassiveEffectType.ArmorPenetration, true));
            Passive(h, 2, Custom(15f));
            Passive(h, 3, Custom(SwordmasterBattleLogic.HealthPerEnchantedItem));
            Passive(h, 4, SelfDamage(DamageType.Physical, 10f));

            var t = G.ThirtyForms;
            Passive(t, 1, new PassiveEffect(20f, new List<string> { "OneHanded", "TwoHanded" }, c => c != null && !c.IsHero));
            Passive(t, 2, Custom(SwordmasterCampaignBehavior.MeleeTroopDailyXp));
            Passive(t, 3, TroopCharacter(-33f, PassiveEffectType.CustomResourceUpgradeCostModifier, true));
            Passive(t, 4, TroopCharacter(-20f, PassiveEffectType.TroopWages, true));

            var s = G.Storm;
            Passive(s, 1, Custom(50f));
            Passive(s, 2, SelfResist(DamageType.Physical, 20f, AttackTypeMask.Ranged,
                (a, v, m) => m == AttackTypeMask.Ranged && !Focus.UsesShield(v)));
            Passive(s, 3, new PassiveEffect(20f, PassiveEffectType.ShruggedOff));
            Passive(s, 4, Custom(50f));

            var o = G.SwordOfHoeth;
            Passive(o, 1, new PassiveEffect(5f, PassiveEffectType.CompanionLimit));
            Passive(o, 2, new PassiveEffect(PassiveEffectType.TroopResistance,
                new DamageProportionTuple(DamageType.Physical, 10f), AttackTypeMask.All,
                (a, v, m) => IsMeleeTroop(v)));
            Passive(o, 3, new PassiveEffect(PassiveEffectType.TroopDamage,
                new DamageProportionTuple(DamageType.Physical, 15f), AttackTypeMask.Melee,
                (a, v, m) => m == AttackTypeMask.Melee && IsSwordmasterCompanion(a)));
            Passive(o, 4, Custom(SwordmasterCampaignBehavior.CompanionDailyXp));

            var b = G.Bladelord;
            Passive(b, 1, new PassiveEffect(30f, PassiveEffectType.Health));
            Passive(b, 2, SelfResist(DamageType.Physical, 10f));
            Passive(b, 3, new PassiveEffect(PassiveEffectType.TroopDamage,
                new DamageProportionTuple(DamageType.Physical, 15f), AttackTypeMask.Melee,
                (a, v, m) => m == AttackTypeMask.Melee && IsMeleeTroop(a)));
            Passive(b, 4, Custom(0.1f));

            var r = G.Ritual;
            Passive(r, 1, Custom(1f));
            Passive(r, 2, SelfResist(DamageType.Magical, 20f));
            Passive(r, 3, Custom(20f));
            Passive(r, 4, Custom(10f));
        }

        protected override void InitializeKeyStones()
        {
            foreach (var (id, _, _) in G.Groups)
                Keystone(id);
        }
    }
}
