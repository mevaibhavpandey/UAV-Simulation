using System;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Phase 3 GPS-denied Visual-Inertial Odometry (VIO / SLAM) Stub provider for precise local localization without GPS lock.
    /// </summary>
    public class SLAMStub : AIModuleBase
    {
        [Header("SLAM Parameters")]
        [SerializeField] private bool isTracking = true;
        [SerializeField] private float confidenceScore = 0.95f;
        [SerializeField] private float driftRateMetersPerSec = 0.02f;

        private Vector3 estimatedPosition;
        private Quaternion estimatedRotation;
        private Vector3 accumulatedDrift;

        /// <summary>Gets whether SLAM feature tracking is active.</summary>
        public bool IsTracking => isTracking;

        /// <summary>Gets current estimated position in VIO map frame.</summary>
        public Vector3 EstimatedPosition => estimatedPosition;

        /// <summary>Gets current estimated rotation in VIO map frame.</summary>
        public Quaternion EstimatedRotation => estimatedRotation;

        /// <summary>Gets confidence score (0 to 1).</summary>
        public float ConfidenceScore => confidenceScore;

        /// <summary>Fired when estimated pose updates.</summary>
        public event Action<Vector3, Quaternion> OnPoseUpdated;

        private void Reset()
        {
            moduleName = "Visual SLAM Stub (Phase 3)";
        }

        public override void Initialize()
        {
            moduleName = "Visual SLAM Stub (Phase 3)";
            base.Initialize();
            ResetTracking();
            Debug.Log("[SLAMStub] Visual SLAM provider initialized in GPS-denied backup mode.");
        }

        public override void UpdateModule(float deltaTime)
        {
            if (!isTracking) return;

            // Simulate VIO odometry tracking relative to transform
            accumulatedDrift += UnityEngine.Random.insideUnitSphere * driftRateMetersPerSec * deltaTime;
            estimatedPosition = transform.position + accumulatedDrift;
            estimatedRotation = transform.rotation;

            // Slowly decrease confidence with accumulated drift
            confidenceScore = Mathf.Clamp01(0.98f - (accumulatedDrift.magnitude * 0.05f));

            OnPoseUpdated?.Invoke(estimatedPosition, estimatedRotation);
        }

        /// <summary>
        /// Resets map origin and zeroes accumulated drift error.
        /// </summary>
        public void ResetTracking()
        {
            accumulatedDrift = Vector3.zero;
            estimatedPosition = transform.position;
            estimatedRotation = transform.rotation;
            confidenceScore = 0.98f;
            Debug.Log("[SLAMStub] SLAM map origin reset to current position.");
        }
    }
}
