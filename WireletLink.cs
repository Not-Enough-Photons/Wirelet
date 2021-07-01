using System.Reflection;

namespace Wirelet
{
    internal class WireletLink
    {
        public WireletComponent outWirelet;
        public WireletComponent inWirelet;

        public FieldInfo outField;
        public FieldInfo inField;

        public PinnableLineRenderer linerenderer;

        internal void Apply()
        {
            if (inWirelet != null)
                inField.SetValue(inWirelet, outField.GetValue(outWirelet));
        }
    }
}
