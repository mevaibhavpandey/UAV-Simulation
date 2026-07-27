using System;
using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Enumeration of UI screen and overlay panel types.
    /// </summary>
    public enum UIPanelType
    {
        MainMenu,
        HUD,
        TelemetryPanel,
        MissionSetup,
        PauseMenu,
        SettingsPanel,
        HardwareBridgeStatus
    }

    /// <summary>
    /// Event broadcast when a UI panel's visibility state changes.
    /// </summary>
    public struct UIPanelVisibilityChangedEvent : IEvent
    {
        public UIPanelType PanelType { get; }
        public bool IsVisible { get; }

        public UIPanelVisibilityChangedEvent(UIPanelType panelType, bool isVisible)
        {
            PanelType = panelType;
            IsVisible = isVisible;
        }
    }

    /// <summary>
    /// Serialization container pairing UI panel type to panel GameObject.
    /// </summary>
    [Serializable]
    public struct UIPanelMapping
    {
        public UIPanelType PanelType;
        public GameObject PanelInstance;
    }

    /// <summary>
    /// Controls UI screens, panel visibility state, HUD elements, and responds to game state changes.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panel Mappings")]
        [SerializeField] private List<UIPanelMapping> _panelMappings = new List<UIPanelMapping>();

        private readonly Dictionary<UIPanelType, GameObject> _panelDictionary = new Dictionary<UIPanelType, GameObject>();

        /// <summary>
        /// Action callback when panel visibility state changes.
        /// </summary>
        public event Action<UIPanelType, bool> OnPanelVisibilityChanged;

        private void Awake()
        {
            ServiceLocator.Register<UIManager>(this);

            foreach (var mapping in _panelMappings)
            {
                if (mapping.PanelInstance != null && !_panelDictionary.ContainsKey(mapping.PanelType))
                {
                    _panelDictionary.Add(mapping.PanelType, mapping.PanelInstance);
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UIManager>();
        }

        /// <summary>
        /// Registers a panel instance at runtime.
        /// </summary>
        /// <param name="type">Panel type key.</param>
        /// <param name="panelInstance">GameObject representing the panel.</param>
        public void RegisterPanel(UIPanelType type, GameObject panelInstance)
        {
            if (panelInstance == null) return;
            _panelDictionary[type] = panelInstance;
        }

        /// <summary>
        /// Shows a specific UI panel.
        /// </summary>
        /// <param name="panelType">Panel type to show.</param>
        public void ShowPanel(UIPanelType panelType)
        {
            SetPanelState(panelType, true);
        }

        /// <summary>
        /// Hides a specific UI panel.
        /// </summary>
        /// <param name="panelType">Panel type to hide.</param>
        public void HidePanel(UIPanelType panelType)
        {
            SetPanelState(panelType, false);
        }

        /// <summary>
        /// Toggles a UI panel's visibility state.
        /// </summary>
        /// <param name="panelType">Panel type to toggle.</param>
        public void TogglePanel(UIPanelType panelType)
        {
            bool currentState = IsPanelActive(panelType);
            SetPanelState(panelType, !currentState);
        }

        /// <summary>
        /// Hides all registered UI panels.
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var kvp in _panelDictionary)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetActive(false);
                    OnPanelVisibilityChanged?.Invoke(kvp.Key, false);
                    EventBus.Publish(new UIPanelVisibilityChangedEvent(kvp.Key, false));
                }
            }
        }

        /// <summary>
        /// Checks if a UI panel is currently active and visible.
        /// </summary>
        /// <param name="panelType">Panel type.</param>
        /// <returns>True if active, false otherwise.</returns>
        public bool IsPanelActive(UIPanelType panelType)
        {
            if (_panelDictionary.TryGetValue(panelType, out var panel) && panel != null)
            {
                return panel.activeSelf;
            }
            return false;
        }

        private void SetPanelState(UIPanelType panelType, bool active)
        {
            if (_panelDictionary.TryGetValue(panelType, out var panel) && panel != null)
            {
                panel.SetActive(active);
                OnPanelVisibilityChanged?.Invoke(panelType, active);
                EventBus.Publish(new UIPanelVisibilityChangedEvent(panelType, active));
            }
            else
            {
                Debug.LogWarning($"[UIManager] UI Panel '{panelType}' is not registered or instance is null.");
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case GameState.MainMenu:
                    HideAllPanels();
                    ShowPanel(UIPanelType.MainMenu);
                    break;
                case GameState.Simulating:
                    HidePanel(UIPanelType.MainMenu);
                    HidePanel(UIPanelType.PauseMenu);
                    ShowPanel(UIPanelType.HUD);
                    ShowPanel(UIPanelType.TelemetryPanel);
                    break;
                case GameState.Paused:
                    ShowPanel(UIPanelType.PauseMenu);
                    break;
            }
        }
    }
}



