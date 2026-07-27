using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Mission;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.UI.GCS
{
    /// <summary>
    /// Ground Control Station Mission Planner & Waypoint Manager UI Controller.
    /// Manages waypoint list creation, editing (Lat, Lon, Alt, Speed, HoldTime), mission save/load, and validation.
    /// </summary>
    public class GCSMissionPlannerUI : MonoBehaviour
    {
        [Header("Mission Data")]
        [SerializeField] private string missionName = "Default Research Flight Plan";
        [SerializeField] private List<WaypointData> waypointQueue = new List<WaypointData>();

        public List<WaypointData> WaypointQueue => waypointQueue;
        public string MissionName => missionName;

        private void Awake()
        {
            InitializeSampleWaypoints();
        }

        private void InitializeSampleWaypoints()
        {
            if (waypointQueue.Count > 0) return;

            waypointQueue.Add(new WaypointData
            {
                latitude = 13.0827,
                longitude = 80.2707,
                altitudeMSL = 25.0f,
                targetSpeed = 5.0f,
                holdDurationSeconds = 3.0f,
                action = WaypointActionType.Hover
            });

            waypointQueue.Add(new WaypointData
            {
                latitude = 13.0840,
                longitude = 80.2720,
                altitudeMSL = 35.0f,
                targetSpeed = 8.0f,
                holdDurationSeconds = 5.0f,
                action = WaypointActionType.Hover
            });

            waypointQueue.Add(new WaypointData
            {
                latitude = 13.0855,
                longitude = 80.2740,
                altitudeMSL = 20.0f,
                targetSpeed = 6.0f,
                holdDurationSeconds = 4.0f,
                action = WaypointActionType.ReturnToHome
            });
        }

        public void AddWaypoint(double lat, double lon, float alt, float speed, float hold)
        {
            waypointQueue.Add(new WaypointData
            {
                latitude = lat,
                longitude = lon,
                altitudeMSL = alt,
                targetSpeed = speed,
                holdDurationSeconds = hold,
                action = WaypointActionType.Hover
            });
            Debug.Log($"Added Waypoint WP{waypointQueue.Count} to Mission Plan.", LogCategory.Mission);
        }

        public void RemoveWaypoint(int index)
        {
            if (index >= 0 && index < waypointQueue.Count)
            {
                waypointQueue.RemoveAt(index);
            }
        }

        public void ClearMission()
        {
            waypointQueue.Clear();
        }
    }
}



