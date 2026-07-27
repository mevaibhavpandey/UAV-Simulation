using UnityEngine;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Animates wind direction sock indicator based on dynamic wind speed and direction vector.
    /// </summary>
    public class WindSock : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform sockSwivelTransform;
        [SerializeField] private Transform sockTailTransform;

        [Header("Dynamics")]
        [SerializeField] private float rotationSmoothing = 5.0f;
        [SerializeField] private float maxTailPitchAngle = 60.0f;

        private void Update()
        {
            Vector3 windVector = Vector3.right * 3.5f;
            float windSpeed = 3.5f;

            if (WeatherManager.Instance != null)
            {
                windVector = WeatherManager.Instance.CurrentWindVector;
                windSpeed = WeatherManager.Instance.CurrentWindSpeed;
            }

            if (windVector.sqrMagnitude > 0.01f && sockSwivelTransform != null)
            {
                // Align windsock swivel with wind direction (pointing away from wind source)
                Quaternion targetRot = Quaternion.LookRotation(windVector.normalized, Vector3.up);
                sockSwivelTransform.rotation = Quaternion.Slerp(sockSwivelTransform.rotation, targetRot, rotationSmoothing * Time.deltaTime);
            }

            if (sockTailTransform != null)
            {
                // Pitch windsock tail depending on wind speed
                float speedNormalized = Mathf.Clamp01(windSpeed / 20.0f);
                float targetPitch = Mathf.Lerp(10.0f, maxTailPitchAngle, speedNormalized);
                
                // Add minor wind flutter
                float flutter = (Mathf.PerlinNoise(Time.time * 8.0f, 0f) - 0.5f) * 8.0f * speedNormalized;
                sockTailTransform.localRotation = Quaternion.Euler(targetPitch + flutter, 0f, 0f);
            }
        }
    }
}





