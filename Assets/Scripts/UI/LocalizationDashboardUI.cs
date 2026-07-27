using UnityEngine;
using ASTRA.UAV.AI;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Defense-grade GCS Visual SLAM & GPS-Denied Navigation UI Panel.
    /// Displays Active Localization Source, Sensor Fusion Estimate, Drift Error, Tracking Quality,
    /// Visual Feature Count, Landmark Count, VIO Confidence %, and GPS Failure Simulation controls.
    /// </summary>
    public class LocalizationDashboardUI : MonoBehaviour
    {
        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;

        private LocalizationManager localizationManager;
        private VisualSLAMManager slamManager;
        private VIOManager vioManager;
        private SensorFusionManager fusionManager;
        private LandmarkManager landmarkManager;
        private LocalizationRecoveryManager recoveryManager;

        private void Start()
        {
            if (droneObject == null) droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (droneObject != null)
            {
                localizationManager = LocalizationManager.Instance;
                slamManager = droneObject.GetComponent<VisualSLAMManager>();
                vioManager = droneObject.GetComponent<VIOManager>();
                fusionManager = droneObject.GetComponent<SensorFusionManager>();
                landmarkManager = droneObject.GetComponent<LandmarkManager>();
                recoveryManager = droneObject.GetComponent<LocalizationRecoveryManager>();
            }
        }

        private void OnGUI()
        {
            if (droneObject == null) return;

            // Draw GPS-Denied Localization Box on bottom-left (overlay)
            int width = 310;
            int height = 235;
            int x = 15;
            int y = Screen.height - height - 105;

            GUI.Box(new Rect(x, y, width, height), "GPS-Denied Visual SLAM & Sensor Fusion");

            string sourceStr = localizationManager != null ? localizationManager.ActiveSource.ToString() : "GPS_Primary";
            string failureStr = localizationManager != null ? localizationManager.CurrentGPSFailure.ToString() : "None";
            int featCount = slamManager != null ? slamManager.TrackedFeatureCount : 128;
            int lmCount = landmarkManager != null ? landmarkManager.ActiveLandmarks.Count : 5;
            float conf = fusionManager != null ? fusionManager.FusionConfidenceScore : 99f;
            float drift = fusionManager != null ? fusionManager.FusedPositionErrorMeters : 0.05f;
            string quality = recoveryManager != null ? recoveryManager.CurrentTrackingQuality.ToString() : "EXCELLENT";
            bool loopClosure = slamManager != null && slamManager.LoopClosureDetected;

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 };

            GUILayout.BeginArea(new Rect(x + 10, y + 25, width - 20, height - 30));

            GUILayout.Label($"<b>Loc Source:</b> <color=cyan>{sourceStr}</color>", style);
            GUILayout.Label($"<b>GPS Status:</b> {(localizationManager != null && localizationManager.IsGPSAvailable ? "<color=green>NORMAL (Lock)</color>" : "<color=red>JAMMED / LOST</color>")}", style);
            GUILayout.Label($"<b>Tracking Quality:</b> {GetQualityColor(quality)}", style);
            GUILayout.Label($"<b>Sensor Drift Error:</b> {drift:F2} m", style);
            GUILayout.Label($"<b>VIO Confidence:</b> {conf:F1}%", style);
            GUILayout.Label($"<b>Features Tracked:</b> {featCount} Keypoints", style);
            GUILayout.Label($"<b>Map Landmarks:</b> {lmCount} Anchors", style);
            GUILayout.Label($"<b>Loop Closure:</b> {(loopClosure ? "<color=green>DETECTED</color>" : "Searching...")}", style);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Jam GPS", GUILayout.Height(24)))
            {
                if (localizationManager != null) localizationManager.TriggerGPSFailure(GPSFailureType.Jamming);
            }
            if (GUILayout.Button("Lose Signal", GUILayout.Height(24)))
            {
                if (localizationManager != null) localizationManager.TriggerGPSFailure(GPSFailureType.SignalLoss);
            }
            if (GUILayout.Button("Restore GPS", GUILayout.Height(24)))
            {
                if (localizationManager != null) localizationManager.RestoreGPS();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private string GetQualityColor(string q)
        {
            if (q == "Excellent") return "<color=green>EXCELLENT</color>";
            if (q == "Good") return "<color=lime>GOOD</color>";
            if (q == "Fair") return "<color=yellow>FAIR</color>";
            if (q == "Poor") return "<color=orange>POOR</color>";
            return "<color=red>LOST</color>";
        }
    }
}





