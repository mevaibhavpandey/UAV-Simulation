using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Utility and builder component for generating pattern-based flight paths and validating waypoint sequences.
    /// </summary>
    public class MissionPlanner : MonoBehaviour
    {
        [Header("Default Planning Parameters")]
        [SerializeField] private float defaultAltitude = 15f;
        [SerializeField] private float defaultSpeed = 6f;

        /// <summary>
        /// Creates a simple point-to-point mission asset or instance from a list of local vectors.
        /// </summary>
        /// <param name="missionName">Name of the generated mission.</param>
        /// <param name="points">List of 3D target coordinates.</param>
        /// <param name="speed">Cruise speed in m/s.</param>
        /// <param name="holdTime">Hold time at each waypoint in seconds.</param>
        /// <returns>Populated Mission object.</returns>
        public Mission CreatePointToPointMission(string missionName, List<Vector3> points, float speed = 5f, float holdTime = 0f)
        {
            Mission mission = ScriptableObject.CreateInstance<Mission>();
            mission.MissionName = missionName;
            mission.CruiseSpeed = speed;

            if (points == null || points.Count == 0) return mission;

            // Takeoff waypoint
            mission.AddWaypoint(new Waypoint(new Vector3(0, points[0].y, 0), speed, 2f, WaypointAction.Takeoff));

            for (int i = 0; i < points.Count; i++)
            {
                WaypointAction action = (i == points.Count - 1) ? WaypointAction.Hover : WaypointAction.FlyThrough;
                mission.AddWaypoint(new Waypoint(points[i], speed, holdTime, action));
            }

            // Land waypoint
            Vector3 finalPos = points[points.Count - 1];
            mission.AddWaypoint(new Waypoint(new Vector3(finalPos.x, 0, finalPos.z), speed, 0f, WaypointAction.Land));

            return mission;
        }

        /// <summary>
        /// Generates a grid/lawnmower survey pattern for area scanning missions.
        /// </summary>
        /// <param name="center">Center origin of survey grid.</param>
        /// <param name="width">Width along X axis in meters.</param>
        /// <param name="height">Length along Z axis in meters.</param>
        /// <param name="spacing">Spacing between grid passes in meters.</param>
        /// <param name="altitude">Survey flight altitude in meters.</param>
        /// <returns>Generated grid survey Mission object.</returns>
        public Mission GenerateGridSurvey(Vector3 center, float width, float height, float spacing, float altitude)
        {
            Mission mission = ScriptableObject.CreateInstance<Mission>();
            mission.MissionName = "Grid Survey Pattern";
            mission.TargetAltitude = altitude;

            float startX = center.x - (width / 2f);
            float endX = center.x + (width / 2f);
            float startZ = center.z - (height / 2f);

            int passes = Mathf.Max(1, Mathf.FloorToInt(height / spacing));

            // Initial takeoff
            mission.AddWaypoint(new Waypoint(new Vector3(startX, altitude, startZ), defaultSpeed, 1f, WaypointAction.Takeoff));

            for (int i = 0; i <= passes; i++)
            {
                float currentZ = startZ + (i * spacing);
                if (i % 2 == 0)
                {
                    mission.AddWaypoint(new Waypoint(new Vector3(startX, altitude, currentZ), defaultSpeed, 0f, WaypointAction.TriggerPayload));
                    mission.AddWaypoint(new Waypoint(new Vector3(endX, altitude, currentZ), defaultSpeed, 0f, WaypointAction.TriggerPayload));
                }
                else
                {
                    mission.AddWaypoint(new Waypoint(new Vector3(endX, altitude, currentZ), defaultSpeed, 0f, WaypointAction.TriggerPayload));
                    mission.AddWaypoint(new Waypoint(new Vector3(startX, altitude, currentZ), defaultSpeed, 0f, WaypointAction.TriggerPayload));
                }
            }

            // Return to home & land
            mission.AddWaypoint(new Waypoint(Vector3.zero, defaultSpeed, 0f, WaypointAction.ReturnToHome));
            return mission;
        }

        /// <summary>
        /// Generates a circular orbit mission around a target Region of Interest (ROI).
        /// </summary>
        /// <param name="center">Target center point.</param>
        /// <param name="radius">Radius of orbit circle in meters.</param>
        /// <param name="altitude">Altitude of orbit in meters.</param>
        /// <param name="segments">Number of discrete waypoints along the circle.</param>
        /// <returns>Generated Orbit Mission object.</returns>
        public Mission GenerateOrbitPattern(Vector3 center, float radius, float altitude, int segments = 12)
        {
            Mission mission = ScriptableObject.CreateInstance<Mission>();
            mission.MissionName = "Orbit Survey Pattern";
            mission.TargetAltitude = altitude;

            segments = Mathf.Max(4, segments);
            float angleStep = (2f * Mathf.PI) / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                float x = center.x + radius * Mathf.Cos(angle);
                float z = center.z + radius * Mathf.Sin(angle);

                WaypointAction action = (i == 0) ? WaypointAction.Takeoff : WaypointAction.FlyThrough;
                mission.AddWaypoint(new Waypoint(new Vector3(x, altitude, z), defaultSpeed, 0f, action));
            }

            return mission;
        }

        /// <summary>
        /// Validates an existing mission and logs diagnostic status.
        /// </summary>
        /// <param name="mission">Mission instance to validate.</param>
        /// <returns>True if mission passes all safety rules.</returns>
        public bool Validate(Mission mission)
        {
            if (mission == null)
            {
                Debug.LogError("[MissionPlanner] Mission object is null.");
                return false;
            }

            bool isValid = mission.ValidateMission(out List<string> errors);
            if (!isValid)
            {
                foreach (var err in errors)
                {
                    Debug.LogWarning($"[MissionPlanner] Validation Error: {err}");
                }
            }
            else
            {
                Debug.Log($"[MissionPlanner] Mission '{mission.MissionName}' successfully validated ({mission.Waypoints.Count} waypoints).");
            }

            return isValid;
        }
    }
}




