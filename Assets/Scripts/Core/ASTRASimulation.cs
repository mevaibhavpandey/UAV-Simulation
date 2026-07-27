using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// ASTRA UAV Complete Simulation Bootstrap.
    /// Attach this to any empty GameObject in the scene and press Play.
    /// Builds the entire simulation procedurally - drone, environment, HUD, cameras.
    /// Zero external script dependencies.
    /// </summary>
    public class ASTRASimulation : MonoBehaviour
    {
        // ─── Drone State ───────────────────────────────────────────────
        private GameObject droneRoot;
        private Rigidbody droneRb;
        private Transform[] propellers = new Transform[4];

        private bool isArmed = false;
        private bool isFlying = false;
        private float throttle = 0f;
        private float targetAltitude = 0f;
        private float currentAltitude = 0f;
        private float batteryLevel = 100f;
        private float flightTime = 0f;
        private Vector3 velocity = Vector3.zero;

        // Flight tuning
        private const float MAX_THRUST = 35f;
        private const float HOVER_THRUST = 27.44f; // 2.8kg * 9.81
        private const float MAX_TILT = 25f;
        private const float YAW_SPEED = 90f;
        private const float PITCH_SPEED = 60f;
        private const float ROLL_SPEED = 60f;
        private const float MAX_SPEED = 15f;

        // ─── Flight Mode ────────────────────────────────────────────────
        private enum FlightMode { Manual, AltitudeHold, ReturnHome }
        private FlightMode currentMode = FlightMode.AltitudeHold;
        private Vector3 homePosition;

        // ─── Camera ─────────────────────────────────────────────────────
        private Camera mainCam;
        private int cameraView = 0; // 0=Follow, 1=FPV, 2=Top, 3=Free
        private Vector3 camOffset = new Vector3(0, 3, -8);
        private float freeCamYaw = 0f;
        private float freeCamPitch = 20f;
        private float freeCamDist = 12f;

        // ─── UI References ──────────────────────────────────────────────
        private Text txtMode, txtAlt, txtSpeed, txtBattery, txtArmed;
        private Text txtThrottle, txtFlightTime, txtControls, txtGPS;
        private Text txtWind, txtMessage;
        private Image batteryBar, throttleBar;
        private GameObject hudCanvas;
        private GameObject controlsPanel;
        private bool showControls = true;

        // ─── Environment ─────────────────────────────────────────────────
        private float windStrength = 0f;
        private Vector3 windDirection = Vector3.right;
        private float weatherTimer = 0f;

        // ─── Message System ──────────────────────────────────────────────
        private Queue<string> messageQueue = new Queue<string>();
        private float messageClearTimer = 0f;

        // ─────────────────────────────────────────────────────────────────
        #region INIT
        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildEnvironment();
            BuildDrone();
            BuildCameras();
            BuildHUD();
            homePosition = droneRoot.transform.position;
            ShowMessage("ASTRA UAV Simulation Loaded — Press [SPACE] to ARM");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region BUILD ENVIRONMENT
        // ─────────────────────────────────────────────────────────────────

        private void BuildEnvironment()
        {
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(100, 1, 100);
            ground.transform.position = Vector3.zero;
            SetColor(ground, new Color(0.25f, 0.28f, 0.22f)); // military green-grey

            // Runway (long dark strip)
            GameObject runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Runway";
            runway.transform.position = new Vector3(0, 0.01f, 60f);
            runway.transform.localScale = new Vector3(15, 0.02f, 200);
            SetColor(runway, new Color(0.15f, 0.15f, 0.15f));

            // Runway center line markings
            for (int i = -4; i <= 4; i++)
            {
                GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = "RunwayMark";
                mark.transform.position = new Vector3(0, 0.02f, 60f + i * 20f);
                mark.transform.localScale = new Vector3(0.5f, 0.02f, 8f);
                SetColor(mark, Color.white);
                Destroy(mark.GetComponent<Collider>());
            }

            // Helipad
            GameObject helipad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            helipad.name = "Helipad";
            helipad.transform.position = new Vector3(0, 0.05f, 0);
            helipad.transform.localScale = new Vector3(8, 0.05f, 8);
            SetColor(helipad, new Color(0.1f, 0.1f, 0.12f));

            // Helipad H marking
            for (int i = 0; i < 3; i++)
            {
                GameObject hbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hbar.name = "HelipadH";
                hbar.transform.position = new Vector3(i == 1 ? 0 : (i == 0 ? -1.2f : 1.2f), 0.12f, 0);
                hbar.transform.localScale = i == 1 ? new Vector3(2.4f, 0.05f, 0.3f) : new Vector3(0.3f, 0.05f, 2.5f);
                SetColor(hbar, Color.yellow);
                Destroy(hbar.GetComponent<Collider>());
            }

            // GCS Building
            BuildBuilding(new Vector3(40, 0, -20), new Vector3(20, 8, 12), new Color(0.4f, 0.42f, 0.45f), "GCS_Building");

            // Hangar
            BuildBuilding(new Vector3(-45, 0, 10), new Vector3(30, 10, 20), new Color(0.55f, 0.52f, 0.48f), "Hangar");

            // Control Tower
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "ControlTower";
            tower.transform.position = new Vector3(35, 10, -15);
            tower.transform.localScale = new Vector3(5, 20, 5);
            SetColor(tower, new Color(0.45f, 0.48f, 0.52f));

            // Tower top (glass cabin)
            GameObject towerTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            towerTop.name = "TowerTop";
            towerTop.transform.position = new Vector3(35, 22, -15);
            towerTop.transform.localScale = new Vector3(7, 4, 7);
            SetColor(towerTop, new Color(0.3f, 0.6f, 0.8f, 0.5f));

            // Fence posts around perimeter
            for (int i = 0; i < 24; i++)
            {
                float angle = i / 24f * 360f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 180f, 1.5f, Mathf.Sin(angle) * 180f);
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.transform.position = pos;
                post.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
                SetColor(post, new Color(0.6f, 0.6f, 0.6f));
                Destroy(post.GetComponent<Collider>());
            }

            // Directional light (Sun)
            if (FindObjectOfType<Light>() == null)
            {
                GameObject sun = new GameObject("DirectionalLight");
                Light sunLight = sun.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.intensity = 1.2f;
                sunLight.color = new Color(1f, 0.95f, 0.85f);
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private void BuildBuilding(Vector3 pos, Vector3 scale, Color col, string name)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = name;
            b.transform.position = pos + Vector3.up * scale.y * 0.5f;
            b.transform.localScale = scale;
            SetColor(b, col);

            // Roof detail
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = name + "_Roof";
            roof.transform.position = pos + Vector3.up * (scale.y + 0.3f);
            roof.transform.localScale = new Vector3(scale.x + 0.5f, 0.4f, scale.z + 0.5f);
            SetColor(roof, col * 0.8f);
            Destroy(roof.GetComponent<Collider>());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region BUILD DRONE
        // ─────────────────────────────────────────────────────────────────

        private void BuildDrone()
        {
            // Find or create drone root
            droneRoot = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (droneRoot == null)
            {
                droneRoot = new GameObject("ASTRA_UAV_DigitalTwin");
            }
            else
            {
                // Remove broken scripts
                foreach (var comp in droneRoot.GetComponents<MonoBehaviour>())
                {
                    if (comp == null || comp.GetType() == typeof(ASTRASimulation)) continue;
                    // Only keep ASTRASimulation, remove others that may be broken
                }
            }

            droneRoot.transform.position = new Vector3(0, 0.5f, 0);

            // Rigidbody
            droneRb = droneRoot.GetComponent<Rigidbody>();
            if (droneRb == null) droneRb = droneRoot.AddComponent<Rigidbody>();
            droneRb.mass = 2.8f;
            droneRb.linearDamping = 0.5f;
            droneRb.angularDamping = 3f;
            droneRb.useGravity = true;
            droneRb.interpolation = RigidbodyInterpolation.Interpolate;
            droneRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            droneRb.constraints = RigidbodyConstraints.None;

            // Box collider (hull)
            BoxCollider col = droneRoot.GetComponent<BoxCollider>();
            if (col == null) col = droneRoot.AddComponent<BoxCollider>();
            col.size = new Vector3(0.65f, 0.12f, 0.65f);
            col.center = Vector3.zero;

            // ── Frame Center Body ──────────────────────────
            GameObject centerBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerBody.name = "CenterBody";
            centerBody.transform.SetParent(droneRoot.transform, false);
            centerBody.transform.localPosition = Vector3.zero;
            centerBody.transform.localScale = new Vector3(0.22f, 0.06f, 0.22f);
            SetColor(centerBody, new Color(0.12f, 0.12f, 0.14f));
            Destroy(centerBody.GetComponent<Collider>());

            // Battery plate on top
            GameObject battery = GameObject.CreatePrimitive(PrimitiveType.Cube);
            battery.name = "Battery";
            battery.transform.SetParent(droneRoot.transform, false);
            battery.transform.localPosition = new Vector3(0, 0.045f, 0);
            battery.transform.localScale = new Vector3(0.18f, 0.035f, 0.08f);
            SetColor(battery, new Color(0.1f, 0.5f, 0.1f));
            Destroy(battery.GetComponent<Collider>());

            // Pixhawk FC
            GameObject fc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fc.name = "FlightController";
            fc.transform.SetParent(droneRoot.transform, false);
            fc.transform.localPosition = new Vector3(0, 0.07f, 0.04f);
            fc.transform.localScale = new Vector3(0.055f, 0.02f, 0.038f);
            SetColor(fc, new Color(0.6f, 0.1f, 0.1f));
            Destroy(fc.GetComponent<Collider>());

            // ── Arms (X-config) ────────────────────────────
            float armLen = 0.35f;
            Vector3[] armDirs = {
                new Vector3( 1, 0,  1).normalized,
                new Vector3(-1, 0,  1).normalized,
                new Vector3(-1, 0, -1).normalized,
                new Vector3( 1, 0, -1).normalized
            };
            Color armColor = new Color(0.08f, 0.08f, 0.1f);

            for (int i = 0; i < 4; i++)
            {
                GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arm.name = $"Arm_{i}";
                arm.transform.SetParent(droneRoot.transform, false);
                arm.transform.localPosition = armDirs[i] * armLen * 0.5f;
                arm.transform.localRotation = Quaternion.LookRotation(armDirs[i]) * Quaternion.Euler(0, 45, 0);
                arm.transform.localScale = new Vector3(0.032f, 0.025f, armLen);
                SetColor(arm, armColor);
                Destroy(arm.GetComponent<Collider>());
            }

            // ── Motors & Propellers ────────────────────────
            Color[] motorColors = {
                new Color(0.9f, 0.3f, 0.1f),
                new Color(0.9f, 0.3f, 0.1f),
                new Color(0.1f, 0.3f, 0.9f),
                new Color(0.1f, 0.3f, 0.9f)
            };

            for (int i = 0; i < 4; i++)
            {
                Vector3 motorPos = armDirs[i] * armLen;

                // Motor housing
                GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                motor.name = $"Motor_{i}";
                motor.transform.SetParent(droneRoot.transform, false);
                motor.transform.localPosition = motorPos + Vector3.up * 0.02f;
                motor.transform.localScale = new Vector3(0.055f, 0.025f, 0.055f);
                SetColor(motor, motorColors[i]);
                Destroy(motor.GetComponent<Collider>());

                // Propeller hub
                GameObject propHub = new GameObject($"Propeller_{i}");
                propHub.transform.SetParent(droneRoot.transform, false);
                propHub.transform.localPosition = motorPos + Vector3.up * 0.05f;
                propellers[i] = propHub.transform;

                // 2 prop blades
                for (int b = 0; b < 2; b++)
                {
                    GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    blade.name = $"Blade_{i}_{b}";
                    blade.transform.SetParent(propHub.transform, false);
                    blade.transform.localPosition = new Vector3(b == 0 ? 0.075f : -0.075f, 0, 0);
                    blade.transform.localScale = new Vector3(0.15f, 0.006f, 0.025f);
                    SetColor(blade, new Color(0.85f, 0.85f, 0.85f));
                    Destroy(blade.GetComponent<Collider>());
                }

                // Landing leg
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = $"Leg_{i}";
                leg.transform.SetParent(droneRoot.transform, false);
                leg.transform.localPosition = motorPos * 0.7f + Vector3.down * 0.1f;
                leg.transform.localScale = new Vector3(0.018f, 0.15f, 0.018f);
                SetColor(leg, new Color(0.3f, 0.3f, 0.35f));
                Destroy(leg.GetComponent<Collider>());
            }

            // Camera gimbal
            GameObject gimbal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gimbal.name = "GimbalCamera";
            gimbal.transform.SetParent(droneRoot.transform, false);
            gimbal.transform.localPosition = new Vector3(0, -0.04f, 0.1f);
            gimbal.transform.localScale = new Vector3(0.055f, 0.04f, 0.055f);
            SetColor(gimbal, new Color(0.1f, 0.1f, 0.12f));
            Destroy(gimbal.GetComponent<Collider>());

            // GPS antenna
            GameObject gps = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gps.name = "GPS";
            gps.transform.SetParent(droneRoot.transform, false);
            gps.transform.localPosition = new Vector3(0, 0.07f, -0.07f);
            gps.transform.localScale = new Vector3(0.04f, 0.015f, 0.04f);
            SetColor(gps, Color.white);
            Destroy(gps.GetComponent<Collider>());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region BUILD CAMERAS
        // ─────────────────────────────────────────────────────────────────

        private void BuildCameras()
        {
            Camera existing = Camera.main;
            if (existing != null)
            {
                mainCam = existing;
            }
            else
            {
                GameObject camGO = new GameObject("MainCamera");
                camGO.tag = "MainCamera";
                mainCam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }
            mainCam.backgroundColor = new Color(0.45f, 0.6f, 0.8f);
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 65f;
            mainCam.nearClipPlane = 0.1f;
            mainCam.farClipPlane = 2000f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region BUILD HUD
        // ─────────────────────────────────────────────────────────────────

        private void BuildHUD()
        {
            // Destroy any old canvas
            GameObject oldCanvas = GameObject.Find("ASTRA_HUD");
            if (oldCanvas != null) Destroy(oldCanvas);

            hudCanvas = new GameObject("ASTRA_HUD");
            Canvas canvas = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            hudCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            hudCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            hudCanvas.AddComponent<GraphicRaycaster>();

            // ── Top Status Bar ──────────────────────────────
            GameObject topBar = MakePanel(hudCanvas.transform, new Vector2(0, -25), new Vector2(1920, 50),
                new Color(0, 0, 0, 0.75f), TextAnchor.MiddleCenter);
            topBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            topBar.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            topBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            MakeLabel(topBar.transform, "ASTRA UAV DIGITAL TWIN  |  AUTONOMOUS SYSTEMS RESEARCH  |  BMSCE",
                new Vector2(0, 0), new Vector2(1400, 40), 18, Color.white, FontStyle.Bold);

            // ── Left Telemetry Panel ─────────────────────────
            GameObject leftPanel = MakePanel(hudCanvas.transform, new Vector2(10, -60), new Vector2(280, 340),
                new Color(0f, 0.03f, 0.08f, 0.88f));
            leftPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            leftPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            leftPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            MakeLabel(leftPanel.transform, "◈ FLIGHT TELEMETRY", new Vector2(0, -10), new Vector2(260, 28), 13,
                new Color(0.3f, 0.8f, 1f), FontStyle.Bold);
            AddDivider(leftPanel.transform, new Vector2(0, -30));

            txtArmed = MakeLabel(leftPanel.transform, "STATUS: DISARMED", new Vector2(0, -48), new Vector2(260, 24), 12, new Color(1f, 0.3f, 0.3f));
            txtMode = MakeLabel(leftPanel.transform, "MODE: ALTITUDE HOLD", new Vector2(0, -72), new Vector2(260, 24), 12, new Color(0.3f, 1f, 0.5f));
            txtAlt = MakeLabel(leftPanel.transform, "ALTITUDE:    0.00 m", new Vector2(0, -96), new Vector2(260, 24), 12, Color.white);
            txtSpeed = MakeLabel(leftPanel.transform, "SPEED:       0.00 m/s", new Vector2(0, -120), new Vector2(260, 24), 12, Color.white);
            txtThrottle = MakeLabel(leftPanel.transform, "THROTTLE:    0%", new Vector2(0, -144), new Vector2(260, 24), 12, Color.white);
            txtFlightTime = MakeLabel(leftPanel.transform, "FLIGHT TIME: 0:00", new Vector2(0, -168), new Vector2(260, 24), 12, Color.white);
            txtWind = MakeLabel(leftPanel.transform, "WIND:        0.0 m/s", new Vector2(0, -192), new Vector2(260, 24), 12, Color.white);
            txtGPS = MakeLabel(leftPanel.transform, "GPS: 3D FIX  SAT: 14", new Vector2(0, -216), new Vector2(260, 24), 12, new Color(0.3f, 1f, 0.5f));

            AddDivider(leftPanel.transform, new Vector2(0, -244));
            MakeLabel(leftPanel.transform, "BATTERY", new Vector2(-70, -258), new Vector2(80, 20), 11, Color.white);
            txtBattery = MakeLabel(leftPanel.transform, "100%", new Vector2(60, -258), new Vector2(60, 20), 11, new Color(0.3f, 1f, 0.3f));
            batteryBar = MakeFillBar(leftPanel.transform, new Vector2(0, -278), new Vector2(250, 14), new Color(0.15f, 0.8f, 0.2f));

            MakeLabel(leftPanel.transform, "THROTTLE", new Vector2(-60, -300), new Vector2(100, 20), 11, Color.white);
            throttleBar = MakeFillBar(leftPanel.transform, new Vector2(0, -320), new Vector2(250, 14), new Color(0.2f, 0.6f, 1f));

            // ── Right Camera/Mode Panel ──────────────────────
            GameObject rightPanel = MakePanel(hudCanvas.transform, new Vector2(-10, -60), new Vector2(240, 200),
                new Color(0f, 0.03f, 0.08f, 0.88f));
            rightPanel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            rightPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            rightPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

            MakeLabel(rightPanel.transform, "◈ CAMERA VIEW", new Vector2(0, -10), new Vector2(220, 28), 13,
                new Color(0.3f, 0.8f, 1f), FontStyle.Bold);
            AddDivider(rightPanel.transform, new Vector2(0, -30));
            MakeLabel(rightPanel.transform, "[V] Follow Camera", new Vector2(0, -50), new Vector2(220, 22), 11, Color.white);
            MakeLabel(rightPanel.transform, "[V] FPV Camera", new Vector2(0, -72), new Vector2(220, 22), 11, Color.white);
            MakeLabel(rightPanel.transform, "[V] Top-Down Camera", new Vector2(0, -94), new Vector2(220, 22), 11, Color.white);
            MakeLabel(rightPanel.transform, "[V] Free-Look Camera", new Vector2(0, -116), new Vector2(220, 22), 11, Color.white);
            AddDivider(rightPanel.transform, new Vector2(0, -140));
            MakeLabel(rightPanel.transform, "[M] Cycle Flight Modes", new Vector2(0, -158), new Vector2(220, 22), 11, new Color(1f, 0.85f, 0.3f));
            MakeLabel(rightPanel.transform, "[R] Return to Home", new Vector2(0, -178), new Vector2(220, 22), 11, new Color(1f, 0.6f, 0.3f));

            // ── Controls Panel (bottom) ──────────────────────
            controlsPanel = MakePanel(hudCanvas.transform, new Vector2(0, 10), new Vector2(940, 130),
                new Color(0f, 0.03f, 0.08f, 0.88f));
            controlsPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            controlsPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            controlsPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

            MakeLabel(controlsPanel.transform, "◈ FLIGHT CONTROLS", new Vector2(0, -8), new Vector2(900, 24), 12,
                new Color(0.3f, 0.8f, 1f), FontStyle.Bold);
            string ctrlText = "[SPACE] ARM / DISARM     [W/S] THROTTLE UP/DOWN     [↑/↓] PITCH     [←/→] ROLL     [Q/E] YAW     [V] CAMERA     [M] MODE     [H] HIDE";
            MakeLabel(controlsPanel.transform, ctrlText, new Vector2(0, -36), new Vector2(900, 22), 11, Color.white);
            string ctrlText2 = "[1] Manual Mode     [2] Altitude Hold     [3] Return Home     [F1-F4] Camera Views     [SHIFT+S] Emergency Stop";
            MakeLabel(controlsPanel.transform, ctrlText2, new Vector2(0, -60), new Vector2(900, 22), 11, new Color(0.9f, 0.9f, 0.7f));
            string ctrlText3 = "Scroll Wheel: Zoom     Right-Click+Drag: Free Look     Middle-Click+Drag: Pan";
            MakeLabel(controlsPanel.transform, ctrlText3, new Vector2(0, -84), new Vector2(900, 22), 11, new Color(0.7f, 0.9f, 0.7f));
            MakeLabel(controlsPanel.transform, "[H] Toggle this panel", new Vector2(0, -108), new Vector2(900, 20), 10, new Color(0.6f, 0.6f, 0.6f));

            // ── Message Banner (center bottom) ───────────────
            GameObject msgPanel = MakePanel(hudCanvas.transform, new Vector2(0, 160), new Vector2(700, 44),
                new Color(0f, 0f, 0f, 0.7f));
            msgPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            msgPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            msgPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            txtMessage = MakeLabel(msgPanel.transform, "", new Vector2(0, 0), new Vector2(680, 40), 15,
                new Color(1f, 0.9f, 0.3f), FontStyle.Bold);

            // ── Crosshair ────────────────────────────────────
            MakeCrosshair(hudCanvas.transform);

            // ── Compass ──────────────────────────────────────
            MakeCompass(hudCanvas.transform);
        }

        private void MakeCrosshair(Transform parent)
        {
            // Horizontal line
            GameObject h = new GameObject("CrosshairH");
            h.transform.SetParent(parent, false);
            RectTransform rh = h.AddComponent<RectTransform>();
            rh.anchorMin = rh.anchorMax = new Vector2(0.5f, 0.5f);
            rh.sizeDelta = new Vector2(20, 2);
            Image ih = h.AddComponent<Image>();
            ih.color = new Color(1, 1, 1, 0.7f);

            GameObject v = new GameObject("CrosshairV");
            v.transform.SetParent(parent, false);
            RectTransform rv = v.AddComponent<RectTransform>();
            rv.anchorMin = rv.anchorMax = new Vector2(0.5f, 0.5f);
            rv.sizeDelta = new Vector2(2, 20);
            Image iv = v.AddComponent<Image>();
            iv.color = new Color(1, 1, 1, 0.7f);
        }

        private void MakeCompass(Transform parent)
        {
            // Placeholder compass ring in top-center
            GameObject comp = MakePanel(parent, new Vector2(0, -60), new Vector2(200, 36),
                new Color(0, 0.03f, 0.08f, 0.85f));
            comp.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            comp.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            comp.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            MakeLabel(comp.transform, "N  NE  E  SE  S  SW  W  NW", new Vector2(0, 0), new Vector2(195, 32), 11,
                new Color(0.3f, 0.8f, 1f));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region UPDATE - FLIGHT PHYSICS
        // ─────────────────────────────────────────────────────────────────

        private void Update()
        {
            HandleInput();
            AnimatePropellers();
            UpdateCamera();
            UpdateHUD();
            UpdateWeather();
        }

        private void FixedUpdate()
        {
            if (!isArmed) return;
            ApplyFlightPhysics();
        }

        private void HandleInput()
        {
            // ── ARM / DISARM ────────────────────────────────
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isArmed = !isArmed;
                if (isArmed)
                {
                    homePosition = droneRoot.transform.position;
                    droneRb.isKinematic = false;
                    ShowMessage("MOTORS ARMED — Increase throttle [W] to take off");
                }
                else
                {
                    throttle = 0f;
                    isFlying = false;
                    ShowMessage("MOTORS DISARMED — Drone safe");
                }
            }

            if (!isArmed) return;

            // ── EMERGENCY STOP ──────────────────────────────
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            {
                isArmed = false;
                throttle = 0f;
                isFlying = false;
                droneRb.linearVelocity = Vector3.zero;
                droneRb.angularVelocity = Vector3.zero;
                ShowMessage("⚠ EMERGENCY STOP ENGAGED");
                return;
            }

            // ── THROTTLE ────────────────────────────────────
            if (currentMode == FlightMode.Manual)
            {
                if (Input.GetKey(KeyCode.W)) throttle = Mathf.Min(throttle + Time.deltaTime * 0.6f, 1f);
                if (Input.GetKey(KeyCode.S)) throttle = Mathf.Max(throttle - Time.deltaTime * 0.6f, 0f);
            }
            else if (currentMode == FlightMode.AltitudeHold)
            {
                if (Input.GetKey(KeyCode.W)) targetAltitude += Time.deltaTime * 4f;
                if (Input.GetKey(KeyCode.S)) targetAltitude = Mathf.Max(0.3f, targetAltitude - Time.deltaTime * 4f);
                if (targetAltitude < 0.3f && currentAltitude < 0.5f) targetAltitude = 0f;
            }
            else if (currentMode == FlightMode.ReturnHome)
            {
                // Auto-handled in physics
            }

            // ── PITCH (Up/Down arrows) ──────────────────────
            float pitchInput = 0f;
            if (Input.GetKey(KeyCode.UpArrow)) pitchInput = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) pitchInput = -1f;

            // ── ROLL (Left/Right arrows) ────────────────────
            float rollInput = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) rollInput = 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) rollInput = -1f;

            // ── YAW (Q/E) ───────────────────────────────────
            float yawInput = 0f;
            if (Input.GetKey(KeyCode.E)) yawInput = 1f;
            if (Input.GetKey(KeyCode.Q)) yawInput = -1f;

            // Apply yaw rotation
            if (isFlying || currentAltitude > 0.1f)
            {
                droneRb.AddTorque(Vector3.up * yawInput * YAW_SPEED * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }

            // Store inputs for FixedUpdate via fields
            _pitchInput = pitchInput;
            _rollInput = rollInput;

            // ── CAMERA ──────────────────────────────────────
            if (Input.GetKeyDown(KeyCode.V)) cameraView = (cameraView + 1) % 4;
            if (Input.GetKeyDown(KeyCode.F1)) cameraView = 0;
            if (Input.GetKeyDown(KeyCode.F2)) cameraView = 1;
            if (Input.GetKeyDown(KeyCode.F3)) cameraView = 2;
            if (Input.GetKeyDown(KeyCode.F4)) cameraView = 3;

            // ── FLIGHT MODES ─────────────────────────────────
            if (Input.GetKeyDown(KeyCode.M))
            {
                currentMode = (FlightMode)(((int)currentMode + 1) % 3);
                ShowMessage($"Mode: {currentMode}");
                if (currentMode == FlightMode.AltitudeHold)
                    targetAltitude = currentAltitude;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) { currentMode = FlightMode.Manual; ShowMessage("Manual Mode"); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { currentMode = FlightMode.AltitudeHold; targetAltitude = currentAltitude; ShowMessage("Altitude Hold Mode"); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { currentMode = FlightMode.ReturnHome; targetAltitude = Mathf.Max(currentAltitude, 8f); ShowMessage("Return to Home Engaged"); }

            // ── HIDE UI ──────────────────────────────────────
            if (Input.GetKeyDown(KeyCode.H))
            {
                showControls = !showControls;
                if (controlsPanel != null) controlsPanel.SetActive(showControls);
            }

            // Free cam mouse
            if (cameraView == 3)
            {
                if (Input.GetMouseButton(1))
                {
                    freeCamYaw += Input.GetAxis("Mouse X") * 3f;
                    freeCamPitch -= Input.GetAxis("Mouse Y") * 3f;
                    freeCamPitch = Mathf.Clamp(freeCamPitch, -10f, 85f);
                }
                freeCamDist -= Input.GetAxis("Mouse ScrollWheel") * 10f;
                freeCamDist = Mathf.Clamp(freeCamDist, 3f, 80f);
            }
            else
            {
                freeCamDist -= Input.GetAxis("Mouse ScrollWheel") * 10f;
                freeCamDist = Mathf.Clamp(freeCamDist, 3f, 80f);
            }
        }

        private float _pitchInput = 0f;
        private float _rollInput = 0f;

        private void ApplyFlightPhysics()
        {
            currentAltitude = droneRoot.transform.position.y;
            float dt = Time.fixedDeltaTime;

            if (currentMode == FlightMode.ReturnHome)
            {
                // Fly toward home
                Vector3 homeDir = (homePosition - droneRoot.transform.position);
                homeDir.y = 0;
                if (homeDir.magnitude > 1f)
                {
                    _pitchInput = Mathf.Clamp(homeDir.z * 0.2f, -1f, 1f);
                    _rollInput = Mathf.Clamp(homeDir.x * 0.2f, -1f, 1f);
                }
                else { _pitchInput = 0; _rollInput = 0; }
            }

            // ── Tilt for movement ───────────────────────────
            float targetPitch = -_pitchInput * MAX_TILT;
            float targetRoll = _rollInput * MAX_TILT;

            Quaternion targetRot = Quaternion.Euler(targetPitch, droneRoot.transform.eulerAngles.y, targetRoll);
            droneRb.MoveRotation(Quaternion.Slerp(droneRoot.transform.rotation, targetRot, dt * 5f));

            // ── Altitude Hold auto-throttle ─────────────────
            if (currentMode == FlightMode.AltitudeHold || currentMode == FlightMode.ReturnHome)
            {
                float altError = targetAltitude - currentAltitude;
                throttle = Mathf.Clamp01(0.5f + altError * 0.15f + droneRb.linearVelocity.y * -0.05f);
                if (targetAltitude < 0.1f && currentAltitude < 0.3f) throttle = 0f;
            }

            // ── Thrust force ─────────────────────────────────
            float thrustForce = throttle * MAX_THRUST;
            Vector3 thrustDir = droneRoot.transform.up;
            droneRb.AddForce(thrustDir * thrustForce, ForceMode.Force);

            // ── Wind disturbance ─────────────────────────────
            if (isFlying)
                droneRb.AddForce(windDirection * windStrength * 0.3f, ForceMode.Force);

            // ── Speed clamp ──────────────────────────────────
            if (droneRb.linearVelocity.magnitude > MAX_SPEED)
                droneRb.linearVelocity = droneRb.linearVelocity.normalized * MAX_SPEED;

            // ── Ground snap ──────────────────────────────────
            if (currentAltitude < 0.25f && throttle < 0.45f)
            {
                Vector3 pos = droneRoot.transform.position;
                pos.y = 0.25f;
                droneRoot.transform.position = pos;
                Vector3 v = droneRb.linearVelocity;
                v.y = Mathf.Max(v.y, 0);
                droneRb.linearVelocity = v;
            }

            // Track flying state
            isFlying = currentAltitude > 0.5f;
            if (isFlying) flightTime += dt;

            // Battery drain
            if (isFlying) batteryLevel = Mathf.Max(0f, batteryLevel - dt * 0.04f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region CAMERA
        // ─────────────────────────────────────────────────────────────────

        private void UpdateCamera()
        {
            if (mainCam == null || droneRoot == null) return;
            Vector3 dronePos = droneRoot.transform.position;

            switch (cameraView)
            {
                case 0: // Follow
                    Vector3 followTarget = dronePos + droneRoot.transform.rotation * new Vector3(0, 3, -freeCamDist);
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, followTarget, Time.deltaTime * 6f);
                    mainCam.transform.LookAt(dronePos + Vector3.up * 0.5f);
                    break;

                case 1: // FPV
                    mainCam.transform.position = dronePos + droneRoot.transform.rotation * new Vector3(0, 0.05f, 0.12f);
                    mainCam.transform.rotation = droneRoot.transform.rotation * Quaternion.Euler(15f, 0, 0);
                    break;

                case 2: // Top-down
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position,
                        dronePos + Vector3.up * Mathf.Max(freeCamDist, 20f), Time.deltaTime * 5f);
                    mainCam.transform.rotation = Quaternion.Euler(90, 0, 0);
                    break;

                case 3: // Free orbit
                    Quaternion orbit = Quaternion.Euler(freeCamPitch, freeCamYaw, 0);
                    Vector3 freePos = dronePos + orbit * new Vector3(0, 0, -freeCamDist);
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, freePos, Time.deltaTime * 8f);
                    mainCam.transform.LookAt(dronePos + Vector3.up * 0.5f);
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region HUD UPDATE
        // ─────────────────────────────────────────────────────────────────

        private void UpdateHUD()
        {
            if (txtArmed == null) return;

            // Armed status
            txtArmed.text = isArmed ? "STATUS: ARMED ●" : "STATUS: DISARMED ○";
            txtArmed.color = isArmed ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);

            // Mode
            string modeStr = currentMode.ToString().ToUpper().Replace("ALTITUDEHOLD", "ALTITUDE HOLD").Replace("RETURNHOME", "RETURN HOME");
            txtMode.text = $"MODE: {modeStr}";

            // Telemetry
            currentAltitude = droneRoot != null ? droneRoot.transform.position.y : 0f;
            float spd = droneRb != null ? droneRb.linearVelocity.magnitude : 0f;
            txtAlt.text = $"ALTITUDE:  {currentAltitude:F1} m";
            txtSpeed.text = $"SPEED:     {spd:F1} m/s";
            txtThrottle.text = $"THROTTLE:  {(throttle * 100f):F0}%";

            int mins = (int)(flightTime / 60f);
            int secs = (int)(flightTime % 60f);
            txtFlightTime.text = $"FLIGHT TIME: {mins}:{secs:D2}";
            txtWind.text = $"WIND:      {windStrength:F1} m/s";
            txtGPS.text = isFlying ? "GPS: 3D FIX  SAT: 14" : "GPS: 3D FIX  SAT: 14";

            // Battery
            txtBattery.text = $"{batteryLevel:F0}%";
            txtBattery.color = batteryLevel > 40f ? new Color(0.3f, 1f, 0.3f) :
                               batteryLevel > 20f ? new Color(1f, 0.8f, 0.1f) : new Color(1f, 0.2f, 0.2f);
            if (batteryBar != null) batteryBar.fillAmount = batteryLevel / 100f;
            if (throttleBar != null) throttleBar.fillAmount = throttle;

            // Message timer
            if (messageClearTimer > 0f)
            {
                messageClearTimer -= Time.deltaTime;
                if (messageClearTimer <= 0f && txtMessage != null)
                    txtMessage.text = "";
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region PROPELLER ANIMATION
        // ─────────────────────────────────────────────────────────────────

        private void AnimatePropellers()
        {
            float rpm = isArmed ? (0.3f + throttle * 0.7f) * 2400f : 0f;
            float deg = rpm * Time.deltaTime / 60f * 360f;

            for (int i = 0; i < propellers.Length; i++)
            {
                if (propellers[i] == null) continue;
                float dir = (i % 2 == 0) ? 1f : -1f;
                propellers[i].Rotate(Vector3.up, deg * dir, Space.Self);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region WEATHER
        // ─────────────────────────────────────────────────────────────────

        private void UpdateWeather()
        {
            weatherTimer += Time.deltaTime;
            if (weatherTimer > 30f)
            {
                weatherTimer = 0f;
                windStrength = Random.Range(0f, 5f);
                windDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region UI HELPERS
        // ─────────────────────────────────────────────────────────────────

        private GameObject MakePanel(Transform parent, Vector2 anchoredPos, Vector2 size,
            Color bgColor, TextAnchor? align = null)
        {
            GameObject go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private Text MakeLabel(Transform parent, string text, Vector2 anchoredPos, Vector2 size,
            int fontSize, Color color, FontStyle style = FontStyle.Normal)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            Text t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }

        private Image MakeFillBar(Transform parent, Vector2 anchoredPos, Vector2 size, Color fillColor)
        {
            // Background
            GameObject bg = new GameObject("BarBG");
            bg.transform.SetParent(parent, false);
            RectTransform bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchoredPosition = anchoredPos;
            bgRt.sizeDelta = size;
            bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f, 1f);
            bgRt.pivot = new Vector2(0.5f, 1f);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0);
            fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;

            return fillImg;
        }

        private void AddDivider(Transform parent, Vector2 pos)
        {
            GameObject d = new GameObject("Divider");
            d.transform.SetParent(parent, false);
            RectTransform rt = d.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(250, 1);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            Image img = d.AddComponent<Image>();
            img.color = new Color(0.3f, 0.8f, 1f, 0.3f);
        }

        private void SetColor(GameObject go, Color color)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard"));
                mat.color = color;
                r.material = mat;
            }
        }

        public void ShowMessage(string msg)
        {
            if (txtMessage != null)
            {
                txtMessage.text = msg;
                messageClearTimer = 4f;
            }
            Debug.Log($"[ASTRA] {msg}");
        }

        #endregion
    }
}




