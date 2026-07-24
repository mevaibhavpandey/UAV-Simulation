using System;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Synthetic telemetry provider that simulates realistic UAV sensors, GPS noise, battery consumption, motor RPM, and flight dynamics.
    /// </summary>
    public class MockTelemetryProvider : MonoBehaviour, ITelemetryProvider
    {
        [Header("Update Frequency")]
        [SerializeField] private float updateRateHz = 20.0f;

        [Header("Simulated Parameters")]
        [SerializeField] private float initialBatteryPercent = 100.0f;
        [SerializeField] private float dischargeRatePerMinute = 2.5f; // 2.5% per minute
        [SerializeField] private bool addSensorNoise = true;

        private TelemetryData currentData;
        private bool isActive = false;
        private float timer = 0f;
        private float uptime = 0f;

        /// <summary>Gets the current simulated telemetry snapshot.</summary>
        public TelemetryData CurrentTelemetry => currentData;

        /// <summary>Gets whether the simulation stream is active.</summary>
        public bool IsActive => isActive;

        /// <summary>Fired whenever a new synthetic telemetry frame is generated.</summary>
        public event Action<TelemetryData> OnTelemetryUpdated;

        private void Awake()
        {
            currentData = TelemetryData.CreateDefault();
            currentData.BatteryPercentage = initialBatteryPercent;
        }

        private void Start()
        {
            StartProvider();
        }

        private void Update()
        {
            if (!isActive) return;

            uptime += Time.deltaTime;
            timer += Time.deltaTime;

            float interval = 1.0f / Mathf.Max(1.0f, updateRateHz);
            if (timer >= interval)
            {
                timer -= interval;
                GenerateSyntheticTelemetry();
            }
        }

        /// <summary>
        /// Starts streaming synthetic telemetry snapshots.
        /// </summary>
        public void StartProvider()
        {
            isActive = true;
            Debug.Log("[MockTelemetryProvider] Synthetic telemetry provider STARTED.");
        }

        /// <summary>
        /// Halts synthetic telemetry streaming.
        /// </summary>
        public void StopProvider()
        {
            isActive = false;
            Debug.Log("[MockTelemetryProvider] Synthetic telemetry provider STOPPED.");
        }

        private void GenerateSyntheticTelemetry()
        {
            Vector3 pos = transform.position;
            Vector3 rot = transform.eulerAngles;

            // Calculate simulated dynamics
            float speed = addSensorNoise ? (5.0f + UnityEngine.Random.Range(-0.3f, 0.3f)) : 5.0f;
            float battery = Mathf.Max(0f, currentData.BatteryPercentage - (dischargeRatePerMinute / 60f * (1f / updateRateHz)));

            // Motor RPM calculation (scale baseline 6000 RPM with noise)
            float baseRpm = 6200f + (pos.y * 15f);
            float noise = addSensorNoise ? UnityEngine.Random.Range(-50f, 50f) : 0f;
            float[] rpms = new float[4]
            {
                baseRpm + noise,
                baseRpm - noise,
                baseRpm + (noise * 0.5f),
                baseRpm - (noise * 0.5f)
            };

            // Synthetic GPS shift
            double latNoise = addSensorNoise ? (UnityEngine.Random.Range(-0.00001f, 0.00001f)) : 0.0;
            double lonNoise = addSensorNoise ? (UnityEngine.Random.Range(-0.00001f, 0.00001f)) : 0.0;

            currentData.Latitude = 37.7749 + (pos.z * 0.000009) + latNoise;
            currentData.Longitude = -122.4194 + (pos.x * 0.000009) + lonNoise;
            currentData.Altitude = pos.y;
            currentData.SatellitesLocked = addSensorNoise ? UnityEngine.Random.Range(10, 16) : 14;

            currentData.LocalPosition = pos;
            currentData.Velocity = transform.forward * speed;
            currentData.Acceleration = Vector3.up * (addSensorNoise ? UnityEngine.Random.Range(-0.1f, 0.1f) : 0f);
            currentData.Attitude = transform.rotation;
            currentData.EulerAngles = rot;

            currentData.GroundSpeed = speed;
            currentData.AirSpeed = speed * 1.05f;

            currentData.BatteryPercentage = battery;
            currentData.BatteryVoltage = 10f + (battery / 100f * 2.6f);
            currentData.BatteryCurrentAmps = 14.5f + (addSensorNoise ? UnityEngine.Random.Range(-1f, 1f) : 0f);
            currentData.PowerDrawWatts = currentData.BatteryVoltage * currentData.BatteryCurrentAmps;

            currentData.MotorRPMs = rpms;
            currentData.SystemTemperatureCelsius = 38.0f + Mathf.Sin(uptime * 0.05f) * 4.0f;
            currentData.SignalStrengthDbm = -60.0f + (addSensorNoise ? UnityEngine.Random.Range(-3f, 3f) : 0f);
            currentData.SignalQualityPercent = Mathf.Clamp(100f + currentData.SignalStrengthDbm, 0f, 100f);

            currentData.SystemUptimeSeconds = uptime;
            currentData.TimestampUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

            OnTelemetryUpdated?.Invoke(currentData);
        }
    }
}
