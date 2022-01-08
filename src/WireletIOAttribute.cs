using System;

namespace Wirelet
{
    [AttributeUsage(AttributeTargets.Field)]
    public class WireletIOAttribute : Attribute
    {
        internal WireletIOType type;

        public WireletIOAttribute(WireletIOType type)
        {
            this.type = type;
        }

        public string Name { get; set; }
    }
}
