//-----------------------------------------------------------------------
// <copyright file="DroneNavigator.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Enumerates specific behaviors associated with a navigation waypoint.
    /// </summary>
    public enum WaypointActionType
    {
        /// <summary>Fly straight through waypoint without stopping.</summary>
        FlyThrough,

        /// <summary>Hover at waypoint position for designated hold duration.</summary>
        HoverHold,

        /// <summary>Execute controlled landing sequence at waypoint position.</summary>
        Land,

        /// <summary>Execute takeoff sequence to designated altitude.</summary>
        Takeoff,

        /// <summary>Return to initial home position and land.</summary>
        ReturnToHome
    }

    /// <summary>
    /// Enumerates the operational execution status of an autonomous mission.
    /// </summary>
    public enum NavigationMissionState
    {
        /// <summary>No active navigation mission loaded or running.</summary>
        Idle,

        /// <summary>Currently navigating towards active target waypoint.</summary>
        NavigatingToWaypoint,

        /// <summary>Holding position at target waypoint location.</summary>
        HoldingAtWaypoint,

        /// <summary>Mission execution paused by pilot or safety system.</summary>
        Paused,

        /// <summary>Mission completed all waypoints successfully.</summary>
        Completed,

        /// <summary>Mission cancelled due to safety violation or disarm.</summary>
        Aborted
    }

    /// <summary>
    /// Represents a single spatial waypoint in an autonomous flight mission profile.
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        /// <summary>Target position coordinates in world space (meters).</summary>
        public Vector3 TargetPosition;

        /// <summary>Target cruise speed approaching waypoint (meters per second).</summary>
        public float CruiseSpeedMS;

        /// <summary>Hover hold duration at waypoint in seconds (if ActionType is HoverHold).</summary>
        public float HoldDurationSeconds;

        /// <summary>Spherical acceptance radius in meters to register waypoint achievement.</summary>
        public float AcceptanceRadiusMeters;

        /// <summary>Action type to execute upon arrival.</summary>
        public WaypointActionType ActionType;

        /// <summary>
        /// Initializes a new instance of the Waypoint struct with default parameters.
        /// </summary>
        public Waypoint(Vector3 position, float speed = 5f, float acceptanceRadius = 1.5f, WaypointActionType actionType = WaypointActionType.FlyThrough, float holdSeconds = 0f)
        {
            TargetPosition = position;
            CruiseSpeedMS = Mathf.Max(0.5f, speed);
            AcceptanceRadiusMeters = Mathf.Max(0.2f, acceptanceRadius);
            ActionType = actionType;
            HoldDurationSeconds = Mathf.Max(0f, holdSeconds);
        }
    }

    /// <summary>
    /// Autonomous navigation guidance module that manages waypoint mission queues,
    /// computes trajectory steering vectors, and provides control inputs to the flight controller.
    /// </summary>
    public class DroneNavigator : MonoBehaviour
    {
        [Header("Mission Configuration")]
        [SerializeField, Tooltip("List of active mission waypoints.")]
        private List<Waypoint> waypointQueue = new List<Waypoint>();

        [SerializeField, Tooltip("Default navigation cruise speed in m/s.")]
        private float defaultCruiseSpeed = 5.0f;

        [SerializeField, Tooltip("Default acceptance sphere radius around target waypoint in meters.")]
        private float defaultAcceptanceRadius = 1.5f;

        [SerializeField, Tooltip("Proportional gain for trajectory positioning loop.")]
        private float positionKp = 0.5f;

        [Header("Live Mission Status")]
        [SerializeField, Tooltip("Index of the active target waypoint in the queue.")]
        private int currentWaypointIndex = -1;

        [SerializeField, Tooltip("Current status of autonomous mission execution.")]
        private NavigationMissionState missionState = NavigationMissionState.Idle;

        private float currentHoldTimer = 0f;

        /// <summary>
        /// Gets the current state of autonomous mission execution.
        /// </summary>
        public NavigationMissionState MissionState => missionState;

        /// <summary>
        /// Gets the total number of waypoints in the active mission queue.
        /// </summary>
        public int WaypointCount => waypointQueue.Count;

        /// <summary>
        /// Gets the current zero-based index of the active target waypoint (-1 if idle).
        /// </summary>
        public int CurrentWaypointIndex => currentWaypointIndex;

        /// <summary>
        /// Gets the current active waypoint target, or null if no mission is active.
        /// </summary>
        public Waypoint? CurrentWaypoint => (currentWaypointIndex >= 0 && currentWaypointIndex < waypointQueue.Count) ? waypointQueue[currentWaypointIndex] : null;

        /// <summary>
        /// Fired when the active waypoint changes or is reached.
        /// </summary>
        public event Action<int, Waypoint> OnWaypointReached;

        /// <summary>
        /// Fired when the overall mission execution state changes.
        /// </summary>
        public event Action<NavigationMissionState> OnMissionStateChanged;

        /// <summary>
        /// Fired when all waypoints in the mission profile have been successfully executed.
        /// </summary>
        public event Action OnMissionCompleted;

        /// <summary>
        /// Clears all existing waypoints and loads a new list of waypoints into the mission queue.
        /// </summary>
        /// <param name="waypoints">Collection of mission waypoints to load.</param>
        public void LoadMission(IEnumerable<Waypoint> waypoints)
        {
            ClearMission();
            if (waypoints != null)
            {
                waypointQueue.AddRange(waypoints);
            }
        }

        /// <summary>
        /// Appends a new waypoint to the end of the current mission queue.
        /// </summary>
        /// <param name="waypoint">Waypoint to add.</param>
        public void AddWaypoint(Waypoint waypoint)
        {
            waypointQueue.Add(waypoint);
        }

        /// <summary>
        /// Clears all waypoints and resets mission state to Idle.
        /// </summary>
        public void ClearMission()
        {
            waypointQueue.Clear();
            currentWaypointIndex = -1;
            currentHoldTimer = 0f;
            SetState(NavigationMissionState.Idle);
        }

        /// <summary>
        /// Begins execution of the loaded mission starting at waypoint index 0.
        /// </summary>
        /// <returns>True if mission started successfully, false if queue is empty.</returns>
        public bool StartMission()
        {
            if (waypointQueue.Count == 0)
            {
                Debug.LogWarning("[DroneNavigator] Cannot start mission: Waypoint queue is empty.");
                return false;
            }

            currentWaypointIndex = 0;
            currentHoldTimer = 0f;
            SetState(NavigationMissionState.NavigatingToWaypoint);
            return true;
        }

        /// <summary>
        /// Pauses active mission execution.
        /// </summary>
        public void PauseMission()
        {
            if (missionState == NavigationMissionState.NavigatingToWaypoint || missionState == NavigationMissionState.HoldingAtWaypoint)
            {
                SetState(NavigationMissionState.Paused);
            }
        }

        /// <summary>
        /// Resumes a paused mission from the active target waypoint.
        /// </summary>
        public void ResumeMission()
        {
            if (missionState == NavigationMissionState.Paused && currentWaypointIndex >= 0)
            {
                SetState(NavigationMissionState.NavigatingToWaypoint);
            }
        }

        /// <summary>
        /// Aborts active mission execution immediately.
        /// </summary>
        public void AbortMission()
        {
            SetState(NavigationMissionState.Aborted);
        }

        /// <summary>
        /// Guidance navigation loop update. Computes guidance control command vectors based on current position.
        /// </summary>
        /// <param name="currentPosition">Current quadcopter world position in meters.</param>
        /// <param name="currentVelocity">Current linear velocity in m/s.</param>
        /// <param name="deltaTime">Time step duration in seconds.</param>
        /// <param name="outPitch">Computed steering pitch command output [-1.0, 1.0].</param>
        /// <param name="outRoll">Computed steering roll command output [-1.0, 1.0].</param>
        /// <param name="outYaw">Computed steering yaw command output [-1.0, 1.0].</param>
        /// <param name="outThrottle">Computed steering throttle command output [0.0, 1.0].</param>
        public void CalculateGuidanceCommands(Vector3 currentPosition, Vector3 currentVelocity, float deltaTime, out float outPitch, out float outRoll, out float outYaw, out float outThrottle)
        {
            outPitch = 0f;
            outRoll = 0f;
            outYaw = 0f;
            outThrottle = 0.5f; // Baseline hover throttle stub

            if (missionState != NavigationMissionState.NavigatingToWaypoint && missionState != NavigationMissionState.HoldingAtWaypoint)
            {
                return;
            }

            if (currentWaypointIndex < 0 || currentWaypointIndex >= waypointQueue.Count)
            {
                SetState(NavigationMissionState.Completed);
                OnMissionCompleted?.Invoke();
                return;
            }

            Waypoint targetWp = waypointQueue[currentWaypointIndex];
            Vector3 errorVector = targetWp.TargetPosition - currentPosition;
            float distanceToTarget = errorVector.magnitude;

            if (missionState == NavigationMissionState.NavigatingToWaypoint)
            {
                if (distanceToTarget <= targetWp.AcceptanceRadiusMeters)
                {
                    OnWaypointReached?.Invoke(currentWaypointIndex, targetWp);

                    if (targetWp.ActionType == WaypointActionType.HoverHold && targetWp.HoldDurationSeconds > 0f)
                    {
                        currentHoldTimer = targetWp.HoldDurationSeconds;
                        SetState(NavigationMissionState.HoldingAtWaypoint);
                    }
                    else
                    {
                        AdvanceToNextWaypoint();
                    }
                }
            }

            if (missionState == NavigationMissionState.HoldingAtWaypoint)
            {
                currentHoldTimer -= deltaTime;
                if (currentHoldTimer <= 0f)
                {
                    AdvanceToNextWaypoint();
                }
            }

            // Proportional guidance calculations
            Vector3 targetVelocity = errorVector.normalized * Mathf.Min(targetWp.CruiseSpeedMS, distanceToTarget * positionKp);
            Vector3 velocityError = targetVelocity - currentVelocity;

            // Map guidance velocity vector to pitch, roll, and throttle
            outPitch = Mathf.Clamp(velocityError.z * 0.2f, -1f, 1f);
            outRoll = Mathf.Clamp(velocityError.x * 0.2f, -1f, 1f);
            outThrottle = Mathf.Clamp01(0.5f + (velocityError.y * 0.1f));

            if (errorVector.sqrMagnitude > 0.01f)
            {
                float targetYawDeg = Mathf.Atan2(errorVector.x, errorVector.z) * Mathf.Rad2Deg;
                outYaw = Mathf.Clamp(targetYawDeg / 180f, -1f, 1f);
            }
        }

        /// <summary>
        /// Advances mission index to next waypoint in the queue.
        /// </summary>
        private void AdvanceToNextWaypoint()
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypointQueue.Count)
            {
                SetState(NavigationMissionState.Completed);
                OnMissionCompleted?.Invoke();
            }
            else
            {
                SetState(NavigationMissionState.NavigatingToWaypoint);
            }
        }

        /// <summary>
        /// Updates state and fires state change event.
        /// </summary>
        private void SetState(NavigationMissionState newState)
        {
            if (missionState == newState) return;
            missionState = newState;
            OnMissionStateChanged?.Invoke(missionState);
        }
    }
}



