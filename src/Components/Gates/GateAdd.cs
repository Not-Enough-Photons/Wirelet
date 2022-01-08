namespace Wirelet.Components.Gates
{
    internal class GateAdd : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public float a;
        [WireletIO(WireletIOType.Input)]
        public float b;

        [WireletIO(WireletIOType.Output)]
        public float output;

        public override void TickLocal()
        {
            output = a + b;
        }
    }
}
