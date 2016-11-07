using ColossalFramework.UI;
using System;
using System.Reflection;
using UnityEngine;
using CinematicCameraExtended.GUI;

namespace CinematicCameraExtended
{
    public class CameraDirector : MonoBehaviour
    {
        public static CameraPath cameraPath;
        public static UIMainButton mainButton;
        public static UIMainWindow mainWindow;

        public static Camera camera;
        public static CameraController cameraController;

        public static bool freeCamera = true;
        public static bool startSimulation = false;
        public static bool useFps = false;
        public static float fps = 15;
        public static float originalFov = 45;
        public static bool unlimitedCamera;

        public static FieldInfo m_notificationAlpha;

        public static object EZC_config;
        public static FieldInfo EZC_fovSmoothing;
        public static bool EZC_originalFovSmoothing;

        private void Start()
        {
            EnsureUIComponentsLayout();

            cameraPath = gameObject.AddComponent<CameraPath>();
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            cameraController = camera.GetComponent<CameraController>();

            unlimitedCamera = cameraController.m_unlimitedCamera;

            m_notificationAlpha = typeof(NotificationManager).GetField("m_notificationAlpha", BindingFlags.NonPublic | BindingFlags.Instance);

            UIView view = UIView.GetAView();

            mainButton = view.AddUIComponent(typeof(UIMainButton)) as UIMainButton;
            mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

            try
            {
                EZC_config = Type.GetType("EnhancedZoomContinued.EnhancedZoom, EnhancedZoom").GetField("config", BindingFlags.Public | BindingFlags.Static).GetValue(null);
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

                    if (mainWindow != null && mainWindow.isVisibleSelf)
                    {
                        if (OptionsKeymapping.addPoint.IsKeyUp())
                        {
                            mainWindow.addKnotButton.SimulateClick();
                        }
                        else if(OptionsKeymapping.removePoint.IsKeyUp())
                        {
                            cameraPath.RemoveKnot();
                            mainWindow.RefreshKnotList();
                        }
                        else if (OptionsKeymapping.play.IsKeyUp())
                        {
                            mainWindow.playButton.SimulateClick();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);
            }
        }

        public static void ToggleUI()
        {
            if (cameraPath.playBack)
            {
                DebugUtils.Log("Stopping playback");
                cameraPath.Stop();
            }
            else
            {
                CameraTool cameraTool = ToolsModifierControl.GetCurrentTool<CameraTool>();
                if (cameraTool != null)
                {
                    DebugUtils.Log("Exiting Free Camera Mode");
                    UIView.GetAView().FindUIComponent<UIButton>("Freecamera").SimulateClick();
                    mainWindow.isVisible = true;
                }
                else
                {
                    mainWindow.isVisible = !mainWindow.isVisible;
                }

                if (mainWindow.isVisible)
                {
                    DebugUtils.Log("Showing main window");
                    camera.fieldOfView = mainWindow.fovSlider.value / 2f;
                    cameraController.m_unlimitedCamera = true;
                    cameraController.m_minDistance = 5;

                    if (EZC_fovSmoothing != null)
                    {
                        EZC_originalFovSmoothing = (bool)EZC_fovSmoothing.GetValue(EZC_config);
                        EZC_fovSmoothing.SetValue(EZC_config, false);
                    }
                }
                else
                {
                    DebugUtils.Log("Hiding main window");
                    camera.fieldOfView = originalFov;
                    cameraController.m_unlimitedCamera = unlimitedCamera;
                    cameraController.m_minDistance = 40;

                    if (EZC_fovSmoothing != null)
                    {
                        EZC_fovSmoothing.SetValue(EZC_config, EZC_originalFovSmoothing);
                    }
                }
            }
        }

        public static void SetFreeCamera(bool value)
        {
            if (UIView.isVisible == value)
            {
                try
                {
                    EnsureUIComponentsLayout();
                    UIView.Show(!value);
                }
                finally
                {
                    NotificationManager.instance.NotificationsVisible = !value;
                    GameAreaManager.instance.BordersVisible = !value;
                    DistrictManager.instance.NamesVisible = !value;
                    PropManager.instance.MarkersVisible = !value;
                    GuideManager.instance.TutorialDisabled = value;

                    if (value)
                    {
                        camera.rect = new Rect(0f, 0f, 1f, 1f);
                        m_notificationAlpha.SetValue(NotificationManager.instance, 0f);
                    }
                    else
                    {
                        camera.rect = new Rect(0f, 0.105f, 1f, 0.895f);
                    }
                }
            }
        }

        public static void EnsureUIComponentsLayout()
        {
            UIComponent[] componentsInChildren = UIView.GetAView().GetComponentsInChildren<UIComponent>();

            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (componentsInChildren[i] != null)
                {
                    // This ensure component layout is created
                    // To avoid exception when OnResolutionChanged is called
                    UIAnchorStyle anchor = componentsInChildren[i].anchor;
                }
            }
        }
    }
}
