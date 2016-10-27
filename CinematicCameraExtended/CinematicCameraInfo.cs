using ICities;
using System;

using ColossalFramework.UI;

namespace CinematicCameraExtended
{
    public class CinematicCameraInfo : IUserMod
    {
        public string Name
        {
            get
            {
                return "CinematicCameraExtended";
            }
        }

        public string Description
        {
            get
            {
                return "Extended version of Icob's Cinematic Camera.";
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

        public const string version = "0.1.2";
    }
}
