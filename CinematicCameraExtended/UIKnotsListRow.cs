using UnityEngine;
using ColossalFramework.UI;

using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended
{
    public class UIKnotsListRow : UIPanel, IUIFastListRow
    {
        public UIButton focusButton;
        public UIDropDown easingDropDown;

        public UITextField durationInput;
        public UITextField delayInput;
        public UITextField fovInput;

        public UIButton resetButton;
        public UIButton removeButton;

        public Knot knot;
        public int index;

        public override void Awake()
        {
            height = 46;

            focusButton = UIUtils.CreateButton(this);
            focusButton.name = "CCX_FocusButton";
            focusButton.text = "Point 0";
            focusButton.size = new Vector2(70f, 30f);
            focusButton.relativePosition = new Vector3(8, 8);
            focusButton.tooltip = "Click here to place the camera at this point";

            easingDropDown = UIUtils.CreateDropDown(this);
            easingDropDown.name = "CCX_EasingDropDown";
            easingDropDown.textScale = 0.9f;
            easingDropDown.AddItem("None");
            easingDropDown.AddItem("In");
            easingDropDown.AddItem("Out");
            easingDropDown.AddItem("InOut");
            easingDropDown.selectedValue = "InOut";
            easingDropDown.size = new Vector2(80f, 30f);
            easingDropDown.textFieldPadding.top = 7;
            easingDropDown.relativePosition = new Vector3(focusButton.relativePosition.x + focusButton.width + 8, 8);
            easingDropDown.tooltip = "Camera movement easing";

            durationInput = UIUtils.CreateTextField(this);
            durationInput.name = "CCX_DurationInput";
            durationInput.size = new Vector2(40f, 26f);
            durationInput.numericalOnly = true;
            durationInput.allowFloats = true;
            durationInput.padding.top = 6;
            durationInput.relativePosition = new Vector3(easingDropDown.relativePosition.x + easingDropDown.width + 8, 10);
            durationInput.tooltip = "Duration in seconds";

            delayInput = UIUtils.CreateTextField(this);
            delayInput.name = "CCX_DelayInput";
            delayInput.size = new Vector2(45f, 26f);
            delayInput.numericalOnly = true;
            delayInput.allowFloats = true;
            delayInput.padding.top = 6;
            delayInput.relativePosition = new Vector3(durationInput.relativePosition.x + durationInput.width + 8, 10);
            delayInput.tooltip = "Delay in seconds";

            fovInput = UIUtils.CreateTextField(this);
            fovInput.name = "CCX_DelayInput";
            fovInput.size = new Vector2(45f, 26);
            fovInput.numericalOnly = true;
            fovInput.allowFloats = true;
            fovInput.padding.top = 6;
            fovInput.relativePosition = new Vector3(delayInput.relativePosition.x + delayInput.width + 8, 10);
            fovInput.tooltip = "Field of view in degrees";

            resetButton = UIUtils.CreateButton(this);
            resetButton.name = "CCX_ResetButton";
            resetButton.text = "O";
            resetButton.size = new Vector2(45f, 30f);
            resetButton.relativePosition = new Vector3(fovInput.relativePosition.x + fovInput.width + 8, 8);
            resetButton.tooltip = "Recapture the camera position";

            removeButton = UIUtils.CreateButton(this);
            removeButton.name = "CCX_RemoveButton";
            removeButton.text = "X";
            removeButton.size = new Vector2(40f, 30f);
            removeButton.relativePosition = new Vector3(resetButton.relativePosition.x + resetButton.width + 8, 8);
            removeButton.tooltip = "Remove this point";

            focusButton.eventClicked += (c, p) =>
            {
                CameraPath.SetCitiesCameraTransform(knot);
            };

            easingDropDown.eventSelectedIndexChanged += (c, p) =>
            {
                knot.mode = (EasingMode)p;
            };

            durationInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value) && value > 0)
                {
                    knot.duration = value;
                }

                durationInput.text = knot.duration.ToString();
            };

            durationInput.eventMouseWheel += (c, p) =>
            {
                float value;
                if (float.TryParse(durationInput.text, out value))
                {
                    knot.duration = Mathf.Max(0, value + p.wheelDelta);
                    durationInput.text = knot.duration.ToString();
                }
                p.Use();
            };

            delayInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value))
                {
                    knot.delay = Mathf.Max(0, value);
                }

                delayInput.text = knot.delay.ToString();
            };

            delayInput.eventMouseWheel += (c, p) =>
            {
                float value;
                if (float.TryParse(delayInput.text, out value))
                {
                    knot.delay = Mathf.Max(0, value + p.wheelDelta);
                    delayInput.text = knot.delay.ToString();
                }
                p.Use();
            };

            fovInput.eventTextSubmitted += (c, p) =>
            {
                float value;
                if (float.TryParse(p, out value))
                {
                    knot.fov = Mathf.Clamp(value, 20f, 179f) / 2f;
                }

                fovInput.text = (2f * knot.fov).ToString();
            };

            fovInput.eventMouseWheel += (c, p) =>
            {
                float value;
                if (float.TryParse(fovInput.text, out value))
                {
                    knot.fov = Mathf.Clamp(value, 20f, 179f) / 2f;
                    fovInput.text = (2f * knot.fov).ToString();
                }
                p.Use();
            };

            resetButton.eventClicked += (c, p) =>
            {
                knot.rotation = CameraDirector.camera.transform.rotation;
                knot.position = CameraDirector.camera.transform.position;
                knot.fov = CameraDirector.camera.fieldOfView;
                fovInput.text = (2f * knot.fov).ToString();
            };

            removeButton.eventClicked += (c, p) =>
            {
                CameraDirector.cameraPath.knots.RemoveAt(index);
                CameraDirector.mainWindow.RefreshKnotList();
            };
        }

        public void Display(object data, int i)
        {
            knot = data as Knot;
            index = i;

            focusButton.text = "Point " + index;
            easingDropDown.selectedIndex = (int)knot.mode;
            durationInput.text = knot.duration.ToString();
            delayInput.text = knot.delay.ToString();
            fovInput.text = (2f * knot.fov).ToString();
        }
        public void Select(bool isRowOdd)
        {

        }
        public void Deselect(bool isRowOdd)
        {

        }
    }
}
