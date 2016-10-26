using System;
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
            controllerPosition = CameraPath.cameraController.m_currentPosition;
            controllerAngle = CameraPath.cameraController.m_currentAngle;
            controllerSize = CameraPath.cameraController.m_currentSize;
            controllerHeight = CameraPath.cameraController.m_currentHeight;

            position = CameraPath.camera.transform.position;
            rotation = CameraPath.camera.transform.rotation;
            fov = CameraPath.camera.fieldOfView;
        }
    }
}