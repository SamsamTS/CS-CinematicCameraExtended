using System;
using UnityEngine;

namespace CinematicCameraExtended
{
    public class Spline
    {
        public static Vector3 CalculateSplinePosition(Vector3[] points, int i, float t)
        {
            Vector3 vector = points[i];
            Vector3 vector2 = points[i + 1];
            Vector3 vector3;
            if (i > 0)
            {
                vector3 = 0.5f * (points[i + 1] - points[i - 1]);
            }
            else
            {
                vector3 = points[i + 1] - points[i];
            }
            Vector3 vector4;
            if (i < points.Length - 2)
            {
                vector4 = 0.5f * (points[i + 2] - points[i]);
            }
            else
            {
                vector4 = points[i + 1] - points[i];
            }
            return (2f * t * t * t - 3f * t * t + 1f) * vector + (t * t * t - 2f * t * t + t) * vector3 + (-2f * t * t * t + 3f * t * t) * vector2 + (t * t * t - t * t) * vector4;
        }
    }
}
