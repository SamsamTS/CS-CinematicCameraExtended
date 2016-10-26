using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace CinematicCameraExtended
{
    public class CameraPath : MonoBehaviour
    {
        public static Camera camera;
        public static CameraController cameraController;

        public FastList<object> knots = new FastList<object>();

        public bool playBack;
        public float time;

        public static bool freeCamera = true;
        public static float originalFov;

        public static float scrubTime;
        public static Knot currentTransfrom;

        private object m_simulationFrameLock;
        private MethodInfo simulationStep;

        private void Start()
        {
            CameraPath.scrubTime = 0f;
            simulationStep = typeof(SimulationManager).GetMethod("SimulationStep", BindingFlags.NonPublic | BindingFlags.Instance);
            m_simulationFrameLock = typeof(SimulationManager).GetField("m_simulationFrameLock", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SimulationManager.instance);
        }

        public int AddKnot()
        {
            knots.Add(new Knot());
            return knots.m_size - 1;
        }

        public float CalculateTotalDuraction()
        {
            if (knots.m_size == 0)
            {
                return 0f;
            }
            float num = 0f;
            for (int i = 0; i < knots.m_size ; i++ )
            {
                Knot knot = (Knot)knots.m_buffer[i];
                num += knot.delay + knot.duration;
            }
            return num - ((Knot)knots.m_buffer[knots.m_size - 1]).duration - 0.001f;
        }

        public void Play()
        {
            if (!playBack && knots.m_size > 1)
            {
                /*while (!Monitor.TryEnter(m_simulationFrameLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }
                try
                {
                    SimulationManager.instance.m_simulationThread.Suspend();
                }
                finally
                {
                    Monitor.Exit(m_simulationFrameLock);
                }*/

                currentTransfrom = new Knot();

                CameraDirector.mainWindow.isVisible = false;
                SetFreeCamera(freeCamera);

                if(freeCamera)
                {
                    ((Knot)knots.m_buffer[0]).delay += 0.5f;
                    Cursor.visible = false;
                }

                time = 0f;
                playBack = true;
                camera.GetComponent<CameraController>().enabled = false;
            }
        }

        public void SetToTime(float time)
        {
            if (knots.m_size > 1)
            {
                CameraPath.scrubTime = Time.time;
                if (!cameraController.enabled)
                {
                    CameraPath.MoveCamera(time, camera, knots, out time);
                    return;
                }
                base.StartCoroutine(CameraPath.MoveCameraAsync(time, camera, knots));
            }
        }

        public void Update()
        {
            if (playBack)
            {
                if (!CameraPath.MoveCamera(time, camera, knots, out time))
                {
                    Stop();
                }
                /*else
                {
                    simulationStep.Invoke(SimulationManager.instance, null);
                }*/
            }
        }

        public void Stop()
        {
            CameraDirector.mainWindow.isVisible = true;
            SetFreeCamera(false);

            if (freeCamera)
            {
                ((Knot)knots.m_buffer[0]).delay -= 0.5f;
                Cursor.visible = true;
            }

            playBack = false;
            cameraController.enabled = true;
            camera.fieldOfView = originalFov;
            /*SimulationManager.instance.m_simulationThread.Resume();*/
        }

        public static System.Collections.IEnumerator MoveCameraAsync(float time, Camera camera, FastList<object> knots)
        {
            cameraController.enabled = false;
            yield return new WaitForSeconds(0.01f);
            float num;
            CameraPath.MoveCamera(time, camera, knots, out num);
            do
            {
                yield return new WaitForSeconds(0.05f);
            }
            while (Time.time - CameraPath.scrubTime < 0.03f || Input.GetMouseButton(0));
            cameraController.enabled = true;
            CameraPath.SetCitiesCameraTransform(currentTransfrom);
            yield break;
        }

        public static bool MoveCamera(float time, Camera camera, FastList<object> knots, out float timeOut)
        {
            bool flag = true;
            int num = 0;
            float num2 = 0f;
            for (int i = 0; i < knots.m_size; i++)
            {
                Knot knot = (Knot)knots.m_buffer[i];
                float num3 = knot.duration + knot.delay;
                if (time < num2 + num3)
                {
                    num = i;
                    break;
                }
                num2 += num3;
            }
            bool flag2 = false;
            if (num + 1 >= knots.m_size)
            {
                num = knots.m_size - 2;
                flag2 = true;
                if (time > num2 + ((Knot)knots.m_buffer[knots.m_size - 1]).delay)
                {
                    flag2 = false;
                    flag = false;
                    num = 0;
                }
            }
            float t = time - num2;
            if (t >= ((Knot)knots.m_buffer[num]).delay && flag && !flag2)
            {
                t -= ((Knot)knots.m_buffer[num]).delay;
                switch (((Knot)knots.m_buffer[num]).mode)
                {
                    case EasingMode.None:
                        t /= ((Knot)knots.m_buffer[num]).duration;
                        break;
                    case EasingMode.EaseIn:
                        t = CameraPath.EaseInQuad(t, 0f, 1f, ((Knot)knots.m_buffer[num]).duration);
                        break;
                    case EasingMode.EaseOut:
                        t = CameraPath.EaseOutQuad(t, 0f, 1f, ((Knot)knots.m_buffer[num]).duration);
                        break;
                    case EasingMode.EaseInOut:
                        t = CameraPath.EaseInOutQuad(t, 0f, 1f, ((Knot)knots.m_buffer[num]).duration);
                        break;
                }
            }
            else
            {
                t = (float)(flag2 ? 1 : 0);
            }
            Vector3 position = Spline.CalculateSplinePosition(knots.m_buffer, knots.m_size, num, t);
            Quaternion rotation = Quaternion.Slerp(((Knot)knots.m_buffer[num]).rotation, ((Knot)knots.m_buffer[num + 1]).rotation, t);
            float fov = Mathf.Lerp(((Knot)knots.m_buffer[num]).fov, ((Knot)knots.m_buffer[num + 1]).fov, t);

            camera.transform.position = position;
            camera.transform.rotation = rotation;
            camera.fieldOfView = fov;

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

        public static void SetCitiesCameraTransform(Knot knot)
        {
            cameraController.m_currentPosition = knot.controllerPosition;
            cameraController.m_targetPosition = knot.controllerPosition;
            cameraController.m_currentAngle = knot.controllerAngle;
            cameraController.m_targetAngle = knot.controllerAngle;
            camera.fieldOfView = knot.fov;
            CameraDirector.mainWindow.fovSlider.value = knot.fov * 2f;
        }

        public static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }
            return angle;
        }

        public static void SetFreeCamera(bool value)
        {
            if (UIView.isVisible == value)
            {
                UIView.Show(!value);
                NotificationManager.instance.NotificationsVisible = !value;
                GameAreaManager.instance.BordersVisible = !value;
                DistrictManager.instance.NamesVisible = !value;
                PropManager.instance.MarkersVisible = !value;
                GuideManager.instance.TutorialDisabled = value;

                if (value)
                {
                    camera.rect = new Rect(0f, 0f, 1f, 1f);
                }
                else
                {
                    camera.rect = new Rect(0f, 0.105f, 1f, 0.895f);
                }
            }
        }
    }
}