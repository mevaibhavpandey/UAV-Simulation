using ASTRA.UAV.Mission;
using TMPro;
using UnityEngine;

namespace ASTRA.UAV.UI
{
    /// <summary>
    /// UI Controller handling mission authoring, waypoint creation, validation reporting, and mission upload to flight computer.
    /// </summary>
    public class MissionPlannerUI : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private TMP_InputField missionNameInput;
        [SerializeField] private TMP_InputField altitudeInput;
        [SerializeField] private TMP_InputField speedInput;

        [Header("Status & Validation")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI waypointCountText;

        [Header("References")]
        [SerializeField] private MissionPlanner missionPlanner;
        [SerializeField] private MissionExecutor missionExecutor;

        private ASTRA.UAV.Mission.Mission currentWorkingMission;

        private void Start()
        {
            CreateNewMission();
        }

        /// <summary>
        /// Creates a blank working mission object.
        /// </summary>
        public void CreateNewMission()
        {
            currentWorkingMission = ScriptableObject.CreateInstance<ASTRA.UAV.Mission.Mission>();
            currentWorkingMission.MissionName = "New Flight Mission";
            UpdateUI();
        }

        /// <summary>
        /// Adds a waypoint at the current camera target position.
        /// </summary>
        public void AddWaypointAtTarget()
        {
            if (currentWorkingMission == null) CreateNewMission();

            float alt = float.TryParse(altitudeInput != null ? altitudeInput.text : "20", out float a) ? a : 20f;
            float spd = float.TryParse(speedInput != null ? speedInput.text : "5", out float s) ? s : 5f;

            Vector3 nextPos = Vector3.zero;
            if (currentWorkingMission.Waypoints.Count > 0)
            {
                Vector3 last = currentWorkingMission.Waypoints[currentWorkingMission.Waypoints.Count - 1].LocalPosition;
                nextPos = last + new Vector3(20f, 0f, 20f);
            }
            nextPos.y = alt;

            Waypoint wp = new Waypoint(nextPos, spd);
            currentWorkingMission.AddWaypoint(wp);
            UpdateUI();
        }

        /// <summary>
        /// Clears all waypoints from current mission.
        /// </summary>
        public void ClearWaypoints()
        {
            if (currentWorkingMission != null)
            {
                currentWorkingMission.Waypoints.Clear();
                UpdateUI();
            }
        }

        /// <summary>
        /// Validates current mission and uploads to MissionExecutor.
        /// </summary>
        public void UploadMissionToFlightComputer()
        {
            if (currentWorkingMission == null || currentWorkingMission.Waypoints.Count == 0)
            {
                if (statusText != null) statusText.text = "Error: No waypoints to upload!";
                return;
            }

            if (missionExecutor != null)
            {
                missionExecutor.LoadMission(currentWorkingMission);
                if (statusText != null) statusText.text = "Mission Uploaded Successfully!";
            }
        }

        private void UpdateUI()
        {
            if (currentWorkingMission == null) return;
            if (waypointCountText != null) waypointCountText.text = $"Waypoints: {currentWorkingMission.Waypoints.Count}";
        }
    }
}





