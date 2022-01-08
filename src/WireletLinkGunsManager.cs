using HarmonyLib;
using MelonLoader;
using StressLevelZero.Interaction;
using StressLevelZero.Props.Weapons;
using StressLevelZero.Rig;
using StressLevelZero.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Wirelet
{
    internal static class WireletLinkGunsManager
    {
        private static readonly Dictionary<int, WireletSpawnGunStateDescriptor> stateDescriptors = new Dictionary<int, WireletSpawnGunStateDescriptor>();

        private static ControllerRig playerControllerRig;
        private static bool playerLocked = false;

        private static float[] lastScrollTime = new float[2];

        public static void Init()
        {
            WireletMod.Instance.HarmonyInstance.Patch(
                typeof(SpawnGun).GetMethod("OnModeSelect"),
                postfix: new HarmonyMethod(typeof(WireletLinkGunsManager).GetMethod("OnToolSelected", BindingFlags.NonPublic | BindingFlags.Static)));

            WireletMod.Instance.HarmonyInstance.Patch(
                typeof(Gun).GetMethod("OnDestroy"),
                postfix: new HarmonyMethod(typeof(WireletLinkGunsManager).GetMethod("OnGunDestroyed", BindingFlags.NonPublic | BindingFlags.Static)));

            MelonMod easymenuMod = MelonHandler.Mods.FirstOrDefault(mod => mod.Info.Name == "EasyMenu");
            if (easymenuMod != null)
            {
                MethodInfo easymenuCloseMethod = easymenuMod.GetType().GetMethod("CloseMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                if (easymenuCloseMethod != null)
                    WireletMod.Instance.HarmonyInstance.Patch(
                        easymenuCloseMethod,
                        postfix: new HarmonyMethod(typeof(WireletLinkGunsManager).GetMethod("EasymenuClosePostfix", BindingFlags.NonPublic | BindingFlags.Static)));
                else
                    MelonLogger.Warning("[WireletLinkGunsManager] Failed to patch EasyMenu OpenMenu method. This may lead into weird behaviours");
            }
        }

        private static GameObject CreateUI()
        {
            GameObject uiPrefab = new GameObject("WireletLinkUI");
            GameObject textGO = new GameObject("Title");
            textGO.transform.SetParent(uiPrefab.transform, false);
            TextMeshPro textMeshPro = UIUtils.CreateText(textGO, FontStyles.Bold, 0.1f, Color.red, Color.white, 0.1f, new Vector4() * 0.5f, "Wirelet 1.0.0");
            textMeshPro.transform.localPosition = new Vector3(0, 0.2f, -0.12f);
            GameObject.DontDestroyOnLoad(uiPrefab);

            return uiPrefab;
        }

        private static void OnToolSelected(SpawnGun __instance)
        {
            if (SpawnGunUI.Instance.spawnItemText.text == "WL: Wire Tool")
            {
                if (!stateDescriptors.TryGetValue(__instance.GetInstanceID(), out WireletSpawnGunStateDescriptor statedescriptor))
                {
                    MelonLogger.Msg("SpawnGun doesn't have an associated State Descriptor. Creating");
                    GameObject ui = CreateUI();
                    ui.transform.SetParent(__instance.transform, false);
                    ui.transform.localPosition = Vector3.zero;
                    stateDescriptors[__instance.GetInstanceID()] = new WireletSpawnGunStateDescriptor(__instance, ui);
                }
            }
            else
                RemoveStateDescriptor(__instance);
        }

        private static void OnGunDestroyed(Gun __instance)
        {
            RemoveStateDescriptor(__instance);
        }

        private static void ClearWireState(WireletSpawnGunStateDescriptor descriptor, bool refreshPlayerLockIfNeeded)
        {
            if (descriptor.linkCreationState != LinkCreationState.NotCreating)
            {
                bool refreshPlayerLock = refreshPlayerLockIfNeeded && descriptor.linkCreationState == LinkCreationState.SelectingInput || descriptor.linkCreationState == LinkCreationState.SelectingOutput;

                descriptor.linkCreationState = LinkCreationState.NotCreating;
                descriptor.inWirelet = null;
                descriptor.outWirelet = null;
                descriptor.selectionIndex = 0;
                descriptor.pointedField = null;
                descriptor.aimpos = default;
                descriptor.aimtransform = null;

                descriptor.linerenderer?.Destroy();
                descriptor.linerenderer = null;

                descriptor.Gun.triggerGrip.remove_detachedHandDelegate(descriptor.detachedHandDelegate);
                descriptor.detachedHandDelegate = null;

                if (refreshPlayerLock)
                    RefreshPlayerLock();
            }
        }

        private static void RemoveStateDescriptor(Gun gun)
        {
            if (stateDescriptors.TryGetValue(gun.GetInstanceID(), out WireletSpawnGunStateDescriptor statedescriptor))
            {
                ClearWireState(statedescriptor, true);
                GameObject.Destroy(statedescriptor.ui);
                statedescriptor.ui = null;
                stateDescriptors.Remove(gun.GetInstanceID());
            }
        }

        internal static void OnTrigger(SpawnGun spawnGun)
        {
            if (!stateDescriptors.TryGetValue(spawnGun.GetInstanceID(), out WireletSpawnGunStateDescriptor statedescriptor))
            {
                MelonLogger.Error("[WireletLinkGunsManager] SpawnGun State Descriptor not found! This should not happen!");
                return;
            }

            switch (statedescriptor.linkCreationState)
            {
                case LinkCreationState.NotCreating:
                    if (statedescriptor.pointedWirelet != null)
                    {
                        statedescriptor.linkCreationState = LinkCreationState.SelectingInput;
                        statedescriptor.inWirelet = statedescriptor.pointedWirelet;

                        statedescriptor.linerenderer = new PinnableLineRenderer(statedescriptor.aimpos, statedescriptor.aimtransform);
                        spawnGun.triggerGrip.add_detachedHandDelegate(statedescriptor.detachedHandDelegate = (Grip.HandDelegate)(hand => OnDetachedHand(hand, statedescriptor)));
                        LockPlayer(true);
                    }
                    break;

                case LinkCreationState.SelectingInput:
                    if (statedescriptor.inWirelet.inlinks.Any(w => w.inField == statedescriptor.pointedField))
                        break;

                    statedescriptor.inField = statedescriptor.pointedField;
                    statedescriptor.selectionIndex = 0;
                    statedescriptor.pointedField = null;
                    statedescriptor.linkCreationState = LinkCreationState.Stepping;
                    break;

                case LinkCreationState.Stepping:
                    if (statedescriptor.pointedWirelet != null)
                    {
                        bool hasValidOutput = statedescriptor.pointedWirelet.GetType().GetFields((BindingFlags)(-1)).Any(f =>
                        {
                            if (f.FieldType != statedescriptor.inField.FieldType)
                                return false;
                            return f.GetCustomAttribute<WireletIOAttribute>() != null && f.GetCustomAttribute<WireletIOAttribute>().type == WireletIOType.Output;
                        });

                        if (hasValidOutput)
                        {
                            statedescriptor.linerenderer.AddPin(statedescriptor.aimpos, statedescriptor.aimtransform);
                            statedescriptor.outWirelet = statedescriptor.pointedWirelet;
                            statedescriptor.linkCreationState = LinkCreationState.SelectingOutput;
                        }
                    }
                    else if (statedescriptor.aimpos != Vector3.zero)
                        statedescriptor.linerenderer.AddPin(statedescriptor.aimpos, statedescriptor.aimtransform);
                    break;

                case LinkCreationState.SelectingOutput:

                    if (statedescriptor.pointedField.FieldType != statedescriptor.inField.FieldType)
                        return;

                    //statedescriptor.linerenderer.AddPin(statedescriptor.aimpos, statedescriptor.outWirelet.behaviour.transform);

                    WireletLogic.CreateLink(new WireletLink()
                    {
                        inWirelet = statedescriptor.inWirelet,
                        inField = statedescriptor.inField,
                        outWirelet = statedescriptor.outWirelet,
                        outField = statedescriptor.pointedField,
                        linerenderer = statedescriptor.linerenderer
                    });

                    statedescriptor.linerenderer = null; // Avoid it being destroyed when clearing

                    ClearWireState(statedescriptor, true);
                    break;
            }
        }

        private static void OnDetachedHand(Hand hand, WireletSpawnGunStateDescriptor stateDescriptor)
        {
            MelonLogger.Msg("Dropped hand with descriptor " + stateDescriptor);
            ClearWireState(stateDescriptor, true);
        }

        internal static void Update()
        {
            foreach (WireletSpawnGunStateDescriptor statedescriptor in stateDescriptors.Values)
            {
                SpawnGun gun = statedescriptor.Gun;
                if (Physics.Raycast(gun.firePointTransform.position, gun.firePointTransform.forward, out RaycastHit raycastHit, gun.effectiveRange))
                {
                    WireletComponent pointedWirelet = raycastHit.transform.GetComponentInParent<WireletBehaviour>()?.Component;
                    if (statedescriptor.pointedWirelet != pointedWirelet)
                        statedescriptor.pointedWirelet = pointedWirelet;
                }
                else if (statedescriptor.pointedWirelet != null)
                    statedescriptor.pointedWirelet = null;

                statedescriptor.aimpos = raycastHit.point;
                statedescriptor.aimtransform = raycastHit.transform;

                TextMeshPro tmp = statedescriptor.Gun.transform.Find("WireletLinkUI/Title").GetComponent<TextMeshPro>();
                string text = "Wirelet 1.0.0";
                text += $"\n[State: {statedescriptor.linkCreationState}]";

                if (statedescriptor.linkCreationState == LinkCreationState.NotCreating || statedescriptor.linkCreationState == LinkCreationState.Stepping)
                {
                    WireletIOType listingType = statedescriptor.linkCreationState == LinkCreationState.NotCreating ? WireletIOType.Input : WireletIOType.Output;

                    if (statedescriptor.pointedWirelet != null)
                    {
                        text += $"\n____________{listingType.ToString().ToUpper()}____________";

                        IEnumerable<FieldInfo> fields = statedescriptor.pointedWirelet.GetType().GetFields((BindingFlags)(-1));
                        foreach (FieldInfo field in fields)
                        {
                            WireletIOAttribute wireletIO = field.GetCustomAttribute<WireletIOAttribute>();
                            if (wireletIO == null || wireletIO.type != listingType)
                                continue;

                            string ioName = wireletIO.Name ?? GetHumanReadableField(field.Name);

                            bool isAlreadyLinked = listingType == WireletIOType.Input && statedescriptor.pointedWirelet.inlinks.Any(w => w.inField == field);
                            if (isAlreadyLinked)
                                ioName = $"<color=gray>{ioName}</color>";

                            text += "\n" + ioName;
                        }
                    }
                }
                else
                {
                    WireletIOType listingType = statedescriptor.linkCreationState == LinkCreationState.SelectingInput ? WireletIOType.Input : WireletIOType.Output;

                    text += $"\n____________{listingType.ToString().ToUpper()}____________";


                    WireletComponent wirelet = statedescriptor.linkCreationState == LinkCreationState.SelectingInput ? statedescriptor.inWirelet : statedescriptor.outWirelet;
                    IEnumerable<FieldInfo> fields = wirelet.GetType().GetFields((BindingFlags)(-1));


                    Vector2 vector = playerControllerRig.rightController.controllerType == StressLevelZero.Player.ControllerInfo.Type.VIVE_WANDS ? playerControllerRig.rightController.GetTouchpadAxis() : playerControllerRig.rightController.GetThumbStickAxis();
                    if ((vector.y > 0.9f || vector.y < -0.9f) && Time.time - lastScrollTime[1] > 0.4f)
                    {
                        lastScrollTime[1] = Time.time;
                        if (vector.y > 0.9f)
                        {
                            if (statedescriptor.selectionIndex > 0)
                                --statedescriptor.selectionIndex;
                        }
                        else
                        {
                            int maxIndex = fields.Count(f =>
                            {
                                if (listingType == WireletIOType.Output && f.FieldType != statedescriptor.inField)
                                    return false;
                                return f.GetCustomAttribute<WireletIOAttribute>()?.type == listingType;
                            }) - 1;

                            if (statedescriptor.selectionIndex < maxIndex)
                                ++statedescriptor.selectionIndex;
                        }
                    }





                    int index = -1;
                    foreach (FieldInfo field in fields)
                    {
                        WireletIOAttribute wireletIO = field.GetCustomAttribute<WireletIOAttribute>();
                        if (wireletIO == null || wireletIO.type != listingType)
                            continue;

                        ++index;

                        string ioName = wireletIO.Name ?? GetHumanReadableField(field.Name);

                        bool isAlreadyLinked = listingType == WireletIOType.Input && statedescriptor.inWirelet.inlinks.Any(w => w.inField == field);
                        if (isAlreadyLinked)
                            ioName = $"<color=gray>{ioName}</color>";

                        if (statedescriptor.selectionIndex == index)
                        {
                            ioName = "> " + ioName;
                            statedescriptor.pointedField = field;
                        }
                        else
                            ioName = "   " + ioName;

                        text += $"\n{ioName}";
                    }
                }

                tmp.text = text;


                if (statedescriptor.linerenderer != null)
                    statedescriptor.linerenderer.Update();
            }
        }

        private static string GetHumanReadableField(string fieldName)
        {
            if (fieldName == null || fieldName.Length < 1)
                return "";

            return fieldName.First().ToString().ToUpper() + string.Join(" ", Regex.Split(fieldName, @"(?<!^)(?=[A-Z])")).Substring(1);
        }

        internal static void OnWireletDestroyed(WireletComponent wirelet)
        {
            bool refreshPlayerLock = false;
            foreach (WireletSpawnGunStateDescriptor statedescriptor in stateDescriptors.Values)
            {
                if (statedescriptor.linkCreationState != LinkCreationState.NotCreating)
                {
                    if (statedescriptor.inWirelet == wirelet)
                    {
                        refreshPlayerLock |= statedescriptor.linkCreationState == LinkCreationState.SelectingInput || statedescriptor.linkCreationState == LinkCreationState.SelectingOutput;
                        ClearWireState(statedescriptor, false);
                    }
                }
            }

            if (refreshPlayerLock)
                RefreshPlayerLock();
        }

        private static void RefreshPlayerLock()
        {
            LockPlayer(stateDescriptors.Values.Any(sd => sd.linkCreationState == LinkCreationState.SelectingInput || sd.linkCreationState == LinkCreationState.SelectingOutput));
        }

        private static void LockPlayer(bool @lock)
        {
            playerLocked = @lock;
            if (playerControllerRig == null)
                playerControllerRig = GameObject.FindObjectOfType<ControllerRig>();

            playerControllerRig.crouchEnabled = !@lock;
            playerControllerRig.turnEnabled = !@lock;
            playerControllerRig.jumpEnabled = !@lock;
            playerControllerRig.locoEnabled = !@lock;
        }

        #region EasyMenu

        private static void EasymenuClosePostfix()
        {
            if (playerLocked)
                LockPlayer(true);
        }

        #endregion
    }
}
