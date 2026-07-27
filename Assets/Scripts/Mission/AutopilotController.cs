using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Drone;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Higher-level Autopilot Manager steering target 3D positions, velocities, and headings to the UAV flight controller.
    /// Provides modular architecture for future PX4, ArduPilot, MAVLink, and ROS2 autopilot bridging.
    /// </summary>
    public class AutopilotController : MonoBehaviour
    {
        [Header("Target Autopilot Commands")]
        [SerializeField] private Vector3 targetWorldPosition;
        [SerializeField] private float targetHeadingDegrees = 0.0f;
        [SerializeField] private float targetCruiseSpeed = 8.0f;
        [SerializeField] private bool isAutopilotEngaged = false;

        private Rigidbody rb;
        private FlightModeManager flightModeManager;
        private ManualFlightController manualFlightController;

        public bool IsAutopilotEngaged => isAutopilotEngaged;
        public Vector3 TargetWorldPosition => targetWorldPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            flightModeManager = GetComponent<FlightModeManager>();
            manualFlightController = GetComponent<ManualFlightController>();
        }

        /// <summary>
        /// Engages the autopilot with target destination, speed, and heading.
        /// </summary>
        public void EngageAutopilot(Vector3 destinationPosition, float speed, float heading)
        {
            targetWorldPosition = destinationPosition;
            targetCruiseSpeed = speed;
            targetHeadingDegrees = heading;
            isAutopilotEngaged = true;

            if (flightModeManager != null)
            {
                flightModeManager.SetFlightMode(FlightModeType.Auto);
            }

            Debug.Log($"Autopilot Engaged: Target Pos {destinationPosition}, Speed {speed} m/s", LogCategory.Mission);
        }

        /// <summary>
        /// Disengages autopilot and restores manual flight mode.
        /// </summary>
        public void DisengageAutopilot()
        {
            isAutopilotEngaged = false;
            if (flightModeManager != null)
            {
                flightModeManager.SetFlightMode(FlightModeType.Manual);
            }
            Debug.Log("Autopilot Disengaged. Handed over to Manual Mode.", LogCategory.Mission);
        }

        private void FixedUpdate()
        {
            if (!isAutopilotEngaged || rb == null || !flightModeManager.IsArmed) return;

            // Compute distance vector to target
            Vector3 posError = targetWorldPosition - transform.position;
            float distToTarget = posError.magnitude;

            if (distToTarget < 0.2f)
            {
                // Target reached
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 4.0f);
                return;
            }

            // Direction & Speed Guidance
            Vector3 desiredVel = posError.normalized * Mathf.Min(targetCruiseSpeed, distToTarget * 1.5f);
            Vector3 velError = desiredVel - rb.linearVelocity;

            // Apply proportional guidance force
            rb.AddForce(velError * 3.0f, ForceMode.Force);

            // Smooth heading alignment
            Quaternion targetRot = Quaternion.Euler(0f, targetHeadingDegrees, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 3.0f);
        }
    }
}




