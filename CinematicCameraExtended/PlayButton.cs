using System;
using UnityEngine;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class PlayButton : MonoBehaviour
    {
        public Button button;

        public CameraPath path;

        private void Start()
        {
            this.button.onClick.AddListener(delegate
            {
                this.OnClick();
            });
        }

        private void OnClick()
        {
            CameraDirector.ClearFocus();
            this.path.Play();
        }
    }
}
