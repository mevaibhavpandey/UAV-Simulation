using System;

namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Summarizes performance metrics and outcome statistics for a completed or aborted mission.
    /// </summary>
    [Serializable]
    public class MissionResult
    {
        /// <summary>Gets or sets whether the mission achieved its primary objectives.</summary>
        public bool IsSuccess { get; set; }

        /// <summary>Gets or sets the final state of the mission lifecycle.</summary>
        public MissionState FinalState { get; set; }

        /// <summary>Gets or sets the total flight duration in seconds.</summary>
        public float TotalFlightTime { get; set; }

        /// <summary>Gets or sets the total cumulative distance traveled in meters.</summary>
        public float TotalDistanceTraveled { get; set; }

        /// <summary>Gets or sets the average ground speed during flight in m/s.</summary>
        public float AverageSpeed { get; set; }

        /// <summary>Gets or sets the peak speed attained during flight in m/s.</summary>
        public float MaxSpeed { get; set; }

        /// <summary>Gets or sets the percentage of battery capacity consumed (0 - 100%).</summary>
        public float BatteryConsumedPercent { get; set; }

        /// <summary>Gets or sets the number of waypoints successfully visited.</summary>
        public int WaypointsCompleted { get; set; }

        /// <summary>Gets or sets the total number of waypoints in the flight plan.</summary>
        public int TotalWaypoints { get; set; }

        /// <summary>Gets or sets details explaining why the mission was aborted, if applicable.</summary>
        public string AbortReason { get; set; }

        /// <summary>Gets or sets the UTC timestamp when the mission ended.</summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissionResult"/> class with default values.
        /// </summary>
        public MissionResult()
        {
            IsSuccess = false;
            FinalState = MissionState.Idle;
            TotalFlightTime = 0f;
            TotalDistanceTraveled = 0f;
            AverageSpeed = 0f;
            MaxSpeed = 0f;
            BatteryConsumedPercent = 0f;
            WaypointsCompleted = 0;
            TotalWaypoints = 0;
            AbortReason = string.Empty;
            ExecutionTime = DateTime.UtcNow;
        }
    }
}
