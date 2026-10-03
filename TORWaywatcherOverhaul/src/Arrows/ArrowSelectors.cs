using System;
using System.Linq;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TORWaywatcherOverhaul.Bootstrap;

namespace TORWaywatcherOverhaul.Arrows
{
    internal static class ArrowSelectors
    {
        internal static bool Add(Agent agent)
        {
            var component = agent?.GetComponent<AbilityComponent>();
            if (component == null) return false;

            var abilities = component.KnownAbilitySystem;
            foreach (var arrow in EnchantedArrow.All)
            {
                var id = EnchantedArrow.SelectorId(arrow);
                if (abilities.Any(x => x.Template?.StringID == id)) continue;

                var ability = AbilityFactory.CreateNew(id, agent);
                if (ability == null)
                {
                    Log.Warn("Missing ability template " + id + ".");
                    continue;
                }

                Reflection.WireCastEvents(component, ability);
                ability.OnCastComplete += OnSelectorCast;
                abilities.Add(ability);
            }
            return true;
        }

        private static void OnSelectorCast(Ability ability)
        {
            try
            {
                if (EnchantedArrow.TryParseSelector(ability.Template?.StringID, out var arrow))
                    Quiver.Load(arrow);
            }
            catch (Exception e)
            {
                Log.Error("Arrow selection failed: " + e.Message);
            }
        }
    }
}
