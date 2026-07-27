using UnityEngine;

namespace ASTRA.UAV.Simulation
{
    /// <summary>
    /// Singleton/Manager component responsible for updating and querying dynamic environmental conditions such as wind turbulence, atmospheric pressure, and air density.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        private static EnvironmentManager instance;

        /// <summary>Gets static singleton instance.</summary>
        public static EnvironmentManager Instance => instance;

        [Header("Configuration")]
        [SerializeField] private SimulationConfig config;

        [Header("Runtime Wind Visualization")]
        [SerializeField] private Vector3 currentWindVector;
        [SerializeField] private float currentAirDensity;

        /// <summary>Gets active simulation configuration asset.</summary>
        public SimulationConfig Config => config;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Update()
        {
            if (config == null) return;

            // Apply timescale
            Time.timeScale = config.TimeScale;

            // Dynamic Perlin wind gusts calculation
            float time = Time.time * config.WindFrequencyScale;
            float gustX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * config.WindTurbulenceIntensity;
            float gustY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 0.5f * config.WindTurbulenceIntensity;
            float gustZ = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * config.WindTurbulenceIntensity;

            currentWindVector = config.BaseWindVector + new Vector3(gustX, gustY, gustZ);
            currentAirDensity = config.BaseAirDensity;
        }

        public void SetWindVector(Vector3 wind)
        {
            currentWindVector = wind;
        }

        /// <summary>
        /// Computes current 3D wind velocity vector at a specified position in world space.
        /// </summary>
        /// <param name="position">World position in meters.</param>
        /// <returns>Wind vector in m/s.</returns>
        public Vector3 GetWindAtPosition(Vector3 position)
        {
            if (config == null) return Vector3.zero;

            // Altitude shear attenuation model: wind increases with altitude
            float altFactor = Mathf.Clamp(position.y / 100f, 0.5f, 2.5f);
            return currentWindVector * altFactor;
        }

        /// <summary>
        /// Computes air density at a given altitude using barometric formula approximation.
        /// </summary>
        /// <param name="altitudeMeters">Altitude above sea level in meters.</param>
        /// <returns>Air density in kg/m^3.</returns>
        public float GetAirDensityAtAltitude(float altitudeMeters)
        {
            if (config == null) return 1.225f;

            // Standard barometric density attenuation ~ 10000m scale height
            float density = config.BaseAirDensity * Mathf.Exp(-altitudeMeters / 10000f);
            return Mathf.Max(0.1f, density);
        }

        /// <summary>
        /// Computes barometric pressure in hPa at a given altitude.
        /// </summary>
        /// <param name="altitudeMeters">Altitude above sea level in meters.</param>
        /// <returns>Pressure in hPa.</returns>
        public float GetAtmosphericPressureAtAltitude(float altitudeMeters)
        {
            if (config == null) return 1013.25f;
            return config.SeaLevelPressureHpa * Mathf.Exp(-altitudeMeters / 8400f);
        }
    }
}



