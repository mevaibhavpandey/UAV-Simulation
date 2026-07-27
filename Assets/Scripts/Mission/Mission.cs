using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// ScriptableObject representing a complete UAV flight mission containing waypoints and flight safety parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMission", menuName = "ASTRA/UAV/Mission", order = 1)]
    public class Mission : ScriptableObject
    {
        [Header("Mission Identity")]
        [Tooltip("Unique identifier for this mission.")]
        [SerializeField] private string missionId = Guid.NewGuid().ToString();

        [Tooltip("Human-readable title of the mission.")]
        [SerializeField] private string missionName = "Default Mission";

        [TextArea(2, 5)]
        [Tooltip("Detailed description of the mission objectives.")]
        [SerializeField] private string description = "Standard reconnaissance mission.";

        [Header("Flight Parameters")]
        [Tooltip("Default target flight speed in m/s.")]
        [Range(0.5f, 30f)]
        [SerializeField] private float cruiseSpeed = 5.0f;

        [Tooltip("Maximum allowed speed during flight in m/s.")]
        [Range(1f, 50f)]
        [SerializeField] private float maxSpeed = 15.0f;

        [Tooltip("Target altitude above ground in meters.")]
        [Range(2f, 500f)]
        [SerializeField] private float targetAltitude = 10.0f;

        [Header("Safety Configuration")]
        [Tooltip("Automatically trigger Return-To-Home when battery falls below threshold.")]
        [SerializeField] private bool autoReturnToHomeOnLowBattery = true;

        [Tooltip("Low battery threshold percentage to trigger RTH.")]
        [Range(10f, 40f)]
        [SerializeField] private float lowBatteryThresholdPercent = 25.0f;

        [Tooltip("Maximum allowed distance from home base before triggering RTH (geofence limit).")]
        [Range(50f, 10000f)]
        [SerializeField] private float geofenceRadiusMeters = 1000.0f;

        [Header("Waypoints")]
        [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();

        /// <summary>Gets the unique identifier for this mission.</summary>
        public string MissionId => missionId;

        /// <summary>Gets or sets the human-readable mission name.</summary>
        public string MissionName
        {
            get => missionName;
            set => missionName = value;
        }

        /// <summary>Gets or sets the mission description.</summary>
        public string Description
        {
            get => description;
            set => description = value;
        }

        /// <summary>Gets or sets default cruise speed in m/s.</summary>
        public float CruiseSpeed
        {
            get => cruiseSpeed;
            set => cruiseSpeed = Mathf.Clamp(value, 0.5f, maxSpeed);
        }

        /// <summary>Gets or sets maximum speed limit in m/s.</summary>
        public float MaxSpeed
        {
            get => maxSpeed;
            set => maxSpeed = Mathf.Max(0.5f, value);
        }

        /// <summary>Gets or sets target altitude in meters.</summary>
        public float TargetAltitude
        {
            get => targetAltitude;
            set => targetAltitude = Mathf.Max(1.0f, value);
        }

        /// <summary>Gets or sets whether auto-RTH on low battery is enabled.</summary>
        public bool AutoReturnToHomeOnLowBattery
        {
            get => autoReturnToHomeOnLowBattery;
            set => autoReturnToHomeOnLowBattery = value;
        }

        /// <summary>Gets or sets low battery threshold percent.</summary>
        public float LowBatteryThresholdPercent
        {
            get => lowBatteryThresholdPercent;
            set => lowBatteryThresholdPercent = Mathf.Clamp(value, 5f, 50f);
        }

        /// <summary>Gets or sets the geofence radius limit in meters.</summary>
        public float GeofenceRadiusMeters
        {
            get => geofenceRadiusMeters;
            set => geofenceRadiusMeters = Mathf.Max(10f, value);
        }

        /// <summary>Gets the list of waypoints in this mission.</summary>
        public List<Waypoint> Waypoints => waypoints;

        /// <summary>
        /// Adds a new waypoint to the mission plan.
        /// </summary>
        /// <param name="waypoint">Waypoint to append.</param>
        public void AddWaypoint(Waypoint waypoint)
        {
            waypoints.Add(waypoint);
        }

        /// <summary>
        /// Removes a waypoint at the specified index.
        /// </summary>
        /// <param name="index">Index of waypoint to remove.</param>
        public void RemoveWaypointAt(int index)
        {
            if (index >= 0 && index < waypoints.Count)
            {
                waypoints.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clears all waypoints from the mission plan.
        /// </summary>
        public void ClearWaypoints()
        {
            waypoints.Clear();
        }

        /// <summary>
        /// Calculates the total path distance across all waypoints in sequence.
        /// </summary>
        /// <returns>Total distance in meters.</returns>
        public float CalculateTotalDistance()
        {
            if (waypoints == null || waypoints.Count < 2) return 0f;

            float totalDist = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                totalDist += waypoints[i].DistanceTo(waypoints[i + 1]);
            }
            return totalDist;
        }

        /// <summary>
        /// Validates the mission for missing data or safety rule violations.
        /// </summary>
        /// <param name="validationErrors">Outputs list of warning/error messages if any.</param>
        /// <returns>True if mission is valid for flight execution.</returns>
        public bool ValidateMission(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (waypoints == null || waypoints.Count == 0)
            {
                validationErrors.Add("Mission contains zero waypoints.");
            }

            if (cruiseSpeed <= 0.1f)
            {
                validationErrors.Add("Cruise speed must be greater than 0.1 m/s.");
            }

            if (maxSpeed < cruiseSpeed)
            {
                validationErrors.Add("Max speed cannot be lower than cruise speed.");
            }

            for (int i = 0; i < waypoints.Count; i++)
            {
                var wp = waypoints[i];
                if (wp.Altitude <= 0f)
                {
                    validationErrors.Add($"Waypoint #{i + 1} has an invalid altitude ({wp.Altitude}m). Must be > 0.");
                }

                if (wp.LocalPosition.magnitude > geofenceRadiusMeters)
                {
                    validationErrors.Add($"Waypoint #{i + 1} exceeds geofence radius limit ({geofenceRadiusMeters}m).");
                }
            }

            return validationErrors.Count == 0;
        }
    }
}



