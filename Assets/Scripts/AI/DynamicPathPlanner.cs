using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Mission;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    public enum AvoidanceStrategy
    {
        Clear,
        Bypass_Left,
        Bypass_Right,
        Bypass_Above,
        Hover_Hold,
        ReturnHome
    }

    /// <summary>
    /// Dynamic Potential Field Path Planner generating smooth alternative detour trajectories
    /// around static and moving obstacles while maintaining safety distance margins.
    /// </summary>
    [RequireComponent(typeof(CollisionPredictionEngine))]
    public class DynamicPathPlanner : MonoBehaviour
    {
        [Header("Safety Parameters")]
        [SerializeField] private float safetyDistanceMargin = 5.0f; // 5.0m clearance buffer
        [SerializeField] private float detourOffsetDistance = 8.0f; // 8.0m lateral detour push

        [Header("Live Output")]
        [SerializeField] private AvoidanceStrategy activeStrategy = AvoidanceStrategy.Clear;
        [SerializeField] private Vector3 detourWaypointPosition;
        [SerializeField] private List<Vector3> activeDetourPath = new List<Vector3>();

        private CollisionPredictionEngine predictionEngine;
        private ObstacleDetectionManager detectionManager;
        private AutopilotController autopilot;

        public AvoidanceStrategy ActiveStrategy => activeStrategy;
        public Vector3 DetourWaypointPosition => detourWaypointPosition;
        public List<Vector3> ActiveDetourPath => activeDetourPath;

        private void Awake()
        {
            predictionEngine = GetComponent<CollisionPredictionEngine>();
            detectionManager = GetComponent<ObstacleDetectionManager>();
            autopilot = GetComponent<AutopilotController>();
        }

        private void Update()
        {
            ReevaluatePath();
        }

        private void ReevaluatePath()
        {
            if (predictionEngine == null || predictionEngine.HighestThreatLevel < ThreatLevel.Medium)
            {
                if (activeStrategy != AvoidanceStrategy.Clear)
                {
                    activeStrategy = AvoidanceStrategy.Clear;
                    activeDetourPath.Clear();
                    Debug.Log("Flight corridor clear. Restoring original mission trajectory.", LogCategory.AI);
                }
                return;
            }

            // Find closest critical obstacle
            DetectedObstacle threat = null;
            float minDist = 999f;
            foreach (var obs in detectionManager.DetectedObstacles)
            {
                if (obs.distance < minDist)
                {
                    minDist = obs.distance;
                    threat = obs;
                }
            }

            if (threat == null) return;

            // Decide Avoidance Strategy (Vector Potential Field)
            Vector3 uavForward = transform.forward;
            Vector3 uavRight = transform.right;
            Vector3 obsDirection = (threat.position - transform.position).normalized;

            // Determine if obstacle is slightly left or right of center
            float dotRight = Vector3.Dot(obsDirection, uavRight);

            if (dotRight >= 0f)
            {
                // Obstacle is to the right -> Bypass Left
                activeStrategy = AvoidanceStrategy.Bypass_Left;
                detourWaypointPosition = transform.position + (-uavRight * detourOffsetDistance) + (uavForward * (threat.distance + safetyDistanceMargin));
            }
            else
            {
                // Obstacle is to the left -> Bypass Right
                activeStrategy = AvoidanceStrategy.Bypass_Right;
                detourWaypointPosition = transform.position + (uavRight * detourOffsetDistance) + (uavForward * (threat.distance + safetyDistanceMargin));
            }

            // Build dynamic detour path visualization
            activeDetourPath.Clear();
            activeDetourPath.Add(transform.position);
            activeDetourPath.Add(detourWaypointPosition);
            activeDetourPath.Add(detourWaypointPosition + (uavForward * 15.0f)); // Re-entry point

            // Engage Autopilot steering towards detour waypoint if high/critical threat
            if (predictionEngine.HighestThreatLevel >= ThreatLevel.High && autopilot != null)
            {
                autopilot.EngageAutopilot(detourWaypointPosition, 6.0f, transform.eulerAngles.y);
            }
        }
    }
}




