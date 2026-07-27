using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Engineering Debug Visualizer rendering camera frustum, 3D feature keypoints (cyan/green dots),
    /// landmark anchors, camera pose history, and actual vs. sensor fusion estimated trajectory trails.
    /// </summary>
    [RequireComponent(typeof(VisualSLAMManager))]
    [RequireComponent(typeof(LandmarkManager))]
    [RequireComponent(typeof(SensorFusionManager))]
    public class SLAMDebugVisualizer : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color featurePointColor = new Color(0f, 0.9f, 1f, 0.8f);
        [SerializeField] private Color keyframePointColor = new Color(0.1f, 1f, 0.3f, 0.9f);
        [SerializeField] private Color landmarkColor = new Color(1f, 0.85f, 0f, 0.9f);
        [SerializeField] private Color actualPathColor = new Color(1f, 0.8f, 0.2f, 0.8f);
        [SerializeField] private Color estimatedPathColor = new Color(0f, 1f, 1f, 0.9f);

        [Header("Trajectory History")]
        [SerializeField] private List<Vector3> actualPathHistory = new List<Vector3>();
        [SerializeField] private List<Vector3> estimatedPathHistory = new List<Vector3>();

        private VisualSLAMManager slamManager;
        private LandmarkManager landmarkManager;
        private SensorFusionManager fusionManager;

        private void Awake()
        {
            slamManager = GetComponent<VisualSLAMManager>();
            landmarkManager = GetComponent<LandmarkManager>();
            fusionManager = GetComponent<SensorFusionManager>();
        }

        private void Update()
        {
            RecordPathHistory();
        }

        private void RecordPathHistory()
        {
            if (actualPathHistory.Count == 0 || Vector3.Distance(actualPathHistory[actualPathHistory.Count - 1], transform.position) > 1.5f)
            {
                actualPathHistory.Add(transform.position);
                Vector3 est = fusionManager != null ? fusionManager.FusedEstimatedPosition : transform.position;
                estimatedPathHistory.Add(est);
            }
        }

        private void OnDrawGizmos()
        {
            if (slamManager == null) slamManager = GetComponent<VisualSLAMManager>();
            if (landmarkManager == null) landmarkManager = GetComponent<LandmarkManager>();
            if (fusionManager == null) fusionManager = GetComponent<SensorFusionManager>();

            if (slamManager == null) return;

            // 1. Draw 3D Feature Keypoint Cloud
            foreach (var pt in slamManager.FeaturePoints)
            {
                Gizmos.color = pt.isKeyframe ? keyframePointColor : featurePointColor;
                Gizmos.DrawSphere(pt.worldPosition, pt.isKeyframe ? 0.15f : 0.08f);
            }

            // 2. Draw Landmark Anchors
            if (landmarkManager != null)
            {
                Gizmos.color = landmarkColor;
                foreach (var lm in landmarkManager.ActiveLandmarks)
                {
                    Gizmos.DrawWireCube(lm.worldPosition, new Vector3(2.5f, 2.5f, 2.5f));
                    Gizmos.DrawLine(transform.position, lm.worldPosition);
                }
            }

            // 3. Draw Actual vs. Estimated Trajectory Trails
            if (actualPathHistory.Count > 1)
            {
                Gizmos.color = actualPathColor;
                for (int i = 0; i < actualPathHistory.Count - 1; i++)
                {
                    Gizmos.DrawLine(actualPathHistory[i], actualPathHistory[i + 1]);
                }
            }

            if (estimatedPathHistory.Count > 1)
            {
                Gizmos.color = estimatedPathColor;
                for (int i = 0; i < estimatedPathHistory.Count - 1; i++)
                {
                    Gizmos.DrawLine(estimatedPathHistory[i], estimatedPathHistory[i + 1]);
                }
            }
        }
    }
}





