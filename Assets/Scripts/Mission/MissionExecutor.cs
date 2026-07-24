using System;

using UnityEngine;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Core state machine responsible for executing UAV flight missions, handling transitions, and monitoring waypoint progression.
    /// </summary>
    public class MissionExecutor : MonoBehaviour
    {
        [Header("State Settings")]
        [SerializeField] private MissionState currentState = MissionState.Idle;
        [SerializeField] private Mission activeMission;
        [SerializeField] private float waypointArrivalRadius = 1.5f;

        [Header("Runtime Status")]
        [SerializeField] private int currentWaypointIndex = -1;
        [SerializeField] private float currentHoldTimer = 0f;
        [SerializeField] private float missionStartTime = 0f;
        [SerializeField] private float totalDistanceFlown = 0f;
        [SerializeField] private float maxObservedSpeed = 0f;

        private Vector3 lastPosition;
        private MissionResult activeResult;

        /// <summary>Fired when the mission state transitions.</summary>
        public event Action<MissionState, MissionState> OnStateChanged;

        /// <summary>Fired when a specific waypoint is reached during flight.</summary>
        public event Action<int, Waypoint> OnWaypointReached;

        /// <summary>Fired when the entire mission reaches completion or abort.</summary>
        public event Action<MissionResult> OnMissionFinished;

        /// <summary>Gets the current active mission state.</summary>
        public MissionState CurrentState => currentState;

        /// <summary>Gets the currently assigned active mission.</summary>
        public Mission ActiveMission => activeMission;

        /// <summary>Gets the 0-indexed index of the current target waypoint.</summary>
        public int CurrentWaypointIndex => currentWaypointIndex;

        private void Awake()
        {
            lastPosition = transform.position;
            activeResult = new MissionResult();
        }

        private void Update()
        {
            TrackFlightStats();

            switch (currentState)
            {
                case MissionState.Arming:
                    ProcessArmingState();
                    break;
                case MissionState.Takeoff:
                    ProcessTakeoffState();
                    break;
                case MissionState.Mission:
                    ProcessMissionState();
                    break;
                case MissionState.Hover:
                    ProcessHoverState();
                    break;
                case MissionState.ReturnHome:
                    ProcessReturnHomeState();
                    break;
                case MissionState.Landing:
                    ProcessLandingState();
                    break;
            }
        }

        /// <summary>
        /// Loads a validated mission into the executor.
        /// </summary>
        /// <param name="mission">Mission asset to load.</param>
        /// <returns>True if successfully loaded.</returns>
        public bool LoadMission(Mission mission)
        {
            if (mission == null)
            {
                Debug.LogError("[MissionExecutor] Cannot load null mission.");
                return false;
            }

            if (!mission.ValidateMission(out var errors))
            {
                Debug.LogError($"[MissionExecutor] Failed to load mission. Validation errors count: {errors.Count}");
                return false;
            }

            activeMission = mission;
            currentWaypointIndex = 0;
            SetState(MissionState.Ready);
            Debug.Log($"[MissionExecutor] Successfully loaded mission '{mission.MissionName}'.");
            return true;
        }

        /// <summary>
        /// Begins execution of the currently loaded mission.
        /// </summary>
        public void StartMission()
        {
            if (activeMission == null || currentState != MissionState.Ready)
            {
                Debug.LogWarning("[MissionExecutor] Mission not ready for launch.");
                return;
            }

            missionStartTime = Time.time;
            totalDistanceFlown = 0f;
            maxObservedSpeed = 0f;
            activeResult = new MissionResult
            {
                TotalWaypoints = activeMission.Waypoints.Count,
                ExecutionTime = DateTime.UtcNow
            };

            SetState(MissionState.Arming);
        }

        /// <summary>
        /// Immediately pauses execution and enters hover state.
        /// </summary>
        public void PauseMission()
        {
            if (currentState == MissionState.Mission)
            {
                SetState(MissionState.Hover);
            }
        }

        /// <summary>
        /// Resumes navigation from hover state.
        /// </summary>
        public void ResumeMission()
        {
            if (currentState == MissionState.Hover)
            {
                SetState(MissionState.Mission);
            }
        }

        /// <summary>
        /// Aborts active mission and commands immediate land or return home.
        /// </summary>
        /// <param name="reason">Reason for aborting.</param>
        public void AbortMission(string reason)
        {
            Debug.LogWarning($"[MissionExecutor] ABORTING MISSION: {reason}");
            activeResult.IsSuccess = false;
            activeResult.AbortReason = reason;
            activeResult.FinalState = MissionState.Aborted;

            SetState(MissionState.Aborted);
            OnMissionFinished?.Invoke(activeResult);
        }

        /// <summary>
        /// Commands immediate Return To Home (RTH).
        /// </summary>
        public void ReturnToHome()
        {
            SetState(MissionState.ReturnHome);
        }

        private void SetState(MissionState newState)
        {
            if (currentState == newState) return;
            MissionState previous = currentState;
            currentState = newState;
            Debug.Log($"[MissionExecutor] State changed: {previous} -> {newState}");
            OnStateChanged?.Invoke(previous, newState);
        }

        private void ProcessArmingState()
        {
            // Simulate arming sequence delay
            if (Time.time - missionStartTime > 2f)
            {
                SetState(MissionState.Takeoff);
            }
        }

        private void ProcessTakeoffState()
        {
            if (activeMission == null || activeMission.Waypoints.Count == 0) return;

            float targetAlt = activeMission.TargetAltitude;
            Vector3 currentPos = transform.position;

            // Ascend to target altitude
            Vector3 targetPos = new Vector3(currentPos.x, targetAlt, currentPos.z);
            transform.position = Vector3.MoveTowards(currentPos, targetPos, activeMission.CruiseSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.y - targetAlt) < 0.2f)
            {
                SetState(MissionState.Mission);
            }
        }

        private void ProcessMissionState()
        {
            if (activeMission == null || currentWaypointIndex < 0 || currentWaypointIndex >= activeMission.Waypoints.Count)
            {
                CompleteMission();
                return;
            }

            Waypoint targetWp = activeMission.Waypoints[currentWaypointIndex];
            Vector3 targetPos = targetWp.LocalPosition;

            // Move vehicle towards current waypoint
            transform.position = Vector3.MoveTowards(transform.position, targetPos, targetWp.TargetSpeed * Time.deltaTime);

            // Orient towards movement direction
            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
            }

            // Check waypoint arrival
            if (Vector3.Distance(transform.position, targetPos) <= waypointArrivalRadius)
            {
                OnWaypointReached?.Invoke(currentWaypointIndex, targetWp);
                activeResult.WaypointsCompleted++;

                if (targetWp.HoldTime > 0f)
                {
                    currentHoldTimer = targetWp.HoldTime;
                    SetState(MissionState.Hover);
                }
                else
                {
                    AdvanceToNextWaypoint();
                }
            }
        }

        private void ProcessHoverState()
        {
            if (currentHoldTimer > 0f)
            {
                currentHoldTimer -= Time.deltaTime;
            }
            else
            {
                AdvanceToNextWaypoint();
            }
        }

        private void ProcessReturnHomeState()
        {
            Vector3 homePos = new Vector3(0f, activeMission != null ? activeMission.TargetAltitude : 15f, 0f);
            transform.position = Vector3.MoveTowards(transform.position, homePos, 8f * Time.deltaTime);

            if (Vector3.Distance(transform.position, homePos) < 1f)
            {
                SetState(MissionState.Landing);
            }
        }

        private void ProcessLandingState()
        {
            Vector3 groundPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, groundPos, 2f * Time.deltaTime);

            if (transform.position.y <= 0.1f)
            {
                CompleteMission();
            }
        }

        private void AdvanceToNextWaypoint()
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= activeMission.Waypoints.Count)
            {
                SetState(MissionState.ReturnHome);
            }
            else
            {
                SetState(MissionState.Mission);
            }
        }

        private void CompleteMission()
        {
            activeResult.IsSuccess = true;
            activeResult.FinalState = MissionState.Completed;
            activeResult.TotalFlightTime = Time.time - missionStartTime;
            activeResult.TotalDistanceTraveled = totalDistanceFlown;
            activeResult.MaxSpeed = maxObservedSpeed;
            activeResult.AverageSpeed = activeResult.TotalFlightTime > 0f ? totalDistanceFlown / activeResult.TotalFlightTime : 0f;

            SetState(MissionState.Completed);
            OnMissionFinished?.Invoke(activeResult);
        }

        private void TrackFlightStats()
        {
            float stepDist = Vector3.Distance(transform.position, lastPosition);
            float currentSpeed = stepDist / Mathf.Max(Time.deltaTime, 0.0001f);

            if (currentState == MissionState.Mission || currentState == MissionState.Takeoff || currentState == MissionState.ReturnHome)
            {
                totalDistanceFlown += stepDist;
                if (currentSpeed > maxObservedSpeed)
                {
                    maxObservedSpeed = currentSpeed;
                }
            }

            lastPosition = transform.position;
        }
    }
}
