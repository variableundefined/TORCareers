using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.Core;
using TOR_Core.CampaignMechanics.ServeAsAHireling;
using TOR_Core.CharacterDevelopment;
using TORCouncilGuard.Career;

namespace TORCouncilGuard.Bootstrap
{
    [HarmonyPatch(typeof(ServeAsAHirelingActivities), MethodType.Constructor)]
    internal static class CouncilGuardHireling
    {
        private static readonly FieldInfo ActivitySets = AccessTools.Field(
            typeof(ServeAsAHirelingActivities), nameof(ServeAsAHirelingActivities._activitySets));

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.StoresField(ActivitySets))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(CouncilGuardHireling), nameof(Add));
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(ServeAsAHirelingActivities __instance) => Add(__instance);

        private static void Add(ServeAsAHirelingActivities activities)
        {
            try
            {
                var career = CouncilGuardCareer.Career;
                var sets = activities?._activitySets;
                if (career == null || sets == null || sets.ContainsKey(career)) return;

                sets[career] = new List<SkillObject>
                {
                    DefaultSkills.Polearm,
                    DefaultSkills.Athletics,
                    TORSkills.Faith,
                    DefaultSkills.Charm,
                    DefaultSkills.Leadership,
                };
            }
            catch (Exception e)
            {
                Log.Warn("Could not add hireling activities: " + e.Message);
            }
        }
    }
}
