using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Extensions;
using static TOR_Core.CharacterDevelopment.CareerSystem.CareerChoiceObject;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Career
{
    internal class ImperialEngineerCareerChoices : TORCareerChoicesBase
    {
        internal const string Root = "ImperialEngineerRoot";

        private readonly Dictionary<string, CareerChoiceObject> _choices =
            new Dictionary<string, CareerChoiceObject>();

        internal ImperialEngineerCareerChoices(CareerObject id) : base(id) { }

        private CareerChoiceObject C(string id) { return _choices[id]; }

        protected override void RegisterAll()
        {
            Reg(Root);

            foreach (var g in G.All)
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

        private static PassiveEffect Pending() => new PassiveEffect(0f, PassiveEffectType.Special);

        private static PassiveEffect SelfDamage(DamageType type, float pct, AttackTypeMask mask) =>
            new PassiveEffect(PassiveEffectType.Damage, new DamageProportionTuple(type, pct), mask,
                (a, v, m) => a != null && a.IsMainAgent);

        private static PassiveEffect SelfResist(DamageType type, float pct) =>
            new PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => v != null && v.IsMainAgent);

        private static PassiveEffect GunpowderTroopDamage(DamageType type, float pct) =>
            new PassiveEffect(PassiveEffectType.TroopDamage, new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => a != null && a.BelongsToMainParty() && Firearms.IsGunpowderTroop(a));

        private static PassiveEffect GunpowderTroopResist(DamageType type, float pct) =>
            new PassiveEffect(PassiveEffectType.TroopResistance, new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => v != null && v.BelongsToMainParty() && Firearms.IsGunpowderTroop(v));

        private static PassiveEffect PartyResist(DamageType type, float pct) =>
            new PassiveEffect(PassiveEffectType.TroopResistance, new DamageProportionTuple(type, pct), AttackTypeMask.All,
                (a, v, m) => v != null && v.BelongsToMainParty());

        protected override void InitializePassives()
        {
            C(Root).Initialize(CareerID, string.Empty, string.Empty, true, ChoiceType.Passive, null, null);

            Passive(G.Logistics, 1, new PassiveEffect(5f, PassiveEffectType.Ammo));
            Passive(G.Logistics, 2, new PassiveEffect(-25f, PassiveEffectType.TroopWages, true,
                c => !c.IsHero && c.IsRanged));
            Passive(G.Logistics, 3, new PassiveEffect(-25f, PassiveEffectType.CustomResourceUpgradeCostModifier, true,
                c => !c.IsHero && (c.IsRanged || c.StringId.Contains("engineer"))));
            Passive(G.Logistics, 4, new PassiveEffect(1f, PassiveEffectType.PartyMovementSpeed));

            Passive(G.Customized, 1, new PassiveEffect(10f, PassiveEffectType.Health));
            Passive(G.Customized, 2, SelfDamage(DamageType.Physical, 10f, AttackTypeMask.Ranged));
            Passive(G.Customized, 3, Pending());
            Passive(G.Customized, 4, Pending());

            Passive(G.School, 1, SelfResist(DamageType.Fire, 25f));
            Passive(G.School, 2, new PassiveEffect(15f, PassiveEffectType.Health));
            Passive(G.School, 3, Pending());
            Passive(G.School, 4, Pending());

            Passive(G.Gunnery, 1, new PassiveEffect(20f, PassiveEffectType.PartySpottingRange, true));
            Passive(G.Gunnery, 2, GunpowderTroopDamage(DamageType.Physical, 10f));
            Passive(G.Gunnery, 3, Pending());
            Passive(G.Gunnery, 4, Pending());

            Passive(G.Cannons, 1, Pending());
            Passive(G.Cannons, 2, new PassiveEffect(25f, PassiveEffectType.InventoryCapacity, true));
            Passive(G.Cannons, 3, Pending());
            Passive(G.Cannons, 4, GunpowderTroopResist(DamageType.Physical, 10f));

            Passive(G.Leonardo, 1, new PassiveEffect(5f, PassiveEffectType.CompanionLimit));
            Passive(G.Leonardo, 2, PartyResist(DamageType.Fire, 20f));
            Passive(G.Leonardo, 3, SelfDamage(DamageType.Fire, 25f, AttackTypeMask.All));
            Passive(G.Leonardo, 4, Pending());

            Passive(G.Cavalcade, 1, Pending());
            Passive(G.Cavalcade, 2, GunpowderTroopDamage(DamageType.Physical, 15f));
            Passive(G.Cavalcade, 3, GunpowderTroopResist(DamageType.Physical, 10f));
            Passive(G.Cavalcade, 4, SelfResist(DamageType.Physical, 10f));
        }

        protected override void InitializeKeyStones()
        {
            foreach (var g in G.All)
                Keystone(g);
        }
    }
}
