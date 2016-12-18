using UnityEngine;
using ColossalFramework.UI;

using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended.GUI
{
    public class UISaveLoadFileRow : UIPanel, IUIFastListRow
    {
        public UILabel fileNameLabel;

        public UIButton saveButton;
        public UIButton loadButton;
        public UIButton deleteButton;

        private UIPanel m_background;

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
                    m_background.width = width;
                    m_background.height = 40;
                    m_background.relativePosition = Vector2.zero;

                    m_background.zOrder = 0;
                }

                return m_background;
            }
        }

        public override void Awake()
        {
            height = 46;

            fileNameLabel = AddUIComponent<UILabel>();
            fileNameLabel.textScale = 0.9f;
            fileNameLabel.autoSize = false;
            fileNameLabel.height = 30;
            fileNameLabel.verticalAlignment = UIVerticalAlignment.Middle;
            fileNameLabel.relativePosition = new Vector3(8, 8);

            deleteButton = UIUtils.CreateButton(this);
            deleteButton.name = "CCX_DeleteFileButton";
            deleteButton.text = "X";
            deleteButton.size = new Vector2(40f, 30f);
            deleteButton.relativePosition = new Vector3(430 - deleteButton.width - 8, 8);
            deleteButton.tooltip = "Delete saved path";

            saveButton = UIUtils.CreateButton(this);
            saveButton.name = "CCX_SaveFileButton";
            saveButton.text = "Save";
            saveButton.size = new Vector2(80f, 30f);
            saveButton.relativePosition = new Vector3(deleteButton.relativePosition.x - saveButton.width - 8, 8);
            saveButton.tooltip = "Save path";

            loadButton = UIUtils.CreateButton(this);
            loadButton.name = "CCX_LoadFileButton";
            loadButton.text = "Load";
            loadButton.size = new Vector2(80f, 30f);
            loadButton.relativePosition = new Vector3(saveButton.relativePosition.x - loadButton.width - 8, 8);
            loadButton.tooltip = "Load path";

            saveButton.eventClicked += (c, p) =>
            {
                CameraDirector.cameraPath.Serialize(fileNameLabel.text);
            };

            loadButton.eventClicked += (c, p) =>
            {
                CameraDirector.cameraPath.Deserialize(fileNameLabel.text);
                CameraDirector.mainWindow.RefreshKnotList();
            };

            deleteButton.eventClicked += (c, p) =>
            {
                CameraDirector.cameraPath.Delete(fileNameLabel.text);
                CameraDirector.mainWindow.saveLoadWindow.RefreshFileList();
            };

            fileNameLabel.width = loadButton.relativePosition.x - 16f;
        }

        public void Display(object data, int i)
        {
            fileNameLabel.text = (string)data;

            if (i % 2 == 1)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
                background.width = parent.width;
            }
            else
            {
                background.backgroundSprite = null;
            }
        }
        public void Select(bool isRowOdd)
        {

        }
        public void Deselect(bool isRowOdd)
        {

        }
    }
}
