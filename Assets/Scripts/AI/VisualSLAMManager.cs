using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    [System.Serializable]
    public class VisualFeaturePoint
    {
        public Vector3 worldPosition;
        public float responseScore;
        public bool isKeyframe;
    }

    /// <summary>
    /// Simulates Visual SLAM pipeline: 3D feature keypoint cloud generation, keyframe selection,
    /// landmark tracking, local map expansion, and loop closure detection simulation.
    /// </summary>
    public class VisualSLAMManager : MonoBehaviour
    {
        [Header("SLAM Parameters")]
        [SerializeField] private int maxFeaturePoints = 140;
        [SerializeField] private float fovDegrees = 60.0f;
        [SerializeField] private float maxDistance = 25.0f;

        [Header("Live Metrics")]
        [SerializeField] private int trackedFeatureCount = 128;
        [SerializeField] private int keyframeCount = 24;
        [SerializeField] private bool loopClosureDetected = false;
        [SerializeField] private List<VisualFeaturePoint> featurePoints = new List<VisualFeaturePoint>();

        public int TrackedFeatureCount => trackedFeatureCount;
        public int KeyframeCount => keyframeCount;
        public bool LoopClosureDetected => loopClosureDetected;
        public List<VisualFeaturePoint> FeaturePoints => featurePoints;

        private void Update()
        {
            SimulateVisualFeatures();
        }

        private void SimulateVisualFeatures()
        {
            featurePoints.Clear();

            // Generate synthetic visual feature keypoint cloud in front of camera frustum
            int count = Random.Range(110, maxFeaturePoints);
            trackedFeatureCount = count;

            for (int i = 0; i < count; i++)
            {
                float angleX = Random.Range(-fovDegrees * 0.5f, fovDegrees * 0.5f);
                float angleY = Random.Range(-fovDegrees * 0.4f, fovDegrees * 0.4f);
                float dist = Random.Range(3.0f, maxDistance);

                Quaternion rot = Quaternion.Euler(angleY, angleX, 0f);
                Vector3 localPoint = rot * Vector3.forward * dist;
                Vector3 worldPt = transform.TransformPoint(localPoint);

                featurePoints.Add(new VisualFeaturePoint
                {
                    worldPosition = worldPt,
                    responseScore = Random.Range(0.6f, 0.99f),
                    isKeyframe = (i % 5 == 0)
                });
            }

            keyframeCount = Mathf.RoundToInt(featurePoints.Count * 0.2f);
            
            // Loop closure simulation when returning near starting point (<10m)
            float distFromStart = Vector3.Distance(transform.position, new Vector3(35f, 0.25f, 20f));
            loopClosureDetected = (distFromStart < 10.0f && Time.time > 15.0f);
        }
    }
}



