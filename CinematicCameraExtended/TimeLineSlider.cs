using System;
using UnityEngine;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class TimeLineSlider : MonoBehaviour
    {
        public Slider slider;

        public CameraPath cameraPath;

        private void Start()
        {
            this.slider.onValueChanged.AddListener(delegate(float value)
            {
                this.SliderValue(value);
            });
        }

        private void SliderValue(float value)
        {
            CameraDirector.ClearFocus();
            this.cameraPath.SetToTime(value);
        }
    }
}
