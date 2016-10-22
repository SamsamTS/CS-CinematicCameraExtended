using System;
using UnityEngine;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class AddKnotButton : MonoBehaviour
    {
        public Button button;

        public CameraPath path;

        public Transform cameraTransform;

        public Transform panel;

        public Transform listItemPrefab;

        public Slider timelineSlider;

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
            int num = this.path.AddKnot(this.cameraTransform.transform.position, this.cameraTransform.transform.rotation);
            Transform transform = UnityEngine.Object.Instantiate(this.listItemPrefab, this.listItemPrefab.position, Quaternion.identity) as Transform;
            transform.gameObject.SetActive(true);
            transform.gameObject.layer = 5;
            transform.SetParent(this.panel, false);
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -9f - (float)(this.panel.childCount - 1) * this.listItemPrefab.GetComponent<RectTransform>().rect.height);
            ListItem listItem = transform.gameObject.AddComponent<ListItem>();
            listItem.index = num;
            listItem.timelineSlider = this.timelineSlider;
            listItem.cameraPath = this.path;
            listItem.focusButton = CameraDirector.FindChildByName(transform, "Name").GetComponent<Button>();
            listItem.easingButton = CameraDirector.FindChildByName(transform, "Easing").GetComponent<Button>();
            listItem.durationInput = CameraDirector.FindChildByName(transform, "Duration").GetComponent<InputField>();
            listItem.delayInput = CameraDirector.FindChildByName(transform, "Delay").GetComponent<InputField>();
            listItem.resetButton = CameraDirector.FindChildByName(transform, "Set").GetComponent<Button>();
            listItem.removeButton = CameraDirector.FindChildByName(transform, "Remove").GetComponent<Button>();
            listItem.focusButton.GetComponentInChildren<Text>().text = "Point " + num;
            this.timelineSlider.minValue = 0f;
            float num2 = this.path.CalculateTotalDuraction();
            if (num2 < 0f)
            {
                num2 = 1f;
            }
            this.timelineSlider.maxValue = num2;
        }
    }
}
