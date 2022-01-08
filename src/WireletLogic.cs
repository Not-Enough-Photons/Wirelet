using System.Collections.Generic;

namespace Wirelet
{
    internal static class WireletLogic
    {
        private static List<WireletComponent> components = new List<WireletComponent>();
        private static List<WireletLink> links = new List<WireletLink>();

        internal static void Update()
        {
            // if (MultiplayerManager.isMultiplayer) {
            //     foreach (WireletComponent component in components)
            //         // TODO apply values
            //     foreach (WireletComponent component in components)
            //         component.TickRemote();
            // }
            // else
            // {
            foreach (WireletComponent component in components)
                foreach (WireletLink link in component.outlinks)
                    link.Apply();
            foreach (WireletComponent component in components)
                component.TickLocal();
            // }
            foreach (WireletLink link in links)
                link.linerenderer.Update();
        }

        internal static void AddComponent(WireletComponent component)
        {
            // TODO Add the component in the "logic" order, so that everything is updated in one tick
            components.Add(component);
        }

        internal static void CreateLink(WireletLink link)
        {
            link.outWirelet.outlinks.Add(link);
            link.inWirelet.inlinks.Add(link);
            links.Add(link);
        }

        internal static void RemoveComponent(WireletComponent component)
        {
            components.Remove(component);
            WireletLinkGunsManager.OnWireletDestroyed(component);

            for (int i = links.Count - 1; i >= 0; --i)
            {
                WireletLink link = links[i];
                if (link.outWirelet == component || link.inWirelet == component)
                {
                    link.outWirelet?.RemoveOutLink(link);
                    link.inWirelet?.RemoveInLink(link);
                    link.linerenderer.Destroy();
                    links.RemoveAt(i);
                }
            }
        }
    }
}