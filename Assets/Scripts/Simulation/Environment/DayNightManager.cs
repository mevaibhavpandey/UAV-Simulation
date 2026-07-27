using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Supported diurnal time of day states.
    /// </summary>
    public enum DayNightState
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }

    /// <summary>
    /// Event broadcast when the time of day state transitions.
    /// </summary>
    public struct DayNightChangedEvent : IEvent
    {
        public DayNightState PreviousState;
        public DayNightState NewState;
        public float SunAngle;
    }

    /// <summary>
    /// Architecture for diurnal time of day management.
    /// Rotates directional light (Sun), modulates skybox ambient, and updates facility streetlights.
    /// </summary>
    public class DayNightManager : Singleton<DayNightManager>
    {
        [Header("Configuration")]
        [SerializeField] private EnvironmentConfig config;
        [SerializeField] private Light sunDirectionalLight;
        [SerializeField] private DayNightState currentState = DayNightState.Morning;

        [Header("Lighting Presets")]
        [SerializeField] private Color morningSunColor = new Color(1.0f, 0.85f, 0.7f);
        [SerializeField] private Color afternoonSunColor = new Color(1.0f, 0.98f, 0.9f);
        [SerializeField] private Color eveningSunColor = new Color(1.0f, 0.45f, 0.2f);
        [SerializeField] private Color nightSunColor = new Color(0.15f, 0.2f, 0.35f);

        [Header("Automatic Cycle")]
        [SerializeField] private bool autoAdvanceCycle = false;
        [SerializeField] private float timeOfDayNormalized = 0.25f; // 0=Midnight, 0.25=6am Morning, 0.5=Noon, 0.75=6pm Evening

        /// <summary>
        /// Gets current time of day state.
        /// </summary>
        public DayNightState CurrentState => currentState;

        /// <summary>
        /// Gets normalized time of day [0.0 - 1.0].
        /// </summary>
        public float TimeOfDayNormalized => timeOfDayNormalized;

        protected override void Awake()
        {
            base.Awake();
            if (sunDirectionalLight == null)
            {
                sunDirectionalLight = RenderSettings.sun != null ? RenderSettings.sun : FindFirstObjectByType<Light>();
            }
            if (config != null)
            {
                currentState = config.defaultTimeOfDay;
            }
        }

        private void Start()
        {
            ApplyState(currentState);
        }

        private void Update()
        {
            if (autoAdvanceCycle && config != null && config.dayCycleDurationMinutes > 0)
            {
                float cycleSpeed = 1.0f / (config.dayCycleDurationMinutes * 60.0f);
                timeOfDayNormalized = (timeOfDayNormalized + Time.deltaTime * cycleSpeed) % 1.0f;
                UpdateSunPositionAndLighting();
            }
        }

        /// <summary>
        /// Sets the time of day state explicitly.
        /// </summary>
        /// <param name="newState">Target time of day.</param>
        public void SetTimeOfDay(DayNightState newState)
        {
            if (currentState == newState) return;

            DayNightState oldState = currentState;
            currentState = newState;
            ApplyState(newState);

            EventBus.Publish(new DayNightChangedEvent
            {
                PreviousState = oldState,
                NewState = newState,
                SunAngle = sunDirectionalLight != null ? sunDirectionalLight.transform.eulerAngles.x : 0f
            });

            Debug.Log($"Day/Night state changed from {oldState} to {newState}", LogCategory.Simulation);
        }

        private void ApplyState(DayNightState state)
        {
            switch (state)
            {
                case DayNightState.Morning:
                    timeOfDayNormalized = 0.25f;
                    break;
                case DayNightState.Afternoon:
                    timeOfDayNormalized = 0.5f;
                    break;
                case DayNightState.Evening:
                    timeOfDayNormalized = 0.75f;
                    break;
                case DayNightState.Night:
                    timeOfDayNormalized = 0.95f;
                    break;
            }

            UpdateSunPositionAndLighting();
        }

        private void UpdateSunPositionAndLighting()
        {
            if (sunDirectionalLight == null) return;

            // Calculate sun elevation pitch angle (0 to 360)
            float sunPitch = (timeOfDayNormalized * 360.0f) - 90.0f; // Morning ~ 0 deg pitch, Noon ~ 90 deg pitch
            float sunYaw = 170.0f; // Fixed realistic South-East to South-West arc

            sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, sunYaw, 0f);

            // Set color & intensity based on time normalized
            if (timeOfDayNormalized >= 0.2f && timeOfDayNormalized < 0.4f)
            {
                // Morning
                sunDirectionalLight.color = morningSunColor;
                sunDirectionalLight.intensity = config != null ? config.morningSunIntensity : 1.2f;
            }
            else if (timeOfDayNormalized >= 0.4f && timeOfDayNormalized < 0.65f)
            {
                // Afternoon / Noon
                sunDirectionalLight.color = afternoonSunColor;
                sunDirectionalLight.intensity = config != null ? config.noonSunIntensity : 1.5f;
            }
            else if (timeOfDayNormalized >= 0.65f && timeOfDayNormalized < 0.85f)
            {
                // Evening / Sunset
                sunDirectionalLight.color = eveningSunColor;
                sunDirectionalLight.intensity = config != null ? config.eveningSunIntensity : 0.8f;
            }
            else
            {
                // Night
                sunDirectionalLight.color = nightSunColor;
                sunDirectionalLight.intensity = config != null ? config.nightSunIntensity : 0.1f;
            }
        }
    }
}



