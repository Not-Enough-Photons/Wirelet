using System.Collections.Generic;

namespace Wirelet
{
    public abstract class WireletComponent
    {
        internal List<WireletLink> outlinks = new List<WireletLink>();
        internal List<WireletLink> inlinks = new List<WireletLink>();
        internal WireletBehaviour behaviour;

        public virtual void OnCreate() { }
        public virtual void TickLocal() { }
        public virtual void TickRemote() { }
        public virtual void Render() { }
        public virtual void OnDestroy() { }

        internal void RemoveOutLink(WireletLink link) =>
            outlinks.Remove(link);
        internal void RemoveInLink(WireletLink link) =>
            inlinks.Remove(link);
    }
}