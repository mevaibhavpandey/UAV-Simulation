using UnityEngine;
using ASTRA.UAV.Drone;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Defense-grade Flight HUD controller displaying real-time vehicle metrics
    /// (Flight Mode, Arm Status, Altitude, Airspeed, Battery %, Throttle %, Pitch/Roll/Yaw).
    /// </summary>
    public class FlightHUDController : MonoBehaviour
    {
        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;

        private FlightModeManager flightModeManager;
        private ManualFlightController flightController;
        private BatterySimulator batterySimulator;
        private DroneStateMachine stateMachine;

        private void Start()
        {
            if (droneObject == null)
            {
                droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            }

            if (droneObject != null)
            {
                flightModeManager = droneObject.GetComponent<FlightModeManager>();
                flightController = droneObject.GetComponent<ManualFlightController>();
                batterySimulator = droneObject.GetComponent<BatterySimulator>();
                stateMachine = droneObject.GetComponent<DroneStateMachine>();
            }
        }

        private void OnGUI()
        {
            if (droneObject == null) return;

            // Draw HUD Box overlay on top-left
            GUI.Box(new Rect(15, 15, 290, 240), "ASTRA UAV — Mission Control HUD");

            string armText = (flightModeManager != null && flightModeManager.IsArmed) ? "<color=green>ARMED</color>" : "<color=red>DISARMED</color>";
            string modeText = flightModeManager != null ? flightModeManager.CurrentMode.ToString() : "N/A";
            string stateText = stateMachine != null ? stateMachine.CurrentState.ToString() : "N/A";
            float alt = droneObject.transform.position.y;
            float speed = flightController != null ? flightController.Velocity.magnitude : 0f;
            float batt = batterySimulator != null ? batterySimulator.BatteryPercentage : 100f;
            float volt = batterySimulator != null ? batterySimulator.CurrentVoltage : 22.2f;
            float throttle = flightController != null ? flightController.ThrottlePercentage * 100f : 0f;

            Vector3 euler = droneObject.transform.eulerAngles;
            float pitch = NormalizeAngle(euler.x);
            float roll = NormalizeAngle(euler.z);
            float yaw = euler.y;

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true };

            GUILayout.BeginArea(new Rect(25, 45, 270, 200));
            GUILayout.Label($"<b>Status:</b> {armText} | <b>State:</b> {stateText}", style);
            GUILayout.Label($"<b>Flight Mode:</b> {modeText}", style);
            GUILayout.Label($"<b>Altitude:</b> {alt:F2} m", style);
            GUILayout.Label($"<b>Airspeed:</b> {speed:F1} m/s ({speed * 3.6f:F1} km/h)", style);
            GUILayout.Label($"<b>Battery:</b> {batt:F0}% ({volt:F1}V)", style);
            GUILayout.Label($"<b>Throttle:</b> {throttle:F0}%", style);
            GUILayout.Label($"<b>Attitude (P/R/Y):</b> {pitch:F1}° / {roll:F1}° / {yaw:F0}°", style);
            GUILayout.EndArea();

            // Hotkey quick legend
            GUI.Box(new Rect(15, Screen.height - 85, 420, 70), "Keyboard Controls Legend");
            GUILayout.BeginArea(new Rect(25, Screen.height - 65, 400, 50));
            GUILayout.Label("<b>W/S:</b> Pitch | <b>A/D:</b> Roll | <b>Q/E:</b> Yaw | <b>Space/Ctrl:</b> Throttle");
            GUILayout.Label("<b>R:</b> Arm | <b>F:</b> Disarm | <b>H:</b> Hover Mode | <b>L:</b> Land | <b>X:</b> E-Stop");
            GUILayout.EndArea();
        }

        private float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}





