using UnityEngine;

namespace ASTRA.UAV.Simulation
{
    /// <summary>
    /// ScriptableObject defining global environment parameters, atmospheric physics constants, wind vectors, and time scale settings.
    /// </summary>
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "ASTRA/UAV/Simulation Config", order = 10)]
    public class SimulationConfig : ScriptableObject
    {
        [Header("Gravity & Environment")]
        [Tooltip("Gravitational acceleration vector in m/s^2.")]
        [SerializeField] private Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);

        [Tooltip("Standard air density at sea level in kg/m^3.")]
        [Range(0.5f, 2.0f)]
        [SerializeField] private float baseAirDensity = 1.225f;

        [Tooltip("Standard atmospheric pressure at sea level in hPa.")]
        [Range(800f, 1100f)]
        [SerializeField] private float seaLevelPressureHpa = 1013.25f;

        [Tooltip("Ambient environment temperature in degrees Celsius.")]
        [Range(-40f, 60f)]
        [SerializeField] private float temperatureCelsius = 20.0f;

        [Header("Wind & Turbulence")]
        [Tooltip("Base mean wind velocity vector in m/s.")]
        [SerializeField] private Vector3 baseWindVector = new Vector3(2.0f, 0f, 1.0f);

        [Tooltip("Gust turbulence intensity multiplier (0 = static wind, 1 = heavy gusts).")]
        [Range(0f, 2f)]
        [SerializeField] private float windTurbulenceIntensity = 0.25f;

        [Tooltip("Perlin frequency scaling factor for temporal wind variation.")]
        [Range(0.01f, 2.0f)]
        [SerializeField] private float windFrequencyScale = 0.5f;

        [Header("Simulation Timing")]
        [Tooltip("Simulation timescale multiplier (1.0 = real-time).")]
        [Range(0.1f, 10.0f)]
        [SerializeField] private float timeScale = 1.0f;

        /// <summary>Gets or sets the gravitational acceleration vector.</summary>
        public Vector3 GravityVector
        {
            get => gravityVector;
            set => gravityVector = value;
        }

        /// <summary>Gets base sea-level air density in kg/m^3.</summary>
        public float BaseAirDensity => baseAirDensity;

        /// <summary>Gets atmospheric pressure at sea level in hPa.</summary>
        public float SeaLevelPressureHpa => seaLevelPressureHpa;

        /// <summary>Gets ambient temperature in Celsius.</summary>
        public float TemperatureCelsius => temperatureCelsius;

        /// <summary>Gets base wind vector in m/s.</summary>
        public Vector3 BaseWindVector => baseWindVector;

        /// <summary>Gets turbulence intensity.</summary>
        public float WindTurbulenceIntensity => windTurbulenceIntensity;

        /// <summary>Gets frequency scaling factor for gusts.</summary>
        public float WindFrequencyScale => windFrequencyScale;

        /// <summary>Gets physics simulation time scale.</summary>
        public float TimeScale => timeScale;
    }
}



