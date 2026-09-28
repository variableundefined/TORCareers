using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TORImperialEngineer.Career;
using G = TORImperialEngineer.Career.ImperialEngineerChoiceGroups;

namespace TORImperialEngineer.Combat
{
    internal static class Artillery
    {
        internal const int ExtraSlots = 2;
        internal static readonly string[] FreePieces = { "GreatCannonSpawner", "GreatCannonSpawner" };

        internal static bool Active =>
            ImperialEngineerCareer.IsPlayer && Hero.MainHero.HasCareerChoice(G.Cannons + "Passive3");
    }

    [HarmonyPatch(typeof(HeroExtensions), nameof(HeroExtensions.GetPlaceableArtilleryCount))]
    internal static class ArtillerySlotsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Hero hero, ref int __result)
        {
            if (hero == Hero.MainHero && Artillery.Active)
                __result += Artillery.ExtraSlots;
        }
    }

    [HarmonyPatch(typeof(MobilePartyExtensions), nameof(MobilePartyExtensions.GetMaxNumberOfArtillery))]
    internal static class ArtilleryLimitPatch
    {
        [HarmonyPostfix]
        private static void Postfix(MobileParty party, ref int __result)
        {
            if (party == MobileParty.MainParty && Artillery.Active)
                __result += Artillery.ExtraSlots;
        }
    }

    [HarmonyPatch(typeof(AbilityComponent), MethodType.Constructor, new[] { typeof(Agent) })]
    internal static class FreeArtilleryPatch
    {
        private static readonly MethodInfo OnCastStart = AccessTools.Method(typeof(AbilityComponent), "OnCastStart");
        private static readonly MethodInfo OnCastComplete = AccessTools.Method(typeof(AbilityComponent), "OnCastComplete");

        [HarmonyPostfix]
        private static void Postfix(AbilityComponent __instance, Agent agent)
        {
            try
            {
                if (agent == null || agent.GetHero() != Hero.MainHero || !Artillery.Active) return;
                if (!agent.CanPlaceArtillery() || agent.Mission == null || agent.Mission.IsSiegeBattle) return;
                if (OnCastStart == null || OnCastComplete == null) return;

                var abilities = __instance.KnownAbilitySystem;
                var wasEmpty = abilities.Count == 0;

                foreach (var id in Artillery.FreePieces)
                {
                    var owned = abilities.OfType<ItemBoundAbility>().FirstOrDefault(a => a.Template.StringID.StartsWith(id));
                    if (owned != null)
                    {
                        owned.SetChargeNum(owned.GetRemainingCharges() + 1);
                        continue;
                    }

                    if (!(AbilityFactory.CreateNew(id, agent) is ItemBoundAbility piece)) continue;
                    piece.OnCastStart += (Ability.OnCastStartHandler)Delegate.CreateDelegate(typeof(Ability.OnCastStartHandler), __instance, OnCastStart);
                    piece.OnCastComplete += (Ability.OnCastCompleteHandler)Delegate.CreateDelegate(typeof(Ability.OnCastCompleteHandler), __instance, OnCastComplete);
                    piece.SetChargeNum(1);
                    abilities.Add(piece);
                }

                if (wasEmpty && abilities.Count > 0)
                    __instance.SelectAbility(0);
            }
            catch (Exception e)
            {
                Log.Warn("Could not add free artillery: " + e.Message);
            }
        }
    }
}
