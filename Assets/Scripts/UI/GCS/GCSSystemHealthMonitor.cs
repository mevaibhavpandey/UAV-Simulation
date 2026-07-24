using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.UI.GCS
{
    public enum SubsystemHealthState
    {
        Healthy,
        Warning,
        Critical
    }

    [System.Serializable]
    public class SubsystemStatus
    {
        public string subsystemName;
        public SubsystemHealthState status = SubsystemHealthState.Healthy;
        public string details = "OK";
    }

    /// <summary>
    /// Tracks and reports health indicators for FC, Battery, Motors, ESCs, GPS, Compass, Telemetry, Camera, Companion Computer, and Sensors.
    /// </summary>
    public class GCSSystemHealthMonitor : MonoBehaviour
    {
        [SerializeField] private List<SubsystemStatus> subsystemStatuses = new List<SubsystemStatus>();

        public List<SubsystemStatus> Subsystems => subsystemStatuses;

        private void Awake()
        {
            InitializeSubsystems();
        }

        private void InitializeSubsystems()
        {
            if (subsystemStatuses.Count > 0) return;

            string[] names = new string[]
            {
                "Pixhawk 6X FC", "6S LiPo Battery", "Brushless Motors", "120A ESCs",
                "U-Blox M9N GPS", "Internal Compass", "Telemetry Radio", "RGB/Depth Camera",
                "Raspberry Pi 5", "Obstacle Sensors"
            };

            foreach (var n in names)
            {
                subsystemStatuses.Add(new SubsystemStatus
                {
                    subsystemName = n,
                    status = SubsystemHealthState.Healthy,
                    details = "Nominal Operation"
                });
            }
        }

        public void SetStatus(string name, SubsystemHealthState state, string details)
        {
            SubsystemStatus target = subsystemStatuses.Find(s => s.subsystemName == name);
            if (target != null)
            {
                target.status = state;
                target.details = details;
            }
        }
    }
}
