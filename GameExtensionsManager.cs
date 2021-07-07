using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using Wirelet.Components.Extensions;

namespace Wirelet
{
    internal static class GameExtensionsManager
    {
        private static Dictionary<string, (Type type, int childDepth)> extensionComponents = new Dictionary<string, (Type, int)>();

        internal static void Init()
        {
            WireletMod.Instance.HarmonyInstance.Patch(
                typeof(PuppetMasta.BehaviourBase).GetMethod("OnEnable"),
                postfix: new HarmonyMethod(typeof(GameExtensionsManager).GetMethod("TrySetupGameobject", BindingFlags.NonPublic | BindingFlags.Static)));

            WireletMod.Instance.HarmonyInstance.Patch(
                typeof(StressLevelZero.Interaction.ButtonToggle).GetMethod("Awake"),
                prefix: new HarmonyMethod(typeof(GameExtensionsManager).GetMethod("TrySetupGameobject", BindingFlags.NonPublic | BindingFlags.Static)));
                        
            WireletMod.Instance.HarmonyInstance.Patch(
                typeof(StressLevelZero.Props.Weapons.Gun).GetMethod("OnEnable"),
                prefix: new HarmonyMethod(typeof(GameExtensionsManager).GetMethod("TrySetupGameobject", BindingFlags.NonPublic | BindingFlags.Static)));

            //extensionComponents.Add(Il2CppType.Of<PuppetMasta.BehaviourCrablet>().FullName, (typeof(CrabletComponent), 0));
            extensionComponents.Add(Il2CppType.Of<StressLevelZero.Interaction.ButtonToggle>().FullName, (typeof(ButtonToggleComponent), 1));
            extensionComponents.Add(Il2CppType.Of<PuppetMasta.BehaviourBaseNav>().FullName, (typeof(AIComponent), 2));
            extensionComponents.Add(Il2CppType.Of<StressLevelZero.Props.Weapons.Gun>().FullName, (typeof(GunComponent), 3));
        }

        #region BehaviourBaseNav

        private static void TrySetupGameobject(MonoBehaviour __instance)
        {
            MelonLogger.Msg("OnEnable called on " + __instance.GetIl2CppType().Name);

            if (__instance.gameObject.GetComponentInParent<WireletBehaviour>() != null)
                return;

            if (extensionComponents.TryGetValue(__instance.GetIl2CppType().FullName, out (Type type, int childDepth) wireletType)) {
                GameObject go = GetParentRecursive(__instance.transform, wireletType.childDepth).gameObject;
                if (go.GetComponentInParent<WireletBehaviour>() != null)
                    return;

                MelonLogger.Msg("Adding wirelet extension to instance of " + __instance.GetIl2CppType().Name);
                WireletBehaviour wb = go.AddComponent<WireletBehaviour>();
                wb.Setup(wireletType.type);
            }
            else
                MelonLogger.Msg("Could not find wirelet extension for " + __instance.GetIl2CppType().Name);
        }

        #endregion
        
        private static Transform GetParentRecursive(Transform t, int depth)
        {
            while (t != null && depth > 0)
            {
                t = t.parent;
                --depth;
            }

            return t;
        }
    }
}
