using ASTRA.UAV.Core;
using ASTRA.UAV.Telemetry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Flight Heads-Up Display (HUD) presentation controller managing artificial horizon pitch/roll, compass heading, altitude ladder, and telemetry readout gauges.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Artificial Horizon & Flight Instruments")]
        [SerializeField] private RectTransform artificialHorizonPitchRollGroup;
        [SerializeField] private RectTransform compassHeadingTape;
        [SerializeField] private Image pitchLadderImage;

        [Header("Text Indicators")]
        [SerializeField] private TextMeshProUGUI altitudeText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI headingText;
        [SerializeField] private TextMeshProUGUI batteryText;
        [SerializeField] private TextMeshProUGUI modeText;

        [Header("Gauges & Bars")]
        [SerializeField] private Image batteryBarFill;
        [SerializeField] private Image signalBarFill;

        private void OnEnable()
        {
            EventBus.Subscribe<TelemetryData>(UpdateHUD);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TelemetryData>(UpdateHUD);
        }

        /// <summary>
        /// Updates HUD flight indicators from incoming telemetry snapshot.
        /// </summary>
        /// <param name="data">Latest telemetry snapshot.</param>
        public void UpdateHUD(TelemetryData data)
        {
            // Update Pitch & Roll Artificial Horizon
            if (artificialHorizonPitchRollGroup != null)
            {
                float pitch = data.EulerAngles.x;
                float roll = data.EulerAngles.z;

                // Wrap pitch degrees (-180 to 180)
                if (pitch > 180f) pitch -= 360f;

                artificialHorizonPitchRollGroup.localRotation = Quaternion.Euler(0f, 0f, -roll);
                artificialHorizonPitchRollGroup.anchoredPosition = new Vector2(0f, -pitch * 3.0f);
            }

            // Update Compass Heading Tape
            if (compassHeadingTape != null)
            {
                float yaw = data.EulerAngles.y;
                compassHeadingTape.anchoredPosition = new Vector2(-yaw * 2.5f, compassHeadingTape.anchoredPosition.y);
            }

            // Text Indicators
            if (altitudeText != null) altitudeText.text = $"{data.Altitude:F1} m";
            if (speedText != null) speedText.text = $"{data.GroundSpeed:F1} m/s";
            if (headingText != null) headingText.text = $"{data.EulerAngles.y:F0}°";
            if (batteryText != null) batteryText.text = $"{data.BatteryPercentage:F0}%";
            if (modeText != null) modeText.text = string.IsNullOrEmpty(data.FlightStateName) ? "AUTO" : data.FlightStateName;

            // Gauge Fills
            if (batteryBarFill != null) batteryBarFill.fillAmount = Mathf.Clamp01(data.BatteryPercentage / 100f);
            if (signalBarFill != null) signalBarFill.fillAmount = Mathf.Clamp01(data.SignalQualityPercent / 100f);
        }
    }
}





