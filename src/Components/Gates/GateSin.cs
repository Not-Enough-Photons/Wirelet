using UnityEngine;

namespace Wirelet.Components.Gates
{
    internal class GateSin : WireletComponent
    {
        [WireletIO(WireletIOType.Input, Name = "Input (deg)")]
        public float input;

        [WireletIO(WireletIOType.Output)]
        public float output;

        public override void TickLocal()
        {
            output = Mathf.Sin(input);
        }
    }
}
