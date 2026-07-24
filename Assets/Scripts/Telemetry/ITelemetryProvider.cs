using System;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Contract for telemetry data stream sources (real UAV hardware connection or synthetic simulation).
    /// </summary>
    public interface ITelemetryProvider
    {
        /// <summary>Gets the latest telemetry snapshot.</summary>
        TelemetryData CurrentTelemetry { get; }

        /// <summary>Event raised whenever fresh telemetry data is received or sampled.</summary>
        event Action<TelemetryData> OnTelemetryUpdated;

        /// <summary>Gets whether the telemetry provider is actively sampling and streaming.</summary>
        bool IsActive { get; }

        /// <summary>Starts sampling telemetry.</summary>
        void StartProvider();

        /// <summary>Stops sampling telemetry.</summary>
        void StopProvider();
    }
}
