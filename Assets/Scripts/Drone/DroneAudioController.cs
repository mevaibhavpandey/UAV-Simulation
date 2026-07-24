using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Audio feedback controller for quadcopter brushless motor engine sound.
    /// Modulates audio pitch and volume dynamically based on motor RPM and throttle.
    /// </summary>
    public class DroneAudioController : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource motorAudioSource;
        [SerializeField] private float minPitch = 0.6f;
        [SerializeField] private float maxPitch = 2.2f;
        [SerializeField] private float maxVolume = 0.85f;

        private PropellerAnimator propellerAnimator;
        private FlightModeManager flightModeManager;

        private void Awake()
        {
            propellerAnimator = GetComponentInChildren<PropellerAnimator>();
            flightModeManager = GetComponent<FlightModeManager>();
            if (motorAudioSource == null) motorAudioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (motorAudioSource == null) return;

            if (flightModeManager != null && flightModeManager.IsArmed)
            {
                if (!motorAudioSource.isPlaying) motorAudioSource.Play();

                float rpm = propellerAnimator != null ? propellerAnimator.CurrentRPM : 0f;
                float normalizedRPM = Mathf.Clamp01(rpm / 8500.0f);

                motorAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedRPM);
                motorAudioSource.volume = Mathf.Lerp(0.1f, maxVolume, normalizedRPM);
            }
            else
            {
                if (motorAudioSource.isPlaying) motorAudioSource.Stop();
            }
        }
    }
}
