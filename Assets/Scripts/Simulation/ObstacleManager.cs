using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Simulation
{
    /// <summary>
    /// Registry and query provider for spatial obstacles, terrain hazard colliders, and collision distance checks.
    /// </summary>
    public class ObstacleManager : MonoBehaviour
    {
        private static ObstacleManager instance;

        /// <summary>Gets static singleton instance.</summary>
        public static ObstacleManager Instance => instance;

        [Header("Collision Layers")]
        [SerializeField] private LayerMask obstacleLayerMask = -1;

        private readonly List<Collider> registeredObstacles = new List<Collider>();

        /// <summary>Gets read-only collection of registered obstacles.</summary>
        public IReadOnlyList<Collider> RegisteredObstacles => registeredObstacles;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        /// <summary>
        /// Registers a collider into the active obstacle tracking list.
        /// </summary>
        /// <param name="obstacle">Collider to track.</param>
        public void RegisterObstacle(Collider obstacle)
        {
            if (obstacle != null && !registeredObstacles.Contains(obstacle))
            {
                registeredObstacles.Add(obstacle);
            }
        }

        /// <summary>
        /// Unregisters an obstacle collider.
        /// </summary>
        /// <param name="obstacle">Collider to remove.</param>
        public void UnregisterObstacle(Collider obstacle)
        {
            if (obstacle != null)
            {
                registeredObstacles.Remove(obstacle);
            }
        }

        /// <summary>
        /// Queries the nearest obstacle to a given 3D position within maximum search radius.
        /// </summary>
        /// <param name="point">Origin position.</param>
        /// <param name="radius">Search radius in meters.</param>
        /// <param name="nearestPoint">Outputs nearest point on closest obstacle collider surface.</param>
        /// <returns>Nearest collider if found within radius, otherwise null.</returns>
        public Collider GetNearestObstacle(Vector3 point, float radius, out Vector3 nearestPoint)
        {
            nearestPoint = point;
            Collider closest = null;
            float minDistanceSq = radius * radius;

            for (int i = 0; i < registeredObstacles.Count; i++)
            {
                Collider col = registeredObstacles[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;

                Vector3 closestOnCol = col.ClosestPoint(point);
                float distSq = (closestOnCol - point).sqrMagnitude;

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closest = col;
                    nearestPoint = closestOnCol;
                }
            }

            return closest;
        }

        /// <summary>
        /// Performs sphere cast query against obstacles in scene along a ray.
        /// </summary>
        /// <param name="origin">Sphere cast starting point.</param>
        /// <param name="radius">Radius of bounding sphere.</param>
        /// <param name="direction">Cast direction vector.</param>
        /// <param name="maxDistance">Maximum cast distance.</param>
        /// <param name="hitInfo">Hit output result.</param>
        /// <returns>True if an obstacle collision is detected.</returns>
        public bool SphereCastObstacles(Vector3 origin, float radius, Vector3 direction, float maxDistance, out RaycastHit hitInfo)
        {
            return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, obstacleLayerMask);
        }
    }
}
