using System;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Operating mode for the simulation camera controller.
    /// </summary>
    public enum CameraMode
    {
        ThirdPersonFollow,
        FirstPersonGimbal,
        Orbit,
        FreeLook,
        FixedGround,
        ThirdPerson,
        FPV,
        TopView,
        Free,
        Cinematic
    }

    /// <summary>
    /// Contract for controlling simulation camera views, tracking, and onboard camera gimbals.
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Gets the current camera operational mode.
        /// </summary>
        CameraMode CurrentMode { get; }

        /// <summary>
        /// Gets or sets the main target transform for camera tracking.
        /// </summary>
        Transform TargetTransform { get; set; }

        /// <summary>
        /// Gets or sets the field of view in degrees.
        /// </summary>
        float FieldOfView { get; set; }

        /// <summary>
        /// Gets the current orientation of the onboard camera gimbal.
        /// </summary>
        Quaternion GimbalOrientation { get; }

        /// <summary>
        /// Fired when the active camera mode changes.
        /// </summary>
        event Action<CameraMode> OnCameraModeChanged;

        /// <summary>
        /// Switches the active camera mode.
        /// </summary>
        /// <param name="mode">Target camera mode.</param>
        void SetMode(CameraMode mode);

        /// <summary>
        /// Directs camera tracking to a specific target transform.
        /// </summary>
        /// <param name="target">Transform target.</param>
        void SetTarget(Transform target);

        /// <summary>
        /// Sets camera zoom ratio normalized [0.0 to 1.0].
        /// </summary>
        /// <param name="zoomNormalized">Zoom value.</param>
        void SetZoom(float zoomNormalized);

        /// <summary>
        /// Sets onboard gimbal target pitch, roll, and yaw angles in degrees.
        /// </summary>
        /// <param name="pitch">Pitch angle (-90 to +90 deg).</param>
        /// <param name="roll">Roll angle (-180 to +180 deg).</param>
        /// <param name="yaw">Yaw angle (-180 to +180 deg).</param>
        void SetGimbalAngles(float pitch, float roll, float yaw);

        /// <summary>
        /// Resets camera offset, angles, and tracking parameters to defaults.
        /// </summary>
        void ResetCamera();
    }
}





