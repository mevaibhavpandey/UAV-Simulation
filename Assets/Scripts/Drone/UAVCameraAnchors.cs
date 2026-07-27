using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Supported UAV camera anchor vantage positions.
    /// </summary>
    public enum UAVCameraAnchorType
    {
        FPV,
        Front,
        Rear,
        Top,
        Bottom,
        Orbit,
        Engineering
    }

    /// <summary>
    /// Provides predefined transform anchor points attached to the UAV for inspection cameras.
    /// Anchor points: FPV, Front, Rear, Top, Bottom, Orbit, and Engineering.
    /// </summary>
    public class UAVCameraAnchors : MonoBehaviour
    {
        [Header("Anchors")]
        [SerializeField] private Transform fpvAnchor;
        [SerializeField] private Transform frontAnchor;
        [SerializeField] private Transform rearAnchor;
        [SerializeField] private Transform topAnchor;
        [SerializeField] private Transform bottomAnchor;
        [SerializeField] private Transform orbitAnchor;
        [SerializeField] private Transform engineeringAnchor;

        private Dictionary<UAVCameraAnchorType, Transform> anchorMap = new Dictionary<UAVCameraAnchorType, Transform>();

        private void Awake()
        {
            InitializeAnchorMap();
        }

        public void InitializeAnchorMap()
        {
            anchorMap.Clear();
            if (fpvAnchor != null) anchorMap[UAVCameraAnchorType.FPV] = fpvAnchor;
            if (frontAnchor != null) anchorMap[UAVCameraAnchorType.Front] = frontAnchor;
            if (rearAnchor != null) anchorMap[UAVCameraAnchorType.Rear] = rearAnchor;
            if (topAnchor != null) anchorMap[UAVCameraAnchorType.Top] = topAnchor;
            if (bottomAnchor != null) anchorMap[UAVCameraAnchorType.Bottom] = bottomAnchor;
            if (orbitAnchor != null) anchorMap[UAVCameraAnchorType.Orbit] = orbitAnchor;
            if (engineeringAnchor != null) anchorMap[UAVCameraAnchorType.Engineering] = engineeringAnchor;
        }

        /// <summary>
        /// Registers a specific camera anchor transform node.
        /// </summary>
        public void RegisterAnchor(UAVCameraAnchorType type, Transform node)
        {
            anchorMap[type] = node;
            switch (type)
            {
                case UAVCameraAnchorType.FPV: fpvAnchor = node; break;
                case UAVCameraAnchorType.Front: frontAnchor = node; break;
                case UAVCameraAnchorType.Rear: rearAnchor = node; break;
                case UAVCameraAnchorType.Top: topAnchor = node; break;
                case UAVCameraAnchorType.Bottom: bottomAnchor = node; break;
                case UAVCameraAnchorType.Orbit: orbitAnchor = node; break;
                case UAVCameraAnchorType.Engineering: engineeringAnchor = node; break;
            }
        }

        /// <summary>
        /// Gets anchor transform node for the requested anchor type.
        /// </summary>
        public Transform GetAnchor(UAVCameraAnchorType type)
        {
            if (anchorMap.TryGetValue(type, out Transform anchor))
            {
                return anchor;
            }
            return transform;
        }
    }
}



