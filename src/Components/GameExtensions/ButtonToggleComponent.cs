using StressLevelZero.Interaction;

namespace Wirelet.Components.Extensions
{
    class ButtonToggleComponent : WireletComponent
    {
        private ButtonToggle buttonLogic;

        [WireletIO(WireletIOType.Output)]
        private bool pressed;

        public override void OnCreate()
        {
            buttonLogic = behaviour.GetComponent<ButtonToggle>();
        }

        public override void TickLocal()
        {
            pressed = buttonLogic._isPressed;
        }
    }
}
