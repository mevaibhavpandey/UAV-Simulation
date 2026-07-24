using ASTRA.UAV.Core;
using UnityEngine;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Connects to an <see cref="ITelemetryProvider"/> and broadcasts <see cref="TelemetryData"/> updates globally via <see cref="EventBus"/>.
    /// </summary>
    public class TelemetryBroadcaster : MonoBehaviour
    {
        [Header("Provider Reference")]
        [SerializeField] private MonoBehaviour telemetryProviderSource;

        private ITelemetryProvider provider;

        private void Awake()
        {
            if (telemetryProviderSource != null && telemetryProviderSource is ITelemetryProvider p)
            {
                provider = p;
            }
            else
            {
                provider = GetComponent<ITelemetryProvider>();
                if (provider == null)
                {
                    provider = FindAnyObjectByType<MockTelemetryProvider>();
                }
            }
        }

        private void OnEnable()
        {
            if (provider != null)
            {
                provider.OnTelemetryUpdated += HandleTelemetryUpdated;
            }
        }

        private void OnDisable()
        {
            if (provider != null)
            {
                provider.OnTelemetryUpdated -= HandleTelemetryUpdated;
            }
        }

        private void HandleTelemetryUpdated(TelemetryData data)
        {
            // Publish snapshot over static decoupled EventBus
            EventBus.Publish(data);
        }
    }
}
