using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Quality metric for SLAM visual/inertial tracking lock.
    /// </summary>
    public enum SLAMTrackingQuality
    {
        /// <summary>No visual/inertial features tracked.</summary>
        NotTracking,
        /// <summary>Tracking lost or degraded; low feature match count.</summary>
        Low,
        /// <summary>Moderate feature tracking lock.</summary>
        Medium,
        /// <summary>High precision robust feature tracking lock.</summary>
        High
    }

    /// <summary>
    /// Pose estimate provided by SLAM algorithm.
    /// </summary>
    [Serializable]
    public struct SLAMPoseEstimate
    {
        /// <summary>Estimated 3D position vector in map frame.</summary>
        public Vector3 Position;

        /// <summary>Estimated orientation quaternion in map frame.</summary>
        public Quaternion Orientation;

        /// <summary>Confidence covariance metric [0..1].</summary>
        public float Confidence;

        /// <summary>Timestamp of pose estimate in seconds.</summary>
        public double Timestamp;
    }

    /// <summary>
    /// Contract for Visual-Inertial SLAM / LiDAR SLAM providers in UAV simulation.
    /// </summary>
    public interface ISLAMProvider
    {
        /// <summary>
        /// Gets current tracking quality metric.
        /// </summary>
        SLAMTrackingQuality TrackingQuality { get; }

        /// <summary>
        /// Gets a value indicating whether SLAM engine is actively tracking pose.
        /// </summary>
        bool IsTracking { get; }

        /// <summary>
        /// Gets the latest estimated camera/UAV pose.
        /// </summary>
        SLAMPoseEstimate CurrentPose { get; }

        /// <summary>
        /// Gets the number of map points/features currently stored in the SLAM point cloud map.
        /// </summary>
        int MapPointCount { get; }

        /// <summary>
        /// Fired when estimated pose is updated by SLAM engine.
        /// </summary>
        event Action<SLAMPoseEstimate> OnPoseUpdated;

        /// <summary>
        /// Fired when tracking quality transitions.
        /// </summary>
        event Action<SLAMTrackingQuality> OnTrackingStateChanged;

        /// <summary>
        /// Fired when the map point cloud is updated with new feature points.
        /// </summary>
        event Action OnMapUpdated;

        /// <summary>
        /// Resets mapping state, clears point cloud, and reinitializes origin frame.
        /// </summary>
        void ResetMapping();

        /// <summary>
        /// Saves current generated point cloud and keyframe map to file path.
        /// </summary>
        /// <param name="filePath">Target destination file path.</param>
        /// <returns>True if map saved successfully.</returns>
        bool SaveMap(string filePath);

        /// <summary>
        /// Loads pre-built map point cloud file.
        /// </summary>
        /// <param name="filePath">Source map file path.</param>
        /// <returns>True if map loaded successfully.</returns>
        bool LoadMap(string filePath);

        /// <summary>
        /// Retrieves generated point cloud feature positions.
        /// </summary>
        /// <param name="outputPoints">List buffer to populate with 3D map points.</param>
        void GetMapPoints(List<Vector3> outputPoints);
    }
}



