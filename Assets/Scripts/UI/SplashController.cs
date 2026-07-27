using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Splash screen presentation controller performing system initialization checks, brand logo fade sequences, and automatic main menu scene loading.
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image logoImage;
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Sequence Settings")]
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private float displayDuration = 2.0f;
        [SerializeField] private string targetMenuSceneName = "MainMenuScene";

        private IEnumerator Start()
        {
            if (progressBar != null) progressBar.value = 0f;
            if (statusText != null) statusText.text = "Initializing ASTRA UAV System...";

            // Fade in logo
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));

            // Simulate background subsystem initialization steps
            yield return StartCoroutine(PerformSystemChecks());

            // Fade out logo
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));

            // Transition to target scene
            LoadTargetScene();
        }

        private IEnumerator PerformSystemChecks()
        {
            string[] steps = new string[]
            {
                "Loading Physics Config...",
                "Initializing Telemetry Broadcaster...",
                "Checking Mission Planner Engine...",
                "Loading AI Perception Pipelines...",
                "System Ready."
            };

            for (int i = 0; i < steps.Length; i++)
            {
                if (statusText != null) statusText.text = steps[i];
                if (progressBar != null) progressBar.value = (float)(i + 1) / steps.Length;
                yield return new WaitForSeconds(displayDuration / steps.Length);
            }
        }

        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
        {
            if (logoCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                logoCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            logoCanvasGroup.alpha = endAlpha;
        }

        private void LoadTargetScene()
        {
            if (Application.CanStreamedLevelBeLoaded(targetMenuSceneName))
            {
                SceneManager.LoadScene(targetMenuSceneName);
            }
            else
            {
                Debug.Log($"[SplashController] Target scene '{targetMenuSceneName}' not found in Build Settings. Staying in current scene.");
            }
        }
    }
}


