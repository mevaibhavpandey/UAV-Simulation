using UnityEngine;
using ASTRA.UAV.Drone;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Defense-grade Component Inspector UI displaying technical specifications (Voltage, Current, Mass, Protocols)
    /// for selected drone sub-assemblies (Pixhawk 6X, Raspberry Pi 5, 4114 Motors, 6S Battery, Gimbal Camera, PDB).
    /// </summary>
    public class ComponentInspectorUI : MonoBehaviour
    {
        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;

        [Header("Selected Component")]
        [SerializeField] private int selectedComponentIndex = 0;

        private EngineeringManager engManager;
        private PresentationTourMode presentationTour;

        private readonly string[] componentNames = new string[]
        {
            "Pixhawk 6X Autopilot",
            "Raspberry Pi 5 Companion PC",
            "Tarot 4114 320KV Motors (4x)",
            "6S LiPo 8000mAh Battery",
            "2-Axis Gimbal Camera Payload",
            "Power Distribution Board (120A)"
        };

        private readonly string[] componentSpecs = new string[]
        {
            "<b>Processor:</b> STM32H753 @ 480MHz\n<b>Sensors:</b> Triple Redundant IMUs (ICM-42688-P / ICM-20649)\n<b>Mass:</b> 90 grams\n<b>Power:</b> 5.0V / 1.2A (6.0W)\n<b>Interfaces:</b> MAVLink, UART, CAN, SPI, I2C, Micro-USB",
            "<b>Processor:</b> Quad-Core ARM Cortex-A76 @ 2.4GHz\n<b>Memory:</b> 8GB LPDDR4X SDRAM\n<b>Mass:</b> 46 grams\n<b>Power:</b> 5.0V / 3.0A (15.0W)\n<b>Role:</b> Visual SLAM, VIO, ROS2 Node Host",
            "<b>Configuration:</b> 4x Brushless Outrunner 4114 320KV\n<b>Max Thrust:</b> 18.5 N / motor (7.4 kg Total Lift)\n<b>Propellers:</b> 15x5.5 Carbon Fiber CW/CCW\n<b>Mass:</b> 140g per motor (560g Total)",
            "<b>Chemistry:</b> Lithium Polymer (6S1P)\n<b>Nominal Voltage:</b> 22.2V (25.2V Peak)\n<b>Capacity:</b> 8000 mAh (177.6 Wh)\n<b>Discharge Rate:</b> 35C Continuous\n<b>Mass:</b> 980 grams",
            "<b>Sensor:</b> 4K Sony CMOS Sensor + Depth Stream\n<b>Gimbal:</b> 2-Axis Brushless Stabilization\n<b>Mass:</b> 185 grams\n<b>Power:</b> 12.0V / 0.8A (9.6W)\n<b>Interfaces:</b> HDMI, RTSP Video Stream",
            "<b>Rating:</b> 120A Continuous / 180A Burst\n<b>Output Voltage:</b> 5V / 12V Dual BEC\n<b>Mass:</b> 45 grams\n<b>Features:</b> Integrated Current Sensor, TVS Surge Diode"
        };

        private void Start()
        {
            if (droneObject == null) droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (droneObject != null)
            {
                engManager = EngineeringManager.Instance;
                presentationTour = droneObject.GetComponent<PresentationTourMode>();
            }
        }

        private void OnGUI()
        {
            if (EngineeringManager.Instance == null || !EngineeringManager.Instance.IsEngineeringModeActive) return;

            int width = 340;
            int height = 300;
            int x = 15;
            int y = 50;

            GUI.Box(new Rect(x, y, width, height), "Digital Twin — Component Spec Inspector");

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 };

            GUILayout.BeginArea(new Rect(x + 10, y + 25, width - 20, height - 30));

            GUILayout.Label("<b>Select Component to Inspect:</b>", style);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev", GUILayout.Width(50)))
            {
                selectedComponentIndex = (selectedComponentIndex - 1 + componentNames.Length) % componentNames.Length;
            }
            GUILayout.Label($"<color=yellow><b>{componentNames[selectedComponentIndex]}</b></color>", style);
            if (GUILayout.Button("Next", GUILayout.Width(50)))
            {
                selectedComponentIndex = (selectedComponentIndex + 1) % componentNames.Length;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label(componentSpecs[selectedComponentIndex], style);

            GUILayout.Space(12);
            GUILayout.Label("<b>Visualization Controls:</b>", style);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X-Ray View"))
            {
                if (engManager != null) engManager.SetRenderMode(EngineeringRenderMode.XRaySemiTransparent);
            }
            if (GUILayout.Button("Exploded Assembly"))
            {
                if (engManager != null) engManager.SetRenderMode(EngineeringRenderMode.ExplodedAssembly);
            }
            if (GUILayout.Button("Flight Normal"))
            {
                if (engManager != null) engManager.SetRenderMode(EngineeringRenderMode.Normal);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            if (presentationTour != null && !presentationTour.IsTourActive)
            {
                if (GUILayout.Button("RUN AUTOMATED 360° PRESENTATION TOUR", GUILayout.Height(28)))
                {
                    presentationTour.StartPresentationTour();
                }
            }
            else if (presentationTour != null && presentationTour.IsTourActive)
            {
                GUILayout.Button("<color=cyan><b>PRESENTATION TOUR IN PROGRESS...</b></color>", GUILayout.Height(28));
            }

            GUILayout.EndArea();
        }
    }
}



