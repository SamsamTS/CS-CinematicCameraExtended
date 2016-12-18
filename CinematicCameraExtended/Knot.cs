using UnityEngine;

using System.ComponentModel;

namespace CinematicCameraExtended
{
    public enum EasingMode
    {
        None,
        Auto,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public class Knot
    {
        public Vector3 position;
        public Quaternion rotation;
        public float size;
        public float height;

        [DefaultValue(2f)]
        public float duration = 2f;
        [DefaultValue(0f)]
        public float delay;
        [DefaultValue(45f)]
        public float fov;

        [DefaultValue(EasingMode.Auto)]
        public EasingMode mode = EasingMode.Auto;

        public Knot()
        {
            CaptureCamera();
        }

        public void CaptureCamera()
        {
            position = CameraDirector.cameraController.m_currentPosition;
            size = CameraDirector.cameraController.m_currentSize;
            height = CameraDirector.cameraController.m_currentHeight;
            fov = CameraDirector.camera.fieldOfView;

            float distance = size * (1f - height / CameraDirector.cameraController.m_maxDistance) / Mathf.Tan(0.0174532924f * fov);

            Vector2 angle = CameraDirector.cameraController.m_currentAngle;
            rotation = Quaternion.AngleAxis(angle.x, Vector3.up) * Quaternion.AngleAxis(angle.y, Vector3.right);

            Vector3 cameraPosition = position + rotation * new Vector3(0f, 0f, -distance);
            position.y += CalculateCameraHeightOffset(cameraPosition, distance);
        }

        public Vector3 cameraPosition
        {
            get
            {
                float distance = size * (1f - height / CameraDirector.cameraController.m_maxDistance) / Mathf.Tan(0.0174532924f * fov);

                Vector3 cameraPosition = position + rotation * new Vector3(0f, 0f, -distance);
                cameraPosition.y += CalculateCameraHeightOffset(cameraPosition, distance);

                return cameraPosition;
            }
        }

        private static float CalculateCameraHeightOffset(Vector3 worldPos, float distance)
        {
            float num = TerrainManager.instance.SampleRawHeightSmoothWithWater(worldPos, true, 2f);
            float num2 = num - worldPos.y;
            distance *= 0.45f;
            num2 = Mathf.Max(num2, -distance);
            num2 += distance * 0.375f * Mathf.Pow(1f + 1f / distance, -num2);
            num = worldPos.y + num2;
            ItemClass.Availability mode = ToolManager.instance.m_properties.m_mode;
            if ((mode & ItemClass.Availability.AssetEditor) == ItemClass.Availability.None)
            {
                ItemClass.Layer layer = ItemClass.Layer.Default;
                if ((mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
                {
                    layer |= ItemClass.Layer.Markers;
                }
                worldPos.y -= 5f;
                num = Mathf.Max(num, BuildingManager.instance.SampleSmoothHeight(worldPos, layer) + 5f);
                num = Mathf.Max(num, NetManager.instance.SampleSmoothHeight(worldPos) + 5f);
                num = Mathf.Max(num, PropManager.instance.SampleSmoothHeight(worldPos) + 5f);
                num = Mathf.Max(num, TreeManager.instance.SampleSmoothHeight(worldPos) + 5f);
                worldPos.y += 5f;
            }
            return num - worldPos.y;
        }
    }
}