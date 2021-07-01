using StressLevelZero.Interaction;
using StressLevelZero.Props.Weapons;
using System.Reflection;
using UnityEngine;

namespace Wirelet
{
    internal class WireletSpawnGunStateDescriptor
    {
        public SpawnGun Gun { get; private set; }
        internal GameObject ui;

        internal PinnableLineRenderer linerenderer;

        internal Grip.HandDelegate detachedHandDelegate;

        internal LinkCreationState linkCreationState;
        internal int selectionIndex = 0;

        internal WireletComponent pointedWirelet;
        internal WireletComponent inWirelet;
        internal WireletComponent outWirelet;

        internal FieldInfo pointedField;
        internal FieldInfo inField;

        internal Vector3 aimpos;
        internal Transform aimtransform;

        // no outField, because we create the wirelink at this point

        public WireletSpawnGunStateDescriptor(SpawnGun gun, GameObject ui)
        {
            Gun = gun;
            this.ui = ui;
        }
    }
}