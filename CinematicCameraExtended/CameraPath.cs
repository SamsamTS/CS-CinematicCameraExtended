using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CinematicCameraExtended
{
    public class CameraPath : MonoBehaviour
    {
        public Transform cameraTransform;

        public System.Collections.Generic.List<Knot> knots = new System.Collections.Generic.List<Knot>();

        public Vector3[] positions;

        public bool playBack;

        public float time;

        public static float scrubTime;

        private void Start()
        {
            CameraPath.scrubTime = 0f;
        }

        public int AddKnot(Vector3 position, Quaternion rotation)
        {
            this.knots.Add(new Knot
            {
                rotation = rotation
            });
            Vector3[] array = new Vector3[this.knots.Count];
            for (int i = 0; i < this.knots.Count - 1; i++)
            {
                array[i] = this.positions[i];
            }
            array[this.knots.Count - 1] = position;
            this.positions = array;
            return this.knots.Count - 1;
        }

        public void RemoveKnot(int index)
        {
            this.knots.RemoveAt(index);
            Vector3[] array = new Vector3[this.knots.Count];
            for (int i = 0; i < index; i++)
            {
                array[i] = this.positions[i];
            }
            for (int j = index; j < this.knots.Count; j++)
            {
                array[j] = this.positions[j + 1];
            }
            this.positions = array;
        }

        public Knot GetKnot(int index)
        {
            return this.knots[index];
        }

        public Vector3 GetPosition(int index)
        {
            return this.positions[index];
        }

        public void SetPosition(int index, Vector3 position)
        {
            this.positions[index] = position;
        }

        public float CalculateTotalDuraction()
        {
            if (this.knots.Count == 0)
            {
                return 0f;
            }
            float num = 0f;
            foreach (Knot current in this.knots)
            {
                num += current.delay + current.duration;
            }
            return num - this.knots[this.knots.Count - 1].duration - 0.001f;
        }

        public void Play()
        {
            if (!this.playBack && this.knots.Count > 1)
            {
                this.time = 0f;
                this.playBack = true;
                this.cameraTransform.GetComponent<CameraController>().enabled = false;
            }
        }

        public void SetToTime(float time)
        {
            if (this.knots.Count > 1)
            {
                CameraPath.scrubTime = Time.time;
                if (!this.cameraTransform.GetComponent<CameraController>().enabled)
                {
                    CameraPath.MoveCamera(time, this.cameraTransform, this.knots, this.positions, out time);
                    return;
                }
                base.StartCoroutine(CameraPath.MoveCameraAsync(time, this.cameraTransform, this.knots, this.positions));
            }
        }

        public void Update()
        {
            if (this.playBack && !CameraPath.MoveCamera(this.time, this.cameraTransform, this.knots, this.positions, out this.time))
            {
                Stop();
            }
        }

        public void Stop()
        {
            this.playBack = false;
            this.cameraTransform.GetComponent<CameraController>().enabled = true;
        }

        public static System.Collections.IEnumerator MoveCameraAsync(float time, Transform camera, System.Collections.Generic.List<Knot> knots, Vector3[] points)
        {
            CameraController component = camera.GetComponent<CameraController>();
            component.enabled = false;
            yield return new WaitForSeconds(0.01f);
            float num;
            CameraPath.MoveCamera(time, camera, knots, points, out num);
            do
            {
                yield return new WaitForSeconds(0.05f);
            }
            while (Time.time - CameraPath.scrubTime < 0.03f || Input.GetMouseButton(0));
            component.enabled = true;
            CameraPath.SetCitiesCameraTransform(camera, camera.position, camera.rotation);
            yield break;
        }

        public static bool MoveCamera(float time, Transform camera, System.Collections.Generic.List<Knot> knots, Vector3[] points, out float timeOut)
        {
            bool flag = true;
            int num = 0;
            float num2 = 0f;
            for (int i = 0; i < knots.Count; i++)
            {
                Knot knot = knots[i];
                float num3 = knot.duration + knot.delay;
                if (time < num2 + num3)
                {
                    num = i;
                    break;
                }
                num2 += num3;
            }
            bool flag2 = false;
            if (num + 1 >= knots.Count)
            {
                num = knots.Count - 2;
                flag2 = true;
                if (time > num2 + knots[knots.Count - 1].delay)
                {
                    flag2 = false;
                    flag = false;
                    num = 0;
                }
            }
            float num4 = time - num2;
            if (num4 >= knots[num].delay && flag && !flag2)
            {
                num4 -= knots[num].delay;
                switch (knots[num].mode)
                {
                    case EasingMode.None:
                        num4 /= knots[num].duration;
                        break;
                    case EasingMode.EaseIn:
                        num4 = CameraPath.EaseInQuad(num4, 0f, 1f, knots[num].duration);
                        break;
                    case EasingMode.EaseOut:
                        num4 = CameraPath.EaseOutQuad(num4, 0f, 1f, knots[num].duration);
                        break;
                    case EasingMode.EaseInOut:
                        num4 = CameraPath.EaseInOutQuad(num4, 0f, 1f, knots[num].duration);
                        break;
                }
            }
            else
            {
                num4 = (float)(flag2 ? 1 : 0);
            }
            Vector3 position = Spline.CalculateSplinePosition(points, num, num4);
            Quaternion rotation = Quaternion.Slerp(knots[num].rotation, knots[num + 1].rotation, num4);
            camera.position = position;
            camera.rotation = rotation;
            time += Time.deltaTime;
            timeOut = time;
            return flag;
        }

        public static float EaseInOutQuad(float t, float b, float c, float d)
        {
            t /= d / 2f;
            if (t < 1f)
            {
                return c / 2f * t * t + b;
            }
            t -= 1f;
            return -c / 2f * (t * (t - 2f) - 1f) + b;
        }

        public static float EaseInQuad(float t, float b, float c, float d)
        {
            t /= d;
            return c * t * t + b;
        }

        public static float EaseOutQuad(float t, float b, float c, float d)
        {
            t /= d;
            return -c * t * (t - 2f) + b;
        }

        public static void SetCitiesCameraTransform(Transform camera, Vector3 position, Quaternion rotation)
        {
            CameraController component = camera.GetComponent<CameraController>();
            component.m_currentPosition = position;
            component.m_targetPosition = component.m_currentPosition;
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.Set(CameraPath.NormalizeAngle(eulerAngles.y), CameraPath.NormalizeAngle(eulerAngles.x), 0f);
            component.m_currentAngle = eulerAngles;
            component.m_targetAngle = eulerAngles;
        }

        public static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }
            return angle;
        }
    }
}