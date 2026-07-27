using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Animates quadcopter propeller rotation based on throttle and individual motor RPMs.
    /// Manages Clockwise (CW) and Counter-Clockwise (CCW) rotation directions.
    /// </summary>
    public class PropellerAnimator : MonoBehaviour
    {
        [Header("Propeller Transforms")]
        [SerializeField] private Transform propFL; // CW
        [SerializeField] private Transform propFR; // CCW
        [SerializeField] private Transform propRL; // CCW
        [SerializeField] private Transform propRR; // CW

        [Header("Motor RPM Settings")]
        [SerializeField] private float maxRPM = 8500.0f;
        [SerializeField] private float idleRPM = 1200.0f;

        private ManualFlightController flightController;
        private FlightModeManager flightModeManager;
        private float currentRPM = 0f;

        public float CurrentRPM => currentRPM;

        private void Awake()
        {
            flightController = GetComponentInParent<ManualFlightController>();
            flightModeManager = GetComponentInParent<FlightModeManager>();
            FindPropellersIfNull();
        }

        private void FindPropellersIfNull()
        {
            if (propFL == null) propFL = transform.Find("Propeller_FL");
            if (propFR == null) propFR = transform.Find("Propeller_FR");
            if (propRL == null) propRL = transform.Find("Propeller_RL");
            if (propRR == null) propRR = transform.Find("Propeller_RR");
        }

        private void Update()
        {
            CalculateTargetRPM();
            AnimatePropellers();
        }

        private void CalculateTargetRPM()
        {
            float targetRPM = 0f;

            if (flightModeManager != null && flightModeManager.IsArmed)
            {
                float throttle = flightController != null ? flightController.ThrottlePercentage : 0f;
                targetRPM = Mathf.Lerp(idleRPM, maxRPM, throttle);
            }

            // Smooth RPM response curve
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime * 10.0f);
        }

        private void AnimatePropellers()
        {
            if (currentRPM <= 5.0f) return;

            // Rotation angle step (degrees per frame)
            float degreesPerSecond = (currentRPM * 360.0f) / 60.0f;
            float step = degreesPerSecond * Time.deltaTime;

            // Rotate CW / CCW pairs
            if (propFL != null) propFL.Rotate(Vector3.up, step, Space.Self);        // CW
            if (propFR != null) propFR.Rotate(Vector3.up, -step, Space.Self);       // CCW
            if (propRL != null) propRL.Rotate(Vector3.up, -step, Space.Self);       // CCW
            if (propRR != null) propRR.Rotate(Vector3.up, step, Space.Self);        // CW
        }
    }
}


