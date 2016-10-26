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
        public static readonly SavedInputKey toggleUI = new SavedInputKey("toggleUI", CinematicCameraExtended.settingsFileName, SavedInputKey.Encode(KeyCode.C, false, false, false), false);
        
        public static CameraPath cameraPath;
        public static UIMainWindow mainWindow;

        private void Start()
        {
            cameraPath = CameraDirector.CreateCameraPath();

            mainWindow = UIView.GetAView().AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;
            mainWindow.fastList.rowsData = cameraPath.knots;
        }

        private void Update()
        {
            try
            {
                if (toggleUI.IsKeyUp())
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

                        if(!mainWindow.isVisible)
                        {
                            CameraPath.camera.fieldOfView = CameraPath.originalFov;
                        }
                        else
                        {
                            CameraPath.camera.fieldOfView = mainWindow.fovSlider.value / 2f;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                DebugUtils.LogException(e);
            }
        }

        public static CameraPath CreateCameraPath()
        {
            GameObject gameObject = new GameObject("CameraDirector");
            CameraPath cameraPath = gameObject.AddComponent<CameraPath>();
            CameraPath.camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            CameraPath.cameraController = CameraPath.camera.GetComponent<CameraController>();
            CameraPath.originalFov = CameraPath.camera.fieldOfView;
            return cameraPath;
        }
    }
}
