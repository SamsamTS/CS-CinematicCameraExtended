using System;
using System.Collections;
using System.Reflection;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

using SamsamTS;
using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended
{
    public class UIMainWindow : UIPanel
    {
        public static readonly SavedInt savedWindowX = new SavedInt("windowX", CinematicCameraExtended.settingsFileName, -1000, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", CinematicCameraExtended.settingsFileName, -1000, true);

        public UIButton addKnotButton;
        public UIButton playButton;
        public UISlider timelineSlider;

        public UISlider fovSlider;
        public UITextField fovInput;

        public UICheckBox fpsCheckBox;
        public UITextField fpsInput;

        public UICheckBox hideUICheckBox;
        public UIFastList fastList;

        public override void Awake()
        {
            isVisible = false;

            name = "CCX_MainWindow";
            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);
            absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.size = size;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = parent;

            // Control panel
            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Cinematic Camera Extended";
            label.relativePosition = new Vector2(8, 8);
            label.SendToBack();

            UIPanel controlPanel = AddUIComponent<UIPanel>();
            controlPanel.atlas = atlas;
            controlPanel.backgroundSprite = "GenericPanel";
            controlPanel.color = new Color32(206, 206, 206, 255);
            controlPanel.size = new Vector2(width - 16, 46);
            controlPanel.relativePosition = new Vector2(8, 28);

            addKnotButton = UIUtils.CreateButton(controlPanel);
            addKnotButton.name = "CCX_AddKnot";
            addKnotButton.textScale = 1.5f;
            addKnotButton.text = "+";
            addKnotButton.size = new Vector2(40f, 30f);
            addKnotButton.relativePosition = new Vector3(8, 8);

            playButton = UIUtils.CreateButton(controlPanel);
            playButton.name = "CCX_Play";
            playButton.textScale = 1.5f;
            playButton.text = ">";
            playButton.size = new Vector2(40f, 30f);
            playButton.playAudioEvents = false;
            playButton.relativePosition = new Vector3(controlPanel.width - playButton.width - 8, 8);

            timelineSlider = controlPanel.AddUIComponent<UISlider>();
            timelineSlider.name = "CCX_TimelineSlider";
            timelineSlider.size = new Vector2(playButton.relativePosition.x - addKnotButton.relativePosition.x - addKnotButton.width - 20, 18);
            timelineSlider.relativePosition = new Vector2(addKnotButton.relativePosition.x + addKnotButton.width + 10, 14);

            UISlicedSprite bgSlider = timelineSlider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(timelineSlider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);

            UISlicedSprite thumb = timelineSlider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = atlas;
            thumb.spriteName = "SliderBudget";
            timelineSlider.thumbObject = thumb;

            timelineSlider.minValue = 0f;
            timelineSlider.maxValue = 10f;
            timelineSlider.stepSize = 0;
            timelineSlider.value = 0;

            // FOV panel
            UIPanel fovPanel = AddUIComponent<UIPanel>();
            fovPanel.atlas = atlas;
            fovPanel.backgroundSprite = "GenericPanel";
            fovPanel.color = new Color32(206, 206, 206, 255);
            fovPanel.size = new Vector2(width - 120 - 8 * 3, 46);
            fovPanel.relativePosition = new Vector2(8, controlPanel.relativePosition.y + controlPanel.height + 8);

            label = fovPanel.AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Fov:";
            label.autoSize = false;
            label.height = 30;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.relativePosition = new Vector2(8, 8);

            fovInput = UIUtils.CreateTextField(fovPanel);
            fovInput.name = "CCX_MainFovInput";
            fovInput.size = new Vector2(45f, 26);
            fovInput.text = (2f * CameraPath.camera.fieldOfView).ToString();
            fovInput.numericalOnly = true;
            fovInput.allowFloats = true;
            fovInput.padding.top = 6;
            fovInput.relativePosition = new Vector3(fovPanel.width - fovInput.width - 8, 10);
            fovInput.tooltip = "Field of view in degrees";

            fovSlider = fovPanel.AddUIComponent<UISlider>();
            fovSlider.name = "CCX_TimelineSlider";
            fovSlider.size = new Vector2(fovInput.relativePosition.x - label.relativePosition.x - label.width - 20, 18);
            fovSlider.relativePosition = new Vector2(label.relativePosition.x + label.width + 10, 14);

            bgSlider = fovSlider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(fovSlider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);

            thumb = fovSlider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = atlas;
            thumb.spriteName = "SliderBudget";
            fovSlider.thumbObject = thumb;

            fovSlider.minValue = 20f;
            fovSlider.maxValue = 179f;
            fovSlider.stepSize = 1f;
            fovSlider.value = CameraPath.camera.fieldOfView * 2f;

            // FPS panel
            UIPanel fpsPanel = AddUIComponent<UIPanel>();
            fpsPanel.atlas = atlas;
            fpsPanel.backgroundSprite = "GenericPanel";
            fpsPanel.color = new Color32(206, 206, 206, 255);
            fpsPanel.size = new Vector2(120, 46);
            fpsPanel.relativePosition = new Vector2(fovPanel.relativePosition.x + fovPanel.width + 8, fovPanel.relativePosition.y);

            fpsInput = UIUtils.CreateTextField(fpsPanel);
            fpsInput.name = "CCX_FpsInput";
            fpsInput.size = new Vector2(45f, 26);
            fpsInput.numericalOnly = true;
            fpsInput.allowFloats = true;
            fpsInput.padding.top = 6;
            fpsInput.relativePosition = new Vector3(fpsPanel.width - fpsInput.width - 8, 10);
            fpsInput.tooltip = "Fps";

            fpsCheckBox = UIUtils.CreateCheckBox(fpsPanel);
            fpsCheckBox.text = "Fps:";
            fpsCheckBox.isChecked = false;
            fpsCheckBox.width = fpsPanel.width - fpsInput.width - 24;
            fpsCheckBox.relativePosition = new Vector2(8, 16);
            fpsCheckBox.isEnabled = false;

            // Hide UI checkbox
            hideUICheckBox = UIUtils.CreateCheckBox(this);
            hideUICheckBox.text = "Hide UI during playback";
            hideUICheckBox.isChecked = true;
            hideUICheckBox.width = width - 16;
            hideUICheckBox.relativePosition = new Vector2(8, fovPanel.relativePosition.y + fovPanel.height + 8);

            // FastList
            fastList = UIFastList.Create<UIKnotsListRow>(this);
            fastList.backgroundSprite = "UnlockingPanel";
            fastList.width = width - 16;
            fastList.height = 46*5;
            fastList.canSelect = true;
            fastList.relativePosition = new Vector3(8, hideUICheckBox.relativePosition.y + hideUICheckBox.height + 8);

            fastList.rowHeight = 46f;
            fastList.DisplayAt(0);

            height = fastList.relativePosition.y + fastList.height + 8;

            addKnotButton.eventClicked += (c, p) =>
            {
                int i = CameraDirector.cameraPath.AddKnot();
                fastList.DisplayAt(i);

                timelineSlider.minValue = 0f;
                float duration = CameraDirector.cameraPath.CalculateTotalDuraction();
                if (duration < 0f)
                {
                    duration = 1f;
                }
                timelineSlider.maxValue = duration;
            };

            playButton.eventClicked += (c, p) =>
            {
                CameraDirector.cameraPath.Play();
            };

            timelineSlider.eventMouseDown += (c, p) =>
            {
                CameraPath.currentTransfrom = new Knot();
            };

            timelineSlider.eventValueChanged += (c, p) =>
            {
                CameraDirector.cameraPath.SetToTime(p);
            };

            hideUICheckBox.eventCheckChanged += (c, p) =>
            {
                CameraPath.freeCamera = p;
            };

            fovSlider.eventValueChanged += (c, p) =>
            {
                CameraPath.camera.fieldOfView = p / 2f;
                fovInput.text = p.ToString();
            };

            fovInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value))
                {
                    CameraPath.camera.fieldOfView = Mathf.Clamp(value, 20f, 179.9f) / 2f;

                    if(fovSlider.value != value)
                    {
                        fovSlider.value = value;
                    }
                }

                fovInput.text = (2f * CameraPath.camera.fieldOfView).ToString();
            };
        }

        public void RefreshKnotList()
        {
            if(fastList != null)
            {
                fastList.DisplayAt(fastList.listPosition);

                float duration = CameraDirector.cameraPath.CalculateTotalDuraction();
                if (duration < 0f)
                {
                    duration = 1f;
                }
                timelineSlider.maxValue = duration;
            }
        }
    }
}
