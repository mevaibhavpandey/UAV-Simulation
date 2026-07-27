using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Represents the current status of a mission execution.
    /// </summary>
    public enum MissionState
    {
        /// <summary>No mission loaded or idle.</summary>
        Idle,
        /// <summary>Mission loaded and ready to start.</summary>
        Ready,
        /// <summary>Mission is actively executing.</summary>
        Executing,
        /// <summary>Mission execution is temporarily paused.</summary>
        Paused,
        /// <summary>Mission was completed successfully.</summary>
        Completed,
        /// <summary>Mission was aborted by pilot or failsafe.</summary>
        Aborted,
        /// <summary>Mission failed due to error or constraint violation.</summary>
        Failed
    }

    /// <summary>
    /// Type of action performed at a waypoint.
    /// </summary>
    public enum WaypointAction
    {
        /// <summary>Fly directly through without stopping.</summary>
        FlyThrough,
        /// <summary>Hover at waypoint for a set duration.</summary>
        Hover,
        /// <summary>Capture photo or sensor snapshot.</summary>
        TakePicture,
        /// <summary>Perform 360-degree yaw sweep.</summary>
        PanoramaScan,
        /// <summary>Execute automated landing at waypoint.</summary>
        Land
    }

    /// <summary>
    /// Data structure describing a mission waypoint.
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        /// <summary>Unique index of the waypoint in mission sequence.</summary>
        public int Index;

        /// <summary>Target position in local world coordinates (or converted GPS position).</summary>
        public Vector3 Position;

        /// <summary>Latitude in degrees (WGS84).</summary>
        public double Latitude;

        /// <summary>Longitude in degrees (WGS84).</summary>
        public double Longitude;

        /// <summary>Altitude in meters (MSL or AGL depending on mode).</summary>
        public float Altitude;

        /// <summary>Desired flight speed towards this waypoint (m/s).</summary>
        public float TargetSpeed;

        /// <summary>Heading angle in degrees (0..360).</summary>
        public float TargetHeading;

        /// <summary>Acceptance radius to consider waypoint reached (meters).</summary>
        public float AcceptanceRadius;

        /// <summary>Action to execute upon reaching this waypoint.</summary>
        public WaypointAction Action;

        /// <summary>Hold/hover duration in seconds if action is Hover.</summary>
        public float HoldTimeSeconds;
    }

    /// <summary>
    /// Defines contract for mission planning, waypoint management, and mission execution.
    /// </summary>
    public interface IMissionModule
    {
        /// <summary>
        /// Gets the current state of the mission.
        /// </summary>
        MissionState CurrentState { get; }

        /// <summary>
        /// Gets the index of the currently active waypoint.
        /// </summary>
        int CurrentWaypointIndex { get; }

        /// <summary>
        /// Gets the total number of waypoints in the current mission.
        /// </summary>
        int TotalWaypoints { get; }

        /// <summary>
        /// Gets mission completion progress normalized from [0.0 to 1.0].
        /// </summary>
        float ProgressNormalized { get; }

        /// <summary>
        /// Gets an read-only list of all waypoints in the loaded mission.
        /// </summary>
        IReadOnlyList<Waypoint> Waypoints { get; }

        /// <summary>
        /// Fired when a waypoint is reached by the drone.
        /// </summary>
        event Action<int, Waypoint> OnWaypointReached;

        /// <summary>
        /// Fired when the mission state changes.
        /// </summary>
        event Action<MissionState> OnMissionStateChanged;

        /// <summary>
        /// Fired when the overall mission completes.
        /// </summary>
        event Action OnMissionCompleted;

        /// <summary>
        /// Fired when the mission fails or is aborted.
        /// </summary>
        event Action<string> OnMissionFailed;

        /// <summary>
        /// Loads a list of waypoints into the mission module.
        /// </summary>
        /// <param name="waypoints">Waypoints sequence.</param>
        /// <returns>True if payload is valid and loaded successfully.</returns>
        bool LoadMission(IEnumerable<Waypoint> waypoints);

        /// <summary>
        /// Begins execution of the currently loaded mission.
        /// </summary>
        void StartMission();

        /// <summary>
        /// Pauses mission execution and commands drone to hover.
        /// </summary>
        void PauseMission();

        /// <summary>
        /// Resumes execution of a paused mission.
        /// </summary>
        void ResumeMission();

        /// <summary>
        /// Aborts the current mission immediately.
        /// </summary>
        void AbortMission();

        /// <summary>
        /// Clears all loaded waypoints and resets mission state.
        /// </summary>
        void ClearMission();

        /// <summary>
        /// Jumps mission execution to a specific waypoint index.
        /// </summary>
        /// <param name="index">Target waypoint index.</param>
        void SkipToWaypoint(int index);
    }
}


