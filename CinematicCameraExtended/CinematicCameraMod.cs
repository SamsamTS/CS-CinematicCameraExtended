using ICities;
using System;
using UnityEngine;

namespace CinematicCameraExtended
{
    public class CinematicCameraMod : LoadingExtensionBase
    {
        public GameObject cameraDirector;

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

