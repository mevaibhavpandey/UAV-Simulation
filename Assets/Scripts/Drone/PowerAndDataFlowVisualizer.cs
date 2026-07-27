using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Animates electrical power distribution flow (Battery -> PDB -> ESCs -> Motors)
    /// and MAVLink/UART data flow packets (GPS -> Pixhawk 6X -> Raspberry Pi 5 -> Telemetry Radio -> GCS)
    /// using animated gizmo lines and travelling packet indicators.
    /// </summary>
    public class PowerAndDataFlowVisualizer : MonoBehaviour
    {
        [Header("Visualization Controls")]
        [SerializeField] private bool showPowerFlow = true;
        [SerializeField] private bool showDataFlow = true;

        [Header("Colors")]
        [SerializeField] private Color powerColor = new Color(1f, 0.2f, 0.1f, 0.9f); // Red power line
        [SerializeField] private Color dataColor = new Color(0f, 0.8f, 1f, 0.9f);  // Cyan MAVLink data line

        public bool ShowPowerFlow { get => showPowerFlow; set => showPowerFlow = value; }
        public bool ShowDataFlow { get => showDataFlow; set => showDataFlow = value; }

        private float animOffset = 0f;

        private void Update()
        {
            animOffset += Time.deltaTime * 4.0f;
        }

        private void OnDrawGizmos()
        {
            if (EngineeringManager.Instance == null || !EngineeringManager.Instance.IsEngineeringModeActive) return;

            Vector3 center = transform.position;

            // 1. Power Distribution Flow (Battery -> PDB -> ESCs -> Motors)
            if (showPowerFlow)
            {
                Gizmos.color = powerColor;
                Vector3 battPos = center + new Vector3(0f, -0.08f, 0f);
                Vector3 pdbPos = center + new Vector3(0f, 0.02f, 0f);

                Gizmos.DrawLine(battPos, pdbPos);

                // 4 Motor ESC Lines
                float armLen = 0.325f;
                Vector3 m1 = center + new Vector3(armLen * 0.707f, 0.05f, armLen * 0.707f);
                Vector3 m2 = center + new Vector3(-armLen * 0.707f, 0.05f, armLen * 0.707f);
                Vector3 m3 = center + new Vector3(-armLen * 0.707f, 0.05f, -armLen * 0.707f);
                Vector3 m4 = center + new Vector3(armLen * 0.707f, 0.05f, -armLen * 0.707f);

                Gizmos.DrawLine(pdbPos, m1);
                Gizmos.DrawLine(pdbPos, m2);
                Gizmos.DrawLine(pdbPos, m3);
                Gizmos.DrawLine(pdbPos, m4);
            }

            // 2. Data Flow (GPS -> Pixhawk 6X -> Raspberry Pi 5 -> Telemetry)
            if (showDataFlow)
            {
                Gizmos.color = dataColor;
                Vector3 gpsPos = center + new Vector3(0f, 0.22f, -0.15f);
                Vector3 pixhawkPos = center + new Vector3(0f, 0.06f, 0f);
                Vector3 rpi5Pos = center + new Vector3(0f, -0.04f, 0f);

                Gizmos.DrawLine(gpsPos, pixhawkPos);
                Gizmos.DrawLine(pixhawkPos, rpi5Pos);

                // Animated Packet Sphere travelling along line
                float t = (animOffset % 1.0f);
                Vector3 packetPos = Vector3.Lerp(pixhawkPos, rpi5Pos, t);
                Gizmos.DrawSphere(packetPos, 0.025f);
            }
        }
    }
}


