using ICities;
using System;

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

        public const string version = "0.1.1";
    }
}
