using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// Presentation controller handling top-level main menu screen transitions and application launch controls.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject missionPlannerPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject engineeringPanel;

        [Header("Scene Loading")]
        [SerializeField] private string flightSimulationSceneName = "FlightSimulationScene";

        private void Start()
        {
            ShowMainPanel();
        }

        /// <summary>
        /// Displays the main navigation panel and hides secondary sub-panels.
        /// </summary>
        public void ShowMainPanel()
        {
            SetPanelState(mainPanel, true);
            SetPanelState(missionPlannerPanel, false);
            SetPanelState(settingsPanel, false);
            SetPanelState(engineeringPanel, false);
        }

        /// <summary>
        /// Triggers transition into the main flight simulation environment scene.
        /// </summary>
        public void OnPlayClicked()
        {
            Debug.Log($"[MainMenuController] Loading flight simulation scene: '{flightSimulationSceneName}'");
            if (Application.CanStreamedLevelBeLoaded(flightSimulationSceneName))
            {
                SceneManager.LoadScene(flightSimulationSceneName);
            }
            else
            {
                Debug.LogWarning($"[MainMenuController] Scene '{flightSimulationSceneName}' not in Build Settings.");
            }
        }

        /// <summary>
        /// Opens the mission planner panel UI.
        /// </summary>
        public void OnMissionPlannerClicked()
        {
            SetPanelState(mainPanel, false);
            SetPanelState(missionPlannerPanel, true);
        }

        /// <summary>
        /// Opens the system settings configuration panel.
        /// </summary>
        public void OnSettingsClicked()
        {
            SetPanelState(mainPanel, false);
            SetPanelState(settingsPanel, true);
        }

        /// <summary>
        /// Opens the engineering mode diagnostics panel.
        /// </summary>
        public void OnEngineeringClicked()
        {
            SetPanelState(mainPanel, false);
            SetPanelState(engineeringPanel, true);
        }

        /// <summary>
        /// Quits the application or stops playmode in Unity Editor.
        /// </summary>
        public void OnQuitClicked()
        {
            Debug.Log("[MainMenuController] Quitting UAV Simulation Suite.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetPanelState(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}





