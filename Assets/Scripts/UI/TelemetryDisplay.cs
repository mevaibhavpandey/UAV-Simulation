using ASTRA.UAV.Core;
using ASTRA.UAV.Telemetry;
using TMPro;
using UnityEngine;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// UI readout panel updating numerical telemetry readouts (GPS, Battery Voltage/Current, Motor RPMs, RSSI, and Temperature).
    /// </summary>
    public class TelemetryDisplay : MonoBehaviour
    {
        [Header("Geographic Readouts")]
        [SerializeField] private TextMeshProUGUI latitudeText;
        [SerializeField] private TextMeshProUGUI longitudeText;
        [SerializeField] private TextMeshProUGUI altitudeText;
        [SerializeField] private TextMeshProUGUI satellitesText;

        [Header("Power System Readouts")]
        [SerializeField] private TextMeshProUGUI batteryVoltageText;
        [SerializeField] private TextMeshProUGUI batteryCurrentText;
        [SerializeField] private TextMeshProUGUI powerDrawText;

        [Header("Propulsion & Health")]
        [SerializeField] private TextMeshProUGUI motorRpmText;
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private TextMeshProUGUI rssiText;

        private void OnEnable()
        {
            EventBus.Subscribe<TelemetryData>(OnTelemetryReceived);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TelemetryData>(OnTelemetryReceived);
        }

        /// <summary>
        /// Populates numerical UI fields when new telemetry snapshot arrives.
        /// </summary>
        /// <param name="data">Telemetry snapshot.</param>
        public void OnTelemetryReceived(TelemetryData data)
        {
            if (latitudeText != null) latitudeText.text = $"Lat: {data.Latitude:F6}°";
            if (longitudeText != null) longitudeText.text = $"Lon: {data.Longitude:F6}°";
            if (altitudeText != null) altitudeText.text = $"Alt: {data.Altitude:F2} m";
            if (satellitesText != null) satellitesText.text = $"Sats: {data.SatellitesLocked}";

            if (batteryVoltageText != null) batteryVoltageText.text = $"{data.BatteryVoltage:F2} V";
            if (batteryCurrentText != null) batteryCurrentText.text = $"{data.BatteryCurrentAmps:F1} A";
            if (powerDrawText != null) powerDrawText.text = $"{data.PowerDrawWatts:F0} W";

            if (motorRpmText != null && data.MotorRPMs != null && data.MotorRPMs.Length >= 4)
            {
                motorRpmText.text = $"M1: {data.MotorRPMs[0]:F0} | M2: {data.MotorRPMs[1]:F0}\nM3: {data.MotorRPMs[2]:F0} | M4: {data.MotorRPMs[3]:F0}";
            }

            if (temperatureText != null) temperatureText.text = $"{data.SystemTemperatureCelsius:F1} °C";
            if (rssiText != null) rssiText.text = $"{data.SignalStrengthDbm:F0} dBm";
        }
    }
}


