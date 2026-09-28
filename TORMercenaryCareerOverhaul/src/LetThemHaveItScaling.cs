using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TORMercenaryCareerOverhaul
{
    internal static class LetThemHaveItScaling
    {
        internal static void Apply()
        {
            var career = TORCareers.Mercenary;
            var root = career?.RootNode;
            if (root == null)
            {
                Log.Write("LetThemHaveItScaling: Mercenary root node not found.");
                return;
            }

            // Root mutations run before keystone ones and CommanderKeystone replaces Radius with x2,
            // so halve the Leadership share for Commander to keep the effect radius at base x2 + 0.05/pt.
            root.Initialize(career, MercenaryCards.RootDescription, null, true, ChoiceType.Keystone,
                new List<CareerChoiceObject.MutationObject>
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
                        PropertyValue = (choice, original, agent) => LeadershipDuration(agent),
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
                });
        }

        private static float LeadershipRadius(Agent caster)
        {
            var hero = PlayersHero(caster);
            if (hero == null) return 0f;

            var radius = Data.LeadershipRadius(hero.CharacterObject);
            if (hero.HasCareerChoice(Data.CommanderKeystone)) radius /= 2f;
            return radius;
        }

        private static float LeadershipDuration(Agent caster)
        {
            var hero = PlayersHero(caster);
            return hero == null ? 0f : Data.LeadershipDuration(hero.CharacterObject);
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
