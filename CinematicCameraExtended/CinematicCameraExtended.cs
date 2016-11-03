using ICities;
using UnityEngine;

using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.IO;

namespace CinematicCameraExtended
{
    public class CinematicCameraExtended : LoadingExtensionBase
    {
        public GameObject cameraDirector;

        public static readonly string settingsFileName = "CinematicCameraExtended";

        public static string saveFolder
        {
            get
            {
                return Path.Combine(DataLocation.localApplicationData, settingsFileName);
            }
        }

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

