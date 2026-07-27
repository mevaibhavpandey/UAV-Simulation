using UnityEngine;
using ASTRA.UAV.Drone;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Engineering Debug Panel displaying real-time Rigidbody physical telemetry,
    /// angular velocities, 4-motor RPMs, Center of Mass, and flight recording metrics.
    /// </summary>
    public class EngineeringDebugPanel : MonoBehaviour
    {
        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;
        [SerializeField] private bool showDebugPanel = true;

        private Rigidbody rb;
        private PropellerAnimator propellerAnimator;
        private ManualFlightController flightController;
        private FlightRecorder flightRecorder;

        private void Start()
        {
            if (droneObject == null)
            {
                droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            }

            if (droneObject != null)
            {
                rb = droneObject.GetComponent<Rigidbody>();
                propellerAnimator = droneObject.GetComponentInChildren<PropellerAnimator>();
                flightController = droneObject.GetComponent<ManualFlightController>();
                flightRecorder = droneObject.GetComponent<FlightRecorder>();
            }
        }

        private void OnGUI()
        {
            if (!showDebugPanel || droneObject == null || rb == null) return;

            // Render top-right Engineering Debug Box
            int width = 310;
            int height = 230;
            int x = Screen.width - width - 15;
            int y = 15;

            GUI.Box(new Rect(x, y, width, height), "Engineering Telemetry & Physics Debug");

            Vector3 vel = rb.linearVelocity;
            Vector3 angVel = rb.angularVelocity;
            Vector3 com = rb.centerOfMass;
            float rpm = propellerAnimator != null ? propellerAnimator.CurrentRPM : 0f;
            float dist = flightRecorder != null ? flightRecorder.TotalDistanceMeters : 0f;
            float dur = flightRecorder != null ? flightRecorder.FlightDurationSeconds : 0f;

            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 11 };

            GUILayout.BeginArea(new Rect(x + 10, y + 25, width - 20, height - 30));
            GUILayout.Label($"<b>Rigidbody Mass:</b> {rb.mass:F1} kg", style);
            GUILayout.Label($"<b>Velocity Vector (m/s):</b> ({vel.x:F2}, {vel.y:F2}, {vel.z:F2})", style);
            GUILayout.Label($"<b>Angular Vel (rad/s):</b> ({angVel.x:F2}, {angVel.y:F2}, {angVel.z:F2})", style);
            GUILayout.Label($"<b>Center of Mass (local):</b> ({com.x:F2}, {com.y:F2}, {com.z:F2})", style);
            GUILayout.Label($"<b>Motor 1-4 RPM (Avg):</b> {rpm:F0} RPM", style);
            GUILayout.Label($"<b>Flight Duration:</b> {dur:F1} s", style);
            GUILayout.Label($"<b>Distance Traveled:</b> {dist:F1} m", style);
            GUILayout.EndArea();
        }
    }
}





