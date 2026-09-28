using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem.Scripts;
using TOR_Core.Extensions;
using TOR_Core.Items;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Abilities
{
    public class ExperimentalMunitionScript : CareerAbilityScript
    {
        internal const string VolleyReloadEffect = "ie_gunnery_reload";
        internal const string LeonardoReloadEffect = "ie_leonardo_reload";
        internal const string LeonardoDamageEffect = "ie_leonardo_damage";

        private const float VolleyRadius = 10f;
        private const float VolleyDuration = 10f;

        private bool _loaded;

        protected override void OnBeforeTick(float dt)
        {
            base.OnBeforeTick(dt);
            if (_loaded && !IsFading && Munition.Remaining(CasterAgent) <= 0)
                Stop();
        }

        protected override void OnInit()
        {
            base.OnInit();

            var caster = CasterAgent;
            var hero = Hero.MainHero;
            if (caster == null || hero == null || Mission.Current == null) return;

            var duration = Ability.Template.Duration;
            var shots = Munition.Shots(hero);

            Munition.Load(caster, hero, shots, duration);
            _loaded = true;

            if (hero.HasCareerChoice(G.Leonardo + "Passive4"))
                LoadCompanions(caster, hero, shots, duration);
        }

        internal static void Pulse(Agent caster)
        {
            var hero = Hero.MainHero;
            if (caster == null || hero == null || !caster.IsMainAgent) return;

            if (hero.HasCareerChoice(G.Gunnery + "Keystone"))
                Volley(caster, VolleyRadius, VolleyReloadEffect, null);
            if (hero.HasCareerChoice(G.Leonardo + "Keystone"))
                Volley(caster, VolleyRadius, LeonardoReloadEffect, LeonardoDamageEffect);
        }

        private static void LoadCompanions(Agent caster, Hero hero, int shots, float duration)
        {
            foreach (var agent in Mission.Current.Agents)
            {
                if (agent == caster || !agent.IsActive() || !agent.IsHero || agent.Team != caster.Team) continue;
                if (!agent.BelongsToMainParty() || !Firearms.CanLoad(agent)) continue;

                Munition.Load(agent, hero, shots, duration);
            }
        }

        private static void Volley(Agent caster, float radius, string reload, string damage)
        {
            var allies = Mission.Current.GetNearbyAllyAgents(caster.Position.AsVec2, radius, caster.Team, new MBList<Agent>());
            foreach (var ally in allies)
            {
                if (ally == null || ally == caster || !ally.IsActive()) continue;
                if (!Firearms.IsGunpowderTroop(ally) && !(ally.IsHero && ally.BelongsToMainParty() && Firearms.CarriesGun(ally))) continue;

                ally.ApplyStatusEffect(reload, caster, VolleyDuration, append: false);
                if (damage != null)
                    ally.ApplyStatusEffect(damage, caster, VolleyDuration, append: false);
            }
        }
    }
}
