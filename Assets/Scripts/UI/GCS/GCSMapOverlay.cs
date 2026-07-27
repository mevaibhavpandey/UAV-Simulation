using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Mission;

namespace ASTRA.UAV.UI.GCS
{
    /// <summary>
    /// Interactive 3D Mission Map Overlay rendering Home position marker,
    /// Waypoint markers, flight path line gizmos, and Geofence restricted boundary radius.
    /// </summary>
    public class GCSMapOverlay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GCSMissionPlannerUI missionPlanner;
        [SerializeField] private GameObject droneObject;

        [Header("Visualization Settings")]
        [SerializeField] private Color flightPathColor = new Color(0f, 0.8f, 1f, 0.9f);
        [SerializeField] private Color geofenceColor = new Color(1f, 0.2f, 0.2f, 0.4f);
        [SerializeField] private float geofenceRadiusMeters = 250.0f;

        private void Start()
        {
            if (missionPlanner == null) missionPlanner = GetComponent<GCSMissionPlannerUI>();
            if (droneObject == null) droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
        }

        private void OnDrawGizmos()
        {
            // Draw Geofence Radius
            Gizmos.color = geofenceColor;
            Gizmos.DrawWireSphere(Vector3.zero, geofenceRadiusMeters);

            // Draw Home Base Marker
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(new Vector3(35f, 1f, 20f), new Vector3(3f, 0.2f, 3f));

            if (missionPlanner == null || missionPlanner.WaypointQueue.Count == 0) return;

            // Draw Waypoints and Flight Path Lines
            Gizmos.color = flightPathColor;
            Vector3 prevPos = droneObject != null ? droneObject.transform.position : new Vector3(35f, 0.2f, 20f);

            for (int i = 0; i < missionPlanner.WaypointQueue.Count; i++)
            {
                var wp = missionPlanner.WaypointQueue[i];
                // Map Lat/Lon offset to local Unity world space (approx 1 deg = 100m scale demo)
                Vector3 wpWorldPos = new Vector3((float)(wp.longitude - 80.2707) * 1000f, wp.altitudeMSL, (float)(wp.latitude - 13.0827) * 1000f);

                Gizmos.DrawSphere(wpWorldPos, 1.5f);
                Gizmos.DrawLine(prevPos, wpWorldPos);
                prevPos = wpWorldPos;
            }
        }
    }
}



