using UnityEngine;

namespace CinematicCameraExtended
{
    public class Spline
    {
        public static float CalculateSplineT(object[] points, int size, int i, float t)
        {
            float duration = ((Knot)points[i]).duration;

            float startDirection;
            if (i > 0 && ((Knot)points[i]).delay == 0)
            {
                startDirection = 2f * duration / (duration + ((Knot)points[i - 1]).duration);
            }
            else
            {
                startDirection = 0;
            }
            float endDirection;
            if (i < size - 2 && ((Knot)points[i + 1]).delay == 0)
            {
                endDirection = 2f * duration / (duration + ((Knot)points[i + 1]).duration);
            }
            else
            {
                endDirection = 0;
            }

            return (t * t * t - 2f * t * t + t) * startDirection + (-2f * t * t * t + 3f * t * t) + (t * t * t - t * t) * endDirection;
        }

        public static Vector3 CalculateSplinePosition(object[] points, int size, int i, float t)
        {
            Vector3 startPosition = ((Knot)points[i]).position;
            Vector3 endPosition = ((Knot)points[i + 1]).position;
            Vector3 startDirection;
            if (i > 0)
            {
                startDirection = 0.5f * (endPosition - ((Knot)points[i - 1]).position);
            }
            else
            {
                startDirection = endPosition - startPosition;
            }
            Vector3 endDirection;
            if (i < size - 2)
            {
                endDirection = 0.5f * (((Knot)points[i + 2]).position - startPosition);
            }
            else
            {
                endDirection = endPosition - startPosition;
            }
            return (2f * t * t * t - 3f * t * t + 1f) * startPosition + (t * t * t - 2f * t * t + t) * startDirection + (-2f * t * t * t + 3f * t * t) * endPosition + (t * t * t - t * t) * endDirection;
        }

        /*public static Quaternion CalculateSplineRotation(object[] points, int size, int i, float t)
        {
            Vector4 startPosition = ToV4(((Knot)points[i]).rotation);
            Vector4 endPosition = ToV4(((Knot)points[i + 1]).rotation);

            float dot = Vector4.Dot(startPosition, endPosition);
            if (dot < 0f)
            {
                endPosition = endPosition * -1f;
            }

            Vector4 startDirection;
            if (i > 0)
            {
                Vector4 a = endPosition;
                Vector4 b = ToV4(((Knot)points[i - 1]).rotation);
                startDirection = 0.5f * (a - b);
            }
            else
            {

                Vector4 a = endPosition;
                Vector4 b = ToV4(((Knot)points[i]).rotation);
                startDirection = a - b;
            }
            Vector4 endDirection;
            if (i < size - 2)
            {
                Vector4 a = ToV4(((Knot)points[i + 2]).rotation);
                Vector4 b = ToV4(((Knot)points[i]).rotation);

                float dot2 = Vector4.Dot(a, endPosition);

                if (dot2 < 0f)
                {
                    endDirection = 0.5f * (b - a);
                }
                else
                {
                    endDirection = 0.5f * (a - b);
                }
            }
            else
            {
                Vector4 a = endPosition;
                Vector4 b = ToV4(((Knot)points[i]).rotation);
                endDirection = a - b;
            }
            Vector4 angle = (2f * t * t * t - 3f * t * t + 1f) * startPosition + (t * t * t - 2f * t * t + t) * startDirection + (-2f * t * t * t + 3f * t * t) * endPosition + (t * t * t - t * t) * endDirection;
            return ToQuaternion(angle);
        }

        public static Vector4 ToV4(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        public static Quaternion ToQuaternion(Vector4 v)
        {
            Quaternion q;
            q.x = v.x;
            q.y = v.y;
            q.z = v.z;
            q.w = v.w;
            return q;
        }*/

        public static Quaternion CalculateSplineRotationEuler(object[] points, int size, int i, float t)
        {
            Vector3 startPosition = ((Knot)points[i]).rotation.eulerAngles;
            Vector3 endPosition = ClosestAngle(startPosition, ((Knot)points[i + 1]).rotation.eulerAngles);

            Vector3 startDirection;
            if (i > 0)
            {
                startDirection = 0.5f * (endPosition - ClosestAngle(startPosition, ((Knot)points[i - 1]).rotation.eulerAngles));
            }
            else
            {
                startDirection = endPosition - startPosition;
            }
            Vector3 endDirection;
            if (i < size - 2)
            {
                endDirection = 0.5f * (ClosestAngle(endPosition, ((Knot)points[i + 2]).rotation.eulerAngles) - startPosition);
            }
            else
            {
                endDirection = endPosition - startPosition;
            }

            Vector3 angle = (2f * t * t * t - 3f * t * t + 1f) * startPosition + (t * t * t - 2f * t * t + t) * startDirection + (-2f * t * t * t + 3f * t * t) * endPosition + (t * t * t - t * t) * endDirection;

            return Quaternion.Euler(angle);
        }

        public static Vector3 ClosestAngle(Vector3 a, Vector3 b)
        {
            Vector3 diff = a - b;

            if (diff.x > 180)
            {
                b.x = b.x + 360f;
            }
            else if (diff.x < -180)
            {
                b.x = b.x - 360f;
            }
            if (diff.y > 180)
            {
                b.y = b.y + 360f;
            }
            else if (diff.y < -180)
            {
                b.y = b.y - 360f;
            }
            if (diff.z > 180)
            {
                b.z = b.z + 360f;
            }
            else if (diff.z < -180)
            {
                b.z = b.z - 360f;
            }

            return b;
        }

        /*public static float CalculateSplineFov(object[] points, int size, int i, float t)
        {
            float startFov = ((Knot)points[i]).fov;
            float endFov = ((Knot)points[i + 1]).fov;
            float startDirection;
            if (i > 0)
            {
                startDirection = 0.5f * (((Knot)points[i + 1]).fov - ((Knot)points[i - 1]).fov);
            }
            else
            {
                startDirection = ((Knot)points[i + 1]).fov - ((Knot)points[i]).fov;
            }
            float endDirection;
            if (i < size - 2)
            {
                endDirection = 0.5f * (((Knot)points[i + 2]).fov - ((Knot)points[i]).fov);
            }
            else
            {
                endDirection = ((Knot)points[i + 1]).fov - ((Knot)points[i]).fov;
            }
            return (2f * t * t * t - 3f * t * t + 1f) * startFov + (t * t * t - 2f * t * t + t) * startDirection + (-2f * t * t * t + 3f * t * t) * endFov + (t * t * t - t * t) * endDirection;
        }*/
    }
}
