using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Manages Engineering Inspection Mode for the ASTRA UAV Digital Twin.
    /// Toggles body frame transparency (X-Ray effect) while highlighting internal components
    /// (Pixhawk 6X, Raspberry Pi 5, Battery, Power Distribution Board).
    /// </summary>
    public class UAVEngineeringMode : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private bool isEngineeringModeActive = false;

        [Header("Renderers")]
        [SerializeField] private List<Renderer> outerFrameRenderers = new List<Renderer>();
        [SerializeField] private List<Renderer> internalHardwareRenderers = new List<Renderer>();

        [Header("Materials")]
        [SerializeField] private Material xRayTransparentMaterial;
        [SerializeField] private Material hardwareHighlightMaterial;

        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public bool IsEngineeringModeActive => isEngineeringModeActive;

        private void Awake()
        {
            CacheOriginalMaterials();
        }

        private void CacheOriginalMaterials()
        {
            originalMaterials.Clear();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                originalMaterials[r] = r.sharedMaterials;
            }
        }

        /// <summary>
        /// Registers a renderer under outer body frame or internal hardware.
        /// </summary>
        public void RegisterRenderer(Renderer r, bool isInternalHardware)
        {
            if (r == null) return;
            originalMaterials[r] = r.sharedMaterials;

            if (isInternalHardware)
            {
                if (!internalHardwareRenderers.Contains(r)) internalHardwareRenderers.Add(r);
            }
            else
            {
                if (!outerFrameRenderers.Contains(r)) outerFrameRenderers.Add(r);
            }
        }

        /// <summary>
        /// Sets Engineering X-Ray inspection mode on or off.
        /// </summary>
        public void SetEngineeringMode(bool active)
        {
            isEngineeringModeActive = active;

            if (active)
            {
                ApplyXRayState();
            }
            else
            {
                RestoreOriginalMaterials();
            }
        }

        /// <summary>
        /// Toggles Engineering inspection mode state.
        /// </summary>
        public void ToggleEngineeringMode()
        {
            SetEngineeringMode(!isEngineeringModeActive);
        }

        private void ApplyXRayState()
        {
            // Make outer frame semi-transparent
            foreach (var r in outerFrameRenderers)
            {
                if (r == null) continue;
                if (xRayTransparentMaterial != null)
                {
                    Material[] xRayMats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < xRayMats.Length; i++) xRayMats[i] = xRayTransparentMaterial;
                    r.sharedMaterials = xRayMats;
                }
            }

            // Highlight internal components
            foreach (var r in internalHardwareRenderers)
            {
                if (r == null) continue;
                if (hardwareHighlightMaterial != null)
                {
                    Material[] highlightMats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < highlightMats.Length; i++) highlightMats[i] = hardwareHighlightMaterial;
                    r.sharedMaterials = highlightMats;
                }
            }
        }

        private void RestoreOriginalMaterials()
        {
            foreach (var kvp in originalMaterials)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.sharedMaterials = kvp.Value;
                }
            }
        }
    }
}





