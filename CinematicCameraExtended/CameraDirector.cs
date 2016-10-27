using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class CameraDirector : MonoBehaviour
    {
        public static CameraPath cameraPath;
        public static UIMainWindow mainWindow;

        public static Camera camera;
        public static CameraController cameraController;

        public static bool freeCamera = true;
        public static float originalFov = 45;
        public static bool unlimitedCamera;

        public static object EZC_config;
        public static FieldInfo EZC_fovSmoothing;
        public static bool EZC_originalFovSmoothing;

        private void Start()
        {
            cameraPath = gameObject.AddComponent<CameraPath>();
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            cameraController = camera.GetComponent<CameraController>();

            unlimitedCamera = cameraController.m_unlimitedCamera;

            mainWindow = UIView.GetAView().AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;
            mainWindow.fastList.rowsData = cameraPath.knots;

            try
            {
                //var enhancedZoom = GameObject.FindObjectOfType(Type.GetType("EnhancedZoomContinued.EnhancedZoom")).GetType().GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                EZC_config = Type.GetType("EnhancedZoomContinued.EnhancedZoom").GetField("config", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                EZC_fovSmoothing = EZC_config.GetType().GetField("fovSmoothing");
            }
            catch { }
        }

        private void Update()
        {
            try
            {
                if (!UIView.HasModalInput() && !UIView.HasInputFocus())
                {
                    if (OptionsKeymapping.toggleUI.IsKeyUp())
                    {
                        if (cameraPath.playBack)
                        {
                            cameraPath.Stop();
                        }
                        else
                        {
                            CameraTool cameraTool = ToolsModifierControl.GetCurrentTool<CameraTool>();
                            if (cameraTool != null)
                            {
                                UIView.GetAView().FindUIComponent<UIButton>("Freecamera").SimulateClick();
                                mainWindow.isVisible = true;
                            }
                            else
                            {
                                mainWindow.isVisible = !mainWindow.isVisible;
                            }

                            if (mainWindow.isVisible)
                            {
                                camera.fieldOfView = mainWindow.fovSlider.value / 2f;
                                cameraController.m_unlimitedCamera = true;

                                if (EZC_fovSmoothing != null)
                                {
                                    EZC_originalFovSmoothing = (bool)EZC_fovSmoothing.GetValue(EZC_config);
                                    EZC_fovSmoothing.SetValue(EZC_config, false);
                                }
                            }
                            else
                            {
                                camera.fieldOfView = originalFov;
                                cameraController.m_unlimitedCamera = unlimitedCamera;

                                if (EZC_fovSmoothing != null)
                                {
                                    EZC_fovSmoothing.SetValue(EZC_config, EZC_originalFovSmoothing);
                                }
                            }
                        }
                    }
                    else if (OptionsKeymapping.addPoint.IsKeyUp())
                    {
                        if(mainWindow != null && mainWindow.isVisible)
                        {
                            mainWindow.addKnotButton.SimulateClick();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                DebugUtils.LogException(e);
            }
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
