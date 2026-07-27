using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Drone;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.UI.GCS
{
    public enum GCSTab
    {
        Dashboard,
        MissionPlanner,
        ManualFlight,
        AutonomousGPS,
        GPSDeniedNav,
        EngineeringView,
        Settings,
        Logs
    }

    /// <summary>
    /// Master Ground Control Station (GCS) UI Controller.
    /// Manages top status bar, left navigation tabs, mission planner panel, live telemetry panel,
    /// system health monitor, event logs, notification toasts, and action command buttons.
    /// </summary>
    public class GCSController : Singleton<GCSController>
    {
        [Header("State")]
        [SerializeField] private GCSTab activeTab = GCSTab.Dashboard;
        [SerializeField] private bool isConnected = true;

        [Header("Sub-Systems")]
        [SerializeField] private GCSMissionPlannerUI missionPlanner;
        [SerializeField] private GCSTelemetryPanel telemetryPanel;
        [SerializeField] private GCSSystemHealthMonitor healthMonitor;
        [SerializeField] private GCSLogPanel logPanel;

        [Header("Target UAV Reference")]
        [SerializeField] private GameObject droneObject;

        private FlightModeManager flightModeManager;
        private ManualFlightController flightController;

        public GCSTab ActiveTab => activeTab;

        protected override void Awake()
        {
            base.Awake();
            if (missionPlanner == null) missionPlanner = GetOrAddComponent<GCSMissionPlannerUI>();
            if (telemetryPanel == null) telemetryPanel = GetOrAddComponent<GCSTelemetryPanel>();
            if (healthMonitor == null) healthMonitor = GetOrAddComponent<GCSSystemHealthMonitor>();
            if (logPanel == null) logPanel = GetOrAddComponent<GCSLogPanel>();
        }

        private void Start()
        {
            if (droneObject == null) droneObject = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (droneObject != null)
            {
                flightModeManager = droneObject.GetComponent<FlightModeManager>();
                flightController = droneObject.GetComponent<ManualFlightController>();
            }

            if (GCSNotificationSystem.Instance != null)
            {
                GCSNotificationSystem.Instance.PostNotification("GCS Connected", "Ground Control Station established telemetry link with ASTRA UAV.", NotificationType.Success);
            }
        }

        private void OnGUI()
        {
            DrawTopStatusBar();
            DrawLeftNavigationSidebar();

            switch (activeTab)
            {
                case GCSTab.Dashboard:
                    DrawDashboardView();
                    break;
                case GCSTab.MissionPlanner:
                    DrawMissionPlannerView();
                    break;
                case GCSTab.EngineeringView:
                    DrawEngineeringView();
                    break;
                case GCSTab.Logs:
                    DrawLogsView();
                    break;
                default:
                    DrawDashboardView();
                    break;
            }

            DrawBottomLogTerminal();
            DrawNotificationToasts();
        }

        private void DrawTopStatusBar()
        {
            GUI.Box(new Rect(0, 0, Screen.width, 35), "");

            string connStr = isConnected ? "<color=green>CONNECTED (915MHz)</color>" : "<color=red>DISCONNECTED</color>";
            string modeStr = flightModeManager != null ? flightModeManager.CurrentMode.ToString() : "DISARMED";
            string timeStr = System.DateTime.Now.ToString("HH:mm:ss");

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 12 };

            GUILayout.BeginArea(new Rect(15, 8, Screen.width - 30, 25));
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>ASTRA UAV</b> | Defense-Grade Mission Control Station", style, GUILayout.Width(420));
            GUILayout.Label($"Link: {connStr}", style, GUILayout.Width(200));
            GUILayout.Label($"Mode: <b>{modeStr}</b>", style, GUILayout.Width(180));
            GUILayout.Label($"Sim Time: <b>{timeStr}</b>", style, GUILayout.Width(150));
            GUILayout.Label("Operator: <b>Command Alpha</b>", style);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawLeftNavigationSidebar()
        {
            int navWidth = 160;
            int navTop = 40;
            int navHeight = Screen.height - navTop - 110;

            GUI.Box(new Rect(0, navTop, navWidth, navHeight), "Navigation");

            GUILayout.BeginArea(new Rect(10, navTop + 30, navWidth - 20, navHeight - 40));

            if (GUILayout.Button("Dashboard", GUILayout.Height(30))) activeTab = GCSTab.Dashboard;
            if (GUILayout.Button("Mission Planner", GUILayout.Height(30))) activeTab = GCSTab.MissionPlanner;
            if (GUILayout.Button("Manual Flight", GUILayout.Height(30))) activeTab = GCSTab.ManualFlight;
            if (GUILayout.Button("Autonomous GPS", GUILayout.Height(30))) activeTab = GCSTab.AutonomousGPS;
            if (GUILayout.Button("GPS-Denied Nav", GUILayout.Height(30))) activeTab = GCSTab.GPSDeniedNav;
            if (GUILayout.Button("Engineering View", GUILayout.Height(30))) activeTab = GCSTab.EngineeringView;
            if (GUILayout.Button("Settings", GUILayout.Height(30))) activeTab = GCSTab.Settings;
            if (GUILayout.Button("System Logs", GUILayout.Height(30))) activeTab = GCSTab.Logs;

            GUILayout.EndArea();
        }

        private void DrawDashboardView()
        {
            int leftMargin = 170;
            int topMargin = 45;
            int contentWidth = Screen.width - leftMargin - 320;

            // Center Panel (Map / Mission View)
            GUI.Box(new Rect(leftMargin, topMargin, contentWidth, Screen.height - topMargin - 120), "3D Interactive Mission Map & Tactical View");

            // Right Panel (Telemetry & Action Buttons)
            int rightMargin = Screen.width - 310;
            GUI.Box(new Rect(rightMargin, topMargin, 300, Screen.height - topMargin - 120), "Live Telemetry & Controls");

            if (telemetryPanel != null)
            {
                telemetryPanel.RenderTelemetryGUI(new Rect(rightMargin + 10, topMargin + 30, 280, 240));
            }

            // GCS Mission Control Action Buttons
            GUILayout.BeginArea(new Rect(rightMargin + 10, topMargin + 280, 280, 240));
            GUILayout.Label("<b>=== GCS COMMAND CONTROLS ===</b>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ARM MOTORS", GUILayout.Height(32))) ExecuteArm();
            if (GUILayout.Button("DISARM", GUILayout.Height(32))) ExecuteDisarm();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TAKEOFF", GUILayout.Height(32))) ExecuteTakeoff();
            if (GUILayout.Button("AUTO LAND", GUILayout.Height(32))) ExecuteLand();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("RETURN TO HOME (RTH)", GUILayout.Height(32))) ExecuteRTH();
            if (GUILayout.Button("<color=red><b>EMERGENCY STOP</b></color>", GUILayout.Height(36))) ExecuteEmergencyStop();

            GUILayout.EndArea();
        }

        private void DrawMissionPlannerView()
        {
            int leftMargin = 170;
            int topMargin = 45;
            GUI.Box(new Rect(leftMargin, topMargin, Screen.width - leftMargin - 15, Screen.height - topMargin - 120), "GCS Waypoint Mission Editor");

            GUILayout.BeginArea(new Rect(leftMargin + 15, topMargin + 30, 600, 450));
            GUILayout.Label($"<b>Active Mission:</b> {missionPlanner.MissionName}");
            GUILayout.Space(10);

            for (int i = 0; i < missionPlanner.WaypointQueue.Count; i++)
            {
                var wp = missionPlanner.WaypointQueue[i];
                GUILayout.Label($"<b>WP{i + 1:D2}:</b> Lat: {wp.latitude:F4} | Lon: {wp.longitude:F4} | Alt: {wp.altitudeMSL}m | Speed: {wp.targetSpeed}m/s | Hold: {wp.holdDurationSeconds}s");
            }

            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Waypoint", GUILayout.Height(30)))
            {
                missionPlanner.AddWaypoint(13.0860, 80.2750, 30.0f, 7.0f, 4.0f);
            }
            if (GUILayout.Button("Clear Mission", GUILayout.Height(30)))
            {
                missionPlanner.ClearMission();
            }
            if (GUILayout.Button("Upload to UAV FC", GUILayout.Height(30)))
            {
                if (logPanel != null) logPanel.AddLog("Mission plan uploaded to Pixhawk 6X Flight Controller.", "Mission");
                if (GCSNotificationSystem.Instance != null) GCSNotificationSystem.Instance.PostNotification("Mission Uploaded", "Waypoint sequence successfully synchronized with UAV.", NotificationType.Success);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawEngineeringView()
        {
            int leftMargin = 170;
            int topMargin = 45;
            GUI.Box(new Rect(leftMargin, topMargin, Screen.width - leftMargin - 15, Screen.height - topMargin - 120), "Engineering Diagnostics & Hardware Health");
        }

        private void DrawLogsView()
        {
            int leftMargin = 170;
            int topMargin = 45;
            GUI.Box(new Rect(leftMargin, topMargin, Screen.width - leftMargin - 15, Screen.height - topMargin - 120), "Full System Terminal Logs");
        }

        private void DrawBottomLogTerminal()
        {
            int logHeight = 100;
            int y = Screen.height - logHeight;
            GUI.Box(new Rect(0, y, Screen.width, logHeight), "System Log Terminal");

            if (logPanel != null && logPanel.Logs.Count > 0)
            {
                GUILayout.BeginArea(new Rect(15, y + 25, Screen.width - 30, logHeight - 30));
                int start = Mathf.Max(0, logPanel.Logs.Count - 4);
                for (int i = start; i < logPanel.Logs.Count; i++)
                {
                    var log = logPanel.Logs[i];
                    GUILayout.Label($"[{log.Timestamp}] [{log.Category}] {log.LogMessage}");
                }
                GUILayout.EndArea();
            }
        }

        private void DrawNotificationToasts()
        {
            if (GCSNotificationSystem.Instance == null) return;
            var list = GCSNotificationSystem.Instance.ActiveNotifications;

            int toastWidth = 320;
            int toastHeight = 55;
            int right = Screen.width - toastWidth - 20;

            for (int i = 0; i < list.Count; i++)
            {
                var notif = list[i];
                int top = 50 + (i * (toastHeight + 8));
                GUI.Box(new Rect(right, top, toastWidth, toastHeight), $"[ALERT] {notif.Title}");
                GUI.Label(new Rect(right + 10, top + 22, toastWidth - 20, 30), notif.Message);
            }
        }

        // Action Command Handlers
        private void ExecuteArm()
        {
            if (flightModeManager != null) flightModeManager.Arm();
            if (logPanel != null) logPanel.AddLog("GCS Command: ARM MOTORS executed.", "Command");
        }

        private void ExecuteDisarm()
        {
            if (flightModeManager != null) flightModeManager.Disarm();
            if (logPanel != null) logPanel.AddLog("GCS Command: DISARM MOTORS executed.", "Command");
        }

        private void ExecuteTakeoff()
        {
            if (flightModeManager != null) flightModeManager.Arm();
            if (logPanel != null) logPanel.SetFlightMode(FlightModeType.Manual);
            if (logPanel != null) logPanel.AddLog("GCS Command: AUTO TAKEOFF initiated.", "Command");
        }

        private void ExecuteLand()
        {
            if (logPanel != null) logPanel.AddLog("GCS Command: AUTO LANDING initiated.", "Command");
        }

        private void ExecuteRTH()
        {
            if (flightModeManager != null) flightModeManager.SetFlightMode(FlightModeType.RTL);
            if (logPanel != null) logPanel.AddLog("GCS Command: RETURN TO HOME (RTH) engaged.", "Command");
        }

        private void ExecuteEmergencyStop()
        {
            if (flightController != null) flightController.TriggerEmergencyStop();
            if (logPanel != null) logPanel.AddLog("GCS Command: CRITICAL EMERGENCY STOP EXECUTED!", "Emergency");
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T comp = GetComponent<T>();
            if (comp == null) comp = gameObject.AddComponent<T>();
            return comp;
        }
    }
}


