using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Procedural model builder for the Tarot 650 Sport inspired ASTRA UAV Digital Twin.
    /// Builds quadcopter frame, carbon arms, brushless motors, propellers, landing gear,
    /// Pixhawk 6X, Raspberry Pi 5, 6S LiPo battery, PDB, obstacle sensors, camera anchors,
    /// LODGroup, UAVExplodedView, and UAVEngineeringMode components.
    /// </summary>
    [ExecuteAlways]
    public class ASTRAUAVModelBuilder : MonoBehaviour
    {
        [Header("Frame Geometry Settings")]
        [SerializeField] private float armLength = 0.325f; // 650mm motor-to-motor diagonal (325mm arm offset)
        [SerializeField] private float armDiameter = 0.016f; // 16mm carbon tube diameter
        [SerializeField] private float propDiameter = 0.38f; // 15 inch (380mm) propeller

        [Header("Materials")]
        [SerializeField] private Material carbonFiberMaterial;
        [SerializeField] private Material aluminiumRedMaterial;
        [SerializeField] private Material aluminiumBlackMaterial;
        [SerializeField] private Material pcbGreenMaterial;
        [SerializeField] private Material lipoBatteryMaterial;
        [SerializeField] private Material propellerMaterial;
        [SerializeField] private Material xRayTransparentMaterial;
        [SerializeField] private Material hardwareHighlightMaterial;

        [Header("Auto Build On Start")]
        [SerializeField] private bool buildOnAwake = true;

        private void Awake()
        {
            if (buildOnAwake && transform.childCount == 0)
            {
                BuildUAVModel();
            }
        }

        [ContextMenu("Build Tarot 650 UAV Digital Twin")]
        public void BuildUAVModel()
        {
            CreateMaterials();

            // Rigidbody & Core setup
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 2.8f; // 2.8 kg Tarot 650 All-Up-Weight
            rb.linearDamping = 0.1f;
            rb.angularDamping = 2.0f;

            UAVExplodedView explodedView = GetOrAddComponent<UAVExplodedView>();
            UAVEngineeringMode engineeringMode = GetOrAddComponent<UAVEngineeringMode>();
            UAVCameraAnchors cameraAnchors = GetOrAddComponent<UAVCameraAnchors>();

            // Root Containers
            GameObject frameGroup = CreateChild("Frame");
            GameObject landingGearGroup = CreateChild("LandingGear");
            GameObject electronicsBayGroup = CreateChild("ElectronicsBay");
            GameObject batteryBayGroup = CreateChild("BatteryBay");
            GameObject pixhawkMountGroup = CreateChild("PixhawkMount");
            GameObject raspberryPiMountGroup = CreateChild("RaspberryPiMount");
            GameObject gpsMountGroup = CreateChild("GPSMount");
            GameObject cameraMountGroup = CreateChild("CameraMount");

            // 1. Central Body Frame (Top & Bottom Carbon Plates)
            GameObject topPlate = CreateCube("TopPlate", frameGroup.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.18f, 0.003f, 0.18f), carbonFiberMaterial);
            GameObject bottomPlate = CreateCube("BottomPlate", frameGroup.transform, new Vector3(0f, -0.04f, 0f), new Vector3(0.20f, 0.003f, 0.20f), carbonFiberMaterial);
            engineeringMode.RegisterRenderer(topPlate.GetComponent<Renderer>(), false);
            engineeringMode.RegisterRenderer(bottomPlate.GetComponent<Renderer>(), false);

            // 2. Arms, Motor Mounts, Motors & Propellers
            Vector3[] armDirections = new Vector3[]
            {
                new Vector3(1f, 0f, 1f).normalized,   // FL Front-Left (X Layout 45 deg)
                new Vector3(1f, 0f, -1f).normalized,  // FR Front-Right
                new Vector3(-1f, 0f, -1f).normalized, // RR Rear-Right
                new Vector3(-1f, 0f, 1f).normalized   // RL Rear-Left
            };

            string[] armNames = new string[] { "FL", "FR", "RR", "RL" };
            bool[] propIsCW = new bool[] { true, false, true, false };

            for (int i = 0; i < 4; i++)
            {
                Vector3 armDir = armDirections[i];
                string suffix = "_" + armNames[i];

                // Carbon Tube Arm
                GameObject arm = CreateCylinder("Arm" + suffix, frameGroup.transform, armDir * (armLength * 0.5f), new Vector3(armDiameter, armLength * 0.5f, armDiameter), carbonFiberMaterial);
                arm.transform.rotation = Quaternion.LookRotation(armDir, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
                engineeringMode.RegisterRenderer(arm.GetComponent<Renderer>(), false);

                // Motor Mount
                Vector3 motorPos = armDir * armLength;
                Material mountMat = (i % 2 == 0) ? aluminiumRedMaterial : aluminiumBlackMaterial;
                GameObject motorMount = CreateCube("MotorMount" + suffix, frameGroup.transform, motorPos, new Vector3(0.045f, 0.015f, 0.045f), mountMat);
                engineeringMode.RegisterRenderer(motorMount.GetComponent<Renderer>(), false);

                // Motor
                GameObject motor = CreateCylinder("Motor" + suffix, transform, motorPos + new Vector3(0f, 0.02f, 0f), new Vector3(0.042f, 0.018f, 0.042f), aluminiumBlackMaterial);
                motor.AddComponent<DroneMotor>();
                engineeringMode.RegisterRenderer(motor.GetComponent<Renderer>(), true);
                explodedView.RegisterSubAssembly("Motor" + suffix, motor.transform, armDir * 0.15f + Vector3.up * 0.1f);

                // Propeller
                GameObject prop = CreateCube("Propeller" + suffix, transform, motorPos + new Vector3(0f, 0.035f, 0f), new Vector3(propDiameter, 0.003f, 0.025f), propellerMaterial);
                engineeringMode.RegisterRenderer(prop.GetComponent<Renderer>(), false);
                explodedView.RegisterSubAssembly("Propeller" + suffix, prop.transform, armDir * 0.2f + Vector3.up * 0.25f);
            }

            // 3. Landing Gear (Tall wide stance for camera clearance)
            GameObject legL = CreateCylinder("Leg_Left", landingGearGroup.transform, new Vector3(-0.12f, -0.15f, 0f), new Vector3(0.012f, 0.12f, 0.012f), carbonFiberMaterial);
            GameObject legR = CreateCylinder("Leg_Right", landingGearGroup.transform, new Vector3(0.12f, -0.15f, 0f), new Vector3(0.012f, 0.12f, 0.012f), carbonFiberMaterial);
            GameObject footL = CreateCylinder("Foot_Left", landingGearGroup.transform, new Vector3(-0.12f, -0.22f, 0f), new Vector3(0.012f, 0.18f, 0.012f), carbonFiberMaterial);
            footL.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject footR = CreateCylinder("Foot_Right", landingGearGroup.transform, new Vector3(0.12f, -0.22f, 0f), new Vector3(0.012f, 0.18f, 0.012f), carbonFiberMaterial);
            footR.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            engineeringMode.RegisterRenderer(legL.GetComponent<Renderer>(), false);
            engineeringMode.RegisterRenderer(legR.GetComponent<Renderer>(), false);
            engineeringMode.RegisterRenderer(footL.GetComponent<Renderer>(), false);
            engineeringMode.RegisterRenderer(footR.GetComponent<Renderer>(), false);
            explodedView.RegisterSubAssembly("LandingGear", landingGearGroup.transform, Vector3.down * 0.2f);

            // 4. Electronics Bay & PDB
            GameObject pdb = CreateCube("PowerDistributionBoard", electronicsBayGroup.transform, new Vector3(0f, -0.02f, 0f), new Vector3(0.12f, 0.005f, 0.12f), pcbGreenMaterial);
            engineeringMode.RegisterRenderer(pdb.GetComponent<Renderer>(), true);

            // 5. Battery Bay & 6S LiPo
            GameObject lipo = CreateCube("Battery_6S_LiPo", batteryBayGroup.transform, new Vector3(0f, -0.08f, 0f), new Vector3(0.07f, 0.05f, 0.16f), lipoBatteryMaterial);
            engineeringMode.RegisterRenderer(lipo.GetComponent<Renderer>(), true);
            explodedView.RegisterSubAssembly("BatteryBay", batteryBayGroup.transform, Vector3.down * 0.3f);

            // 6. Pixhawk 6X Mount & Flight Controller
            GameObject pixhawkBase = CreateCube("PixhawkSiliconeMount", pixhawkMountGroup.transform, new Vector3(0f, 0.005f, 0f), new Vector3(0.06f, 0.008f, 0.06f), aluminiumRedMaterial);
            GameObject pixhawkFC = CreateCube("Pixhawk6X_FlightController", pixhawkMountGroup.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.05f, 0.015f, 0.05f), aluminiumBlackMaterial);
            engineeringMode.RegisterRenderer(pixhawkBase.GetComponent<Renderer>(), true);
            engineeringMode.RegisterRenderer(pixhawkFC.GetComponent<Renderer>(), true);
            explodedView.RegisterSubAssembly("PixhawkMount", pixhawkMountGroup.transform, Vector3.up * 0.12f);

            // 7. Raspberry Pi 5 Mount
            GameObject rpi5 = CreateCube("RaspberryPi5_CompanionComputer", raspberryPiMountGroup.transform, new Vector3(0f, 0.01f, -0.06f), new Vector3(0.06f, 0.012f, 0.09f), pcbGreenMaterial);
            engineeringMode.RegisterRenderer(rpi5.GetComponent<Renderer>(), true);
            explodedView.RegisterSubAssembly("RaspberryPiMount", raspberryPiMountGroup.transform, Vector3.up * 0.16f + Vector3.back * 0.05f);

            // 8. GPS Mast & Module
            GameObject gpsMast = CreateCylinder("GPS_Mast", gpsMountGroup.transform, new Vector3(0f, 0.12f, 0.05f), new Vector3(0.006f, 0.08f, 0.006f), carbonFiberMaterial);
            GameObject gpsDome = CreateCylinder("GPS_Compass_Module", gpsMountGroup.transform, new Vector3(0f, 0.20f, 0.05f), new Vector3(0.05f, 0.01f, 0.05f), aluminiumBlackMaterial);
            engineeringMode.RegisterRenderer(gpsMast.GetComponent<Renderer>(), false);
            engineeringMode.RegisterRenderer(gpsDome.GetComponent<Renderer>(), true);
            explodedView.RegisterSubAssembly("GPSMount", gpsMountGroup.transform, Vector3.up * 0.35f);

            // 9. Camera Gimbal Mount & Obstacle Sensors
            GameObject cameraGimbal = CreateCube("CameraGimbal_RGB_Depth", cameraMountGroup.transform, new Vector3(0f, -0.06f, 0.12f), new Vector3(0.05f, 0.04f, 0.05f), aluminiumBlackMaterial);
            engineeringMode.RegisterRenderer(cameraGimbal.GetComponent<Renderer>(), true);
            explodedView.RegisterSubAssembly("CameraMount", cameraMountGroup.transform, Vector3.forward * 0.25f + Vector3.down * 0.1f);

            // Obstacle Sensor Anchors
            GameObject sensorFront = CreateCube("Sensor_Front", cameraMountGroup.transform, new Vector3(0f, -0.02f, 0.14f), new Vector3(0.03f, 0.015f, 0.015f), aluminiumRedMaterial);
            GameObject sensorBottom = CreateCube("Sensor_Bottom_OpticalFlow", electronicsBayGroup.transform, new Vector3(0f, -0.045f, 0f), new Vector3(0.025f, 0.01f, 0.025f), aluminiumRedMaterial);
            engineeringMode.RegisterRenderer(sensorFront.GetComponent<Renderer>(), true);
            engineeringMode.RegisterRenderer(sensorBottom.GetComponent<Renderer>(), true);

            // 10. Camera Anchors Setup
            GameObject cameraAnchorsGroup = CreateChild("CameraAnchors");
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_FPV", new Vector3(0f, -0.05f, 0.15f), Quaternion.identity, cameraAnchors, UAVCameraAnchorType.FPV);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Front", new Vector3(0f, 0.2f, 1.2f), Quaternion.Euler(10f, 180f, 0f), cameraAnchors, UAVCameraAnchorType.Front);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Rear", new Vector3(0f, 0.3f, -1.4f), Quaternion.Euler(15f, 0f, 0f), cameraAnchors, UAVCameraAnchorType.Rear);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Top", new Vector3(0f, 1.8f, 0f), Quaternion.Euler(90f, 0f, 0f), cameraAnchors, UAVCameraAnchorType.Top);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Bottom", new Vector3(0f, -1.5f, 0f), Quaternion.Euler(-90f, 0f, 0f), cameraAnchors, UAVCameraAnchorType.Bottom);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Orbit", new Vector3(1.2f, 0.4f, 1.2f), Quaternion.Euler(15f, -135f, 0f), cameraAnchors, UAVCameraAnchorType.Orbit);
            CreateAnchor(cameraAnchorsGroup.transform, "Anchor_Engineering", new Vector3(0.8f, 0.3f, 0.8f), Quaternion.Euler(20f, -135f, 0f), cameraAnchors, UAVCameraAnchorType.Engineering);

            cameraAnchors.InitializeAnchorMap();
            explodedView.InitializeSubAssemblyPositions();

            // DroneCore Component Attachment
            DroneCore core = GetOrAddComponent<DroneCore>();

            Debug.Log("[ASTRA ModelBuilder] Tarot 650 Quadcopter Digital Twin generated successfully.");
        }

        private GameObject CreateChild(string name)
        {
            Transform t = transform.Find(name);
            if (t != null) return t.gameObject;

            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go;
        }

        private GameObject CreateCube(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localScale = scale;
            if (mat != null) cube.GetComponent<Renderer>().sharedMaterial = mat;
            return cube;
        }

        private GameObject CreateCylinder(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = name;
            cyl.transform.SetParent(parent, false);
            cyl.transform.localPosition = localPos;
            cyl.transform.localScale = scale;
            if (mat != null) cyl.GetComponent<Renderer>().sharedMaterial = mat;
            return cyl;
        }

        private void CreateAnchor(Transform parent, string name, Vector3 localPos, Quaternion localRot, UAVCameraAnchors anchors, UAVCameraAnchorType type)
        {
            GameObject anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPos;
            anchor.transform.localRotation = localRot;
            anchors.RegisterAnchor(type, anchor.transform);
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T comp = GetComponent<T>();
            if (comp == null) comp = gameObject.AddComponent<T>();
            return comp;
        }

        private void CreateMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            if (carbonFiberMaterial == null)
            {
                carbonFiberMaterial = new Material(litShader) { name = "Mat_CarbonFiber" };
                carbonFiberMaterial.color = new Color(0.12f, 0.13f, 0.15f);
            }
            if (aluminiumRedMaterial == null)
            {
                aluminiumRedMaterial = new Material(litShader) { name = "Mat_AnodizedAluminium_Red" };
                aluminiumRedMaterial.color = new Color(0.85f, 0.1f, 0.12f);
            }
            if (aluminiumBlackMaterial == null)
            {
                aluminiumBlackMaterial = new Material(litShader) { name = "Mat_AnodizedAluminium_Black" };
                aluminiumBlackMaterial.color = new Color(0.08f, 0.08f, 0.10f);
            }
            if (pcbGreenMaterial == null)
            {
                pcbGreenMaterial = new Material(litShader) { name = "Mat_PCB_Green" };
                pcbGreenMaterial.color = new Color(0.05f, 0.45f, 0.15f);
            }
            if (lipoBatteryMaterial == null)
            {
                lipoBatteryMaterial = new Material(litShader) { name = "Mat_6S_LiPo" };
                lipoBatteryMaterial.color = new Color(0.95f, 0.75f, 0.05f);
            }
            if (propellerMaterial == null)
            {
                propellerMaterial = new Material(litShader) { name = "Mat_PropellerCarbon" };
                propellerMaterial.color = new Color(0.18f, 0.19f, 0.22f);
            }
        }
    }
}



