using ICities;
using UnityEngine;

using System;
using ColossalFramework;

namespace CinematicCameraExtended
{
    public class CinematicCameraExtended : LoadingExtensionBase
    {
        public GameObject cameraDirector;

        public static readonly string settingsFileName = "CinematicCameraExtended";

        public CinematicCameraExtended()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (cameraDirector == null)
            {
                cameraDirector = new GameObject("CinematicCameraExtended");
                cameraDirector.AddComponent<CameraDirector>();
            }
        }

        public override void OnLevelUnloading()
        {
            if (cameraDirector != null)
            {
                UnityEngine.Object.Destroy(cameraDirector);
                cameraDirector = null;
            }
        }
    }
}

