using MelonLoader;
using StressLevelZero.Props.Weapons;
using System;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using Wirelet.Components.Gates;

[assembly: MelonInfo(typeof(Wirelet.WireletMod), "Wirelet", "1.0.0", "Slaynash")]
[assembly: MelonGame("Stress Level Zero", "BONEWORKS")]

namespace Wirelet
{
    public class WireletMod : MelonMod
    {
        public static GameObject wirehandlerGO;

        private MethodInfo addCustomUtilityMethod;

        public static WireletMod Instance { get; private set; }

        public override void OnApplicationStart()
        {
            Instance = this;

            ClassInjector.RegisterTypeInIl2Cpp<WireletBehaviour>();

            WireletLinkGunsManager.Init();

            addCustomUtilityMethod = typeof(CustomUtilities.BuildInfo).Assembly.GetType("CustomUtilities.CustomUtilities").GetMethod("AddCustomUtility");

            RegisterCustomUtility<GateBoolToFloat>("WL: Gate Bool2Float");
            RegisterCustomUtility<GateAdd>("WL: Gate Add");
            RegisterCustomUtility<GateSubtract>("WL: Gate Subtract");
            RegisterCustomUtility<GateMultiply>("WL: Gate Multiply");
            RegisterCustomUtility<GateDivide>("WL: Gate Divide");
            RegisterCustomUtility<GateTimer>("WL: Gate Timer");
            RegisterCustomUtility<GateSin>("WL: Gate Sin");
            RegisterCustomUtility<GateSign>("WL: Gate Sign");
            RegisterCustomUtility<GateToColor>("WL: Gate ToColor");
            RegisterCustomUtility<GateColorTest>("WL: Gate Color Test");

            addCustomUtilityMethod.Invoke(null, new object[]
            {
                "WL: Wire Tool",
                new Action<SpawnGun>(WireletLinkGunsManager.OnTrigger)
            });


            GameExtensionsManager.Init();
        }

        public override void OnUpdate()
        {
            if (wirehandlerGO == null)
            {
                MelonLogger.Msg("WireletHandler doesn't exists. Creating.");
                wirehandlerGO = new GameObject("WireletHandler");
                GameObject.DontDestroyOnLoad(wirehandlerGO);
            }

            WireletLinkGunsManager.Update();
            WireletLogic.Update();
        }

        public void RegisterCustomUtility<T>(string name) where T : WireletComponent
        {
            addCustomUtilityMethod.Invoke(null, new object[]
            {
                name,
                new Action<SpawnGun>(spawnGun => SpawnWireletComponent<T>(spawnGun))
            });
        }

        public void SpawnWireletComponent<T>(SpawnGun spawnGun) where T : WireletComponent
        {
            if (Physics.Raycast(spawnGun.firePointTransform.position, spawnGun.firePointTransform.forward, out RaycastHit raycastHit, spawnGun.effectiveRange))
            {
                // Create element
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Place element
                go.transform.localScale = new Vector3(0.1f, 0.02f, 0.1f);
                go.transform.position = raycastHit.point + raycastHit.normal * 0.01f;

                float dotProduct = Vector3.Dot(raycastHit.normal, Vector3.up);
                if (Mathf.Abs(dotProduct) < 0.98f)
                    go.transform.rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.up) * Quaternion.Euler(-90, 0, 0);
                else
                    go.transform.rotation = GetLookatCamera(go.transform.position, dotProduct < 0 ? Vector3.up : Vector3.down);

                // Auto weld to target rigidbody
                if (raycastHit.rigidbody != null)
                {
                    FixedJoint joint = go.AddComponent<FixedJoint>();
                    joint.connectedBody = raycastHit.rigidbody;
                    joint.enableCollision = false;
                }

                // Setup component
                WireletBehaviour wb = go.AddComponent<WireletBehaviour>();
                wb.Setup<T>();
            }
        }

        private Quaternion GetLookatCamera(Vector3 position, Vector3 up)
        {
            Vector3 directionRaw = Camera.main.transform.position - position;
            directionRaw.y = 0;
            return Quaternion.LookRotation(directionRaw, up);
        }
    }
}
