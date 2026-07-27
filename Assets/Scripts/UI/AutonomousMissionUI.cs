using UnityEngine;
using ASTRA.UAV.Mission;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// UI Controller for Autonomous GPS Navigation & Mission Execution.
    /// Provides target coordinate input, launch controls, pre-flight checklist summary,
    /// progress bar %, distance remaining, and ETA readout.
    /// </summary>
    public class AutonomousMissionUI : MonoBehaviour
    {
        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;

        [Header("Target Mission Parameters")]
        [SerializeField] private double targetLatitude = 13.0845;
        [SerializeField] private double targetLongitude = 80.2725;
        [SerializeField] private float targetAltitude = 35.0f;

        private AutonomousNavigationController autoNav;
        private PreFlightChecklist preFlightChecklist;

        private void Start()
        {
            if (droneObject == null) droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (droneObject != null)
            {
                autoNav = droneObject.GetComponent<AutonomousNavigationController>();
                preFlightChecklist = droneObject.GetComponent<PreFlightChecklist>();
            }
        }

        private void OnGUI()
        {
            if (droneObject == null || autoNav == null) return;

            // Render Autonomous Mission Controls Box on top-left (overlay)
            GUI.Box(new Rect(15, 265, 290, 260), "Autonomous GPS Navigation Controls");

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 };

            GUILayout.BeginArea(new Rect(25, 290, 270, 230));

            GUILayout.Label($"<b>Mission State:</b> <color=yellow>{autoNav.CurrentState}</color>", style);
            GUILayout.Label($"<b>Target Lat:</b> {targetLatitude:F4}° N", style);
            GUILayout.Label($"<b>Target Lon:</b> {targetLongitude:F4}° E", style);
            GUILayout.Label($"<b>Target Altitude:</b> {targetAltitude:F0} m MSL", style);

            GUILayout.Space(6);
            GUILayout.Label($"<b>Progress:</b> {autoNav.MissionProgressPercent:F0}%", style);
            GUILayout.Label($"<b>Dist. Remaining:</b> {autoNav.DistanceRemainingMeters:F1} m", style);
            GUILayout.Label($"<b>Est. Arrival ETA:</b> {autoNav.EstimatedTimeArrivalSeconds:F0} s", style);

            GUILayout.Space(10);
            if (autoNav.CurrentState == AutoNavMissionState.Idle || autoNav.CurrentState == AutoNavMissionState.Completed)
            {
                if (GUILayout.Button("LAUNCH AUTONOMOUS MISSION", GUILayout.Height(32)))
                {
                    autoNav.LaunchAutonomousMission(targetLatitude, targetLongitude, targetAltitude);
                }
            }
            else
            {
                GUILayout.Button("<color=cyan><b>MISSION IN PROGRESS...</b></color>", GUILayout.Height(32));
            }

            GUILayout.EndArea();
        }
    }
}



