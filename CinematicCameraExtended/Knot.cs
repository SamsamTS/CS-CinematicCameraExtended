using System;
using UnityEngine;

namespace CinematicCameraExtended
{
    public enum EasingMode
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public class Knot
    {
        public Quaternion rotation;

        public float duration = 2f;

        public float delay;

        public EasingMode mode = EasingMode.EaseInOut;
    }
}