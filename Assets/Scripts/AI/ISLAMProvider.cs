using System;
using UnityEngine;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Contract interface for Visual-Inertial Simultaneous Localization and Mapping (SLAM) providers.
    /// </summary>
    public interface ISLAMProvider
    {
        /// <summary>Gets whether SLAM tracking feature tracking and state estimation is active.</summary>
        bool IsTracking { get; }

        /// <summary>Gets current estimated 3D position vector in local VIO map frame.</summary>
        Vector3 EstimatedPosition { get; }

        /// <summary>Gets current estimated 3D rotation orientation in local VIO map frame.</summary>
        Quaternion EstimatedRotation { get; }

        /// <summary>Gets normalized confidence score (0.0 to 1.0) of feature point estimation quality.</summary>
        float ConfidenceScore { get; }

        /// <summary>Event raised whenever a new estimated pose is published by VIO system.</summary>
        event Action<Vector3, Quaternion> OnPoseUpdated;

        /// <summary>Resets map frame state and re-initializes tracking origin.</summary>
        void ResetTracking();
    }
}
