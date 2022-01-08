using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wirelet.Components.Gates
{
    internal class GateDivide : WireletComponent
    {
        [WireletIO(WireletIOType.Input)]
        public float a;
        [WireletIO(WireletIOType.Input)]
        public float b;

        [WireletIO(WireletIOType.Output)]
        public float output;

        public override void TickLocal()
        {
            // Divide by zero error
            if(b == 0) { return; }

            output = a / b;
        }
    }
}
