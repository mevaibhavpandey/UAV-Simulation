using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Drone;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Mission
{
    public enum AutoNavMissionState
    {
        Idle,
        Validating,
        PreFlightCheck,
        Arming,
        Takeoff,
        Ascending,
        Rotating,
        Cruise,
        ApproachingTarget,
        HoveringAtTarget,
        ReturnHome,
        Landing,
        Completed,
        Failed
    }

    /// <summary>
    /// Autonomous Navigation Controller executing full GPS mission lifecycle from takeoff to landing.
    /// Manages 5 speed profiles (Takeoff 2.5m/s, Cruise 8m/s, Approach 3m/s, Landing 1.2m/s, RTH 10m/s),
    /// target coordinate transformation, mission progress %, distance remaining, and ETA calculations.
    /// </summary>
    [RequireComponent(typeof(PreFlightChecklist))]
    public class AutonomousNavigationController : MonoBehaviour
    {
        [Header("Mission Speed Profiles (m/s)")]
        [SerializeField] private float takeoffSpeed = 2.5f;
        [SerializeField] private float cruiseSpeed = 8.0f;
        [SerializeField] private float approachSpeed = 3.0f;
        [SerializeField] private float landingSpeed = 1.2f;
        [SerializeField] private float rthSpeed = 10.0f;

        [Header("State")]
        [SerializeField] private AutoNavMissionState currentState = AutoNavMissionState.Idle;
        [SerializeField] private Vector3 homeWorldPosition = new Vector3(35f, 0.25f, 20f);
        [SerializeField] private Vector3 targetWorldPosition;
        [SerializeField] private float targetAltitudeMSL = 35.0f;
        [SerializeField] private float missionProgressPercent = 0.0f;
        [SerializeField] private float distanceRemainingMeters = 0.0f;
        [SerializeField] private float estimatedTimeArrivalSeconds = 0.0f;

        [Header("Trail Path")]
        [SerializeField] private List<Vector3> flightTrailPoints = new List<Vector3>();

        private PreFlightChecklist preFlightChecklist;
        private AutopilotController autopilot;
        private FlightModeManager flightModeManager;
        private DroneStateMachine stateMachine;

        public AutoNavMissionState CurrentState => currentState;
        public float MissionProgressPercent => missionProgressPercent;
        public float DistanceRemainingMeters => distanceRemainingMeters;
        public float EstimatedTimeArrivalSeconds => estimatedTimeArrivalSeconds;
        public List<Vector3> FlightTrailPoints => flightTrailPoints;

        private void Awake()
        {
            preFlightChecklist = GetComponent<PreFlightChecklist>();
            autopilot = GetComponent<AutopilotController>();
            flightModeManager = GetComponent<FlightModeManager>();
            stateMachine = GetComponent<DroneStateMachine>();
            homeWorldPosition = transform.position;
        }

        /// <summary>
        /// Starts autonomous mission to target WGS84 coordinates.
        /// </summary>
        public bool LaunchAutonomousMission(double targetLat, double targetLon, float targetAlt)
        {
            if (currentState != AutoNavMissionState.Idle && currentState != AutoNavMissionState.Completed)
            {
                Debug.LogWarning("Autonomous mission already in progress!", LogCategory.Mission);
                return false;
            }

            // Convert Lat/Lon offset to local Unity world space (1 deg ~ 1000m scale demo)
            targetAltitudeMSL = targetAlt;
            targetWorldPosition = new Vector3((float)(targetLon - 80.2707) * 1000f, targetAlt, (float)(targetLat - 13.0827) * 1000f);

            SetMissionState(AutoNavMissionState.Validating);
            return true;
        }

        private void Update()
        {
            UpdateMissionStateMachine();
            CalculateMissionMetrics();
            RecordTrailPoint();
        }

        private void UpdateMissionStateMachine()
        {
            switch (currentState)
            {
                case AutoNavMissionState.Validating:
                    // Validate battery > 20%
                    BatterySimulator batt = GetComponent<BatterySimulator>();
                    if (batt != null && batt.BatteryPercentage < 20f)
                    {
                        Debug.LogError("Mission Validation Failed: Battery below 20% threshold!", LogCategory.Mission);
                        SetMissionState(AutoNavMissionState.Failed);
                        return;
                    }
                    SetMissionState(AutoNavMissionState.PreFlightCheck);
                    break;

                case AutoNavMissionState.PreFlightCheck:
                    if (preFlightChecklist != null && preFlightChecklist.RunAllChecks(gameObject))
                    {
                        SetMissionState(AutoNavMissionState.Arming);
                    }
                    else
                    {
                        SetMissionState(AutoNavMissionState.Failed);
                    }
                    break;

                case AutoNavMissionState.Arming:
                    if (flightModeManager != null) flightModeManager.Arm();
                    if (stateMachine != null) stateMachine.SetState(DroneOperationalState.Armed);
                    SetMissionState(AutoNavMissionState.Takeoff);
                    break;

                case AutoNavMissionState.Takeoff:
                    Vector3 climbTarget = new Vector3(transform.position.x, targetAltitudeMSL, transform.position.z);
                    if (autopilot != null) autopilot.EngageAutopilot(climbTarget, takeoffSpeed, transform.eulerAngles.y);

                    if (Mathf.Abs(transform.position.y - targetAltitudeMSL) < 1.0f)
                    {
                        SetMissionState(AutoNavMissionState.Rotating);
                    }
                    break;

                case AutoNavMissionState.Rotating:
                    Vector3 toTarget = targetWorldPosition - transform.position;
                    float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                    
                    if (autopilot != null) autopilot.EngageAutopilot(transform.position, 0f, targetYaw);

                    if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw)) < 5.0f)
                    {
                        SetMissionState(AutoNavMissionState.Cruise);
                    }
                    break;

                case AutoNavMissionState.Cruise:
                    if (autopilot != null) autopilot.EngageAutopilot(targetWorldPosition, cruiseSpeed, transform.eulerAngles.y);

                    float dist = Vector3.Distance(transform.position, targetWorldPosition);
                    if (dist < 15.0f)
                    {
                        SetMissionState(AutoNavMissionState.ApproachingTarget);
                    }
                    break;

                case AutoNavMissionState.ApproachingTarget:
                    if (autopilot != null) autopilot.EngageAutopilot(targetWorldPosition, approachSpeed, transform.eulerAngles.y);

                    if (Vector3.Distance(transform.position, targetWorldPosition) < 1.5f)
                    {
                        SetMissionState(AutoNavMissionState.HoveringAtTarget);
                    }
                    break;

                case AutoNavMissionState.HoveringAtTarget:
                    // Hold over target for 3 seconds then return home
                    if (autopilot != null) autopilot.EngageAutopilot(targetWorldPosition, 0f, transform.eulerAngles.y);
                    Invoke(nameof(InitiateReturnHome), 3.0f);
                    break;

                case AutoNavMissionState.ReturnHome:
                    Vector3 rthTarget = new Vector3(homeWorldPosition.x, targetAltitudeMSL, homeWorldPosition.z);
                    if (autopilot != null) autopilot.EngageAutopilot(rthTarget, rthSpeed, transform.eulerAngles.y);

                    if (Vector3.Distance(transform.position, rthTarget) < 2.0f)
                    {
                        SetMissionState(AutoNavMissionState.Landing);
                    }
                    break;

                case AutoNavMissionState.Landing:
                    Vector3 touchDownTarget = homeWorldPosition;
                    if (autopilot != null) autopilot.EngageAutopilot(touchDownTarget, landingSpeed, transform.eulerAngles.y);

                    if (transform.position.y <= homeWorldPosition.y + 0.2f)
                    {
                        if (autopilot != null) autopilot.DisengageAutopilot();
                        if (flightModeManager != null) flightModeManager.Disarm();
                        if (stateMachine != null) stateMachine.SetState(DroneOperationalState.Disarmed);
                        SetMissionState(AutoNavMissionState.Completed);
                    }
                    break;
            }
        }

        private void InitiateReturnHome()
        {
            if (currentState == AutoNavMissionState.HoveringAtTarget)
            {
                SetMissionState(AutoNavMissionState.ReturnHome);
            }
        }

        private void SetMissionState(AutoNavMissionState newState)
        {
            currentState = newState;
            Debug.Log($"Autonomous Mission State: {newState}", LogCategory.Mission);
        }

        private void CalculateMissionMetrics()
        {
            distanceRemainingMeters = Vector3.Distance(transform.position, targetWorldPosition);
            float totalDist = Vector3.Distance(homeWorldPosition, targetWorldPosition);
            missionProgressPercent = totalDist > 0.1f ? Mathf.Clamp01(1.0f - (distanceRemainingMeters / totalDist)) * 100.0f : 100.0f;
            estimatedTimeArrivalSeconds = cruiseSpeed > 0f ? distanceRemainingMeters / cruiseSpeed : 0f;
        }

        private void RecordTrailPoint()
        {
            if (flightTrailPoints.Count == 0 || Vector3.Distance(flightTrailPoints[flightTrailPoints.Count - 1], transform.position) > 2.0f)
            {
                flightTrailPoints.Add(transform.position);
            }
        }
    }
}


