using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Simulates 6S LiPo battery discharge dynamics (Voltage 22.2V - 19.8V, Capacity 8000mAh, Current Draw, Flight Time).
    /// </summary>
    public class BatterySimulator : MonoBehaviour
    {
        [Header("Battery Specs (6S LiPo)")]
        [SerializeField] private float nominalVoltage = 22.2f;
        [SerializeField] private float maxVoltage = 25.2f;
        [SerializeField] private float cutoffVoltage = 19.8f;
        [SerializeField] private float capacitymAh = 8000.0f;

        [Header("Live Output")]
        [SerializeField] private float batteryPercentage = 100.0f;
        [SerializeField] private float currentVoltage = 25.2f;
        [SerializeField] private float currentDrawAmps = 0.5f;
        [SerializeField] private float remainingFlightTimeMinutes = 20.0f;

        private ManualFlightController flightController;
        private FlightModeManager flightModeManager;
        private float usedCapacitymAh = 0f;

        public float BatteryPercentage => batteryPercentage;
        public float CurrentVoltage => currentVoltage;
        public float CurrentDrawAmps => currentDrawAmps;
        public float RemainingFlightTimeMinutes => remainingFlightTimeMinutes;

        private void Awake()
        {
            flightController = GetComponent<ManualFlightController>();
            flightModeManager = GetComponent<FlightModeManager>();
        }

        private void Update()
        {
            CalculateDischarge();
        }

        private void CalculateDischarge()
        {
            if (flightModeManager != null && flightModeManager.IsArmed)
            {
                float throttle = flightController != null ? flightController.ThrottlePercentage : 0.05f;
                // Idle draw ~ 2A, Max hover/flight draw ~ 45A
                currentDrawAmps = Mathf.Lerp(2.5f, 45.0f, throttle);

                // Capacity consumed (mAh per second = Amps * 1000 / 3600)
                float consumedmAhPerSec = (currentDrawAmps * 1000.0f) / 3600.0f;
                usedCapacitymAh += consumedmAhPerSec * Time.deltaTime;
            }
            else
            {
                currentDrawAmps = 0.3f; // Electronics standby draw
            }

            batteryPercentage = Mathf.Clamp01(1.0f - (usedCapacitymAh / capacitymAh)) * 100.0f;
            currentVoltage = Mathf.Lerp(cutoffVoltage, maxVoltage, batteryPercentage / 100.0f);

            if (currentDrawAmps > 0.1f)
            {
                float remainingmAh = Mathf.Max(0f, capacitymAh - usedCapacitymAh);
                remainingFlightTimeMinutes = (remainingmAh / (currentDrawAmps * 1000.0f)) * 60.0f;
            }
        }
    }
}


