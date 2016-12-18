using ICities;

using System;
using ColossalFramework;
using ColossalFramework.UI;

namespace CinematicCameraExtended
{
    public class CinematicCameraInfo : IUserMod
    {
        public string Name
        {
            get
            {
                return "Cinematic Camera Extended " + version; ;
            }
        }

        public string Description
        {
            get
            {
                return "Extended version of Icob's Cinematic Camera.";
            }
        }

        public CinematicCameraInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = CinematicCameraExtended.settingsFileName } });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public const string version = "0.5.0";
    }
}
