using UnityEngine;

namespace Wirelet.Components.Gates
{
    class GateColorTest : WireletComponent
    {
        [WireletIO(WireletIOType.Input, Name = "Input")]
        public Color input;

        public override void TickLocal()
        {
            behaviour.GetComponent<Renderer>().material.color = input;
        }
    }
}
