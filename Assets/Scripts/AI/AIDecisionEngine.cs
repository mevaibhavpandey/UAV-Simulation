using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.UI.GCS;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Event broadcast when AI decision engine changes behavior mode or clears threats.
    /// </summary>
    public struct AIDecisionEvent : IEvent
    {
        public ThreatLevel ThreatLevel;
        public AvoidanceStrategy ActiveStrategy;
        public string DecisionSummary;
    }

    /// <summary>
    /// Central AI Decision Orchestrator coordinating detection, risk evaluation, and dynamic path replanning.
    /// Broadcasts decision events and triggers toast alert warnings in GCS.
    /// </summary>
    [RequireComponent(typeof(ObstacleDetectionManager))]
    [RequireComponent(typeof(CollisionPredictionEngine))]
    [RequireComponent(typeof(DynamicPathPlanner))]
    public class AIDecisionEngine : MonoBehaviour
    {
        private ObstacleDetectionManager detectionManager;
        private CollisionPredictionEngine predictionEngine;
        private DynamicPathPlanner pathPlanner;

        private ThreatLevel lastThreatLevel = ThreatLevel.Safe;
        private AvoidanceStrategy lastStrategy = AvoidanceStrategy.Clear;

        private void Awake()
        {
            detectionManager = GetComponent<ObstacleDetectionManager>();
            predictionEngine = GetComponent<CollisionPredictionEngine>();
            pathPlanner = GetComponent<DynamicPathPlanner>();
        }

        private void Update()
        {
            EvaluateAIDecisions();
        }

        private void EvaluateAIDecisions()
        {
            ThreatLevel currentThreat = predictionEngine.HighestThreatLevel;
            AvoidanceStrategy currentStrategy = pathPlanner.ActiveStrategy;

            // Trigger alerts on state change
            if (currentThreat != lastThreatLevel || currentStrategy != lastStrategy)
            {
                lastThreatLevel = currentThreat;
                lastStrategy = currentStrategy;

                string decisionText = $"AI Mode: {currentStrategy} | Threat: {currentThreat}";

                EventBus.Publish(new AIDecisionEvent
                {
                    ThreatLevel = currentThreat,
                    ActiveStrategy = currentStrategy,
                    DecisionSummary = decisionText
                });

                if (currentThreat >= ThreatLevel.High)
                {
                    if (GCSNotificationSystem.Instance != null)
                    {
                        GCSNotificationSystem.Instance.PostNotification("Obstacle Detected", $"Collision Risk High ({currentThreat})! Autonomous {currentStrategy} engaged.", NotificationType.Warning);
                    }
                }
                else if (currentThreat == ThreatLevel.Safe && currentStrategy == AvoidanceStrategy.Clear)
                {
                    if (GCSNotificationSystem.Instance != null)
                    {
                        GCSNotificationSystem.Instance.PostNotification("Path Cleared", "Obstacle cleared. Restoring original mission trajectory.", NotificationType.Info);
                    }
                }
            }
        }
    }
}




