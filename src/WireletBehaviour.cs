using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Wirelet
{
    public class WireletBehaviour : MonoBehaviour
    {
        // Unhollower requirement
        public WireletBehaviour(IntPtr ptr) : base(ptr) { }

        [HideFromIl2Cpp]
        public WireletComponent Component { get; private set; }

        [HideFromIl2Cpp]
        internal void Setup<T>() where T : WireletComponent
        {
            Component = Activator.CreateInstance(typeof(T)) as WireletComponent;
            Component.behaviour = this;
            WireletLogic.AddComponent(Component);
            Component.OnCreate();
        }

        [HideFromIl2Cpp]
        internal void Setup(Type type)
        {
            Component = Activator.CreateInstance(type) as WireletComponent;
            Component.behaviour = this;
            WireletLogic.AddComponent(Component);
            Component.OnCreate();
        }

        private void OnDestroy()
        {
            Component.OnDestroy();
            WireletLogic.RemoveComponent(Component);
        }
    }
}
