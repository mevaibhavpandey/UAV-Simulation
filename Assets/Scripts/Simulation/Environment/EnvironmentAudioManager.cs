using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Manages ambient background audio channels across the UAV testing facility.
    /// Controls spatial ambient audio for wind, nature, hangar hum, and control room atmosphere.
    /// </summary>
    public class EnvironmentAudioManager : Singleton<EnvironmentAudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource windAudioSource;
        [SerializeField] private AudioSource natureAudioSource;
        [SerializeField] private AudioSource hangarHumAudioSource;
        [SerializeField] private AudioSource controlRoomAudioSource;

        [Header("Master Volume Controls")]
        [Range(0f, 1f)] [SerializeField] private float masterAmbientVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float windVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float natureVolume = 0.4f;
        [Range(0f, 1f)] [SerializeField] private float hangarHumVolume = 0.3f;
        [Range(0f, 1f)] [SerializeField] private float controlRoomVolume = 0.3f;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (windAudioSource == null) windAudioSource = CreateAudioSource("WindAudioChannel", true, windVolume);
            if (natureAudioSource == null) natureAudioSource = CreateAudioSource("NatureAudioChannel", true, natureVolume);
            if (hangarHumAudioSource == null) hangarHumAudioSource = CreateAudioSource("HangarHumChannel", true, hangarHumVolume);
            if (controlRoomAudioSource == null) controlRoomAudioSource = CreateAudioSource("ControlRoomChannel", true, controlRoomVolume);
        }

        private AudioSource CreateAudioSource(string name, bool loop, float initialVolume)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.volume = initialVolume * masterAmbientVolume;
            source.spatialBlend = 0.0f; // 2D ambient fallback
            return source;
        }

        private void Start()
        {
            ApplyVolumes();
        }

        /// <summary>
        /// Updates audio source volumes dynamically based on weather or spatial zone transitions.
        /// </summary>
        public void ApplyVolumes()
        {
            if (windAudioSource != null) windAudioSource.volume = windVolume * masterAmbientVolume;
            if (natureAudioSource != null) natureAudioSource.volume = natureVolume * masterAmbientVolume;
            if (hangarHumAudioSource != null) hangarHumAudioSource.volume = hangarHumVolume * masterAmbientVolume;
            if (controlRoomAudioSource != null) controlRoomAudioSource.volume = controlRoomVolume * masterAmbientVolume;
        }

        /// <summary>
        /// Adjusts wind sound intensity according to current wind speed.
        /// </summary>
        /// <param name="windSpeed">Wind speed in m/s.</param>
        public void UpdateWindAudio(float windSpeed)
        {
            float normalizedSpeed = Mathf.Clamp01(windSpeed / 25.0f);
            windVolume = Mathf.Lerp(0.2f, 1.0f, normalizedSpeed);
            if (windAudioSource != null)
            {
                windAudioSource.volume = windVolume * masterAmbientVolume;
                windAudioSource.pitch = Mathf.Lerp(0.8f, 1.3f, normalizedSpeed);
            }
        }
    }
}


