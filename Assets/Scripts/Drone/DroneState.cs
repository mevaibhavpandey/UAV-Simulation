//-----------------------------------------------------------------------
// <copyright file="DroneState.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Enumerates the high-level operational flight modes of the quadcopter system.
    /// </summary>
    public enum FlightMode
    {
        /// <summary>System disarmed, motors unpowered.</summary>
        Disarmed,

        /// <summary>Direct manual rate/throttle pass-through control.</summary>
        Manual,

        /// <summary>Acrobatic mode with self-stabilization disabled (angular rate control only).</summary>
        Acro,

        /// <summary>Angle mode with self-leveling attitude control enabled.</summary>
        Angle,

        /// <summary>Altitude hold using barometer/sonar sensor fusion.</summary>
        AltitudeHold,

        /// <summary>Position hold using GPS sensor fusion.</summary>
        PositionHold,

        /// <summary>Autonomous return to home location and landing.</summary>
        ReturnToHome,

        /// <summary>Autonomous waypoint mission execution.</summary>
        AutonomousWaypoint,

        /// <summary>Emergency controlled landing sequence.</summary>
        EmergencyLanding
    }

    /// <summary>
    /// Enumerates the arming lifecycle state of the quadcopter.
    /// </summary>
    public enum DroneArmState
    {
        /// <summary>Motors disabled, throttle safety active.</summary>
        Disarmed,

        /// <summary>Pre-arm safety checks in progress.</summary>
        Arming,

        /// <summary>Motors active and ready for flight.</summary>
        Armed,

        /// <summary>Disarm sequence requested and executing.</summary>
        Disarming,

        /// <summary>Arming error or safety system lockout.</summary>
        ArmingError
    }

    /// <summary>
    /// Quadcopter motor layout positions in standard X-configuration.
    /// </summary>
    public enum MotorPosition
    {
        /// <summary>Front-Left rotor (Clockwise rotation).</summary>
        FrontLeft = 0,

        /// <summary>Front-Right rotor (Counter-Clockwise rotation).</summary>
        FrontRight = 1,

        /// <summary>Rear-Right rotor (Clockwise rotation).</summary>
        RearRight = 2,

        /// <summary>Rear-Left rotor (Counter-Clockwise rotation).</summary>
        RearLeft = 3
    }

    /// <summary>
    /// Operational health status for onboard sensors and peripherals.
    /// </summary>
    public enum SensorHealthStatus
    {
        /// <summary>Sensor offline or uninitialized.</summary>
        Uninitialized,

        /// <summary>Sensor operating normally with high confidence.</summary>
        Healthy,

        /// <summary>Sensor experiencing elevated noise or minor dropouts.</summary>
        Degraded,

        /// <summary>Sensor critical failure or disconnected.</summary>
        Failed
    }

    /// <summary>
    /// Complete telemetry data structure representing the instantaneous state of the quadcopter flight system.
    /// </summary>
    [Serializable]
    public struct DroneFlightStateData
    {
        /// <summary>
        /// Gets or sets the current arming state of the drone.
        /// </summary>
        public DroneArmState ArmState;

        /// <summary>
        /// Gets or sets the active flight control mode.
        /// </summary>
        public FlightMode CurrentFlightMode;

        /// <summary>
        /// Gets or sets the world space position of the quadcopter in meters (Unity coordinates).
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the linear velocity vector in meters per second (m/s).
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Gets or sets the linear acceleration vector in meters per second squared (m/s²).
        /// </summary>
        public Vector3 Acceleration;

        /// <summary>
        /// Gets or sets the orientation quaternion of the drone frame in world space.
        /// </summary>
        public Quaternion Orientation;

        /// <summary>
        /// Gets or sets the angular velocity vector in radians per second (rad/s).
        /// </summary>
        public Vector3 AngularVelocity;

        /// <summary>
        /// Gets or sets the altitude above mean sea level (MSL) in meters.
        /// </summary>
        public float AltitudeMSL;

        /// <summary>
        /// Gets or sets the altitude above ground level (AGL) in meters.
        /// </summary>
        public float AltitudeAGL;

        /// <summary>
        /// Gets or sets the remaining battery percentage [0.0 - 100.0%].
        /// </summary>
        public float BatteryPercentage;

        /// <summary>
        /// Gets or sets the current battery voltage in Volts.
        /// </summary>
        public float BatteryVoltage;

        /// <summary>
        /// Gets or sets a value indicating whether the landing gear is currently resting on a surface.
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// Gets or sets the timestamp of when this flight state snapshot was recorded in seconds.
        /// </summary>
        public double Timestamp;
    }
}



