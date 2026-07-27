using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Supported flight modes in the UAV simulator.
    /// </summary>
    public enum FlightModeType
    {
        Disarmed,
        Armed,
        Manual,
        Stabilize,
        AltitudeHold,
        Loiter,
        Auto,
        RTL
    }

    /// <summary>
    /// Event broadcast when the active flight mode transitions.
    /// </summary>
    public struct FlightModeChangedEvent : IEvent
    {
        public FlightModeType PreviousMode;
        public FlightModeType NewMode;
    }

    /// <summary>
    /// Manages UAV flight modes (Disarmed, Armed, Manual, Stabilize, AltitudeHold, Loiter, Auto, RTL).
    /// Handles mode switching rules, arming safety checks, and event dispatching.
    /// </summary>
    public class FlightModeManager : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private FlightModeType currentMode = FlightModeType.Disarmed;
        [SerializeField] private bool isArmed = false;

        public FlightModeType CurrentMode => currentMode;
        public bool IsArmed => isArmed;

        /// <summary>
        /// Attempts to switch to a target flight mode.
        /// </summary>
        public bool SetFlightMode(FlightModeType newMode)
        {
            if (currentMode == newMode) return true;

            // Safety rules: Cannot enter active flight modes if disarmed
            if (!isArmed && newMode != FlightModeType.Disarmed && newMode != FlightModeType.Armed)
            {
                Debug.LogWarning($"Cannot switch to {newMode} while UAV is Disarmed!", LogCategory.Drone);
                return false;
            }

            FlightModeType oldMode = currentMode;
            currentMode = newMode;

            EventBus.Publish(new FlightModeChangedEvent
            {
                PreviousMode = oldMode,
                NewMode = newMode
            });

            Debug.Log($"Flight Mode changed from {oldMode} to {newMode}", LogCategory.Drone);
            return true;
        }

        /// <summary>
        /// Arms the UAV motors if safety checks pass.
        /// </summary>
        public bool Arm()
        {
            if (isArmed) return true;

            isArmed = true;
            SetFlightMode(FlightModeType.Armed);
            Debug.Log("UAV Motors ARMED successfully.", LogCategory.Drone);
            return true;
        }

        /// <summary>
        /// Disarms the UAV motors.
        /// </summary>
        public void Disarm()
        {
            isArmed = false;
            SetFlightMode(FlightModeType.Disarmed);
            Debug.Log("UAV Motors DISARMED.", LogCategory.Drone);
        }

        /// <summary>
        /// Toggles arm/disarm state.
        /// </summary>
        public void ToggleArm()
        {
            if (isArmed) Disarm();
            else Arm();
        }
    }
}




