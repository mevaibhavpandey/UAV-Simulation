using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Flight recorder tracking takeoff time, flight duration, total distance traveled, max altitude, and max speed.
    /// </summary>
    public class FlightRecorder : MonoBehaviour
    {
        [Header("Flight Metrics")]
        [SerializeField] private float flightDurationSeconds = 0.0f;
        [SerializeField] private float totalDistanceMeters = 0.0f;
        [SerializeField] private float maxAltitudeMeters = 0.0f;
        [SerializeField] private float maxSpeedMetersPerSec = 0.0f;

        private ManualFlightController flightController;
        private FlightModeManager flightModeManager;
        private Vector3 lastPosition;
        private bool isRecording = false;

        public float FlightDurationSeconds => flightDurationSeconds;
        public float TotalDistanceMeters => totalDistanceMeters;
        public float MaxAltitudeMeters => maxAltitudeMeters;
        public float MaxSpeedMetersPerSec => maxSpeedMetersPerSec;

        private void Awake()
        {
            flightController = GetComponent<ManualFlightController>();
            flightModeManager = GetComponent<FlightModeManager>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (flightModeManager != null && flightModeManager.IsArmed)
            {
                if (!isRecording)
                {
                    isRecording = true;
                    lastPosition = transform.position;
                }

                flightDurationSeconds += Time.deltaTime;

                float distThisFrame = Vector3.Distance(transform.position, lastPosition);
                totalDistanceMeters += distThisFrame;
                lastPosition = transform.position;

                float currentAlt = transform.position.y;
                if (currentAlt > maxAltitudeMeters) maxAltitudeMeters = currentAlt;

                float currentSpeed = flightController != null ? flightController.Velocity.magnitude : 0f;
                if (currentSpeed > maxSpeedMetersPerSec) maxSpeedMetersPerSec = currentSpeed;
            }
            else
            {
                isRecording = false;
            }
        }
    }
}




