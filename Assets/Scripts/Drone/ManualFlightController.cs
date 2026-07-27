using UnityEngine;
using UnityEngine.InputSystem;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Realistic manual flight controller driving 6-DOF Rigidbody physics.
    /// Reads keyboard flight stick inputs (W/S Pitch, A/D Roll, Q/E Yaw, Space/Ctrl Throttle),
    /// enforces safety limits (Max Tilt 30°, Max Speed 15m/s), and manages auto-takeoff, hover, landing, and emergency stop.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FlightModeManager))]
    [RequireComponent(typeof(DroneStateMachine))]
    public class ManualFlightController : MonoBehaviour
    {
        [Header("Flight Performance Limits")]
        [SerializeField] private float maxThrustPerMotor = 18.5f; // Total thrust ~ 74 N (T/W ~ 2.6 for 2.8kg drone)
        [SerializeField] private float maxTiltAngleDegrees = 30.0f;
        [SerializeField] private float maxYawRateDegrees = 120.0f;
        [SerializeField] private float maxHorizontalSpeed = 15.0f;
        [SerializeField] private float maxVerticalSpeed = 5.0f;

        [Header("PID Stabilizer Gains")]
        [SerializeField] private float attitudeKp = 6.0f;
        [SerializeField] private float attitudeKd = 1.2f;
        [SerializeField] private float hoverAltitudeKp = 4.0f;
        [SerializeField] private float hoverAltitudeKd = 2.0f;

        [Header("Live Stick Axes [Normalized -1..1]")]
        [SerializeField] private float pitchInput = 0f;
        [SerializeField] private float rollInput = 0f;
        [SerializeField] private float yawInput = 0f;
        [SerializeField] private float throttleInput = 0f;

        private Rigidbody rb;
        private FlightModeManager flightModeManager;
        private DroneStateMachine stateMachine;
        private float hoverTargetAltitude = 0f;
        private bool isHoverActive = false;

        public float ThrottlePercentage => throttleInput;
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            flightModeManager = GetComponent<FlightModeManager>();
            stateMachine = GetComponent<DroneStateMachine>();
        }

        private void Update()
        {
            HandleHotkeys();
            ReadInputAxes();
        }

        private void HandleHotkeys()
        {
            if (Keyboard.current == null) return;

            // R: Arm
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (flightModeManager.Arm())
                {
                    stateMachine.SetState(DroneOperationalState.Armed);
                }
            }

            // F: Disarm
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                flightModeManager.Disarm();
                stateMachine.SetState(DroneOperationalState.Disarmed);
            }

            // H: Hover / Altitude Hold Toggle
            if (Keyboard.current.hKey.wasPressedThisFrame && flightModeManager.IsArmed)
            {
                isHoverActive = !isHoverActive;
                if (isHoverActive)
                {
                    hoverTargetAltitude = transform.position.y;
                    flightModeManager.SetFlightMode(FlightModeType.AltitudeHold);
                    stateMachine.SetState(DroneOperationalState.Hover);
                }
                else
                {
                    flightModeManager.SetFlightMode(FlightModeType.Manual);
                    stateMachine.SetState(DroneOperationalState.Flying);
                }
            }

            // L: Auto Land
            if (Keyboard.current.lKey.wasPressedThisFrame && flightModeManager.IsArmed)
            {
                flightModeManager.SetFlightMode(FlightModeType.Manual);
                stateMachine.SetState(DroneOperationalState.Landing);
            }

            // X / Esc: Emergency Stop
            if (Keyboard.current.xKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TriggerEmergencyStop();
            }
        }

        private void ReadInputAxes()
        {
            if (Keyboard.current == null) return;

            // Pitch: W/S
            float targetPitch = 0f;
            if (Keyboard.current.wKey.isPressed) targetPitch += 1.0f;
            if (Keyboard.current.sKey.isPressed) targetPitch -= 1.0f;
            pitchInput = Mathf.Lerp(pitchInput, targetPitch, Time.deltaTime * 8.0f);

            // Roll: A/D
            float targetRoll = 0f;
            if (Keyboard.current.dKey.isPressed) targetRoll += 1.0f;
            if (Keyboard.current.aKey.isPressed) targetRoll -= 1.0f;
            rollInput = Mathf.Lerp(rollInput, targetRoll, Time.deltaTime * 8.0f);

            // Yaw: Q/E
            float targetYaw = 0f;
            if (Keyboard.current.eKey.isPressed) targetYaw += 1.0f;
            if (Keyboard.current.qKey.isPressed) targetYaw -= 1.0f;
            yawInput = Mathf.Lerp(yawInput, targetYaw, Time.deltaTime * 8.0f);

            // Throttle: Space / Left Ctrl
            if (Keyboard.current.spaceKey.isPressed)
            {
                throttleInput = Mathf.Clamp01(throttleInput + Time.deltaTime * 0.5f);
                isHoverActive = false; // Manual throttle overrides hover
            }
            else if (Keyboard.current.leftCtrlKey.isPressed)
            {
                throttleInput = Mathf.Clamp01(throttleInput - Time.deltaTime * 0.5f);
                isHoverActive = false;
            }

            // Auto transition state from Armed to Takeoff / Flying when throttle increases
            if (flightModeManager.IsArmed && throttleInput > 0.15f && stateMachine.CurrentState == DroneOperationalState.Armed)
            {
                flightModeManager.SetFlightMode(FlightModeType.Manual);
                stateMachine.SetState(DroneOperationalState.Takeoff);
            }
            else if (stateMachine.CurrentState == DroneOperationalState.Takeoff && transform.position.y > 0.5f)
            {
                stateMachine.SetState(DroneOperationalState.Flying);
            }

            // Auto Landing state logic
            if (stateMachine.CurrentState == DroneOperationalState.Landing)
            {
                throttleInput = Mathf.Lerp(throttleInput, 0.35f, Time.deltaTime * 2.0f); // Gentle descending throttle
                if (transform.position.y <= 0.3f && rb.linearVelocity.magnitude < 0.2f)
                {
                    flightModeManager.Disarm();
                    stateMachine.SetState(DroneOperationalState.Disarmed);
                    throttleInput = 0f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!flightModeManager.IsArmed || stateMachine.CurrentState == DroneOperationalState.EmergencyStop)
            {
                return;
            }

            ApplyAttitudeTorque();
            ApplyMotorThrust();
            ClampVelocities();
        }

        private void ApplyMotorThrust()
        {
            float totalThrustN = 0f;

            if (isHoverActive)
            {
                // Hover PID altitude hold
                float altitudeError = hoverTargetAltitude - transform.position.y;
                float verticalVel = rb.linearVelocity.y;
                float hoverCorrection = (altitudeError * hoverAltitudeKp) - (verticalVel * hoverAltitudeKd);
                
                // Hover base thrust = mass * g (~27.5N)
                float hoverBaseThrust = rb.mass * Mathf.Abs(UnityEngine.Physics.gravity.y);
                totalThrustN = Mathf.Clamp(hoverBaseThrust + hoverCorrection, 0f, maxThrustPerMotor * 4.0f);
            }
            else
            {
                // Manual throttle force
                totalThrustN = throttleInput * (maxThrustPerMotor * 4.0f);
            }

            // Apply main lift force in local upward direction
            Vector3 thrustForce = transform.up * totalThrustN;
            rb.AddForce(thrustForce, ForceMode.Force);
        }

        private void ApplyAttitudeTorque()
        {
            // Target tilt angles derived from pitch & roll sticks
            float targetPitchAngle = pitchInput * maxTiltAngleDegrees;
            float targetRollAngle = rollInput * maxTiltAngleDegrees;

            // Current Euler angles
            Vector3 currentEuler = transform.eulerAngles;
            float currentPitch = AngleNormalizer(currentEuler.x);
            float currentRoll = AngleNormalizer(currentEuler.z);

            // Attitude error
            float pitchError = targetPitchAngle - currentPitch;
            float rollError = targetRollAngle - currentRoll;

            // Angular rates
            Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);

            // Torque calculations (PD loop)
            float torqueX = (pitchError * attitudeKp) - (localAngularVel.x * attitudeKd * Mathf.Rad2Deg);
            float torqueZ = (-rollError * attitudeKp) - (localAngularVel.z * attitudeKd * Mathf.Rad2Deg);
            float torqueY = (yawInput * maxYawRateDegrees * 0.1f) - (localAngularVel.y * attitudeKd * Mathf.Rad2Deg);

            Vector3 localTorque = new Vector3(torqueX, torqueY, torqueZ);
            rb.AddRelativeTorque(localTorque, ForceMode.Force);
        }

        private void ClampVelocities()
        {
            // Horizontal speed limit
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontalVel.magnitude > maxHorizontalSpeed)
            {
                horizontalVel = horizontalVel.normalized * maxHorizontalSpeed;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
            }

            // Vertical speed limit
            float verticalVel = Mathf.Clamp(rb.linearVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, verticalVel, rb.linearVelocity.z);
        }

        public void TriggerEmergencyStop()
        {
            flightModeManager.Disarm();
            stateMachine.SetState(DroneOperationalState.EmergencyStop);
            throttleInput = 0f;
            isHoverActive = false;
            Debug.LogWarning("EMERGENCY STOP TRIGGERED! Motor power cut.", LogCategory.Drone);
        }

        private float AngleNormalizer(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}



