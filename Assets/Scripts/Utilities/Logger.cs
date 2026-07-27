using ASTRA.UAV.Utilities;
using System;
using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Functional categories for logging inside the UAV simulation system.
    /// </summary>
    public enum LogCategory
    {
        Core,
        Drone,
        Mission,
        Telemetry,
        Physics,
        Hardware,
        AI,
        UI,
        Simulation,
        Environment
    }

    /// <summary>
    /// Severity level of log entries.
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Exception = 4
    }

    /// <summary>
    /// Categorized logging wrapper for Unity Debug logging without namespace ambiguity.
    /// </summary>
    public static class UAVLogger
    {
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
        public static bool IncludeTimestamps { get; set; } = true;

        public static void Log(string message)
        {
            Debug.Log(message);
        }

        public static void Log(LogCategory category, string message)
        {
            Debug.Log($"[{category}] {message}");
        }

        public static void Log(LogCategory category, LogLevel level, string message)
        {
            Debug.Log($"[{category}][{level}] {message}");
        }

        public static void LogInfo(LogCategory category, string message)
        {
            Debug.Log($"[{category}][INFO] {message}");
        }

        public static void LogWarning(LogCategory category, string message)
        {
            Debug.LogWarning($"[{category}][WARN] {message}");
        }

        public static void LogError(LogCategory category, string message)
        {
            Debug.LogError($"[{category}][ERROR] {message}");
        }

        public static void LogException(LogCategory category, Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}




