using System;
using System.IO;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

using UIUtils = SamsamTS.UIUtils;

namespace CinematicCameraExtended.GUI
{
    public class UISaveLoadWindow : UIPanel
    {
        public static readonly SavedInt savedWindowX = new SavedInt("saveLoadWindowX", CinematicCameraExtended.settingsFileName, -1000, true);
        public static readonly SavedInt savedWindowY = new SavedInt("saveLoadWindowY", CinematicCameraExtended.settingsFileName, -1000, true);

        public UIFastList fastList;

        public UITextField fileNameInput;
        public UIButton saveButton;

        public UIButton close;

        public override void Start()
        {
            name = "CCX_SaveLoadWindow";
            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.target = parent;
            dragHandle.relativePosition = Vector3.zero;

            close = AddUIComponent<UIButton>();
            close.size = new Vector2(30f,30f);
            close.text = "X";
            close.textScale = 0.9f;
            close.textColor = new Color32(118, 123, 123, 255);
            close.focusedTextColor = new Color32(118, 123, 123, 255);
            close.hoveredTextColor = new Color32(140, 142, 142, 255);
            close.pressedTextColor = new Color32(99, 102, 102, 102);
            close.textPadding = new RectOffset(8, 8, 8, 8);
            close.canFocus = false;
            close.playAudioEvents = true;
            close.relativePosition = new Vector3(width - close.width, 0);

            close.eventClicked += (c, p) =>
            {
                isVisible = false;
                Destroy(gameObject);
            };

            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Save/Load Path";
            label.relativePosition = new Vector2(8, 8);
            label.SendToBack();

            // Save Panel
            UIPanel savePanel = AddUIComponent<UIPanel>();
            savePanel.atlas = atlas;
            savePanel.backgroundSprite = "GenericPanel";
            savePanel.color = new Color32(206, 206, 206, 255);
            savePanel.size = new Vector2(width - 16, 46);
            savePanel.relativePosition = new Vector2(8, 28);

            // Input
            fileNameInput = UIUtils.CreateTextField(savePanel);
            fileNameInput.padding.top = 7;
            fileNameInput.horizontalAlignment = UIHorizontalAlignment.Left;
            fileNameInput.relativePosition = new Vector3(8, 8);

            // Save
            saveButton = UIUtils.CreateButton(savePanel);
            saveButton.name = "CCX_SaveButton";
            saveButton.text = "Save";
            saveButton.size = new Vector2(100f, 30f);
            saveButton.relativePosition = new Vector3(savePanel.width - saveButton.width - 8, 8);

            fileNameInput.size = new Vector2(saveButton.relativePosition.x - 16f, 30f);

            // FastList
            fastList = UIFastList.Create<UISaveLoadFileRow>(this);
            fastList.backgroundSprite = "UnlockingPanel";
            fastList.width = width - 16;
            fastList.height = 46 * 5;
            fastList.canSelect = true;
            fastList.relativePosition = new Vector3(8, savePanel.relativePosition.y + savePanel.height + 8);

            fastList.rowHeight = 46f;

            saveButton.eventClicked += (c, p) =>
            {
                string filename = fileNameInput.text.Trim();
                filename = String.Concat(filename.Split(Path.GetInvalidFileNameChars()));

                if(!fileNameInput.text.IsNullOrWhiteSpace())
                {
                    CameraDirector.cameraPath.Serialize(filename);
                    fileNameInput.text = "";
                    RefreshFileList();
                }
            };

            height = fastList.relativePosition.y + fastList.height + 8;
            dragHandle.size = size;
            absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);
            MakePixelPerfect();

            RefreshFileList();

            fileNameInput.Focus();
        }

        protected override void OnPositionChanged()
        {
            Vector2 resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
                MakePixelPerfect();
            }

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            savedWindowX.value = (int)absolutePosition.x;
            savedWindowY.value = (int)absolutePosition.y;

            base.OnPositionChanged();
        }

        public void RefreshFileList()
        {
            fastList.rowsData.Clear();

            if(Directory.Exists(CinematicCameraExtended.saveFolder))
            {
                string[] files = Directory.GetFiles(CinematicCameraExtended.saveFolder, "*.xml");

                foreach (string file in files)
                {
                    fastList.rowsData.Add(Path.GetFileNameWithoutExtension(file));
                }

                fastList.DisplayAt(0);
            }
        }
    }
}
