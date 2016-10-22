using System;
using UnityEngine;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class ListItem : MonoBehaviour
    {
        public int index;

        public Button focusButton;

        public Button easingButton;

        public InputField durationInput;

        public InputField delayInput;

        public Button resetButton;

        public Button removeButton;

        public Slider timelineSlider;

        public CameraPath cameraPath;

        private void Start()
        {
            this.focusButton.onClick.AddListener(delegate
            {
                this.FocusClick();
            });
            this.easingButton.onClick.AddListener(delegate
            {
                this.EasingClick();
            });
            this.durationInput.onEndEdit.AddListener(delegate(string text)
            {
                this.DurationEdit(text);
            });
            this.delayInput.onEndEdit.AddListener(delegate(string text)
            {
                this.DelayEdit(text);
            });
            this.resetButton.onClick.AddListener(delegate
            {
                this.ResetClick();
            });
            this.removeButton.onClick.AddListener(delegate
            {
                this.RemoveClick();
            });
        }

        private void EasingClick()
        {
            CameraDirector.ClearFocus();
            Knot knot = this.cameraPath.GetKnot(this.index);
            switch (knot.mode)
            {
                case EasingMode.None:
                    this.SetEasingMode(EasingMode.EaseIn, "In");
                    return;
                case EasingMode.EaseIn:
                    this.SetEasingMode(EasingMode.EaseOut, "Out");
                    return;
                case EasingMode.EaseOut:
                    this.SetEasingMode(EasingMode.EaseInOut, "InOut");
                    return;
                case EasingMode.EaseInOut:
                    this.SetEasingMode(EasingMode.None, "None");
                    return;
                default:
                    return;
            }
        }

        private void SetEasingMode(EasingMode mode, string text)
        {
            Knot knot = this.cameraPath.GetKnot(this.index);
            knot.mode = mode;
            this.easingButton.GetComponentInChildren<Text>().text = text;
        }

        private void DurationEdit(string text)
        {
            CameraDirector.ClearFocus();
            Knot knot = this.cameraPath.GetKnot(this.index);
            float.TryParse(text, out knot.duration);
        }

        private void DelayEdit(string text)
        {
            CameraDirector.ClearFocus();
            Knot knot = this.cameraPath.GetKnot(this.index);
            float.TryParse(text, out knot.delay);
        }

        private void FocusClick()
        {
            CameraDirector.ClearFocus();
            Knot knot = this.cameraPath.GetKnot(this.index);
            Vector3 position = this.cameraPath.GetPosition(this.index);
            CameraPath.SetCitiesCameraTransform(this.cameraPath.cameraTransform, position, knot.rotation);
        }

        private void ResetClick()
        {
            CameraDirector.ClearFocus();
            Knot knot = this.cameraPath.GetKnot(this.index);
            knot.rotation = this.cameraPath.cameraTransform.rotation;
            this.cameraPath.SetPosition(this.index, this.cameraPath.cameraTransform.position);
        }

        private void RemoveClick()
        {
            CameraDirector.ClearFocus();
            this.cameraPath.RemoveKnot(this.index);
            UnityEngine.Object.Destroy(base.gameObject);
            int childCount = base.transform.parent.childCount;
            int num = 0;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = base.transform.parent.GetChild(i);
                child.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -9f - (float)num * child.GetComponent<RectTransform>().rect.height);
                child.GetComponent<ListItem>().index = num;
                if (child != base.transform)
                {
                    num++;
                }
            }
            this.timelineSlider.minValue = 0f;
            float num2 = this.cameraPath.CalculateTotalDuraction();
            if (num2 < 0f)
            {
                num2 = 1f;
            }
            this.timelineSlider.maxValue = num2;
        }
    }
}