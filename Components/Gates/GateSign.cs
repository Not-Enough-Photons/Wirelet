using UnityEngine;

namespace Wirelet.Components.Gates
{
    internal class GateSign : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public float a;

        [WireletIO(WireletIOType.Output)]
        public float output;

        public override void TickLocal()
        {
            output = Mathf.Sign(a);
        }
    }
}
