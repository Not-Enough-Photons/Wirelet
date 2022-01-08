using System.Diagnostics;

namespace Wirelet.Components.Gates
{
    internal class GateTimer : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public bool reset;

        [WireletIO(WireletIOType.Output, Name = "Output (seconds)")]
        public float output;

        Stopwatch stopwatch = new Stopwatch();

        public override void TickLocal()
        {
            if (reset)
            {
                if (stopwatch.IsRunning)
                    stopwatch.Reset();
            }
            else
            {
                if (!stopwatch.IsRunning)
                    stopwatch.Start();

                output = stopwatch.ElapsedMilliseconds / 1000f;
            }
        }

    }
}
