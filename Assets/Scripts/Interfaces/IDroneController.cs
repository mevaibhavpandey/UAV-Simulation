using System;
using UnityEngine;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Represents the current operational flight state of the drone.
    /// </summary>
    public enum FlightState
    {
        /// <summary>Motors are disarmed and inactive.</summary>
        Disarmed,
        /// <summary>Motors are priming and armed.</summary>
        Armed,
        /// <summary>Drone is resting on the ground with motors idle.</summary>
        Grounded,
        /// <summary>Automated takeoff sequence in progress.</summary>
        TakingOff,
        /// <summary>Active airborne flight mode.</summary>
        InFlight,
        /// <summary>Automated landing sequence in progress.</summary>
        Landing,
        /// <summary>Automated return to launch position in progress.</summary>
        ReturningToHome,
        /// <summary>Emergency shutdown engaged.</summary>
        EmergencyStop
    }

    /// <summary>
    /// Represents the flight control mode.
    /// </summary>
    public enum FlightMode
    {
        /// <summary>Direct manual pilot control.</summary>
        Manual,
        /// <summary>Self-leveling attitude stabilization.</summary>
        Stabilized,
        /// <summary>Altitude hold using barometer/sonar.</summary>
        AltitudeHold,
        /// <summary>Full 3D position hold using GPS/optical flow.</summary>
        PositionHold,
        /// <summary>Autonomous navigation following waypoint mission.</summary>
        AutoMission,
        /// <summary>Autonomous return to home flight path.</summary>
        ReturnToHome,
        /// <summary>Acrobatic / direct rate control mode.</summary>
        Acro
    }

    /// <summary>
    /// Defines the contract for controlling UAV flight dynamics, modes, and commands.
    /// </summary>
    public interface IDroneController
    {
        /// <summary>
        /// Gets a value indicating whether the drone motors are currently armed.
        /// </summary>
        bool IsArmed { get; }

        /// <summary>
        /// Gets a value indicating whether the drone is currently grounded.
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Gets the current flight state.
        /// </summary>
        FlightState CurrentFlightState { get; }

        /// <summary>
        /// Gets the active flight control mode.
        /// </summary>
        FlightMode CurrentFlightMode { get; }

        /// <summary>
        /// Gets the current world position in Unity coordinate space (ENU).
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets the current linear velocity vector (m/s).
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Gets the current aircraft attitude (orientation quaternion).
        /// </summary>
        Quaternion Attitude { get; }

        /// <summary>
        /// Gets the current angular velocity vector in body frame (rad/s).
        /// </summary>
        Vector3 AngularVelocity { get; }

        /// <summary>
        /// Gets the current altitude above ground level (AGL in meters).
        /// </summary>
        float AltitudeAGL { get; }

        /// <summary>
        /// Gets the current battery remaining capacity normalized [0.0 to 1.0].
        /// </summary>
        float BatteryNormalized { get; }

        /// <summary>
        /// Fired when the flight state transitions.
        /// </summary>
        event Action<FlightState> OnFlightStateChanged;

        /// <summary>
        /// Fired when the flight mode transitions.
        /// </summary>
        event Action<FlightMode> OnFlightModeChanged;

        /// <summary>
        /// Fired when a flight warning or error occurs.
        /// </summary>
        event Action<string> OnFlightError;

        /// <summary>
        /// Attempts to arm the drone motors.
        /// </summary>
        /// <returns>True if armed successfully, false otherwise.</returns>
        bool Arm();

        /// <summary>
        /// Attempts to disarm the drone motors.
        /// </summary>
        /// <returns>True if disarmed successfully, false otherwise.</returns>
        bool Disarm();

        /// <summary>
        /// Initiates automated takeoff to the specified target altitude.
        /// </summary>
        /// <param name="targetAltitude">Desired takeoff altitude in meters above ground level.</param>
        void Takeoff(float targetAltitude);

        /// <summary>
        /// Initiates automated landing at the current position.
        /// </summary>
        void Land();

        /// <summary>
        /// Commands the drone to immediately return to its home launch location.
        /// </summary>
        void ReturnToHome();

        /// <summary>
        /// Cuts motor power immediately for emergency procedures.
        /// </summary>
        void EmergencyStop();

        /// <summary>
        /// Sets desired linear velocity and yaw rate in velocity-control mode.
        /// </summary>
        /// <param name="velocity">Target linear velocity vector (m/s).</param>
        /// <param name="yawRate">Target yaw rotational rate (rad/s).</param>
        void SetVelocity(Vector3 velocity, float yawRate);

        /// <summary>
        /// Sets desired target position and target yaw angle.
        /// </summary>
        /// <param name="position">Target world space position.</param>
        /// <param name="targetYaw">Target heading angle in degrees.</param>
        void SetTargetPosition(Vector3 position, float targetYaw);

        /// <summary>
        /// Sets the active flight control mode.
        /// </summary>
        /// <param name="mode">Target flight mode.</param>
        void SetFlightMode(FlightMode mode);
    }
}
