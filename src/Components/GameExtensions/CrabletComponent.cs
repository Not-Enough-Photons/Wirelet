using PuppetMasta;

namespace Wirelet.Components.Extensions
{
    class CrabletComponent : WireletComponent
    {
        private BehaviourCrablet crabletLogic;

        [WireletIO(WireletIOType.Output)]
        private float health;
        [WireletIO(WireletIOType.Output)]
        private BehaviourBaseNav.LocoState locoState;
        [WireletIO(WireletIOType.Output)]
        private BehaviourBaseNav.MentalState mentalState;
        [WireletIO(WireletIOType.Output)]
        private bool grounded;

        public override void OnCreate()
        {
            crabletLogic = behaviour.GetComponentInChildren<BehaviourCrablet>();
        }

        public override void TickLocal()
        {
            health = crabletLogic.health.cur_hp;
            locoState = crabletLogic.locoState;
            mentalState = crabletLogic.mentalState;
            grounded = crabletLogic.sensors.isGrounded;
        }
    }
}
