namespace ASTRA.UAV.Mission
{
    /// <summary>
    /// Represents the high-level operational lifecycle states of a UAV mission.
    /// </summary>
    public enum MissionState
    {
        /// <summary>Mission system is unassigned or cleared.</summary>
        Idle = 0,

        /// <summary>Mission is being authored or modified.</summary>
        Planning = 1,

        /// <summary>Mission plan is validated and loaded into flight computer.</summary>
        Ready = 2,

        /// <summary>UAV motors are spooling up and pre-flight checks are underway.</summary>
        Arming = 3,

        /// <summary>UAV is executing automated takeoff to target initial altitude.</summary>
        Takeoff = 4,

        /// <summary>UAV is actively navigating between mission waypoints.</summary>
        Mission = 5,

        /// <summary>UAV is holding position at current location.</summary>
        Hover = 6,

        /// <summary>UAV is returning to launch coordinates automatically.</summary>
        ReturnHome = 7,

        /// <summary>UAV is descending for controlled landing.</summary>
        Landing = 8,

        /// <summary>Mission successfully finished all waypoints and disarmed.</summary>
        Completed = 9,

        /// <summary>Mission was aborted due to safety, operator request, or error.</summary>
        Aborted = 10
    }
}




