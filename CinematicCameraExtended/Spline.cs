using System;
using UnityEngine;

namespace CinematicCameraExtended
{
    public class Spline
    {
        public static Vector3 CalculateSplinePosition(object[] points, int size, int i, float t)
        {
            Vector3 vector = ((Knot)points[i]).position;
            Vector3 vector2 = ((Knot)points[i + 1]).position;
            Vector3 vector3;
            if (i > 0)
            {
                vector3 = 0.5f * (((Knot)points[i + 1]).position - ((Knot)points[i - 1]).position);
            }
            else
            {
                vector3 = ((Knot)points[i + 1]).position - ((Knot)points[i]).position;
            }
            Vector3 vector4;
            if (i < size - 2)
            {
                vector4 = 0.5f * (((Knot)points[i + 2]).position - ((Knot)points[i]).position);
            }
            else
            {
                vector4 = ((Knot)points[i + 1]).position - ((Knot)points[i]).position;
            }
            return (2f * t * t * t - 3f * t * t + 1f) * vector + (t * t * t - 2f * t * t + t) * vector3 + (-2f * t * t * t + 3f * t * t) * vector2 + (t * t * t - t * t) * vector4;
        }
    }
}
