using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Interactive Measurement Ruler tool calculating real-time frame dimensions
    /// (Wheelbase 650mm, Arm Length 325mm, Propeller Diameter 15", Total Height 240mm, Ground Clearance 180mm).
    /// </summary>
    public class EngineeringMeasurementTool : MonoBehaviour
    {
        [Header("Frame Dimensions (Calculated)")]
        public float wheelbaseMM = 650.0f;
        public float armLengthMM = 325.0f;
        public float propellerDiameterInches = 15.0f;
        public float heightMM = 240.0f;
        public float groundClearanceMM = 180.0f;

        private void OnDrawGizmos()
        {
            if (EngineeringManager.Instance == null || !EngineeringManager.Instance.IsEngineeringModeActive) return;

            Vector3 center = transform.position;

            // Draw Wheelbase Dimension Line (Diagonally across motors)
            Gizmos.color = Color.white;
            float r = (wheelbaseMM * 0.001f) * 0.5f;
            Vector3 motor1 = center + new Vector3(r * 0.707f, 0.05f, r * 0.707f);
            Vector3 motor3 = center + new Vector3(-r * 0.707f, 0.05f, -r * 0.707f);

            Gizmos.DrawLine(motor1, motor3);
            Gizmos.DrawSphere(motor1, 0.02f);
            Gizmos.DrawSphere(motor3, 0.02f);
        }
    }
}




