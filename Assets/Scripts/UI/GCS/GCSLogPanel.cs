using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.UI.GCS
{
    public struct LogEntry
    {
        public string Timestamp;
        public string LogMessage;
        public string Category;
    }

    /// <summary>
    /// Terminal log viewer panel logging system events, mission states, warnings, and errors.
    /// </summary>
    public class GCSLogPanel : MonoBehaviour
    {
        private List<LogEntry> logs = new List<LogEntry>();

        public List<LogEntry> Logs => logs;

        private void Awake()
        {
            AddLog("System initialized. GCS Command Center connected.", "System");
            AddLog("Telemetry link established @ 20Hz (TelemetryRadio 915MHz).", "Telemetry");
            AddLog("GPS 3D Fix acquired (14 Satellites, HDOP 0.8).", "GPS");
        }

        public void AddLog(string message, string category = "Info")
        {
            string timeStr = System.DateTime.Now.ToString("HH:mm:ss.ff");
            logs.Add(new LogEntry { Timestamp = timeStr, LogMessage = message, Category = category });
            if (logs.Count > 100) logs.RemoveAt(0);
        }

        public void ClearLogs()
        {
            logs.Clear();
        }
    }
}
