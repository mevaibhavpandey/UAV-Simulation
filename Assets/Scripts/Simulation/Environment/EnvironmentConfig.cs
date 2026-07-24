using UnityEngine;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Configuration asset defining environment properties, weather parameters, day/night presets, and lighting setups.
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "ASTRA/Simulation/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [Header("Day / Night Cycle Defaults")]
        [Tooltip("Default initial time of day state.")]
        public DayNightState defaultTimeOfDay = DayNightState.Morning;

        [Tooltip("Duration of a full 24-hour cycle in real-time minutes. Set to 0 to disable automatic progression.")]
        public float dayCycleDurationMinutes = 24.0f;

        [Tooltip("Morning sun intensity multiplier.")]
        public float morningSunIntensity = 1.2f;

        [Tooltip("Noon sun intensity multiplier.")]
        public float noonSunIntensity = 1.5f;

        [Tooltip("Evening sun intensity multiplier.")]
        public float eveningSunIntensity = 0.8f;

        [Tooltip("Night sun/moon intensity multiplier.")]
        public float nightSunIntensity = 0.1f;

        [Header("Weather Defaults")]
        [Tooltip("Default initial weather state.")]
        public WeatherState defaultWeather = WeatherState.Sunny;

        [Tooltip("Base wind speed in meters per second.")]
        public float baseWindSpeed = 3.5f;

        [Tooltip("Wind direction vector (normalized).")]
        public Vector3 defaultWindDirection = new Vector3(1f, 0f, 0.5f);

        [Tooltip("Fog density during foggy weather.")]
        public float fogDensity = 0.03f;

        [Header("Facility Camera Settings")]
        [Tooltip("Default move speed for free camera mode.")]
        public float freeCamMoveSpeed = 15.0f;

        [Tooltip("Boost move speed for free camera mode when holding Shift.")]
        public float freeCamFastMoveSpeed = 35.0f;

        [Tooltip("Mouse rotation sensitivity for free camera.")]
        public float freeCamLookSensitivity = 2.0f;

        [Tooltip("Duration of smooth transition between cinematic presentation camera waypoints.")]
        public float cinematicWaypointTransitionDuration = 6.0f;
    }
}
