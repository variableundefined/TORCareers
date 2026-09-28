using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TOR_Core.CharacterDevelopment.CareerSystem;

namespace TORMercenaryCareerOverhaul.Patches
{

    internal static class CareerReflection
    {

        internal static MethodInfo ChoiceInitialize()
        {
            var target = AccessTools.Method(
                typeof(CareerChoiceObject),
                nameof(CareerChoiceObject.Initialize),
                new[]
                {
                    typeof(CareerObject),
                    typeof(string),
                    typeof(string),
                    typeof(bool),
                    typeof(ChoiceType),
                    typeof(List<CareerChoiceObject.MutationObject>),
                    typeof(CareerChoiceObject.PassiveEffect),
                });

            if (target == null)
                throw new MissingMethodException("CareerChoiceObject.Initialize(7 args) not found.");

            return target;
        }

        internal static MethodInfo GroupInitialize()
        {
            var target = AccessTools.Method(
                typeof(CareerChoiceGroupObject),
                nameof(CareerChoiceGroupObject.Initialize),
                new[]
                {
                    typeof(string),
                    typeof(CareerObject),
                    typeof(int),
                    typeof(CareerChoiceGroupObject.ConditionDelegate),
                    typeof(CareerChoiceGroupObject.UnlockDelegate),
                });

            if (target == null)
                throw new MissingMethodException("CareerChoiceGroupObject.Initialize(5 args) not found.");

            return target;
        }
    }
}
