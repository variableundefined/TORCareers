using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TORCouncilGuard.Bootstrap;
using static TOR_Core.CharacterDevelopment.CareerSystem.CareerChoiceObject;

namespace TORCouncilGuard.Career
{
    internal class CouncilGuardCareerChoices : TORCareerChoicesBase
    {
        private readonly Dictionary<string, CareerChoiceObject> _choices =
            new Dictionary<string, CareerChoiceObject>();

        internal CouncilGuardCareerChoices(CareerObject id) : base(id) { }

        private CareerChoiceObject C(string id) { return _choices[id]; }

        protected override void RegisterAll()
        {
            Reg("CouncilGuardRoot");

            var groups = new[]
            {
                CouncilGuardChoiceGroups.Toriour, CouncilGuardChoiceGroups.Guardian,
                CouncilGuardChoiceGroups.SilverTower, CouncilGuardChoiceGroups.Forge,
                CouncilGuardChoiceGroups.Senate, CouncilGuardChoiceGroups.Temple,
                CouncilGuardChoiceGroups.Blades,
            };

            foreach (var g in groups)
            {
                for (var i = 1; i <= 4; i++) Reg(g + "Passive" + i);
                Reg(g + "Keystone");
            }
        }

        private void Reg(string id)
        {
            _choices[id] = Game.Current.ObjectManager
                .RegisterPresumedObject(new CareerChoiceObject(id));
        }

        private void Passive(string group, int n, PassiveEffect effect)
        {
            C(group + "Passive" + n).Initialize(
                CareerID, string.Empty, group, false, ChoiceType.Passive, null, effect);
        }

        private void Keystone(string group)
        {
            C(group + "Keystone").Initialize(
                CareerID, string.Empty, group, false, ChoiceType.Keystone, new List<MutationObject>(), null);
        }

        private static PassiveEffect SelfDamage(DamageType type, float pct,
            AttackTypeMask mask = AttackTypeMask.Melee)
        {
            return new PassiveEffect(PassiveEffectType.Damage,
                new DamageProportionTuple(type, pct), mask,
                (a, v, m) => a != null && a.IsMainAgent);
        }

        private static PassiveEffect SelfResist(DamageType type, float pct,
            AttackTypeMask mask = AttackTypeMask.All)
        {
            return new PassiveEffect(PassiveEffectType.Resistance,
                new DamageProportionTuple(type, pct), mask,
                (a, v, m) => v != null && v.IsMainAgent);
        }

        private static PassiveEffect TroopDamage(DamageType type, float pct)
        {
            return new PassiveEffect(PassiveEffectType.TroopDamage,
                new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => a != null && a.BelongsToMainParty() && CitybornFilter.IsBuffTarget(a));
        }

        private static PassiveEffect TroopResist(DamageType type, float pct)
        {
            return new PassiveEffect(PassiveEffectType.TroopResistance,
                new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => v != null && v.BelongsToMainParty() && CitybornFilter.IsBuffTarget(v));
        }

        private static PassiveEffect TroopOnly(float value, PassiveEffectType type)
        {
            return new PassiveEffect(value, type, true,
                c => CitybornFilter.IsBuffTarget(c));
        }

        protected override void InitializePassives()
        {
            C("CouncilGuardRoot").Initialize(CareerID, string.Empty, string.Empty, true, ChoiceType.Passive, null, null);

            var t = CouncilGuardChoiceGroups.Toriour;
            Passive(t, 1, TroopOnly(-25f, PassiveEffectType.TroopWages));
            Passive(t, 2, TroopOnly(-30f, PassiveEffectType.CustomResourceUpgradeCostModifier));
            Passive(t, 3, new PassiveEffect(5f, PassiveEffectType.CustomResourceGain, false));
            Passive(t, 4, new PassiveEffect(50f, PassiveEffectType.BattleRenownGain, true));

            var g = CouncilGuardChoiceGroups.Guardian;
            Passive(g, 1, new PassiveEffect(20f, PassiveEffectType.Health));
            Passive(g, 2, SelfDamage(DamageType.Physical, 10f));
            Passive(g, 3, new PassiveEffect(10f, PassiveEffectType.SwingSpeed, true));
            Passive(g, 4, new PassiveEffect(CouncilGuardFaith.XpPerVictimLevel));

            var s = CouncilGuardChoiceGroups.SilverTower;
            Passive(s, 1, TroopResist(DamageType.All, 10f));
            Passive(s, 2, new PassiveEffect(2f, PassiveEffectType.TroopRegeneration));
            Passive(s, 3, TroopOnly(10f, PassiveEffectType.SwingSpeed));
            Passive(s, 4, new PassiveEffect(20f,
                    new List<string> { "Polearm", "TwoHanded" },
                    c => CitybornFilter.IsBuffTarget(c)));

            var f = CouncilGuardChoiceGroups.Forge;
            Passive(f, 1, new PassiveEffect(20f, PassiveEffectType.ArmorPenetration, true));
            Passive(f, 2, SelfDamage(DamageType.Fire, 10f));
            Passive(f, 3, TroopResist(DamageType.Fire, 25f));
            Passive(f, 4, new PassiveEffect(-25f, PassiveEffectType.EnchantmentCostReduction, true));

            var n = CouncilGuardChoiceGroups.Senate;
            Passive(n, 1, new PassiveEffect(35f, PassiveEffectType.Health));
            Passive(n, 2, new PassiveEffect(15f, PassiveEffectType.ShruggedOff));
            Passive(n, 3, new PassiveEffect(10f, PassiveEffectType.MovementSpeed, true));
            Passive(n, 4, new PassiveEffect(1f, PassiveEffectType.PartyMovementSpeed));

            var k = CouncilGuardChoiceGroups.Temple;
            Passive(k, 1, TroopDamage(DamageType.Fire, 10f));
            Passive(k, 2, new PassiveEffect(1f));
            Passive(k, 3, new PassiveEffect(PassiveEffectType.MoraleDamageToEnemyOnKill,
                    new DamageProportionTuple(DamageType.All, 15f), AttackTypeMask.All,
                    (a, v, m) => a != null && a.BelongsToMainParty() && CitybornFilter.IsBuffTarget(a)));
            Passive(k, 4, TroopDamage(DamageType.Physical, 10f));

            var b = CouncilGuardChoiceGroups.Blades;
            Passive(b, 1, new PassiveEffect(15f, PassiveEffectType.SwingSpeed, true));
            Passive(b, 2, new PassiveEffect(-25f, PassiveEffectType.EquipmentWeightReduction, true));
            Passive(b, 3, SelfDamage(DamageType.Physical, 15f));
            Passive(b, 4, SelfResist(DamageType.Fire, 30f));
        }

        protected override void InitializeKeyStones()
        {
            Keystone(CouncilGuardChoiceGroups.Toriour);
            Keystone(CouncilGuardChoiceGroups.Guardian);
            Keystone(CouncilGuardChoiceGroups.SilverTower);
            Keystone(CouncilGuardChoiceGroups.Forge);
            Keystone(CouncilGuardChoiceGroups.Senate);
            Keystone(CouncilGuardChoiceGroups.Temple);
            Keystone(CouncilGuardChoiceGroups.Blades);
        }
    }
}
