using TaleWorlds.CampaignSystem;
using TOR_Core.AbilitySystem.Scripts;
using TORWaywatcherOverhaul.Arrows;

namespace TORWaywatcherOverhaul.LethalShot
{
    public class WaywatcherLethalShotScript : CareerAbilityScript
    {
        private bool _loaded;

        protected override void OnInit()
        {
            base.OnInit();

            Quiver.StartLethalShot(Quiver.LethalShotArrows(Hero.MainHero));
            _loaded = true;
        }

        protected override void OnBeforeTick(float dt)
        {
            base.OnBeforeTick(dt);
            if (_loaded && !IsFading && !Quiver.IsLethal)
                Stop();
        }

        protected override void OnBeforeRemoved(int removeReason)
        {
            Quiver.EndLethalShot();
            base.OnBeforeRemoved(removeReason);
        }
    }
}
