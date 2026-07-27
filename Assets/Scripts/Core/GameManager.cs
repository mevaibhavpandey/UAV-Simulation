using System;
using UnityEngine;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// Game state enumeration for the ASTRA UAV application lifecycle.
    /// </summary>
    public enum GameState
    {
        Initializing,
        MainMenu,
        SimulationLoading,
        Simulating,
        Paused,
        MissionComplete,
        EmergencyStop
    }

    /// <summary>
    /// Event broadcast when the application transitions between game states.
    /// </summary>
    public struct GameStateChangedEvent : IEvent
    {
        /// <summary>Previous game state before transition.</summary>
        public GameState PreviousState { get; }

        /// <summary>New active game state.</summary>
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Central GameManager singleton bootstrapper and core state machine controller for the ASTRA UAV system.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        /// <summary>
        /// Global singleton instance of the GameManager.
        /// </summary>
        public static GameManager Instance => _instance;

        /// <summary>
        /// Current application game state.
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Initializing;

        /// <summary>
        /// Invoked whenever the game state changes. (PreviousState, NewState)
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;

        [Header("Settings Configuration")]
        [SerializeField] private AppSettings _appSettings;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCoreServices();
        }

        private void Start()
        {
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Initializes foundational core settings and registers global services.
        /// </summary>
        private void InitializeCoreServices()
        {
            if (_appSettings == null)
            {
                _appSettings = AppSettings.Instance;
            }
            else
            {
                AppSettings.Instance = _appSettings;
            }

            _appSettings.ApplySettings();
            ServiceLocator.Register<GameManager>(this);

            Debug.Log("[GameManager] Core services initialized successfully.");
        }

        /// <summary>
        /// Transitions the application state machine to a new state.
        /// </summary>
        /// <param name="newState">Target game state.</param>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameManager] Game State changed from {previousState} to {newState}");

            OnGameStateChanged?.Invoke(previousState, newState);
            EventBus.Publish(new GameStateChangedEvent(previousState, newState));
        }

        /// <summary>
        /// Pauses the simulation state.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Simulating)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resumes simulation state from pause.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Simulating);
            }
        }

        /// <summary>
        /// Triggers an immediate emergency stop state across all systems.
        /// </summary>
        public void TriggerEmergencyStop()
        {
            Debug.LogWarning("[GameManager] EMERGENCY STOP TRIGGERED!");
            ChangeState(GameState.EmergencyStop);
        }

        /// <summary>
        /// Gracefully exits the application.
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("[GameManager] Quitting Application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}




