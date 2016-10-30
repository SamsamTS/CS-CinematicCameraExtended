using UnityEngine;

namespace CinematicCameraExtended
{
    public enum EasingMode
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public class Knot
    {
        public Vector3 controllerPosition;
        public Vector2 controllerAngle;
        public float controllerSize;
        public float controllerHeight;

        public Vector3 position;
        public Quaternion rotation;

        public float duration = 2f;
        public float delay;
        public float fov;

        public EasingMode mode = EasingMode.EaseInOut;

        public Knot()
        {
            controllerPosition = CameraDirector.cameraController.m_currentPosition;
            controllerAngle = CameraDirector.cameraController.m_currentAngle;
            controllerSize = CameraDirector.cameraController.m_currentSize;
            controllerHeight = CameraDirector.cameraController.m_currentHeight;

            position = CameraDirector.camera.transform.position;
            rotation = CameraDirector.camera.transform.rotation;
            fov = CameraDirector.camera.fieldOfView;
        }
    }
}