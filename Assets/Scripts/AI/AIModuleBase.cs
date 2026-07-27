using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Abstract base class implementing lifecycle logic and error handling for UAV AI submodules.
    /// </summary>
    public abstract class AIModuleBase : MonoBehaviour
    {
        [Header("Module Base Settings")]
        [SerializeField] protected string moduleName = "Generic AI Module";
        [SerializeField] protected bool autoInitializeOnStart = true;
        [SerializeField] protected bool isInitialized = false;

        /// <summary>Gets the name of the AI subsystem module.</summary>
        public string ModuleName => moduleName;

        /// <summary>Gets whether the AI module is initialized.</summary>
        public bool IsInitialized => isInitialized;

        protected virtual void Start()
        {
            if (autoInitializeOnStart && !isInitialized)
            {
                Initialize();
            }
        }

        protected virtual void Update()
        {
            if (isInitialized)
            {
                UpdateModule(Time.deltaTime);
            }
        }

        protected virtual void OnDestroy()
        {
            if (isInitialized)
            {
                Shutdown();
            }
        }

        /// <summary>
        /// Initializes AI module dependencies and memory structures.
        /// </summary>
        public virtual void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            Debug.Log($"[AIModuleBase] Initialized module: {moduleName}");
        }

        /// <summary>
        /// Abstract method to execute frame-by-frame module logic.
        /// </summary>
        /// <param name="deltaTime">Delta time in seconds.</param>
        public abstract void UpdateModule(float deltaTime);

        /// <summary>
        /// Shuts down and cleans up module resources.
        /// </summary>
        public virtual void Shutdown()
        {
            if (!isInitialized) return;
            isInitialized = false;
            Debug.Log($"[AIModuleBase] Shutdown module: {moduleName}");
        }
    }
}





