using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.Core;
using TOR_Core.CampaignMechanics.ServeAsAHireling;
using TOR_Core.CharacterDevelopment;
using TORImperialEngineer.Career;

namespace TORImperialEngineer.Bootstrap
{
    [HarmonyPatch(typeof(ServeAsAHirelingActivities), MethodType.Constructor)]
    internal static class ImperialEngineerHireling
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
                    yield return CodeInstruction.Call(typeof(ImperialEngineerHireling), nameof(Add));
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(ServeAsAHirelingActivities __instance) => Add(__instance);

        private static void Add(ServeAsAHirelingActivities activities)
        {
            try
            {
                var career = ImperialEngineerCareer.Career;
                var sets = activities?._activitySets;
                if (career == null || sets == null || sets.ContainsKey(career)) return;

                sets[career] = new List<SkillObject>
                {
                    TORSkills.GunPowder,
                    DefaultSkills.Engineering,
                    DefaultSkills.Leadership,
                    DefaultSkills.Medicine,
                    DefaultSkills.Trade,
                };
            }
            catch (Exception e)
            {
                Log.Warn("Could not add hireling activities: " + e.Message);
            }
        }
    }
}
