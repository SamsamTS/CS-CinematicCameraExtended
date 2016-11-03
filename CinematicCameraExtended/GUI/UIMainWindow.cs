using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended.GUI
{
    public class UIMainWindow : UIPanel
    {
        public static readonly SavedInt savedWindowX = new SavedInt("windowX", CinematicCameraExtended.settingsFileName, Screen.width / 2, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", CinematicCameraExtended.settingsFileName, Screen.height / 2, true);

        public UIButton addKnotButton;
        public UIButton playButton;
        public UISlider timelineSlider;

        public UISlider fovSlider;
        public UITextField fovInput;

        public UICheckBox fpsCheckBox;
        public UITextField fpsInput;

        public UICheckBox hideUICheckBox;
        public UICheckBox startSimCheckBox;
        public UIFastList fastList;

        public UIButton saveLoadButton;

        public override void Awake()
        {
            isVisible = false;

            name = "CCX_MainWindow";
            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);
            absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.target = parent;
            dragHandle.relativePosition = Vector3.zero;

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
            addKnotButton.tooltip = "Add a new point to the path";

            playButton = UIUtils.CreateButton(controlPanel);
            playButton.name = "CCX_Play";
            playButton.textScale = 1.5f;
            playButton.text = ">";
            playButton.size = new Vector2(40f, 30f);
            playButton.playAudioEvents = false;
            playButton.relativePosition = new Vector3(controlPanel.width - playButton.width - 8, 8);
            playButton.tooltip = "Play the current path";

            timelineSlider = controlPanel.AddUIComponent<UISlider>();
            timelineSlider.name = "CCX_TimelineSlider";
            timelineSlider.size = new Vector2(playButton.relativePosition.x - addKnotButton.relativePosition.x - addKnotButton.width - 20, 18);
            timelineSlider.relativePosition = new Vector2(addKnotButton.relativePosition.x + addKnotButton.width + 10, 14);

            UISlicedSprite bgSlider = timelineSlider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(timelineSlider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);
            bgSlider.tooltip = "Drag the slider to preview the animation";

            UISlicedSprite thumb = timelineSlider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = atlas;
            thumb.spriteName = "SliderBudget";
            timelineSlider.thumbObject = thumb;

            timelineSlider.minValue = 0f;
            timelineSlider.maxValue = 1f;
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
            fovInput.text = (2f * CameraDirector.camera.fieldOfView).ToString();
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
            bgSlider.tooltip = "Drag the slider to change the field of view";

            thumb = fovSlider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = atlas;
            thumb.spriteName = "SliderBudget";
            fovSlider.thumbObject = thumb;

            fovSlider.minValue = 20f;
            fovSlider.maxValue = 179f;
            fovSlider.stepSize = 1f;
            fovSlider.value = CameraDirector.camera.fieldOfView * 2f;

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
            fpsInput.text = CameraDirector.fps.ToString();
            fpsInput.numericalOnly = true;
            fpsInput.allowFloats = true;
            fpsInput.padding.top = 6;
            fpsInput.relativePosition = new Vector3(fpsPanel.width - fpsInput.width - 8, 10);
            fpsInput.tooltip = "Fps";
            fpsInput.tooltip = "Sync the camera and the simulation at a set frame rate";

            fpsCheckBox = UIUtils.CreateCheckBox(fpsPanel);
            fpsCheckBox.text = "Fps:";
            fpsCheckBox.isChecked = false;
            fpsCheckBox.width = fpsPanel.width - fpsInput.width - 24;
            fpsCheckBox.relativePosition = new Vector2(8, 16);
            fpsCheckBox.tooltip = "Sync the camera and the simulation at a set frame rate";

            // Hide UI checkbox
            hideUICheckBox = UIUtils.CreateCheckBox(this);
            hideUICheckBox.name = "CCX_HideUICheckBox";
            hideUICheckBox.text = "Hide UI during playback";
            hideUICheckBox.isChecked = true;
            hideUICheckBox.width = width - 16;
            hideUICheckBox.relativePosition = new Vector2(8, fovPanel.relativePosition.y + fovPanel.height + 8);

            // Start simulation checkbox
            startSimCheckBox = UIUtils.CreateCheckBox(this);
            startSimCheckBox.name = "CCX_StartSimCheckBox";
            startSimCheckBox.text = "Unpause simulation";
            startSimCheckBox.isChecked = false;
            startSimCheckBox.width = (width - 16) / 2;
            startSimCheckBox.relativePosition = new Vector2(8, hideUICheckBox.relativePosition.y + hideUICheckBox.height + 8);

            // FastList
            fastList = UIFastList.Create<UIKnotsListRow>(this);
            fastList.backgroundSprite = "UnlockingPanel";
            fastList.width = width - 16;
            fastList.height = 46 * 5;
            fastList.canSelect = true;
            fastList.relativePosition = new Vector3(8, startSimCheckBox.relativePosition.y + startSimCheckBox.height + 8);

            fastList.rowHeight = 46f;
            fastList.DisplayAt(0);

            // Load/Save
            saveLoadButton = UIUtils.CreateButton(this);
            saveLoadButton.name = "CCX_SaveLoadButton";
            saveLoadButton.text = "Save/Load";
            saveLoadButton.size = new Vector2(100f, 30f);
            saveLoadButton.isEnabled = false;
            saveLoadButton.relativePosition = new Vector3(width - saveLoadButton.width - 8, fastList.relativePosition.y + fastList.height + 8);
            saveLoadButton.tooltip = "Work in progress";

            height = saveLoadButton.relativePosition.y + saveLoadButton.height + 8;
            dragHandle.size = size;

            addKnotButton.eventClicked += (c, p) =>
            {
                int i = CameraDirector.cameraPath.AddKnot();
                fastList.DisplayAt(i);

                timelineSlider.minValue = 0f;
                float duration = CameraDirector.cameraPath.CalculateTotalDuraction();
                if (duration <= 0f)
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
                CameraDirector.freeCamera = p;
            };

            startSimCheckBox.eventCheckChanged += (c, p) =>
            {
                CameraDirector.startSimulation = p;
            };

            fovSlider.eventValueChanged += (c, p) =>
            {
                CameraDirector.camera.fieldOfView = p / 2f;
                fovInput.text = p.ToString();
            };

            fovInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value))
                {
                    CameraDirector.camera.fieldOfView = Mathf.Clamp(value, 20f, 179f) / 2f;

                    if (fovSlider.value != value)
                    {
                        fovSlider.value = value;
                    }
                }

                fovInput.text = (2f * CameraDirector.camera.fieldOfView).ToString();
            };

            fovInput.eventMouseWheel += (c, p) =>
            {
                if (!p.used)
                {
                    fovSlider.value += p.wheelDelta;
                    p.Use();
                }
            };

            fpsCheckBox.eventCheckChanged += (c, p) =>
            {
                CameraDirector.useFps = p;
            };

            fpsInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value))
                {
                    CameraDirector.fps = Mathf.Max(0, value);
                }

                fpsInput.text = CameraDirector.fps.ToString();
            };

            fpsInput.eventMouseWheel += (c, p) =>
            {
                if (!p.used)
                {
                    CameraDirector.fps = Mathf.Max(0, CameraDirector.fps + p.wheelDelta);
                    fpsInput.text = CameraDirector.fps.ToString();
                    p.Use();
                }
            };
        }

        protected override void OnPositionChanged()
        {
            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((Screen.width - width) / 2, (Screen.height - height) / 2);
            }
            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, Screen.width - width),
                Mathf.Clamp(absolutePosition.y, 0, Screen.height - height));

            savedWindowX.value = (int)absolutePosition.x;
            savedWindowY.value = (int)absolutePosition.y;

            base.OnPositionChanged();
        }

        public void RefreshKnotList()
        {
            if (fastList != null)
            {
                fastList.DisplayAt(fastList.listPosition);

                float duration = CameraDirector.cameraPath.CalculateTotalDuraction();
                if (duration <= 0f)
                {
                    duration = 1f;
                }
                timelineSlider.maxValue = duration;
            }
        }
    }
}
