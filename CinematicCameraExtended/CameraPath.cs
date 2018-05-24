using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using ColossalFramework;

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace CinematicCameraExtended
{
    public class CameraPath : MonoBehaviour
    {
        public FastList<object> knots = new FastList<object>();

        public static bool playBack;
        public static float time;

        public static float scrubTime;
        public static Knot currentTransfrom;

        private object m_simulationFrameLock;
        private MethodInfo simulationStep;

        private bool simPaused;

        private float frequency = Stopwatch.Frequency / 1000f;
        private float waitTimeTarget;

        private long stepcount;
        private long startTime;

        private static TiltShiftEffect m_tiltShift;
        private static DepthOfField m_depthOfField;

        private static SavedFloat tiltShiftAmount = new SavedFloat(Settings.tiltShiftAmount, Settings.gameSettingsFile, DefaultSettings.tiltShiftAmount, true);
        private static SavedInt DOFMode = new SavedInt(Settings.dofMode, Settings.gameSettingsFile, DefaultSettings.dofMode, true);

        private void Start()
        {
            CameraPath.scrubTime = 0f;
            simulationStep = typeof(SimulationManager).GetMethod("SimulationStep", BindingFlags.NonPublic | BindingFlags.Instance);
            m_simulationFrameLock = typeof(SimulationManager).GetField("m_simulationFrameLock", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SimulationManager.instance);

            m_tiltShift = CameraDirector.cameraController.GetComponent<TiltShiftEffect>();
            m_depthOfField = CameraDirector.cameraController.GetComponent<DepthOfField>();
        }

        public int AddKnot()
        {
            knots.Add(new Knot());
            return knots.m_size - 1;
        }

        public void RemoveKnot()
        {
            if (knots.m_size > 0)
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
                    CameraPath.MoveCamera(time, 1f, CameraDirector.camera, knots, out time);
                    return;
                }
                base.StartCoroutine(CameraPath.MoveCameraAsync(time, 1f, CameraDirector.camera, knots));
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

                    if (!CameraPath.MoveCamera(time, CameraDirector.mainWindow.playSpeed, CameraDirector.camera, knots, out time))
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
                    if (!CameraPath.MoveCamera(time, CameraDirector.mainWindow.playSpeed, CameraDirector.camera, knots, out time))
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

        public static System.Collections.IEnumerator MoveCameraAsync(float time, float speed, Camera camera, FastList<object> knots)
        {
            CameraDirector.cameraController.enabled = false;
            yield return new WaitForSeconds(0.01f);
            float num;
            CameraPath.MoveCamera(time, speed, camera, knots, out num);
            do
            {
                yield return new WaitForSeconds(0.05f);
            }
            while (Time.time - CameraPath.scrubTime < 0.03f || Input.GetMouseButton(0));
            CameraDirector.cameraController.enabled = true;
            CameraPath.SetCitiesCameraTransform(currentTransfrom);
            yield break;
        }

        public static bool MoveCamera(float time, float speed, Camera camera, FastList<object> knots, out float timeOut)
        {
            bool result = true;
            int index = 0;
            float duration = 0f;

            for (int i = 0; i < knots.m_size; i++)
            {
                Knot knot = (Knot)knots.m_buffer[i];
                float knotDuration = (knot.duration + knot.delay) / speed;
                if (time < duration + knotDuration)
                {
                    index = i;
                    break;
                }
                duration += knotDuration;
            }

            bool ended = false;
            if (index + 1 >= knots.m_size)
            {
                index = knots.m_size - 2;
                ended = true;
                if (time > duration + ((Knot)knots.m_buffer[knots.m_size - 1]).delay)
                {
                    ended = false;
                    result = false;
                    index = 0;
                }
            }

            Knot currentKnot = (Knot)knots.m_buffer[index];
            Knot nextKnot = (Knot)knots.m_buffer[index + 1];

            float t = time - duration;
            if (t >= currentKnot.delay && result && !ended)
            {
                t -= currentKnot.delay;
                switch (currentKnot.mode)
                {
                    case EasingMode.None:
                        t /= currentKnot.duration / speed;
                        break;
                    case EasingMode.EaseIn:
                        t = CameraPath.EaseInQuad(t, 0f, 1f, currentKnot.duration / speed);
                        break;
                    case EasingMode.EaseOut:
                        t = CameraPath.EaseOutQuad(t, 0f, 1f, currentKnot.duration / speed);
                        break;
                    case EasingMode.EaseInOut:
                        t = CameraPath.EaseInOutQuad(t, 0f, 1f, currentKnot.duration / speed);
                        break;
                    case EasingMode.Auto:
                        t = Spline.CalculateSplineT(knots.m_buffer, knots.m_size, index, t / (currentKnot.duration / speed));
                        break;
                }
            }
            else
            {
                t = ended ? 1f : 0f;
            }

            float fov = Mathf.Lerp(currentKnot.fov, nextKnot.fov, t);

            float distance1 = currentKnot.size * (1f - currentKnot.height / CameraDirector.cameraController.m_maxDistance) / Mathf.Tan(0.0174532924f * fov);
            float distance2 = nextKnot.size * (1f - nextKnot.height / CameraDirector.cameraController.m_maxDistance) / Mathf.Tan(0.0174532924f * fov);
            float distance = Mathf.Lerp(distance1, distance2, t);

            Quaternion rotation = Spline.CalculateSplineRotationEuler(knots.m_buffer, knots.m_size, index, t);
            Vector3 position = Spline.CalculateSplinePosition(knots.m_buffer, knots.m_size, index, t) + rotation * new Vector3(0f, 0f, -distance);

            camera.transform.position = position;
            camera.transform.rotation = rotation;
            camera.fieldOfView = fov;

            float size = Mathf.Lerp(currentKnot.size, nextKnot.size, t);

            ChangeTiltShift(size);
            ChangeDoF(size);

            time += Time.deltaTime;
            timeOut = time;
            return result;
        }

        public static void ChangeTiltShift(float size)
        {
            if (m_tiltShift != null)
            {
                m_tiltShift.enabled = !CameraDirector.cameraController.isTiltShiftDisabled;
                m_tiltShift.m_BlurArea = Mathf.Lerp(CameraDirector.cameraController.m_MaxTiltShiftArea, CameraDirector.cameraController.m_MinTiltShiftArea, Mathf.Clamp((size - CameraDirector.cameraController.m_minDistance) / CameraDirector.cameraController.m_MaxTiltShiftDistance, 0f, 1f)) * tiltShiftAmount;
                m_tiltShift.m_MaxBlurSize = /*CameraDirector.cameraController.m_DefaultMaxBlurSize*/ 5f * tiltShiftAmount;
            }
        }

        public static void ChangeDoF(float size)
        {
            if (m_depthOfField != null)
            {
                m_depthOfField.enabled = !CameraDirector.cameraController.isDepthOfFieldDisabled;

                if (DOFMode == 2)
                {
                    m_depthOfField.blurType = DepthOfField.BlurType.DiscBlur;
                }
                else if (DOFMode == 3)
                {
                    m_depthOfField.blurType = DepthOfField.BlurType.DX11;
                }

                float doftime = Mathf.Clamp((size - CameraDirector.cameraController.m_minDistance) / CameraDirector.cameraController.m_MaxTiltShiftDistance, 0f, 1f);
                m_depthOfField.focalLength = size + CameraDirector.cameraController.m_FocalLength.Evaluate(doftime);
                m_depthOfField.focalSize = CameraDirector.cameraController.m_FocalSize.Evaluate(doftime);
                m_depthOfField.aperture = CameraDirector.cameraController.m_Aperture.Evaluate(doftime);
                m_depthOfField.maxBlurSize = CameraDirector.cameraController.m_MaxBlurSize.Evaluate(doftime) * tiltShiftAmount;
            }
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
            CameraDirector.cameraController.m_currentPosition = knot.position;
            CameraDirector.cameraController.m_targetPosition = knot.position;

            Vector2 angle = new Vector2(knot.rotation.eulerAngles.y, knot.rotation.eulerAngles.x);
            CameraDirector.cameraController.m_currentAngle = angle;
            CameraDirector.cameraController.m_targetAngle = angle;

            CameraDirector.cameraController.m_currentHeight = knot.height;
            CameraDirector.cameraController.m_targetHeight = knot.height;

            CameraDirector.cameraController.m_currentSize = knot.size;
            CameraDirector.cameraController.m_targetSize = knot.size;

            CameraDirector.camera.fieldOfView = knot.fov;
            CameraDirector.mainWindow.fovSlider.value = knot.fov * 2f;
        }

        public void Serialize(string filename)
        {
            string fullPath = Path.Combine(CinematicCameraExtended.saveFolder, filename + ".xml");
            try
            {
                if (!Directory.Exists(CinematicCameraExtended.saveFolder))
                {
                    Directory.CreateDirectory(CinematicCameraExtended.saveFolder);
                }

                List<Knot> knotList = new List<Knot>();

                for (int i = 0; i < knots.m_size; i++)
                {
                    knotList.Add((Knot)knots.m_buffer[i]);
                }

                using (FileStream stream = new FileStream(fullPath, FileMode.OpenOrCreate))
                {
                    stream.SetLength(0); // Emptying the file !!!
                    XmlSerializer xmlSerializer = new XmlSerializer(knotList.GetType());
                    xmlSerializer.Serialize(stream, knotList);
                    DebugUtils.Log("Path saved");
                }
            }
            catch (Exception e)
            {
                DebugUtils.Warning("Couldn't save path at \"" + fullPath + "\"");
                DebugUtils.LogException(e);
            }
        }

        public void Deserialize(string filename)
        {
            string fullPath = Path.Combine(CinematicCameraExtended.saveFolder, filename + ".xml");
            if (File.Exists(fullPath))
            {

                List<Knot> knotList = new List<Knot>();
                try
                {
                    // Trying to Deserialize the configuration file
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(knotList.GetType());
                        knotList = xmlSerializer.Deserialize(stream) as List<Knot>;
                    }
                }
                catch (Exception e)
                {
                    // Couldn't Deserialize (XML malformed?)
                    DebugUtils.Warning("Couldn't load path (XML malformed?)");
                    DebugUtils.LogException(e);
                }

                if (knotList != null)
                {
                    knots.Clear();
                    foreach (Knot knot in knotList)
                    {
                        knots.Add(knot);
                    }
                }
            }

        }

        public void Delete(string filename)
        {
            string fullPath = Path.Combine(CinematicCameraExtended.saveFolder, filename + ".xml");
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}