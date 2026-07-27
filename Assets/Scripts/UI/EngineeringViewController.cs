using UnityEngine;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Presentation controller stub for engineering diagnostic mode toggling debug Gizmos, thrust vector rays, PID controller curve graphs, and obstacle raycasts.
    /// </summary>
    public class EngineeringViewController : MonoBehaviour
    {
        [Header("Visualization Overlays")]
        [SerializeField] private bool showThrustVectors = true;
        [SerializeField] private bool showWindVelocityVectors = true;
        [SerializeField] private bool showSensorRaycasts = true;
        [SerializeField] private bool showSLAMFeaturePoints = true;

        [Header("Debug Keybindings")]
        [SerializeField] private KeyCode toggleEngineeringModeKey = KeyCode.F12;

        [Header("Runtime Overlay Panel")]
        [SerializeField] private GameObject engineeringOverlayPanel;

        /// <summary>Gets or sets whether thrust vector gizmos are rendered.</summary>
        public bool ShowThrustVectors => showThrustVectors;

        /// <summary>Gets or sets whether wind velocity gizmos are rendered.</summary>
        public bool ShowWindVelocityVectors => showWindVelocityVectors;

        private void Update()
        {
            if (Input.GetKeyDown(toggleEngineeringModeKey))
            {
                ToggleEngineeringMode();
            }
        }

        /// <summary>
        /// Toggles visibility of the engineering visualization overlay panel.
        /// </summary>
        public void ToggleEngineeringMode()
        {
            if (engineeringOverlayPanel != null)
            {
                bool currentState = engineeringOverlayPanel.activeSelf;
                engineeringOverlayPanel.SetActive(!currentState);
                Debug.Log($"[EngineeringViewController] Engineering mode overlay state: {!currentState}");
            }
        }

        /// <summary>
        /// Toggles display of individual rotor thrust vectors.
        /// </summary>
        /// <param name="enabled">True to display vectors.</param>
        public void SetThrustVectorsVisible(bool enabled)
        {
            showThrustVectors = enabled;
        }

        /// <summary>
        /// Toggles display of dynamic wind turbulence vectors.
        /// </summary>
        /// <param name="enabled">True to display vectors.</param>
        public void SetWindVectorsVisible(bool enabled)
        {
            showWindVelocityVectors = enabled;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Render engineering debug gizmos
            if (showThrustVectors)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, transform.up * 3f);
            }

            if (showWindVelocityVectors)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + Vector3.up, Vector3.forward * 2f);
            }
        }
    }
}




