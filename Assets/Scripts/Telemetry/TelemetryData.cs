using System;
using UnityEngine;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Data structure capturing a complete snapshot of real-time UAV telemetry parameters.
    /// </summary>
    [Serializable]
    public struct TelemetryData
    {
        [Header("Global Positioning")]
        public double Latitude;
        public double Longitude;
        public float Altitude;
        public float AltitudeMSL;
        public float AltitudeAGL;
        public int SatellitesLocked;

        [Header("Kinematics & Dynamics")]
        public Vector3 LocalPosition;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public Quaternion Attitude;
        public Vector3 EulerAngles; // Roll, Pitch, Yaw in degrees
        public float GroundSpeed;
        public float AirSpeed;
        public float AirspeedMs;
        public float HeadingDegrees;

        [Header("Power & Energy")]
        public float BatteryPercentage;
        public float BatteryVoltage;
        public float BatteryCurrentAmps;
        public float MotorCurrentAmps;
        public float PowerDrawWatts;

        [Header("Propulsion Metrics")]
        public float[] MotorRPMs; // e.g. 4 motor values

        [Header("Environmental & Health")]
        public float SystemTemperatureCelsius;
        public float SignalStrengthDbm; // RSSI in dBm
        public float SignalQualityPercent;
        public float SignalRssi;

        [Header("Mission Progress")]
        public float MissionProgressPercent;
        public int CurrentWaypointIndex;
        public string FlightStateName;

        [Header("System Info")]
        public float SystemUptimeSeconds;
        public double TimestampUtcSeconds;

        /// <summary>
        /// Creates a default baseline telemetry snapshot with populated arrays.
        /// </summary>
        /// <returns>Default TelemetryData instance.</returns>
        public static TelemetryData CreateDefault()
        {
            return new TelemetryData
            {
                Latitude = 37.7749,
                Longitude = -122.4194,
                Altitude = 0f,
                AltitudeMSL = 0f,
                AltitudeAGL = 0f,
                SatellitesLocked = 14,
                LocalPosition = Vector3.zero,
                Velocity = Vector3.zero,
                Acceleration = Vector3.zero,
                Attitude = Quaternion.identity,
                EulerAngles = Vector3.zero,
                GroundSpeed = 0f,
                AirSpeed = 0f,
                AirspeedMs = 0f,
                HeadingDegrees = 0f,
                BatteryPercentage = 100f,
                BatteryVoltage = 25.2f,
                BatteryCurrentAmps = 12.5f,
                MotorCurrentAmps = 12.5f,
                PowerDrawWatts = 300f,
                MotorRPMs = new float[4] { 0f, 0f, 0f, 0f },
                SystemTemperatureCelsius = 32f,
                SignalStrengthDbm = -55f,
                SignalQualityPercent = 98f,
                SignalRssi = 98f,
                MissionProgressPercent = 0f,
                CurrentWaypointIndex = 0,
                FlightStateName = "Disarmed",
                SystemUptimeSeconds = 0f,
                TimestampUtcSeconds = 0
            };
        }
    }
}
