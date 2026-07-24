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

        private Mission currentWorkingMission;

        private void Start()
        {
            CreateNewMission();
        }

        /// <summary>
        /// Creates a blank working mission object.
        /// </summary>
        public void CreateNewMission()
        {
            currentWorkingMission = ScriptableObject.CreateInstance<Mission>();
            currentWorkingMission.MissionName = "New Flight Mission";
            UpdateUI();
        }

        /// <summary>
        /// Adds a sample waypoint to the mission plan.
        /// </summary>
        public void AddSampleWaypoint()
        {
            if (currentWorkingMission == null) CreateNewMission();

            float alt = 15f;
            if (altitudeInput != null && float.TryParse(altitudeInput.text, out float userAlt))
            {
                alt = userAlt;
            }

            float speed = 5f;
            if (speedInput != null && float.TryParse(speedInput.text, out float userSpeed))
            {
                speed = userSpeed;
            }

            int count = currentWorkingMission.Waypoints.Count;
            Vector3 pos = new Vector3(count * 20f, alt, count * 15f);

            WaypointAction action = (count == 0) ? WaypointAction.Takeoff : WaypointAction.FlyThrough;
            currentWorkingMission.AddWaypoint(new Waypoint(pos, speed, 0f, action));

            UpdateUI();
        }

        /// <summary>
        /// Validates current mission and displays status.
        /// </summary>
        public void ValidateMission()
        {
            if (currentWorkingMission == null) return;

            bool valid = currentWorkingMission.ValidateMission(out var errors);
            if (valid)
            {
                if (statusText != null) statusText.text = "<color=green>Mission Validated Successfully!</color>";
            }
            else
            {
                if (statusText != null) statusText.text = $"<color=red>Validation Error: {errors[0]}</color>";
            }
        }

        /// <summary>
        /// Uploads mission to active MissionExecutor.
        /// </summary>
        public void UploadToUAV()
        {
            if (currentWorkingMission == null || missionExecutor == null)
            {
                if (statusText != null) statusText.text = "<color=yellow>Upload Failed: Executor missing.</color>";
                return;
            }

            bool success = missionExecutor.LoadMission(currentWorkingMission);
            if (success)
            {
                if (statusText != null) statusText.text = "<color=green>Uploaded to UAV Flight Computer!</color>";
            }
            else
            {
                if (statusText != null) statusText.text = "<color=red>Upload Failed: Invalid mission.</color>";
            }
        }

        private void UpdateUI()
        {
            if (currentWorkingMission == null) return;

            if (missionNameInput != null) missionNameInput.text = currentWorkingMission.MissionName;
            if (waypointCountText != null) waypointCountText.text = $"Waypoints: {currentWorkingMission.Waypoints.Count}";
        }
    }
}
