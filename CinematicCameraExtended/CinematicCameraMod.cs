using ICities;
using UnityEngine;

using ColossalFramework;

namespace CinematicCameraExtended
{
    public class CinematicCameraMod : LoadingExtensionBase
    {
        public GameObject cameraDirector;

        public CinematicCameraMod()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = "CinematicCameraExtended" } });
            }
            catch {}
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            this.cameraDirector = new GameObject("CinematicCameraExtended");
            this.cameraDirector.AddComponent<CameraDirector>();
        }

        public override void OnLevelUnloading()
        {
            if (this.cameraDirector != null)
            {
                UnityEngine.Object.Destroy(this.cameraDirector);
                this.cameraDirector = null;
            }
        }
    }
}

