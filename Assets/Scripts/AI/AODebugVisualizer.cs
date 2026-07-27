using UnityEngine;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Debug Gizmos Visualizer rendering forward detection cone, color-coded threat rays (Green/Yellow/Red),
    /// obstacle bounding spheres, safe zone radius, and active alternative detour paths.
    /// </summary>
    [RequireComponent(typeof(ObstacleDetectionManager))]
    [RequireComponent(typeof(DynamicPathPlanner))]
    public class AODebugVisualizer : MonoBehaviour
    {
        [Header("Visualization Colors")]
        [SerializeField] private Color safeRayColor = new Color(0f, 1f, 0.2f, 0.8f);
        [SerializeField] private Color warningRayColor = new Color(1f, 0.8f, 0f, 0.9f);
        [SerializeField] private Color criticalRayColor = new Color(1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color detourPathColor = new Color(0f, 0.9f, 1f, 1f);

        private ObstacleDetectionManager detectionManager;
        private DynamicPathPlanner pathPlanner;

        private void Awake()
        {
            detectionManager = GetComponent<ObstacleDetectionManager>();
            pathPlanner = GetComponent<DynamicPathPlanner>();
        }

        private void OnDrawGizmos()
        {
            if (detectionManager == null) detectionManager = GetComponent<ObstacleDetectionManager>();
            if (pathPlanner == null) pathPlanner = GetComponent<DynamicPathPlanner>();

            if (detectionManager == null) return;

            // Draw Forward Detection Cone Outline
            Gizmos.color = safeRayColor;
            float maxRange = detectionManager.MaxDetectionRange;
            float halfAngle = detectionManager.DetectionConeAngle * 0.5f;

            Vector3 leftRay = Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * maxRange;
            Vector3 rightRay = Quaternion.Euler(0f, halfAngle, 0f) * transform.forward * maxRange;

            Gizmos.DrawRay(transform.position, leftRay);
            Gizmos.DrawRay(transform.position, rightRay);
            Gizmos.DrawRay(transform.position, transform.forward * maxRange);

            // Draw Detected Obstacles & Rays
            foreach (var obs in detectionManager.DetectedObstacles)
            {
                switch (obs.threatLevel)
                {
                    case ThreatLevel.Safe:
                    case ThreatLevel.Low:
                        Gizmos.color = safeRayColor;
                        break;
                    case ThreatLevel.Medium:
                        Gizmos.color = warningRayColor;
                        break;
                    case ThreatLevel.High:
                    case ThreatLevel.Critical:
                        Gizmos.color = criticalRayColor;
                        break;
                }

                Gizmos.DrawLine(transform.position, obs.position);
                Gizmos.DrawWireSphere(obs.position, obs.radius > 0.5f ? obs.radius : 1.5f);
            }

            // Draw Active Detour Path
            if (pathPlanner != null && pathPlanner.ActiveDetourPath.Count > 1)
            {
                Gizmos.color = detourPathColor;
                for (int i = 0; i < pathPlanner.ActiveDetourPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(pathPlanner.ActiveDetourPath[i], pathPlanner.ActiveDetourPath[i + 1]);
                    Gizmos.DrawSphere(pathPlanner.ActiveDetourPath[i + 1], 0.8f);
                }
            }
        }
    }
}


