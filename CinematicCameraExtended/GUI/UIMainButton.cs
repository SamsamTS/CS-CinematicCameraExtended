using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended.GUI
{
    public class UIMainButton : UIButton
    {
        public static readonly SavedInt savedX = new SavedInt("mainButtonX", CinematicCameraExtended.settingsFileName, -1000, true);
        public static readonly SavedInt savedY = new SavedInt("mainButtonY", CinematicCameraExtended.settingsFileName, -1000, true);

        public override void Start()
        {
            LoadResources();

            UIComponent freeCameraButton = GetUIView().FindUIComponent<UIComponent>("Freecamera");

            name = "CCX_MainButton";
            tooltipBox = freeCameraButton.tooltipBox;
            tooltip = "Cinematic Camera Extended  " + CinematicCameraInfo.version;

            normalBgSprite = "OptionBase";
            disabledBgSprite = "OptionBaseDisabled";
            hoveredBgSprite = "OptionBaseHovered";
            pressedBgSprite = "OptionBasePressed";

            normalFgSprite = "ClapBoard";

            playAudioEvents = true;

            size = new Vector2(36, 36);

            if (savedX.value == -1000)
            {
                absolutePosition = new Vector2(freeCameraButton.absolutePosition.x - 2 * width - 19, freeCameraButton.absolutePosition.y);
            }
            else
            {
                absolutePosition = new Vector2(savedX.value, savedY.value);
            }
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                CameraDirector.ToggleUI();
            }

            base.OnClick(p);
        }

        private Vector3 m_deltaPos;
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

                m_deltaPos = absolutePosition - mousePos;
                BringToFront();
            }
        }


        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

                absolutePosition = mousePos + m_deltaPos;
                savedX.value = (int)absolutePosition.x;
                savedY.value = (int)absolutePosition.y;
            }
        }

        public void OnGUI()
        {
            if (!UIView.HasModalInput() && !UIView.HasInputFocus() && OptionsKeymapping.toggleUI.IsPressed(Event.current))
            {
                SimulateClick();
            }
        }

        private void LoadResources()
        {

            Texture2D[] textures = 
            {
                atlas["OptionBase"].texture,
                atlas["OptionBaseDisabled"].texture,
                atlas["OptionBaseHovered"].texture,
                atlas["OptionBasePressed"].texture
            };

            string[] spriteNames = new string[]
			{
				"ClapBoard"
			};

            atlas = ResourceLoader.CreateTextureAtlas("CinematicCameraExtended", spriteNames, "CinematicCameraExtended.Icons.");
            ResourceLoader.AddTexturesInAtlas(atlas, textures);

        }
    }
}
