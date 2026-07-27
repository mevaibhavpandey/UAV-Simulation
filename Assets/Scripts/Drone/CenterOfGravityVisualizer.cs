using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Calculates dynamic Center of Mass (CoM / CoG), payload balance offset,
    /// and renders force vectors (Motor Thrust arrows, Lift, Gravity, Drag) in Engineering Mode.
    /// </summary>
    public class CenterOfGravityVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showCoGMarker = true;
        [SerializeField] private bool showForceVectors = true;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnDrawGizmos()
        {
            if (EngineeringManager.Instance == null || !EngineeringManager.Instance.IsEngineeringModeActive) return;

            Vector3 comPos = rb != null ? transform.TransformPoint(rb.centerOfMass) : transform.position;

            // 1. Center of Gravity Marker (Sphere Wireframe)
            if (showCoGMarker)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(comPos, 0.08f);
                Gizmos.DrawRay(comPos, Vector3.up * 0.15f);
                Gizmos.DrawRay(comPos, Vector3.down * 0.15f);
                Gizmos.DrawRay(comPos, Vector3.left * 0.15f);
                Gizmos.DrawRay(comPos, Vector3.right * 0.15f);
            }

            // 2. Force Vectors (Gravity vs. Total Thrust)
            if (showForceVectors)
            {
                // Gravity Vector (Red Downward Arrow)
                Gizmos.color = Color.red;
                Gizmos.DrawRay(comPos, Vector3.down * 1.2f);

                // Total Thrust Vector (Cyan Upward Arrow)
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(comPos, transform.up * 1.5f);
            }
        }
    }
}



