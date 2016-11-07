using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;


namespace CinematicCameraExtended
{
    public class CameraPath : MonoBehaviour
    {
        public FastList<object> knots = new FastList<object>();

        public bool playBack;
        public float time;

        public bool stopping;

        public static float scrubTime;
        public static Knot currentTransfrom;

        private object m_simulationFrameLock;
        private MethodInfo simulationStep;

        private bool simPaused;

        private float frequency = Stopwatch.Frequency / 1000f;
        private float waitTimeTarget;

        private long stepcount;
        private long startTime;

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

        public void RemoveKnot()
        {
            if(knots.m_size > 0)
            {
                knots.m_size--;
            }
        }

        public float CalculateTotalDuraction()
        {
            if (knots.m_size == 0)
            {
                return 0f;
            }
            float num = 0f;
            for (int i = 0; i < knots.m_size; i++)
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
                currentTransfrom = new Knot();

                CameraDirector.mainWindow.isVisible = false;
                CameraDirector.SetFreeCamera(CameraDirector.freeCamera);

                if (CameraDirector.freeCamera)
                {
                    Cursor.visible = false;
                }

                time = 0f;
                playBack = true;
                CameraDirector.camera.GetComponent<CameraController>().enabled = false;

                if (CameraDirector.startSimulation)
                {
                    simPaused = SimulationManager.instance.SimulationPaused;
                    SimulationManager.instance.SimulationPaused = false;
                }

                if (CameraDirector.useFps)
                {
                    while (!Monitor.TryEnter(m_simulationFrameLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        SimulationManager.instance.m_simulationThread.Suspend();
                    }
                    finally
                    {
                        Monitor.Exit(m_simulationFrameLock);
                    }

                    waitTimeTarget = 1000 / CameraDirector.fps;
                    stepcount = 0;
                }
            }
        }

        public void Stop()
        {
            CameraDirector.mainWindow.isVisible = true;
            CameraDirector.SetFreeCamera(false);

            if (CameraDirector.freeCamera)
            {
                Cursor.visible = true;
            }

            playBack = false;
            CameraDirector.cameraController.enabled = true;
            CameraDirector.camera.fieldOfView = CameraDirector.mainWindow.fovSlider.value / 2f;

            if (CameraDirector.startSimulation)
            {
                SimulationManager.instance.SimulationPaused = simPaused;
            }

            if (CameraDirector.useFps)
            {
                SimulationManager.instance.m_simulationThread.Resume();
            }
        }

        public void SetToTime(float time)
        {
            if (knots.m_size > 1)
            {
                CameraPath.scrubTime = Time.time;
                if (!CameraDirector.cameraController.enabled)
                {
                    CameraPath.MoveCamera(time, CameraDirector.camera, knots, out time);
                    return;
                }
                base.StartCoroutine(CameraPath.MoveCameraAsync(time, CameraDirector.camera, knots));
            }
        }

        public void Update()
        {
            if (playBack)
            {
                if (CameraDirector.useFps)
                {
                    if (stepcount == 0)
                    {
                        startTime = Stopwatch.GetTimestamp();
                    }

                    if (!CameraPath.MoveCamera(time, CameraDirector.camera, knots, out time))
                    {
                        Stop();
                        return;
                    }
                    else
                    {
                        simulationStep.Invoke(SimulationManager.instance, null);
                        CameraDirector.camera.nearClipPlane = 1;
                    }

                    long expectedStart = startTime + (int)(waitTimeTarget * frequency * stepcount++);
                    int wait = (int)(waitTimeTarget - ((Stopwatch.GetTimestamp() - expectedStart) / frequency)) - 1;

                    if (wait > 0)
                    {
                        Thread.Sleep(wait);
                    }
                    while (playBack && (Stopwatch.GetTimestamp() - expectedStart) / frequency < waitTimeTarget) ;
                }
                else
                {
                    if (!CameraPath.MoveCamera(time, CameraDirector.camera, knots, out time))
                    {
                        Stop();
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (playBack)
            {
                CameraDirector.camera.nearClipPlane = 1;
            }
        }

        public static System.Collections.IEnumerator MoveCameraAsync(float time, Camera camera, FastList<object> knots)
        {
            CameraDirector.cameraController.enabled = false;
            yield return new WaitForSeconds(0.01f);
            float num;
            CameraPath.MoveCamera(time, camera, knots, out num);
            do
            {
                yield return new WaitForSeconds(0.05f);
            }
            while (Time.time - CameraPath.scrubTime < 0.03f || Input.GetMouseButton(0));
            CameraDirector.cameraController.enabled = true;
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
            CameraDirector.cameraController.m_currentPosition = knot.controllerPosition;
            CameraDirector.cameraController.m_targetPosition = knot.controllerPosition;

            CameraDirector.cameraController.m_currentAngle = knot.controllerAngle;
            CameraDirector.cameraController.m_targetAngle = knot.controllerAngle;

            CameraDirector.cameraController.m_currentHeight = knot.controllerHeight;
            CameraDirector.cameraController.m_targetHeight = knot.controllerHeight;

            CameraDirector.cameraController.m_currentSize = knot.controllerSize;
            CameraDirector.cameraController.m_targetSize = knot.controllerSize;

            CameraDirector.camera.fieldOfView = knot.fov;
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
    }
}