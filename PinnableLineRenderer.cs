using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wirelet
{
    internal class PinnableLineRenderer
    {
        private GameObject gameobject;
        private LineRenderer linerenderer;
        private List<(Vector3 pos, Transform transform)> pins = new List<(Vector3 pos, Transform transform)>();
        private Vector3[] pinsProcessed;

        public PinnableLineRenderer(Vector3 worldposStart, Transform transformStart)
        {
            gameobject = new GameObject("PinnableLineRenderer instance");
            gameobject.transform.SetParent(WireletMod.wirehandlerGO.transform, true);
            linerenderer = gameobject.AddComponent<LineRenderer>();

            //linerenderer.material = new Material(Shader.Find("Diffuse"));
            linerenderer.material = new Material(Shader.Find("Unlit/Texture")); // Standard

            Texture2D tex = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(tex, Convert.FromBase64String(ImageData.wirelinkTexture)))
                linerenderer.material.mainTexture = tex;

            //linerenderer.material.color = new Color(1f, 0.64f, 0f);
            linerenderer.material.SetTextureScale("_MainTex", new Vector2(-20f, 1f));
            linerenderer.SetWidth(0.02f, 0.02f);
            linerenderer.textureMode = LineTextureMode.Tile;
            MelonLogger.Msg("[PinnableLineRenderer] instance created");
            AddPin(worldposStart, transformStart);
        }

        public void Update()
        {
            if (pins.Count > 1)
            {
                for (int i = 0; i < pins.Count; ++i)
                    pinsProcessed[i] = pins[i].transform?.TransformPoint(pins[i].pos) ?? pins[i].pos;

                linerenderer.SetPositions(pinsProcessed);
            }
            //MelonLogger.Msg("PinnableLineRenderer updated [" + string.Join(", ", pinsProcessed.Select(v => v.ToString())) + "]");
        }

        public void AddPin(Vector3 worldpos, Transform transform)
        {
            pins.Add((transform?.InverseTransformPoint(worldpos) ?? worldpos, transform));
            pinsProcessed = new Vector3[pins.Count];
            linerenderer.positionCount = pins.Count;
            MelonLogger.Msg("[PinnableLineRenderer] pin added");
        }

        public void Destroy()
        {
            GameObject.Destroy(linerenderer);
            GameObject.Destroy(gameobject);
            MelonLogger.Msg("[PinnableLineRenderer] instance destroyed");
        }
    }
}