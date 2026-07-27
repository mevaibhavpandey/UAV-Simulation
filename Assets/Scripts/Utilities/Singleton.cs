using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Generic MonoBehaviour Singleton base class for Unity 6 systems.
    /// Provides global instance access, duplicate prevention, and clean teardown checks.
    /// </summary>
    /// <typeparam name="T">MonoBehaviour type deriving from Singleton.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isApplicationQuitting = false;

        /// <summary>
        /// Gets a value indicating whether the instance currently exists in the scene.
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Gets a value indicating whether the application is in the process of quitting.
        /// </summary>
        public static bool IsApplicationQuitting => _isApplicationQuitting;

        /// <summary>
        /// Gets the singleton instance of type <typeparamref name="T"/>.
        /// Creates a new persistent GameObject if no instance exists.
        /// </summary>
        public static T Instance
        {
            get;
        } = GetOrCreateInstance();

        private static T GetOrCreateInstance()
        {
            if (_isApplicationQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' requested during application quit. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        var singletonObject = new GameObject($"[{typeof(T).Name}]");
                        _instance = singletonObject.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Unity Awake event lifecycle handler. Subclasses should override <see cref="OnSingletonAwake"/> instead of Awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (ShouldPersistBetweenScenes())
                {
                    DontDestroyOnLoad(gameObject.transform.root.gameObject);
                }
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of '{typeof(T)}' detected on '{gameObject.name}'. Destroying component.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Override to specify whether the singleton gameObject persists across scene loads. Default is true.
        /// </summary>
        /// <returns>True to keep across scene loads via DontDestroyOnLoad.</returns>
        protected virtual bool ShouldPersistBetweenScenes() => true;

        /// <summary>
        /// Override this method for initialization logic instead of Awake.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        /// <summary>
        /// Unity OnApplicationQuit event.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        /// <summary>
        /// Unity OnDestroy event.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}



