using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;

namespace TORSwordmaster.Trance
{
    // Weapons are sheathed after a Prayer, which makes casting ability awkward. This doesn't occurs with CA
    // To avoid harmony patch and randomly breaking stuffs (Maybe a harmony patch can be done later because these are all implied to be sword technique), uses a script to re-equip instead.
    internal static class Rewield
    {
        private const float Window = 3f;

        private static EquipmentIndex _mainHand = EquipmentIndex.None;
        private static EquipmentIndex _offHand = EquipmentIndex.None;
        private static float _until = float.MinValue;

        internal static void Reset()
        {
            _mainHand = EquipmentIndex.None;
            _offHand = EquipmentIndex.None;
            _until = float.MinValue;
        }

        internal static void Arm()
        {
            _until = Mission.Current.CurrentTime + Window;
        }

        // Remembers what the player holds outside ability mode, and draws it again for a few seconds after a technique.
        internal static void Tick(Agent agent)
        {
            var logic = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
            if (logic == null || logic.CurrentState != AbilityModeState.Off) return;

            var main = agent.GetPrimaryWieldedItemIndex();
            var off = agent.GetOffhandWieldedItemIndex();

            if (Mission.Current.CurrentTime > _until)
            {
                if (main == EquipmentIndex.None) return;
                _mainHand = main;
                _offHand = off;
                return;
            }

            if (logic.ShouldSuppressCombatActions) return;

            if (main == EquipmentIndex.None && _mainHand != EquipmentIndex.None)
                agent.TryToWieldWeaponInSlot(_mainHand, Agent.WeaponWieldActionType.WithAnimation, false);
            else if (main == _mainHand && off == EquipmentIndex.None && _offHand != EquipmentIndex.None)
                agent.TryToWieldWeaponInSlot(_offHand, Agent.WeaponWieldActionType.WithAnimation, false);
        }
    }
}
