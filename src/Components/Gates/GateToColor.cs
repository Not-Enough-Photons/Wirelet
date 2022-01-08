using UnityEngine;

namespace Wirelet.Components.Gates
{
    internal class GateToColor : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public float r;
        [WireletIO(WireletIOType.Input)]
        public float g;
        [WireletIO(WireletIOType.Input)]
        public float b;

        [WireletIO(WireletIOType.Output)]
        public Color color;

        public override void OnCreate()
        {
            color = new Color();
        }

        public override void TickLocal()
        {
            color.r = r;
            color.g = g;
            color.b = b;
        }
    }
}
