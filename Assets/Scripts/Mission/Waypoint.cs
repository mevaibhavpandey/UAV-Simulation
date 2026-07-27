using System;
using UnityEngine;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Enumerates the actions a UAV can execute at a specific waypoint.
    /// </summary>
    public enum WaypointAction
    {
        /// <summary>Standard fly-through waypoint without stopping.</summary>
        FlyThrough = 0,
        /// <summary>Hover at the waypoint for the specified hold time.</summary>
        Hover = 1,
        /// <summary>Initiate automatic takeoff sequence.</summary>
        Takeoff = 2,
        /// <summary>Initiate automatic landing sequence at waypoint location.</summary>
        Land = 3,
        /// <summary>Trigger camera shutter or sensor payload acquisition.</summary>
        TriggerPayload = 4,
        /// <summary>Initiate Return-To-Home (RTH) command.</summary>
        ReturnToHome = 5
    }

    /// <summary>
    /// Defines a single waypoint in a flight mission, including geographic/local coordinates and parameters.
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        [Tooltip("Latitude in decimal degrees (-90 to +90).")]
        public double Latitude;

        [Tooltip("Longitude in decimal degrees (-180 to +180).")]
        public double Longitude;

        [Tooltip("Target altitude above ground/launch level in meters.")]
        public float Altitude;

        [Tooltip("Target speed when traveling to this waypoint in m/s.")]
        public float TargetSpeed;

        [Tooltip("Hold time in seconds upon reaching this waypoint.")]
        public float HoldTime;

        [Tooltip("Action to execute at this waypoint.")]
        public WaypointAction Action;

        [Tooltip("Local Cartesian coordinate relative to mission origin in meters.")]
        public Vector3 LocalPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Waypoint"/> struct using local coordinates.
        /// </summary>
        /// <param name="localPosition">Local 3D position in meters.</param>
        /// <param name="targetSpeed">Target flight speed in m/s.</param>
        /// <param name="holdTime">Hold time in seconds.</param>
        /// <param name="action">Action to execute at waypoint.</param>
        public Waypoint(Vector3 localPosition, float targetSpeed = 5f, float holdTime = 0f, WaypointAction action = WaypointAction.FlyThrough)
        {
            Latitude = 0.0;
            Longitude = 0.0;
            Altitude = localPosition.y;
            TargetSpeed = Mathf.Max(0.5f, targetSpeed);
            HoldTime = Mathf.Max(0f, holdTime);
            Action = action;
            LocalPosition = localPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Waypoint"/> struct using geographic coordinates.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <param name="altitude">Altitude in meters.</param>
        /// <param name="targetSpeed">Target speed in m/s.</param>
        /// <param name="holdTime">Hold time in seconds.</param>
        /// <param name="action">Action to execute.</param>
        public Waypoint(double latitude, double longitude, float altitude, float targetSpeed = 5f, float holdTime = 0f, WaypointAction action = WaypointAction.FlyThrough)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            TargetSpeed = Mathf.Max(0.5f, targetSpeed);
            HoldTime = Mathf.Max(0f, holdTime);
            Action = action;
            LocalPosition = new Vector3((float)longitude, altitude, (float)latitude);
        }

        /// <summary>
        /// Calculates Cartesian distance to another waypoint.
        /// </summary>
        /// <param name="other">Target waypoint.</param>
        /// <returns>Distance in meters.</returns>
        public float DistanceTo(Waypoint other)
        {
            return Vector3.Distance(LocalPosition, other.LocalPosition);
        }
    }

    /// <summary>
    /// Alias struct for Waypoint data — used in Mission Planner UI and GCS.
    /// </summary>
    [Serializable]
    public struct WaypointData
    {
        // Primary fields (used by GCSMissionPlannerUI)
        public double latitude;
        public double longitude;
        public float  altitudeMSL;
        public float  targetSpeed;
        public float  holdDurationSeconds;
        public WaypointActionType action;

        // Convenience aliases
        public double Latitude    { get => latitude;    set => latitude = value; }
        public double Longitude   { get => longitude;   set => longitude = value; }
        public float  Altitude    { get => altitudeMSL; set => altitudeMSL = value; }
        public float  TargetSpeed { get => targetSpeed; set => targetSpeed = value; }
        public float  HoldTime    { get => holdDurationSeconds; set => holdDurationSeconds = value; }
        public Vector3 LocalPosition;

        public WaypointData(Vector3 localPosition, float speed = 5f, float hold = 0f)
        {
            latitude           = 0.0;
            longitude          = 0.0;
            altitudeMSL        = localPosition.y;
            targetSpeed        = Mathf.Max(0.5f, speed);
            holdDurationSeconds = Mathf.Max(0f, hold);
            action             = WaypointActionType.Hover;
            LocalPosition      = localPosition;
        }
    }

    /// <summary>
    /// Action types for waypoint execution — used by GCS Mission Planner.
    /// </summary>
    public enum WaypointActionType
    {
        FlyThrough   = 0,
        Hover        = 1,
        Takeoff      = 2,
        Land         = 3,
        TriggerPayload = 4,
        ReturnToHome = 5
    }
}




