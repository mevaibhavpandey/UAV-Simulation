using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Definition of an exploded view assembly group.
    /// Stores original local position and target exploded offset vector.
    /// </summary>
    [System.Serializable]
    public class ExplodedSubAssembly
    {
        public string assemblyName;
        public Transform targetTransform;
        public Vector3 explodeOffsetVector;
        [HideInInspector] public Vector3 originalLocalPosition;
    }

    /// <summary>
    /// Controls exploded view animation for the ASTRA UAV Digital Twin.
    /// Separates frame, motors, landing gear, battery, electronics, GPS mast, and camera payload outward from center.
    /// </summary>
    public class UAVExplodedView : MonoBehaviour
    {
        [Header("Explode Controls")]
        [Range(0f, 1f)]
        [SerializeField] private float explosionProgress = 0.0f;
        [SerializeField] private float explosionDistanceMultiplier = 1.0f;
        [SerializeField] private float animationSmoothing = 8.0f;

        [Header("Sub-Assemblies")]
        [SerializeField] private List<ExplodedSubAssembly> subAssemblies = new List<ExplodedSubAssembly>();

        private float currentProgress = 0.0f;

        public float ExplosionProgress => explosionProgress;

        private void Awake()
        {
            InitializeSubAssemblyPositions();
        }

        public void InitializeSubAssemblyPositions()
        {
            foreach (var assembly in subAssemblies)
            {
                if (assembly.targetTransform != null)
                {
                    assembly.originalLocalPosition = assembly.targetTransform.localPosition;
                }
            }
        }

        private void Update()
        {
            currentProgress = Mathf.Lerp(currentProgress, explosionProgress, Time.deltaTime * animationSmoothing);
            ApplyExplodedPositions(currentProgress);
        }

        /// <summary>
        /// Sets explosion progress [0.0 = fully assembled, 1.0 = fully exploded].
        /// </summary>
        /// <param name="progress">Normalized value between 0 and 1.</param>
        public void SetExplosionProgress(float progress)
        {
            explosionProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// Registers a sub-assembly for exploded view separation.
        /// </summary>
        public void RegisterSubAssembly(string name, Transform transformNode, Vector3 explodeDirection)
        {
            if (transformNode == null) return;

            ExplodedSubAssembly sub = new ExplodedSubAssembly
            {
                assemblyName = name,
                targetTransform = transformNode,
                explodeOffsetVector = explodeDirection,
                originalLocalPosition = transformNode.localPosition
            };
            subAssemblies.Add(sub);
        }

        private void ApplyExplodedPositions(float progress)
        {
            foreach (var assembly in subAssemblies)
            {
                if (assembly.targetTransform == null) continue;

                Vector3 targetPos = assembly.originalLocalPosition + (assembly.explodeOffsetVector * (progress * explosionDistanceMultiplier));
                assembly.targetTransform.localPosition = targetPos;
            }
        }
    }
}





