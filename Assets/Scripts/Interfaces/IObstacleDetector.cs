using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Representation of a detected environmental obstacle.
    /// </summary>
    [Serializable]
    public struct ObstacleData
    {
        /// <summary>Unique identifier for the obstacle entry.</summary>
        public int Id;

        /// <summary>Estimated 3D position in world space.</summary>
        public Vector3 Position;

        /// <summary>Distance from sensor origin in meters.</summary>
        public float Distance;

        /// <summary>Surface normal vector at hit point.</summary>
        public Vector3 SurfaceNormal;

        /// <summary>Estimated bounding radius of obstacle in meters.</summary>
        public float BoundingRadius;

        /// <summary>Relative velocity vector of dynamic obstacle (m/s).</summary>
        public Vector3 Velocity;

        /// <summary>Timestamp of last detection frame in seconds.</summary>
        public double LastDetectedTime;
    }

    /// <summary>
    /// Contract for obstacle detection sensors (LiDAR, Ultrasonic, Depth Camera, Raycast array).
    /// </summary>
    public interface IObstacleDetector
    {
        /// <summary>
        /// Gets maximum effective range of the obstacle detector in meters.
        /// </summary>
        float DetectionRange { get; }

        /// <summary>
        /// Gets active horizontal field of view in degrees.
        /// </summary>
        float FieldOfViewDegrees { get; }

        /// <summary>
        /// Gets scan interval in seconds between sensor sweeps.
        /// </summary>
        float ScanIntervalSeconds { get; }

        /// <summary>
        /// Gets read-only list of currently tracked obstacles.
        /// </summary>
        IReadOnlyList<ObstacleData> DetectedObstacles { get; }

        /// <summary>
        /// Gets distance in meters to nearest detected obstacle (-1 if no obstacle detected).
        /// </summary>
        float NearestObstacleDistance { get; }

        /// <summary>
        /// Fired when new obstacles enter the detection zone.
        /// </summary>
        event Action<IReadOnlyList<ObstacleData>> OnObstacleDetected;

        /// <summary>
        /// Fired when an obstacle previously tracked is no longer detected.
        /// </summary>
        event Action<int> OnObstacleCleared;

        /// <summary>
        /// Triggers an immediate active sensor scan sweep.
        /// </summary>
        void PerformScan();

        /// <summary>
        /// Checks whether an obstacle lies within a given directional cone.
        /// </summary>
        /// <param name="direction">Normalized direction vector.</param>
        /// <param name="maxDistance">Check distance limit.</param>
        /// <param name="coneAngleDegrees">Angular aperture of check cone.</param>
        /// <returns>True if obstacle present within cone.</returns>
        bool IsObstacleInDirection(Vector3 direction, float maxDistance, float coneAngleDegrees);

        /// <summary>
        /// Gets details of the nearest detected obstacle.
        /// </summary>
        /// <param name="obstacle">Output obstacle data structure.</param>
        /// <returns>True if an obstacle was found, false otherwise.</returns>
        bool TryGetNearestObstacle(out ObstacleData obstacle);
    }
}


