using System;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// GPS Fix lock quality status.
    /// </summary>
    public enum GpsFixStatus
    {
        /// <summary>No GPS signal or fix.</summary>
        NoFix,
        /// <summary>2D Position fix only (Lat/Lon).</summary>
        Fix2D,
        /// <summary>3D Position fix (Lat/Lon/Alt).</summary>
        Fix3D,
        /// <summary>Differential GPS fix.</summary>
        DGPS,
        /// <summary>RTK Float precision fix.</summary>
        RTKFloat,
        /// <summary>RTK Fixed centimeter precision fix.</summary>
        RTKFixed
    }

    /// <summary>
    /// Detailed battery telemetry data.
    /// </summary>
    [Serializable]
    public struct BatteryStatus
    {
        /// <summary>Voltage in Volts.</summary>
        public float Voltage;

        /// <summary>Current draw in Amperes.</summary>
        public float CurrentAmps;

        /// <summary>Remaining charge capacity percentage [0..100].</summary>
        public float RemainingPercentage;

        /// <summary>Battery temperature in degrees Celsius.</summary>
        public float TemperatureCelsius;
    }

    /// <summary>
    /// Snapshot container for comprehensive flight telemetry.
    /// </summary>
    [Serializable]
    public struct TelemetrySnapshot
    {
        /// <summary>Timestamp of sample in seconds since simulation epoch.</summary>
        public double Timestamp;

        /// <summary>Latitude in degrees (WGS84).</summary>
        public double Latitude;

        /// <summary>Longitude in degrees (WGS84).</summary>
        public double Longitude;

        /// <summary>Altitude above mean sea level in meters.</summary>
        public double AltitudeMSL;

        /// <summary>Altitude above ground level in meters.</summary>
        public float AltitudeAGL;

        /// <summary>Local ENU position coordinates.</summary>
        public Vector3 LocalPosition;

        /// <summary>Linear velocity vector (m/s).</summary>
        public Vector3 Velocity;

        /// <summary>Linear acceleration vector from accelerometer (m/s²).</summary>
        public Vector3 Acceleration;

        /// <summary>Aircraft attitude orientation quaternion.</summary>
        public Quaternion Attitude;

        /// <summary>Angular velocity vector from gyroscope (rad/s).</summary>
        public Vector3 AngularVelocity;

        /// <summary>Heading angle in degrees relative to True North.</summary>
        public float HeadingDegrees;

        /// <summary>Current GPS fix status.</summary>
        public GpsFixStatus GpsFix;

        /// <summary>Number of visible GPS satellites.</summary>
        public int SatellitesVisible;

        /// <summary>Battery telemetry snapshot.</summary>
        public BatteryStatus Battery;

        /// <summary>Radio link signal strength indicator (0..100%).</summary>
        public float RssiPercentage;
    }

    /// <summary>
    /// Contract for providing real-time UAV telemetry streaming and sampling.
    /// </summary>
    public interface ITelemetryProvider
    {
        /// <summary>
        /// Gets a value indicating whether telemetry streaming is currently active.
        /// </summary>
        bool IsStreaming { get; }

        /// <summary>
        /// Gets the telemetry update rate in Hz (samples per second).
        /// </summary>
        float FrequencyHz { get; set; }

        /// <summary>
        /// Fired whenever a new telemetry snapshot is sampled.
        /// </summary>
        event Action<TelemetrySnapshot> OnTelemetryUpdated;

        /// <summary>
        /// Gets the latest telemetry snapshot.
        /// </summary>
        TelemetrySnapshot CurrentTelemetry { get; }

        /// <summary>
        /// Starts streaming telemetry updates.
        /// </summary>
        void StartStreaming();

        /// <summary>
        /// Stops streaming telemetry updates.
        /// </summary>
        void StopStreaming();

        /// <summary>
        /// Fetches the latest instantaneous telemetry snapshot.
        /// </summary>
        /// <returns>Current telemetry snapshot.</returns>
        TelemetrySnapshot GetLatestSnapshot();
    }

    /// <summary>
    /// Alias struct for TelemetryData.
    /// </summary>
    [Serializable]
    public struct TelemetryData
    {
        public float AltitudeMSL;
        public float AltitudeAGL;
        public float AirspeedMs;
        public float GroundSpeedMs;
        public float HeadingDegrees;
        public float SignalRssi;
        public float BatteryPercentage;
        public float BatteryVoltage;
        public float MotorCurrentAmps;
        public BatteryStatus BatteryStatus;
        public Vector3 Position;
        public Quaternion Rotation;

        public static TelemetryData CreateDefault()
        {
            return new TelemetryData
            {
                AltitudeMSL = 0f,
                AltitudeAGL = 0f,
                AirspeedMs = 0f,
                GroundSpeedMs = 0f,
                HeadingDegrees = 0f,
                SignalRssi = 100f,
                BatteryPercentage = 100f,
                BatteryVoltage = 24.0f,
                MotorCurrentAmps = 12.5f
            };
        }

        public static implicit operator TelemetrySnapshot(TelemetryData d)
        {
            return new TelemetrySnapshot
            {
                AltitudeMSL = d.AltitudeMSL,
                AltitudeAGL = d.AltitudeAGL,
                HeadingDegrees = d.HeadingDegrees,
                RssiPercentage = d.SignalRssi,
                LocalPosition = d.Position,
                Attitude = d.Rotation
            };
        }
    }
}





