using PuppetMasta;

namespace Wirelet.Components.Extensions
{
    class AIComponent : WireletComponent
    {
        private BehaviourBaseNav aiLogic;

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
            aiLogic = behaviour.GetComponentInChildren<BehaviourBaseNav>();
        }

        public override void TickLocal()
        {
            health = aiLogic.health.cur_hp;
            locoState = aiLogic.locoState;
            mentalState = aiLogic.mentalState;
            grounded = aiLogic.sensors.isGrounded;
        }
    }
}
