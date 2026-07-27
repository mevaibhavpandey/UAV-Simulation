using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Enumeration of supported weather states in the UAV testing facility.
    /// </summary>
    public enum WeatherState
    {
        Sunny,
        Cloudy,
        Rain,
        Fog,
        Wind,
        Storm
    }

    /// <summary>
    /// Event data broadcast when weather state changes.
    /// </summary>
    public struct WeatherChangedEvent : IEvent
    {
        public WeatherState PreviousState;
        public WeatherState NewState;
        public Vector3 WindVector;
        public float RainIntensity;
    }

    /// <summary>
    /// Controls weather conditions, atmospheric fog, wind vectors, and rain/storm effects.
    /// Supports dynamic transition architecture across all 6 specified weather presets.
    /// </summary>
    public class WeatherManager : Singleton<WeatherManager>
    {
        [Header("Configuration")]
        [SerializeField] private EnvironmentConfig config;
        [SerializeField] private WeatherState currentWeather = WeatherState.Sunny;

        [Header("Live Weather Output")]
        [SerializeField] private Vector3 currentWindVector = new Vector3(3.5f, 0f, 1.5f);
        [SerializeField] private float currentWindSpeed = 3.5f;
        [SerializeField] private float rainIntensity = 0f;
        [SerializeField] private float fogDensity = 0.005f;

        [Header("Particle System References (Optional Architecture Stubs)")]
        [SerializeField] private ParticleSystem rainParticleSystem;
        [SerializeField] private ParticleSystem fogParticleSystem;

        /// <summary>
        /// Gets current active weather state.
        /// </summary>
        public WeatherState CurrentWeather => currentWeather;

        /// <summary>
        /// Gets normalized wind vector (m/s).
        /// </summary>
        public Vector3 CurrentWindVector => currentWindVector;

        /// <summary>
        /// Gets current wind speed in m/s.
        /// </summary>
        public float CurrentWindSpeed => currentWindSpeed;

        protected override void Awake()
        {
            base.Awake();
            if (config != null)
            {
                currentWeather = config.defaultWeather;
                currentWindSpeed = config.baseWindSpeed;
                currentWindVector = config.defaultWindDirection.normalized * currentWindSpeed;
            }
        }

        private void Start()
        {
            ApplyWeatherState(currentWeather);
        }

        private void Update()
        {
            // Apply subtle Perlin turbulence to wind vector
            float turbulence = (Mathf.PerlinNoise(Time.time * 0.5f, 0f) - 0.5f) * 1.5f;
            Vector3 turbulentWind = currentWindVector + new Vector3(turbulence, 0f, turbulence);
            
            // Broadcast wind update if environment manager exists
            if (EnvironmentManager.Instance != null)
            {
                EnvironmentManager.Instance.SetWindVector(turbulentWind);
            }
        }

        /// <summary>
        /// Sets new weather condition and triggers smooth parameter transition.
        /// </summary>
        /// <param name="newState">Target weather state.</param>
        public void SetWeather(WeatherState newState)
        {
            if (currentWeather == newState) return;

            WeatherState oldState = currentWeather;
            currentWeather = newState;
            ApplyWeatherState(newState);

            EventBus.Publish(new WeatherChangedEvent
            {
                PreviousState = oldState,
                NewState = newState,
                WindVector = currentWindVector,
                RainIntensity = rainIntensity
            });

            Debug.Log($"Weather state changed from {oldState} to {newState}", LogCategory.Simulation);
        }

        private void ApplyWeatherState(WeatherState state)
        {
            switch (state)
            {
                case WeatherState.Sunny:
                    rainIntensity = 0f;
                    fogDensity = 0.002f;
                    currentWindSpeed = config != null ? config.baseWindSpeed : 3.5f;
                    RenderSettings.fog = false;
                    break;

                case WeatherState.Cloudy:
                    rainIntensity = 0f;
                    fogDensity = 0.008f;
                    currentWindSpeed = 5.0f;
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Exponential;
                    RenderSettings.fogDensity = fogDensity;
                    break;

                case WeatherState.Rain:
                    rainIntensity = 0.6f;
                    fogDensity = 0.015f;
                    currentWindSpeed = 7.5f;
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = fogDensity;
                    break;

                case WeatherState.Fog:
                    rainIntensity = 0f;
                    fogDensity = config != null ? config.fogDensity : 0.04f;
                    currentWindSpeed = 2.0f;
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = fogDensity;
                    break;

                case WeatherState.Wind:
                    rainIntensity = 0f;
                    fogDensity = 0.005f;
                    currentWindSpeed = 14.0f;
                    RenderSettings.fog = false;
                    break;

                case WeatherState.Storm:
                    rainIntensity = 1.0f;
                    fogDensity = 0.035f;
                    currentWindSpeed = 22.0f;
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = fogDensity;
                    break;
            }

            currentWindVector = (config != null ? config.defaultWindDirection.normalized : Vector3.right) * currentWindSpeed;

            if (rainParticleSystem != null)
            {
                var main = rainParticleSystem.main;
                if (rainIntensity > 0f)
                {
                    rainParticleSystem.Play();
                }
                else
                {
                    rainParticleSystem.Stop();
                }
            }
        }

        /// <summary>
        /// Sets dynamic wind vector directly (used by mission parameters).
        /// </summary>
        public void SetWind(Vector3 direction, float speed)
        {
            currentWindSpeed = speed;
            currentWindVector = direction.normalized * speed;
        }
    }
}


