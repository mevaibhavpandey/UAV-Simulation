//-----------------------------------------------------------------------
// <copyright file="DroneGPS.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Enumerates GPS satellite fix quality states.
    /// </summary>
    public enum GPSFixType
    {
        /// <summary>No position fix available.</summary>
        NoFix = 0,

        /// <summary>2D position fix (Latitude/Longitude only).</summary>
        Fix2D = 2,

        /// <summary>3D position fix (Latitude/Longitude/Altitude).</summary>
        Fix3D = 3,

        /// <summary>Differential GPS fix with ground station correction.</summary>
        DGPS = 4,

        /// <summary>Real-Time Kinematic float solution (~0.2m precision).</summary>
        RTKFloat = 5,

        /// <summary>Real-Time Kinematic fixed solution (~0.02m precision).</summary>
        RTKFixed = 6
    }

    /// <summary>
    /// Telemetry container for simulated GPS satellite receiver readings.
    /// </summary>
    [Serializable]
    public struct GPSData
    {
        /// <summary>Latitude coordinate in decimal degrees.</summary>
        public double Latitude;

        /// <summary>Longitude coordinate in decimal degrees.</summary>
        public double Longitude;

        /// <summary>Altitude above Mean Sea Level (MSL) in meters.</summary>
        public float AltitudeMSL;

        /// <summary>Horizontal Dilution of Precision (HDOP).</summary>
        public float HDOP;

        /// <summary>Vertical Dilution of Precision (VDOP).</summary>
        public float VDOP;

        /// <summary>Estimated horizontal position error in meters.</summary>
        public float HorizontalAccuracyMeters;

        /// <summary>Estimated vertical position error in meters.</summary>
        public float VerticalAccuracyMeters;

        /// <summary>Current ground speed in meters per second.</summary>
        public float GroundSpeedMS;

        /// <summary>True heading / course over ground in degrees [0, 360].</summary>
        public float CourseOverGroundDeg;

        /// <summary>Number of visible satellites currently tracked.</summary>
        public int SatellitesTracked;

        /// <summary>GPS fix lock status quality.</summary>
        public GPSFixType FixType;

        /// <summary>Timestamp of measurement in seconds.</summary>
        public double Timestamp;
    }

    /// <summary>
    /// Simulated GPS sensor provider that maps Unity 3D world space coordinates
    /// to geographic WGS84 latitude/longitude/altitude coordinates with realistic noise models.
    /// </summary>
    public class DroneGPS : MonoBehaviour
    {
        [Header("Origin Geographic Reference (WGS84)")]
        [SerializeField, Tooltip("Origin latitude corresponding to Unity (0, 0, 0) in decimal degrees.")]
        private double originLatitude = 37.7749;

        [SerializeField, Tooltip("Origin longitude corresponding to Unity (0, 0, 0) in decimal degrees.")]
        private double originLongitude = -122.4194;

        [SerializeField, Tooltip("Origin altitude above sea level corresponding to Unity Y=0 in meters.")]
        private float originAltitudeMSL = 50.0f;

        [Header("Sensor Simulation Settings")]
        [SerializeField, Tooltip("GPS update frequency in Hertz (Hz).")]
        private float updateFrequencyHz = 10f;

        [SerializeField, Tooltip("Standard deviation of horizontal position noise in meters.")]
        private float positionNoiseStdDev = 0.5f;

        [SerializeField, Tooltip("Standard deviation of altitude noise in meters.")]
        private float altitudeNoiseStdDev = 1.0f;

        [SerializeField, Tooltip("Simulated satellite count currently tracked.")]
        private int satelliteCount = 14;

        [SerializeField, Tooltip("Active GPS fix type.")]
        private GPSFixType fixType = GPSFixType.Fix3D;

        [Header("Live Sensor Data")]
        [SerializeField]
        private GPSData currentData;

        private float timeSinceLastUpdate = 0f;
        private SensorHealthStatus healthStatus = SensorHealthStatus.Healthy;

        /// <summary>
        /// Gets the current simulated GPS data output.
        /// </summary>
        public GPSData CurrentData => currentData;

        /// <summary>
        /// Gets the operational health status of the GPS sensor.
        /// </summary>
        public SensorHealthStatus HealthStatus => healthStatus;

        /// <summary>
        /// Fired whenever new GPS satellite telemetry data is computed.
        /// </summary>
        public event Action<GPSData> OnGPSUpdated;

        /// <summary>
        /// Unity Awake lifecycle initialization.
        /// </summary>
        private void Awake()
        {
            currentData = new GPSData
            {
                FixType = fixType,
                SatellitesTracked = satelliteCount,
                HDOP = 0.8f,
                VDOP = 1.2f,
                HorizontalAccuracyMeters = positionNoiseStdDev,
                VerticalAccuracyMeters = altitudeNoiseStdDev
            };
        }

        /// <summary>
        /// Sets the home origin location coordinates matching Unity space (0,0,0).
        /// </summary>
        /// <param name="lat">Latitude in decimal degrees.</param>
        /// <param name="lon">Longitude in decimal degrees.</param>
        /// <param name="alt">Altitude MSL in meters.</param>
        public void SetOriginReference(double lat, double lon, float alt)
        {
            originLatitude = lat;
            originLongitude = lon;
            originAltitudeMSL = alt;
        }

        /// <summary>
        /// Updates simulated GPS readings using Unity world frame ground truth vectors.
        /// </summary>
        /// <param name="truePosition">True Unity transform position in meters.</param>
        /// <param name="trueVelocity">True velocity vector in m/s.</param>
        /// <param name="deltaTime">Time increment in seconds.</param>
        public void ProcessSensorSimulation(Vector3 truePosition, Vector3 trueVelocity, float deltaTime)
        {
            if (healthStatus == SensorHealthStatus.Failed) return;

            timeSinceLastUpdate += deltaTime;
            float updatePeriod = 1.0f / Mathf.Max(1f, updateFrequencyHz);

            if (timeSinceLastUpdate < updatePeriod) return;
            timeSinceLastUpdate = 0f;

            // Generate Gaussian noise offsets
            float noiseX = GenerateGaussianNoise(0f, positionNoiseStdDev);
            float noiseZ = GenerateGaussianNoise(0f, positionNoiseStdDev);
            float noiseY = GenerateGaussianNoise(0f, altitudeNoiseStdDev);

            Vector3 noisyPosition = truePosition + new Vector3(noiseX, noiseY, noiseZ);

            // WGS84 Equirectangular conversion (Approximation valid for local region)
            const double metersPerDegreeLat = 111132.92;
            double metersPerDegreeLon = 111412.84 * Math.Cos(originLatitude * Math.PI / 180.0);

            double currentLat = originLatitude + (noisyPosition.z / metersPerDegreeLat);
            double currentLon = originLongitude + (noisyPosition.x / metersPerDegreeLon);
            float currentAlt = originAltitudeMSL + noisyPosition.y;

            float groundSpeed = new Vector2(trueVelocity.x, trueVelocity.z).magnitude;
            float cog = Mathf.Atan2(trueVelocity.x, trueVelocity.z) * Mathf.Rad2Deg;
            if (cog < 0f) cog += 360f;

            currentData = new GPSData
            {
                Latitude = currentLat,
                Longitude = currentLon,
                AltitudeMSL = currentAlt,
                HDOP = 0.8f + (noiseX * 0.1f),
                VDOP = 1.2f + (noiseY * 0.1f),
                HorizontalAccuracyMeters = positionNoiseStdDev * 1.5f,
                VerticalAccuracyMeters = altitudeNoiseStdDev * 1.5f,
                GroundSpeedMS = groundSpeed,
                CourseOverGroundDeg = cog,
                SatellitesTracked = satelliteCount,
                FixType = fixType,
                Timestamp = (double)Time.time
            };

            OnGPSUpdated?.Invoke(currentData);
        }

        /// <summary>
        /// Utility method producing Gaussian normally distributed random noise samples.
        /// </summary>
        private float GenerateGaussianNoise(float mean, float stdDev)
        {
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}



