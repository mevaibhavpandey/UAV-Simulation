using System;
using UnityEngine;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Data structure capturing a complete snapshot of real-time UAV telemetry telemetry parameters.
    /// </summary>
    [Serializable]
    public struct TelemetryData
    {
        [Header("Global Positioning")]
        public double Latitude;
        public double Longitude;
        public float Altitude;
        public int SatellitesLocked;

        [Header("Kinematics & Dynamics")]
        public Vector3 LocalPosition;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public Quaternion Attitude;
        public Vector3 EulerAngles; // Roll, Pitch, Yaw in degrees
        public float GroundSpeed;
        public float AirSpeed;

        [Header("Power & Energy")]
        public float BatteryPercentage;
        public float BatteryVoltage;
        public float BatteryCurrentAmps;
        public float PowerDrawWatts;

        [Header("Propulsion Metrics")]
        public float[] MotorRPMs; // e.g. 4 motor values

        [Header("Environmental & Health")]
        public float SystemTemperatureCelsius;
        public float SignalStrengthDbm; // RSSI in dBm
        public float SignalQualityPercent;

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
                SatellitesLocked = 12,
                LocalPosition = Vector3.zero,
                Velocity = Vector3.zero,
                Acceleration = Vector3.zero,
                Attitude = Quaternion.identity,
                EulerAngles = Vector3.zero,
                GroundSpeed = 0f,
                AirSpeed = 0f,
                BatteryPercentage = 100f,
                BatteryVoltage = 12.6f,
                BatteryCurrentAmps = 1.2f,
                PowerDrawWatts = 15.12f,
                MotorRPMs = new float[] { 0f, 0f, 0f, 0f },
                SystemTemperatureCelsius = 35.0f,
                SignalStrengthDbm = -55.0f,
                SignalQualityPercent = 98.0f,
                MissionProgressPercent = 0f,
                CurrentWaypointIndex = 0,
                FlightStateName = "Idle",
                SystemUptimeSeconds = 0f,
                TimestampUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
        }
    }
}




