using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.UI.GCS;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Automated 360° Presentation Tour sequence cycling through X-Ray and Exploded views,
    /// highlighting sub-assemblies with floating leader lines and displaying technical summaries.
    /// </summary>
    public class PresentationTourMode : MonoBehaviour
    {
        [Header("Tour State")]
        [SerializeField] private bool isTourActive = false;
        [SerializeField] private float tourProgressNormalized = 0.0f;

        public bool IsTourActive => isTourActive;

        private UAVExplodedView explodedView;
        private UAVEngineeringMode engineeringXRay;

        private void Awake()
        {
            explodedView = GetComponent<UAVExplodedView>();
            engineeringXRay = GetComponent<UAVEngineeringMode>();
        }

        public void StartPresentationTour()
        {
            if (isTourActive) return;

            isTourActive = true;
            tourProgressNormalized = 0f;

            if (EngineeringManager.Instance != null)
            {
                EngineeringManager.Instance.SetRenderMode(EngineeringRenderMode.XRaySemiTransparent);
            }

            if (GCSNotificationSystem.Instance != null)
            {
                GCSNotificationSystem.Instance.PostNotification("Presentation Tour Started", "Automated 360° Digital Twin Presentation Tour engaged.", NotificationType.Info);
            }

            Debug.Log("Automated Presentation Tour Started.", LogCategory.UI);
        }

        private void Update()
        {
            if (!isTourActive) return;

            tourProgressNormalized += Time.deltaTime * 0.1f; // 10 second tour

            // Rotate drone 360 degrees
            transform.Rotate(Vector3.up, Time.deltaTime * 36.0f, Space.World);

            // Animate exploded view halfway through
            if (tourProgressNormalized > 0.3f && tourProgressNormalized < 0.8f)
            {
                float t = Mathf.Sin((tourProgressNormalized - 0.3f) / 0.5f * Mathf.PI);
                if (explodedView != null) explodedView.SetExplosionProgress(t);
            }

            if (tourProgressNormalized >= 1.0f)
            {
                EndPresentationTour();
            }
        }

        public void EndPresentationTour()
        {
            isTourActive = false;
            tourProgressNormalized = 0f;

            if (explodedView != null) explodedView.SetExplosionProgress(0f);
            if (EngineeringManager.Instance != null) EngineeringManager.Instance.SetRenderMode(EngineeringRenderMode.Normal);

            Debug.Log("Presentation Tour Completed.", LogCategory.UI);
        }
    }
}




