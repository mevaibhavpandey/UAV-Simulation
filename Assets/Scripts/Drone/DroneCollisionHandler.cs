using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Event broadcast when drone collides with an obstacle or ground above crash threshold.
    /// </summary>
    public struct DroneCrashEvent : IEvent
    {
        public Vector3 CrashPosition;
        public float ImpactVelocity;
        public string CollidedObjectName;
    }

    /// <summary>
    /// Handles collision detection for the UAV against ground, buildings, trees, and obstacles.
    /// Triggers crash events and emergency stop if impact velocity exceeds safe thresholds.
    /// </summary>
    public class DroneCollisionHandler : MonoBehaviour
    {
        [Header("Thresholds")]
        [SerializeField] private float crashVelocityThreshold = 4.5f; // m/s impact threshold for crash

        private ManualFlightController flightController;
        private DroneStateMachine stateMachine;

        private void Awake()
        {
            flightController = GetComponent<ManualFlightController>();
            stateMachine = GetComponent<DroneStateMachine>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactVel = collision.relativeVelocity.magnitude;

            if (impactVel > crashVelocityThreshold)
            {
                UAVLogger.LogWarning($"CRASH DETECTED! Impact velocity: {impactVel:F1} m/s with {collision.gameObject.name}");

                if (flightController != null)
                {
                    flightController.TriggerEmergencyStop();
                }

                EventBus.Publish(new DroneCrashEvent
                {
                    CrashPosition = collision.contacts[0].point,
                    ImpactVelocity = impactVel,
                    CollidedObjectName = collision.gameObject.name
                });
            }
        }
    }
}





