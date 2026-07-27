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
        [SerializeField] private float dischargeRatePerMinute = 2.5f;
        [SerializeField] private bool addSensorNoise = true;

        private TelemetryData currentData;
        private TelemetrySnapshot currentSnapshot;
        private bool isActive = false;
        private float timer = 0f;
        private float uptime = 0f;

        public TelemetrySnapshot CurrentTelemetry => currentSnapshot;
        public bool IsActive => isActive;

        // ITelemetryProvider Implementation
        public bool IsStreaming => isActive;
        public float FrequencyHz { get => updateRateHz; set => updateRateHz = value; }
        public event Action<TelemetrySnapshot> OnTelemetryUpdated;
        public event Action<TelemetryData> OnLegacyTelemetryUpdated;

        private void Awake()
        {
            currentData = TelemetryData.CreateDefault();
            currentData.BatteryPercentage = initialBatteryPercent;
        }

        private void Start()
        {
            StartStreaming();
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

        public void StartStreaming()
        {
            isActive = true;
            Debug.Log("[MockTelemetryProvider] Synthetic telemetry provider STARTED.");
        }

        public void StopStreaming()
        {
            isActive = false;
            Debug.Log("[MockTelemetryProvider] Synthetic telemetry provider STOPPED.");
        }

        public void StartProvider() => StartStreaming();
        public void StopProvider() => StopStreaming();

        public TelemetrySnapshot GetLatestSnapshot()
        {
            return currentSnapshot;
        }

        private void GenerateSyntheticTelemetry()
        {
            float noiseX = addSensorNoise ? (UnityEngine.Random.value - 0.5f) * 0.05f : 0f;
            float noiseY = addSensorNoise ? (UnityEngine.Random.value - 0.5f) * 0.05f : 0f;

            currentData.AltitudeMSL = 35.0f + Mathf.Sin(uptime * 0.5f) * 0.5f + noiseY;
            currentData.AirspeedMs = 8.0f + Mathf.Cos(uptime * 0.3f) * 0.4f + noiseX;
            currentData.HeadingDegrees = Mathf.Repeat(uptime * 5f, 360f);
            currentData.BatteryPercentage = Mathf.Max(0f, initialBatteryPercent - (uptime / 60.0f * dischargeRatePerMinute));
            currentData.BatteryVoltage = Mathf.Lerp(19.8f, 25.2f, currentData.BatteryPercentage / 100f);
            currentData.SignalRssi = -55f + noiseX * 10f;

            // Populate Snapshot
            currentSnapshot = new TelemetrySnapshot
            {
                Timestamp = uptime,
                Latitude = 13.0827 + (currentData.AltitudeMSL * 0.00001),
                Longitude = 80.2707 + (currentData.AirspeedMs * 0.00001),
                AltitudeMSL = currentData.AltitudeMSL,
                AltitudeAGL = (float)currentData.AltitudeMSL,
                LocalPosition = transform.position,
                Velocity = transform.forward * currentData.AirspeedMs,
                Acceleration = Vector3.zero,
                Attitude = transform.rotation,
                AngularVelocity = Vector3.zero,
                HeadingDegrees = currentData.HeadingDegrees,
                GpsFix = GpsFixStatus.Fix3D,
                SatellitesVisible = 14,
                Battery = new BatteryStatus
                {
                    Voltage = currentData.BatteryVoltage,
                    CurrentAmps = currentData.MotorCurrentAmps,
                    RemainingPercentage = currentData.BatteryPercentage,
                    TemperatureCelsius = 32.0f
                },
                RssiPercentage = 98.0f
            };

            OnTelemetryUpdated?.Invoke(currentSnapshot);
            OnLegacyTelemetryUpdated?.Invoke(currentData);
        }
    }
}


