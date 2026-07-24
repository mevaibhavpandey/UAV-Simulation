using System;
using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Lifecycle states for mission execution.
    /// </summary>
    public enum MissionState
    {
        Idle,
        Configuring,
        Active,
        Paused,
        Completed,
        Failed,
        Aborted
    }

    /// <summary>
    /// Represents individual waypoint navigation data.
    /// </summary>
    [Serializable]
    public struct WaypointData
    {
        public int WaypointIndex;
        public Vector3 Position;
        public float TargetAltitude;
        public float TargetSpeed;
        public float HoldTimeSeconds;
        public string Command; // "TAKEOFF", "WAYPOINT", "HOVER", "LAND", "RTL"
    }

    /// <summary>
    /// Container holding complete mission details and waypoints.
    /// </summary>
    [Serializable]
    public class MissionData
    {
        public string MissionId = Guid.NewGuid().ToString();
        public string MissionName = "Default Autonomous Mission";
        public List<WaypointData> Waypoints = new List<WaypointData>();
    }

    /// <summary>
    /// Event broadcast when the mission state changes.
    /// </summary>
    public struct MissionStateChangedEvent : IEvent
    {
        public MissionState PreviousState { get; }
        public MissionState NewState { get; }

        public MissionStateChangedEvent(MissionState previousState, MissionState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Event broadcast when a mission waypoint is reached.
    /// </summary>
    public struct WaypointReachedEvent : IEvent
    {
        public int WaypointIndex { get; }
        public WaypointData Waypoint { get; }

        public WaypointReachedEvent(int waypointIndex, WaypointData waypoint)
        {
            WaypointIndex = waypointIndex;
            Waypoint = waypoint;
        }
    }

    /// <summary>
    /// Manages autonomous mission lifecycle, waypoint navigation sequences, and mission progress tracking.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        /// <summary>
        /// Current mission state.
        /// </summary>
        public MissionState CurrentState { get; private set; } = MissionState.Idle;

        /// <summary>
        /// Currently loaded mission configuration data.
        /// </summary>
        public MissionData ActiveMission { get; private set; }

        /// <summary>
        /// Index of the current active waypoint.
        /// </summary>
        public int CurrentWaypointIndex { get; private set; } = -1;

        /// <summary>
        /// Total time elapsed since starting the current mission (seconds).
        /// </summary>
        public float MissionElapsedTime { get; private set; }

        /// <summary>
        /// Action callback invoked on mission state transition.
        /// </summary>
        public event Action<MissionState, MissionState> OnMissionStateChanged;

        /// <summary>
        /// Action callback invoked when a waypoint is reached.
        /// </summary>
        public event Action<int, WaypointData> OnWaypointReached;

        private void Awake()
        {
            ServiceLocator.Register<MissionManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MissionManager>();
        }

        private void Update()
        {
            if (CurrentState == MissionState.Active)
            {
                MissionElapsedTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// Loads and begins execution of a mission configuration.
        /// </summary>
        /// <param name="mission">Mission data container.</param>
        public void StartMission(MissionData mission)
        {
            if (mission == null || mission.Waypoints == null || mission.Waypoints.Count == 0)
            {
                Debug.LogError("[MissionManager] Cannot start mission: Mission data or waypoint list is empty.");
                return;
            }

            ActiveMission = mission;
            CurrentWaypointIndex = 0;
            MissionElapsedTime = 0f;

            ChangeState(MissionState.Active);
            Debug.Log($"[MissionManager] Mission '{mission.MissionName}' started with {mission.Waypoints.Count} waypoints.");
        }

        /// <summary>
        /// Pauses active mission execution.
        /// </summary>
        public void PauseMission()
        {
            if (CurrentState == MissionState.Active)
            {
                ChangeState(MissionState.Paused);
            }
        }

        /// <summary>
        /// Resumes a paused mission.
        /// </summary>
        public void ResumeMission()
        {
            if (CurrentState == MissionState.Paused)
            {
                ChangeState(MissionState.Active);
            }
        }

        /// <summary>
        /// Aborts active or paused mission.
        /// </summary>
        public void AbortMission()
        {
            if (CurrentState == MissionState.Active || CurrentState == MissionState.Paused)
            {
                ChangeState(MissionState.Aborted);
                Debug.LogWarning("[MissionManager] Mission aborted by user command.");
            }
        }

        /// <summary>
        /// Advances the mission to the next waypoint in sequence.
        /// </summary>
        public void AdvanceToNextWaypoint()
        {
            if (ActiveMission == null || CurrentState != MissionState.Active) return;

            if (CurrentWaypointIndex >= 0 && CurrentWaypointIndex < ActiveMission.Waypoints.Count)
            {
                WaypointData reached = ActiveMission.Waypoints[CurrentWaypointIndex];
                OnWaypointReached?.Invoke(CurrentWaypointIndex, reached);
                EventBus.Publish(new WaypointReachedEvent(CurrentWaypointIndex, reached));
            }

            CurrentWaypointIndex++;

            if (CurrentWaypointIndex >= ActiveMission.Waypoints.Count)
            {
                ChangeState(MissionState.Completed);
                Debug.Log($"[MissionManager] Mission '{ActiveMission.MissionName}' completed successfully in {MissionElapsedTime:F1}s!");
            }
            else
            {
                Debug.Log($"[MissionManager] Advanced to waypoint index {CurrentWaypointIndex}/{ActiveMission.Waypoints.Count}.");
            }
        }

        /// <summary>
        /// Gets the current target waypoint data, or null if no waypoint active.
        /// </summary>
        public WaypointData? GetCurrentWaypoint()
        {
            if (ActiveMission != null && CurrentWaypointIndex >= 0 && CurrentWaypointIndex < ActiveMission.Waypoints.Count)
            {
                return ActiveMission.Waypoints[CurrentWaypointIndex];
            }
            return null;
        }

        private void ChangeState(MissionState newState)
        {
            if (CurrentState == newState) return;

            MissionState prevState = CurrentState;
            CurrentState = newState;

            OnMissionStateChanged?.Invoke(prevState, newState);
            EventBus.Publish(new MissionStateChangedEvent(prevState, newState));
        }
    }
}
