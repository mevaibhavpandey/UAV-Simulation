using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.UI.GCS;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.AI
{
    public enum LocalizationSource
    {
        GPS_Primary,
        Visual_SLAM,
        VIO_Inertial,
        IMU_DeadReckoning,
        Fused_Estimate
    }

    public enum GPSFailureType
    {
        None,
        SignalLoss,
        Jamming,
        Spoofing
    }

    /// <summary>
    /// Event broadcast when GPS failure occurs or localization mode changes.
    /// </summary>
    public struct GPSFailureEvent : IEvent
    {
        public GPSFailureType FailureType;
        public LocalizationSource NewSource;
    }

    /// <summary>
    /// Master manager handling active localization source switching (GPS, Visual SLAM, VIO, Sensor Fusion).
    /// Manages GPS failure simulation (Signal Loss, Jamming, Spoofing) and fallback transitions.
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        [Header("State")]
        [SerializeField] private LocalizationSource activeSource = LocalizationSource.GPS_Primary;
        [SerializeField] private GPSFailureType currentGPSFailure = GPSFailureType.None;
        [SerializeField] private bool isGPSAvailable = true;

        public LocalizationSource ActiveSource => activeSource;
        public GPSFailureType CurrentGPSFailure => currentGPSFailure;
        public bool IsGPSAvailable => isGPSAvailable;

        /// <summary>
        /// Simulates GPS signal failure (Jamming, Spoofing, Signal Loss).
        /// </summary>
        public void TriggerGPSFailure(GPSFailureType failureType)
        {
            currentGPSFailure = failureType;
            isGPSAvailable = (failureType == GPSFailureType.None);

            if (!isGPSAvailable)
            {
                activeSource = LocalizationSource.Fused_Estimate;
                Debug.LogWarning($"GPS FAILURE TRIGGERED [{failureType}]! Fallback to GPS-Denied Visual SLAM & VIO Fusion.", LogCategory.AI);

                EventBus.Publish(new GPSFailureEvent
                {
                    FailureType = failureType,
                    NewSource = activeSource
                });

                if (GCSNotificationSystem.Instance != null)
                {
                    GCSNotificationSystem.Instance.PostNotification("GPS Signal Lost!", $"GPS Failure ({failureType})! Switching to onboard Visual SLAM & VIO Sensor Fusion.", NotificationType.Critical);
                }
            }
            else
            {
                activeSource = LocalizationSource.GPS_Primary;
                Debug.Log("GPS Signal Restored. Returned to GPS Primary Navigation.", LogCategory.AI);

                if (GCSNotificationSystem.Instance != null)
                {
                    GCSNotificationSystem.Instance.PostNotification("GPS Signal Restored", "Primary GPS lock re-established.", NotificationType.Info);
                }
            }
        }

        /// <summary>
        /// Restores normal GPS operation.
        /// </summary>
        public void RestoreGPS()
        {
            TriggerGPSFailure(GPSFailureType.None);
        }
    }
}



