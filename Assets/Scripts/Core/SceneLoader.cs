using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// Event fired when async scene loading begins.
    /// </summary>
    public struct SceneLoadStartedEvent : IEvent
    {
        public string SceneName { get; }
        public SceneLoadStartedEvent(string sceneName) => SceneName = sceneName;
    }

    /// <summary>
    /// Event fired during async scene loading progress updates.
    /// </summary>
    public struct SceneLoadProgressEvent : IEvent
    {
        public string SceneName { get; }
        public float Progress { get; }
        public SceneLoadProgressEvent(string sceneName, float progress)
        {
            SceneName = sceneName;
            Progress = progress;
        }
    }

    /// <summary>
    /// Event fired when async scene loading completes.
    /// </summary>
    public struct SceneLoadCompletedEvent : IEvent
    {
        public string SceneName { get; }
        public SceneLoadCompletedEvent(string sceneName) => SceneName = sceneName;
    }

    /// <summary>
    /// Asynchronous scene loader service providing progress callbacks and EventBus notifications.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Invoked when scene loading begins.
        /// </summary>
        public event Action<string> OnLoadStarted;

        /// <summary>
        /// Invoked on progress updates (0.0 to 1.0).
        /// </summary>
        public event Action<float> OnLoadProgress;

        /// <summary>
        /// Invoked when scene loading finishes.
        /// </summary>
        public event Action<string> OnLoadCompleted;

        /// <summary>
        /// Indicates if a scene loading operation is currently in progress.
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Current loading progress normalized between 0.0 and 1.0.
        /// </summary>
        public float CurrentProgress { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register<SceneLoader>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SceneLoader>();
        }

        /// <summary>
        /// Initiates loading a scene asynchronously by scene name.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load in Build Settings.</param>
        /// <param name="mode">Load scene mode (Single or Additive).</param>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Scene load already in progress. Ignoring request for '{sceneName}'.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName, mode));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode mode)
        {
            IsLoading = true;
            CurrentProgress = 0f;

            OnLoadStarted?.Invoke(sceneName);
            EventBus.Publish(new SceneLoadStartedEvent(sceneName));

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            if (asyncOp == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene '{sceneName}'. Verify scene name in Build Settings.");
                IsLoading = false;
                yield break;
            }

            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                // Unity async operation progress goes from 0.0 to 0.9 before activation
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                CurrentProgress = progress;

                OnLoadProgress?.Invoke(progress);
                EventBus.Publish(new SceneLoadProgressEvent(sceneName, progress));

                // When 0.9 reached, scene is fully loaded in background
                if (asyncOp.progress >= 0.9f)
                {
                    CurrentProgress = 1.0f;
                    OnLoadProgress?.Invoke(1.0f);
                    EventBus.Publish(new SceneLoadProgressEvent(sceneName, 1.0f));
                    
                    asyncOp.allowSceneActivation = true;
                }

                yield return null;
            }

            IsLoading = false;
            OnLoadCompleted?.Invoke(sceneName);
            EventBus.Publish(new SceneLoadCompletedEvent(sceneName));

            Debug.Log($"[SceneLoader] Successfully loaded scene '{sceneName}'.");
        }
    }
}


