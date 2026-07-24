using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.AI
{
    [System.Serializable]
    public class TrackedLandmark
    {
        public string landmarkName;
        public Vector3 worldPosition;
        public string type; // Building, Tower, Tree, Bridge, LakeEdge
        public float observationConfidence = 0.95f;
    }

    /// <summary>
    /// Tracks environmental landmarks (Buildings, Towers, Bridges, Trees, Rocks)
    /// and registers landmark observation constraints to prevent unbounded SLAM drift.
    /// </summary>
    public class LandmarkManager : MonoBehaviour
    {
        [Header("Landmark Database")]
        [SerializeField] private List<TrackedLandmark> activeLandmarks = new List<TrackedLandmark>();

        public List<TrackedLandmark> ActiveLandmarks => activeLandmarks;

        private void Awake()
        {
            InitializeFacilityLandmarks();
        }

        private void InitializeFacilityLandmarks()
        {
            if (activeLandmarks.Count > 0) return;

            activeLandmarks.Add(new TrackedLandmark { landmarkName = "GCS Mission Control Tower", worldPosition = new Vector3(50f, 6f, 0f), type = "Building", observationConfidence = 0.98f });
            activeLandmarks.Add(new TrackedLandmark { landmarkName = "UAV Maintenance Hangar", worldPosition = new Vector3(-35f, 7f, 40f), type = "Building", observationConfidence = 0.96f });
            activeLandmarks.Add(new TrackedLandmark { landmarkName = "Security Gate House", worldPosition = new Vector3(100f, 2.5f, 50f), type = "Structure", observationConfidence = 0.92f });
            activeLandmarks.Add(new TrackedLandmark { landmarkName = "Lake Perimeter Edge", worldPosition = new Vector3(-120f, 0.1f, -100f), type = "WaterEdge", observationConfidence = 0.90f });
            activeLandmarks.Add(new TrackedLandmark { landmarkName = "Communication Radar Mast", worldPosition = new Vector3(55f, 18f, 5f), type = "Tower", observationConfidence = 0.99f });
        }
    }
}
