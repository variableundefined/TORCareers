using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul
{
    internal static class LetThemHaveItScaling
    {
        private static readonly AccessTools.FieldRef<CareerChoiceObject, List<CareerChoiceObject.MutationObject>> MutationsOf =
            AccessTools.FieldRefAccess<CareerChoiceObject, List<CareerChoiceObject.MutationObject>>("_mutations");

        internal static void Apply()
        {
            var career = TORCareers.Mercenary;
            var root = career?.RootNode;
            if (root == null)
            {
                Log.Write("LetThemHaveItScaling: Mercenary root node not found.");
                return;
            }

            var mutations = new List<CareerChoiceObject.MutationObject>
            {
                new CareerChoiceObject.MutationObject
                {
                    MutationTargetType = typeof(TriggeredEffectTemplate),
                    MutationTargetOriginalId = Data.EffectId,
                    PropertyName = "Radius",
                    PropertyValue = (choice, original, agent) => LeadershipRadius(agent),
                    MutationType = OperationType.Add,
                },
                new CareerChoiceObject.MutationObject
                {
                    MutationTargetType = typeof(TriggeredEffectTemplate),
                    MutationTargetOriginalId = Data.EffectId,
                    PropertyName = "ImbuedStatusEffectDuration",
                    PropertyValue = (choice, original, agent) => ExtraDuration(agent, (float)original),
                    MutationType = OperationType.Add,
                },
                new CareerChoiceObject.MutationObject
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = Data.AbilityId,
                    PropertyName = "TargetCapturingRadius",
                    PropertyValue = (choice, original, agent) => RingRadius(agent, (float)original),
                    MutationType = OperationType.Replace,
                },
            };

            foreach (var id in Data.Bonuses)
            {
                var bonus = id;
                mutations.Add(new CareerChoiceObject.MutationObject
                {
                    MutationTargetType = typeof(StatusEffectTemplate),
                    MutationTargetOriginalId = bonus,
                    PropertyName = "BaseEffectValue",
                    PropertyValue = (choice, original, agent) => Bonus(agent, bonus, (float)original),
                    MutationType = OperationType.Replace,
                });
            }

            root.Initialize(career, MercenaryCards.RootDescription, null, true, ChoiceType.Keystone, mutations);

            Rewire(career, Data.PaymasterKeystone, new CareerChoiceObject.MutationObject
            {
                MutationTargetType = typeof(TriggeredEffectTemplate),
                MutationTargetOriginalId = Data.EffectId,
                PropertyName = "ImbuedStatusEffects",
                PropertyValue = (choice, original, agent) => MagicResistance(agent, (List<string>)original),
                MutationType = OperationType.Replace,
            });
            Rewire(career, Data.CommanderKeystone);
        }

        private static void Rewire(CareerObject career, string id, params CareerChoiceObject.MutationObject[] mutations)
        {
            var choice = career.AllChoices.FirstOrDefault(c => c.StringId == id);
            var list = choice == null ? null : MutationsOf(choice);
            if (list == null)
            {
                Log.Write("LetThemHaveItScaling: " + id + " not found.");
                return;
            }

            list.Clear();
            list.AddRange(mutations);
        }

        private static float Bonus(Agent caster, string id, float original)
        {
            var hero = PlayersHero(caster);
            return hero == null ? original : Data.BonusValue(id, hero);
        }

        private static List<string> MagicResistance(Agent caster, List<string> original)
        {
            var hero = PlayersHero(caster);
            if (hero == null || Data.IsCaster(hero) || original.Contains(Data.MagicResistanceId)) return original;
            return original.Concat(new[] { Data.MagicResistanceId }).ToList();
        }

        private static float LeadershipRadius(Agent caster)
        {
            var hero = PlayersHero(caster);
            if (hero == null) return 0f;

            return Data.CommanderRadius(hero) + Data.LeadershipRadius(hero.CharacterObject);
        }

        private static float ExtraDuration(Agent caster, float original)
        {
            var hero = PlayersHero(caster);
            if (hero == null) return 0f;

            var leadership = Data.LeadershipDuration(hero.CharacterObject);
            if (Data.IsCaster(hero)) return leadership;
            return leadership + (original + leadership) * Data.NonCasterDuration;
        }

        private static float RingRadius(Agent caster, float original)
        {
            var hero = PlayersHero(caster);
            return hero == null ? original : Data.RingRadius(hero);
        }

        private static Hero PlayersHero(Agent caster)
        {
            var hero = caster?.GetHero();
            if (hero == null || hero != Hero.MainHero) return null;
            return hero.HasCareer(TORCareers.Mercenary) ? hero : null;
        }
    }
}
