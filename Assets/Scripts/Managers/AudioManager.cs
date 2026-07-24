using UnityEngine;
using UnityEngine.Audio;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Wrapper for spatial audio, background music, drone motor SFX, and Unity AudioMixer parameters.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer Reference")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _engineSource;

        [Header("Exposed AudioMixer Parameter Names")]
        [SerializeField] private string _masterParam = "MasterVolume";
        [SerializeField] private string _sfxParam = "SFXVolume";
        [SerializeField] private string _musicParam = "MusicVolume";

        private void Awake()
        {
            ServiceLocator.Register<AudioManager>(this);

            if (_musicSource == null) _musicSource = gameObject.AddComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
            if (_engineSource == null) _engineSource = gameObject.AddComponent<AudioSource>();

            _musicSource.loop = true;
            _engineSource.loop = true;
        }

        private void Start()
        {
            ApplyInitialSettings();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<AudioManager>();
        }

        /// <summary>
        /// Applies volume levels configured in AppSettings.
        /// </summary>
        public void ApplyInitialSettings()
        {
            AppSettings settings = AppSettings.Instance;
            if (settings != null)
            {
                SetMasterVolume(settings.MasterVolume);
                SetSFXVolume(settings.SfxVolume);
                SetMusicVolume(settings.MusicVolume);
            }
        }

        /// <summary>
        /// Plays a one-shot 2D or 3D sound effect.
        /// </summary>
        /// <param name="clip">Audio clip to play.</param>
        /// <param name="position">Optional world position for 3D spatial playback.</param>
        /// <param name="volume">Linear volume scale (0.0 to 1.0).</param>
        public void PlaySFX(AudioClip clip, Vector3? position = null, float volume = 1.0f)
        {
            if (clip == null) return;

            if (position.HasValue)
            {
                AudioSource.PlayClipAtPoint(clip, position.Value, volume);
            }
            else
            {
                _sfxSource.PlayOneShot(clip, volume);
            }
        }

        /// <summary>
        /// Starts playing background music track.
        /// </summary>
        /// <param name="clip">Music clip.</param>
        /// <param name="loop">Should music loop continuously.</param>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        /// <summary>
        /// Stops background music playback.
        /// </summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>
        /// Plays looping drone motor engine sound and adjusts pitch dynamically based on throttle.
        /// </summary>
        /// <param name="engineClip">Engine sound clip.</param>
        /// <param name="initialPitch">Initial pitch multiplier.</param>
        public void PlayEngineSound(AudioClip engineClip, float initialPitch = 1.0f)
        {
            if (engineClip == null) return;

            _engineSource.clip = engineClip;
            _engineSource.pitch = initialPitch;
            if (!_engineSource.isPlaying)
            {
                _engineSource.Play();
            }
        }

        /// <summary>
        /// Updates drone engine motor sound pitch.
        /// </summary>
        /// <param name="pitch">Pitch value (e.g. 0.5 to 2.0).</param>
        public void SetEnginePitch(float pitch)
        {
            if (_engineSource != null)
            {
                _engineSource.pitch = Mathf.Clamp(pitch, 0.2f, 3.0f);
            }
        }

        /// <summary>
        /// Sets master volume on the AudioMixer.
        /// </summary>
        /// <param name="linearVolume">Volume between 0.0 and 1.0.</param>
        public void SetMasterVolume(float linearVolume) => SetMixerVolume(_masterParam, linearVolume);

        /// <summary>
        /// Sets SFX channel volume on the AudioMixer.
        /// </summary>
        /// <param name="linearVolume">Volume between 0.0 and 1.0.</param>
        public void SetSFXVolume(float linearVolume) => SetMixerVolume(_sfxParam, linearVolume);

        /// <summary>
        /// Sets Music channel volume on the AudioMixer.
        /// </summary>
        /// <param name="linearVolume">Volume between 0.0 and 1.0.</param>
        public void SetMusicVolume(float linearVolume) => SetMixerVolume(_musicParam, linearVolume);

        /// <summary>
        /// Helper to convert 0..1 linear volume scale to logarithmic decibels (-80dB to 0dB).
        /// </summary>
        public void SetMixerVolume(string parameterName, float linearVolume)
        {
            if (_audioMixer == null || string.IsNullOrEmpty(parameterName)) return;

            float clamped = Mathf.Clamp(linearVolume, 0.0001f, 1.0f);
            float db = Mathf.Log10(clamped) * 20f;
            _audioMixer.SetFloat(parameterName, db);
        }
    }
}
