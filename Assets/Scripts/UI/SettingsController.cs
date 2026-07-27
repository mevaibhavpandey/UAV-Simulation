using ASTRA.UAV.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// UI presentation controller managing simulation settings, environment turbulence sliders, time scale multipliers, and quality toggles.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Simulation Configuration")]
        [SerializeField] private SimulationConfig simulationConfig;

        [Header("UI Controls")]
        [SerializeField] private Slider timeScaleSlider;
        [SerializeField] private TextMeshProUGUI timeScaleText;
        [SerializeField] private Slider windTurbulenceSlider;
        [SerializeField] private TextMeshProUGUI windTurbulenceText;
        [SerializeField] private Toggle highPhysicsPrecisionToggle;

        private void Start()
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            if (timeScaleSlider != null)
            {
                timeScaleSlider.minValue = 0.1f;
                timeScaleSlider.value = Time.timeScale;
                timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
                OnTimeScaleChanged(timeScaleSlider.value);
            }

            if (windTurbulenceSlider != null && simulationConfig != null)
            {
                windTurbulenceSlider.value = simulationConfig.WindTurbulenceIntensity;
                windTurbulenceSlider.onValueChanged.AddListener(OnWindTurbulenceChanged);
                OnWindTurbulenceChanged(windTurbulenceSlider.value);
            }
        }

        /// <summary>
        /// Handles changes to simulation time scale slider.
        /// </summary>
        /// <param name="val">Time scale multiplier (0.1x to 10.0x).</param>
        public void OnTimeScaleChanged(float val)
        {
            Time.timeScale = Mathf.Clamp(val, 0.1f, 10f);
            if (timeScaleText != null)
            {
                timeScaleText.text = $"Time Scale: {Time.timeScale:F1}x";
            }
        }

        /// <summary>
        /// Handles changes to wind turbulence slider.
        /// </summary>
        /// <param name="val">Turbulence level (0.0 to 2.0).</param>
        public void OnWindTurbulenceChanged(float val)
        {
            if (windTurbulenceText != null)
            {
                windTurbulenceText.text = $"Wind Turbulence: {val:F2}";
            }
        }

        /// <summary>
        /// Restores simulation parameters to defaults.
        /// </summary>
        public void RestoreDefaults()
        {
            OnTimeScaleChanged(1.0f);
            if (timeScaleSlider != null) timeScaleSlider.value = 1.0f;
            if (windTurbulenceSlider != null) windTurbulenceSlider.value = 0.25f;
            Debug.Log("[SettingsController] Simulation settings reset to defaults.");
        }
    }
}





