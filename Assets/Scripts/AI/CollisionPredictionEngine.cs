using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Evaluates Time-To-Collision (TTC), relative closing velocity, collision probability %,
    /// and assigns Threat Levels (Safe, Low, Medium, High, Critical) to detected obstacles.
    /// </summary>
    [RequireComponent(typeof(ObstacleDetectionManager))]
    public class CollisionPredictionEngine : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private ThreatLevel highestThreatLevel = ThreatLevel.Safe;
        [SerializeField] private float minTimeToCollision = 99.0f;
        [SerializeField] private float maxCollisionProbability = 0.0f;

        private ObstacleDetectionManager detectionManager;
        private Rigidbody uavRigidbody;

        public ThreatLevel HighestThreatLevel => highestThreatLevel;
        public float MinTimeToCollision => minTimeToCollision;
        public float MaxCollisionProbability => maxCollisionProbability;

        private void Awake()
        {
            detectionManager = GetComponent<ObstacleDetectionManager>();
            uavRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            EvaluateCollisionRisks();
        }

        private void EvaluateCollisionRisks()
        {
            if (detectionManager == null || detectionManager.DetectedObstacles.Count == 0)
            {
                highestThreatLevel = ThreatLevel.Safe;
                minTimeToCollision = 99f;
                maxCollisionProbability = 0f;
                return;
            }

            Vector3 uavVel = uavRigidbody != null ? uavRigidbody.linearVelocity : transform.forward * 8.0f;
            ThreatLevel topThreat = ThreatLevel.Safe;
            float lowestTTC = 99f;
            float topProb = 0f;

            foreach (var obs in detectionManager.DetectedObstacles)
            {
                Vector3 relativeVel = uavVel - obs.velocity;
                float closingSpeed = Vector3.Dot(relativeVel, (obs.position - transform.position).normalized);

                if (closingSpeed > 0.1f)
                {
                    obs.timeToCollisionSeconds = obs.distance / closingSpeed;
                }
                else
                {
                    obs.timeToCollisionSeconds = 99f;
                }

                // Probability calculation
                float distFactor = Mathf.Clamp01(1.0f - (obs.distance / detectionManager.MaxDetectionRange));
                float velFactor = Mathf.Clamp01(closingSpeed / 15.0f);
                obs.collisionProbability = Mathf.Clamp01((distFactor * 0.7f) + (velFactor * 0.3f)) * 100.0f;

                // Threat Level Classification
                if (obs.distance < 4.0f || obs.timeToCollisionSeconds < 1.2f)
                {
                    obs.threatLevel = ThreatLevel.Critical;
                }
                else if (obs.distance < 10.0f || obs.timeToCollisionSeconds < 3.0f)
                {
                    obs.threatLevel = ThreatLevel.High;
                }
                else if (obs.distance < 18.0f || obs.timeToCollisionSeconds < 5.0f)
                {
                    obs.threatLevel = ThreatLevel.Medium;
                }
                else if (obs.distance < 25.0f)
                {
                    obs.threatLevel = ThreatLevel.Low;
                }
                else
                {
                    obs.threatLevel = ThreatLevel.Safe;
                }

                if (obs.threatLevel > topThreat) topThreat = obs.threatLevel;
                if (obs.timeToCollisionSeconds < lowestTTC) lowestTTC = obs.timeToCollisionSeconds;
                if (obs.collisionProbability > topProb) topProb = obs.collisionProbability;
            }

            highestThreatLevel = topThreat;
            minTimeToCollision = lowestTTC;
            maxCollisionProbability = topProb;
        }
    }
}


