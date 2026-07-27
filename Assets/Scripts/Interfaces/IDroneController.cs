using System;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Represents the current operational flight state of the drone.
    /// </summary>
    public enum FlightState
    {
        Disarmed,
        Armed,
        Grounded,
        TakingOff,
        InFlight,
        Landing,
        ReturningToHome,
        EmergencyStop
    }

    /// <summary>
    /// Represents the flight control mode.
    /// </summary>
    public enum FlightMode
    {
        Angle,
        Manual,
        Stabilized,
        AltitudeHold,
        PositionHold,
        AutoMission,
        ReturnToHome,
        Acro
    }

    /// <summary>
    /// Comprehensive interface defining flight control input contracts, operational state management,
    /// and dynamic position/velocity guidance for UAV quadcopters.
    /// </summary>
    public interface IDroneController
    {
        // Control Axis Inputs
        float RollInput { get; set; }
        float PitchInput { get; set; }
        float YawInput { get; set; }
        float ThrottleInput { get; set; }
        bool IsActive { get; }

        // State Readouts
        bool IsArmed { get; }
        bool IsGrounded { get; }
        FlightState CurrentFlightState { get; }
        FlightMode CurrentFlightMode { get; }
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        Quaternion Attitude { get; }
        Vector3 AngularVelocity { get; }
        float AltitudeAGL { get; }
        float BatteryNormalized { get; }

        // Events
        event Action<FlightState> OnFlightStateChanged;
        event Action<FlightMode> OnFlightModeChanged;
        event Action<string> OnFlightError;

        // Arming & Basic Commands
        bool Arm();
        bool Disarm();
        void Takeoff(float targetAltitude);
        void Land();
        void ReturnToHome();
        void EmergencyStop();

        // Control Method Modifications
        void SetControlInputs(float pitch, float roll, float yaw, float throttle);
        void ResetInputs();
        void SetVelocity(Vector3 velocity, float yawRate);
        void SetTargetPosition(Vector3 position, float targetYaw);
        void SetFlightMode(FlightMode mode);
    }
}


