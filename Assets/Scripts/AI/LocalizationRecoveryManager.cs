using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Drone;
using ASTRA.UAV.Mission;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    public enum TrackingQuality
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Lost
    }

    /// <summary>
    /// Monitors tracking quality (Excellent, Good, Fair, Poor, Lost).
    /// Handles automated pose recovery (reduce speed, hover, search for landmarks) if visual tracking degrades.
    /// </summary>
    [RequireComponent(typeof(SensorFusionManager))]
    public class LocalizationRecoveryManager : MonoBehaviour
    {
        [Header("Tracking Status")]
        [SerializeField] private TrackingQuality currentTrackingQuality = TrackingQuality.Excellent;
        [SerializeField] private bool isRecoveryActive = false;

        private SensorFusionManager fusionManager;
        private AutopilotController autopilot;
        private ManualFlightController flightController;

        public TrackingQuality CurrentTrackingQuality => currentTrackingQuality;
        public bool IsRecoveryActive => isRecoveryActive;

        private void Awake()
        {
            fusionManager = GetComponent<SensorFusionManager>();
            autopilot = GetComponent<AutopilotController>();
            flightController = GetComponent<ManualFlightController>();
        }

        private void Update()
        {
            EvaluateTrackingQuality();
        }

        private void EvaluateTrackingQuality()
        {
            if (fusionManager == null) return;

            float score = fusionManager.FusionConfidenceScore;

            if (score >= 90.0f) currentTrackingQuality = TrackingQuality.Excellent;
            else if (score >= 80.0f) currentTrackingQuality = TrackingQuality.Good;
            else if (score >= 68.0f) currentTrackingQuality = TrackingQuality.Fair;
            else if (score >= 55.0f) currentTrackingQuality = TrackingQuality.Poor;
            else currentTrackingQuality = TrackingQuality.Lost;

            // Trigger Pose Recovery if Poor/Lost
            if (currentTrackingQuality >= TrackingQuality.Poor && !isRecoveryActive)
            {
                TriggerLocalizationRecovery();
            }
            else if (currentTrackingQuality <= TrackingQuality.Good && isRecoveryActive)
            {
                ResolveLocalizationRecovery();
            }
        }

        private void TriggerLocalizationRecovery()
        {
            isRecoveryActive = true;
            Logger.LogWarning("LOCALIZATION TRACKING POOR/LOST! Engaging Hover & Landmark Recovery Search.", LogCategory.AI);

            if (autopilot != null)
            {
                // Hover at current position and search for landmarks
                autopilot.EngageAutopilot(transform.position, 0f, transform.eulerAngles.y + 45f);
            }
        }

        private void ResolveLocalizationRecovery()
        {
            isRecoveryActive = false;
            Logger.Log("Localization Recovery Successful. Tracking quality restored.", LogCategory.AI);
        }
    }
}
