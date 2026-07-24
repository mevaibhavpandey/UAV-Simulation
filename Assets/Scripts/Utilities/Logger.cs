using System;
using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Functional categories for logging inside the UAV simulation system.
    /// </summary>
    public enum LogCategory
    {
        /// <summary>Core system lifecycle and application state.</summary>
        Core,
        /// <summary>Drone flight dynamics, motor control, state estimation.</summary>
        Drone,
        /// <summary>Mission planning, waypoint navigation, pattern execution.</summary>
        Mission,
        /// <summary>Telemetry provider and network broadcast payloads.</summary>
        Telemetry,
        /// <summary>Aerodynamics, wind forces, collisions, drag.</summary>
        Physics,
        /// <summary>Hardware-in-the-loop and serial/UDP protocol bridges.</summary>
        Hardware,
        /// <summary>Artificial intelligence pathfinding and obstacle evasion.</summary>
        AI,
        /// <summary>User Interface and Flight HUD components.</summary>
        UI
    }

    /// <summary>
    /// Severity level of log entries.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Detailed diagnostic information.</summary>
        Debug = 0,
        /// <summary>Informational messages.</summary>
        Info = 1,
        /// <summary>Non-critical warning events.</summary>
        Warning = 2,
        /// <summary>Error conditions that disrupt operation.</summary>
        Error = 3,
        /// <summary>Unhandled exceptions.</summary>
        Exception = 4
    }

    /// <summary>
    /// Categorized and filtered log wrapper for Unity Debug logging.
    /// Provides consistent timestamping, category labeling, and minimum log level threshold filtering.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Gets or sets the minimum active log level. Messages below this level are ignored.
        /// </summary>
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Gets or sets a value indicating whether timestamps should be prepended to log entries.
        /// </summary>
        public static bool IncludeTimestamps { get; set; } = true;

        /// <summary>
        /// Logs an informational message tagged with category.
        /// </summary>
        /// <param name="category">Category identifier.</param>
        /// <param name="message">Log text message.</param>
        public static void LogInfo(LogCategory category, string message)
        {
            Log(category, LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a warning message tagged with category.
        /// </summary>
        /// <param name="category">Category identifier.</param>
        /// <param name="message">Log text message.</param>
        public static void LogWarning(LogCategory category, string message)
        {
            Log(category, LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs an error message tagged with category.
        /// </summary>
        /// <param name="category">Category identifier.</param>
        /// <param name="message">Log text message.</param>
        public static void LogError(LogCategory category, string message)
        {
            Log(category, LogLevel.Error, message);
        }

        /// <summary>
        /// Logs an exception tagged with category.
        /// </summary>
        /// <param name="category">Category identifier.</param>
        /// <param name="exception">Exception object.</param>
        public static void LogException(LogCategory category, Exception exception)
        {
            if (LogLevel.Exception < MinimumLogLevel) return;
            string prefix = FormatPrefix(category, LogLevel.Exception);
            Debug.LogError($"{prefix} Exception: {exception.Message}\n{exception.StackTrace}");
        }

        /// <summary>
        /// Dispatches a formatted log entry to Unity's Debug system if it satisfies minimum log level filters.
        /// </summary>
        /// <param name="category">Category identifier.</param>
        /// <param name="level">Severity level.</param>
        /// <param name="message">Log message text.</param>
        public static void Log(LogCategory category, LogLevel level, string message)
        {
            if (level < MinimumLogLevel) return;

            string formattedMessage = $"{FormatPrefix(category, level)} {message}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Exception:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        private static string FormatPrefix(LogCategory category, LogLevel level)
        {
            string timeStampStr = IncludeTimestamps ? $"[{DateTime.Now:HH:mm:ss.fff}] " : string.Empty;
            return $"{timeStampStr}[{category}][{level.ToString().ToUpper()}]";
        }
    }
}
