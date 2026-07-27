using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    public enum ThreatLevel
    {
        Safe,
        Low,
        Medium,
        High,
        Critical
    }

    [System.Serializable]
    public class DetectedObstacle
    {
        public int obstacleID;
        public string objectName;
        public Vector3 position;
        public Vector3 velocity;
        public float distance;
        public float radius;
        public ThreatLevel threatLevel = ThreatLevel.Safe;
        public float timeToCollisionSeconds = 99f;
        public float collisionProbability = 0f;
    }

    /// <summary>
    /// Multi-range directional obstacle detector using SphereCasts and forward detection cones.
    /// Tracks static and moving obstacles across 4 detection zones (Close 5m, Medium 15m, Long 30m, Critical 3m).
    /// </summary>
    public class ObstacleDetectionManager : MonoBehaviour
    {
        [Header("Scan Parameters")]
        [SerializeField] private float maxDetectionRange = 30.0f;
        [SerializeField] private float detectionConeAngle = 60.0f;
        [SerializeField] private float sphereCastRadius = 2.5f;
        [SerializeField] private LayerMask obstacleLayerMask = ~0; // Scan all layers by default

        [Header("Detected Targets")]
        [SerializeField] private List<DetectedObstacle> detectedObstacles = new List<DetectedObstacle>();

        public List<DetectedObstacle> DetectedObstacles => detectedObstacles;
        public float MaxDetectionRange => maxDetectionRange;
        public float DetectionConeAngle => detectionConeAngle;

        private void Update()
        {
            PerformObstacleScan();
        }

        private void PerformObstacleScan()
        {
            detectedObstacles.Clear();

            // Forward SphereCast along UAV orientation
            RaycastHit[] hits = UnityEngine.Physics.SphereCastAll(transform.position, sphereCastRadius, transform.forward, maxDetectionRange, obstacleLayerMask);

            int idCounter = 1;
            foreach (var hit in hits)
            {
                // Ignore self
                if (hit.transform.root == transform.root) continue;

                Vector3 dirToTarget = (hit.point - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToTarget);

                if (angle <= detectionConeAngle * 0.5f)
                {
                    Vector3 objVel = Vector3.zero;
                    Rigidbody targetRb = hit.collider.attachedRigidbody;
                    if (targetRb != null) objVel = targetRb.linearVelocity;

                    DetectedObstacle obs = new DetectedObstacle
                    {
                        obstacleID = idCounter++,
                        objectName = hit.collider.name,
                        position = hit.point,
                        velocity = objVel,
                        distance = hit.distance,
                        radius = hit.collider.bounds.extents.magnitude
                    };

                    detectedObstacles.Add(obs);
                }
            }
        }
    }
}





