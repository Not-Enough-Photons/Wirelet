namespace Wirelet.Components.Gates
{
    internal class GateBoolToFloat : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public bool input;

        [WireletIO(WireletIOType.Output)]
        public float output;

        public override void TickLocal()
        {
            output = input ? 1 : 0;
        }
    }
}
