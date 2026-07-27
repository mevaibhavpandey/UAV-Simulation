//-----------------------------------------------------------------------
// <copyright file="DroneMotor.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Represents an individual brushless motor and propeller assembly on the quadcopter,
    /// handling RPM response, thrust generation telemetry, and motor health dynamics.
    /// </summary>
    public class DroneMotor : MonoBehaviour
    {
        [Header("Motor Configuration")]
        [SerializeField, Tooltip("Identifies the physical quadcopter arm location for this motor.")]
        private MotorPosition motorPosition = MotorPosition.FrontLeft;

        [SerializeField, Tooltip("Maximum rated revolutions per minute (RPM) under full throttle.")]
        private float maxRPM = 12000f;

        [SerializeField, Tooltip("Rotation direction multiplier (+1 for Clockwise, -1 for Counter-Clockwise).")]
        private int rotationDirection = 1;

        [SerializeField, Tooltip("Propeller diameter in inches.")]
        private float propDiameterInches = 10f;

        [SerializeField, Tooltip("Thrust coefficient factor (N / (RPM^2)).")]
        private float thrustCoefficient = 1.2e-7f;

        [SerializeField, Tooltip("Torque coefficient factor (N*m / (RPM^2)).")]
        private float torqueCoefficient = 1.8e-9f;

        [SerializeField, Tooltip("Motor responsiveness time constant in seconds (acceleration lag).")]
        private float responseTimeConstant = 0.05f;

        [Header("Telemetry & Live Dynamics")]
        [SerializeField, Tooltip("Current throttle input [0.0 - 1.0].")]
        private float targetThrottle = 0f;

        [SerializeField, Tooltip("Current instantaneous motor spin rate in RPM.")]
        private float currentRPM = 0f;

        [SerializeField, Tooltip("Motor operating temperature in degrees Celsius.")]
        private float motorTemperatureCelsius = 25f;

        [SerializeField, Tooltip("Flag indicating whether motor is operational or faulted.")]
        private bool isOperational = true;

        /// <summary>
        /// Gets the assigned motor position on the drone frame.
        /// </summary>
        public MotorPosition Position => motorPosition;

        /// <summary>
        /// Gets the maximum rated RPM of the motor.
        /// </summary>
        public float MaxRPM => maxRPM;

        /// <summary>
        /// Gets the rotation direction multiplier (+1 CW, -1 CCW).
        /// </summary>
        public int RotationDirection => rotationDirection;

        /// <summary>
        /// Gets the current instantaneous RPM of the motor.
        /// </summary>
        public float CurrentRPM => currentRPM;

        /// <summary>
        /// Gets the target throttle level requested by the flight controller [0.0, 1.0].
        /// </summary>
        public float TargetThrottle => targetThrottle;

        /// <summary>
        /// Gets the current motor temperature in Celsius.
        /// </summary>
        public float MotorTemperatureCelsius => motorTemperatureCelsius;

        /// <summary>
        /// Gets a value indicating whether the motor is operating normally without faults.
        /// </summary>
        public bool IsOperational => isOperational;

        /// <summary>
        /// Gets the normalized output thrust ratio [0.0, 1.0].
        /// </summary>
        public float NormalizedThrust => Mathf.Clamp01(currentRPM / Mathf.Max(1f, maxRPM));

        /// <summary>
        /// Occurs when the motor state or telemetry updates.
        /// </summary>
        public event Action<MotorPosition, float> OnRPMChanged;

        /// <summary>
        /// Unity Start lifecycle callback.
        /// </summary>
        private void Start()
        {
            ResetMotor();
        }

        /// <summary>
        /// Sets the requested target throttle input for this motor.
        /// </summary>
        /// <param name="throttle">Normalized throttle value between 0.0 and 1.0.</param>
        public void SetThrottleInput(float throttle)
        {
            if (!isOperational)
            {
                targetThrottle = 0f;
                return;
            }

            targetThrottle = Mathf.Clamp01(throttle);
        }

        /// <summary>
        /// Updates the internal motor dynamics (RPM spin-up lag and thermal simulation) for a given delta time.
        /// </summary>
        /// <param name="deltaTime">Elapsed time in seconds since the last simulation update.</param>
        public void UpdateMotorDynamics(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            if (!isOperational)
            {
                currentRPM = Mathf.MoveTowards(currentRPM, 0f, maxRPM * deltaTime * 2f);
                OnRPMChanged?.Invoke(motorPosition, currentRPM);
                return;
            }

            float targetRPM = targetThrottle * maxRPM;
            float lerpAlpha = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.001f, responseTimeConstant));
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, lerpAlpha);

            // Thermal simulation stub: higher RPM increases temperature, ambient cooling brings it down
            float heatGenerated = (currentRPM / maxRPM) * 0.5f * deltaTime;
            float heatDissipated = (motorTemperatureCelsius - 25f) * 0.05f * deltaTime;
            motorTemperatureCelsius = Mathf.Max(25f, motorTemperatureCelsius + heatGenerated - heatDissipated);

            OnRPMChanged?.Invoke(motorPosition, currentRPM);
        }

        /// <summary>
        /// Calculates total vertical thrust force produced by this rotor in Newtons.
        /// </summary>
        /// <returns>Thrust force magnitude in Newtons (N).</returns>
        public float CalculateThrustForce()
        {
            if (!isOperational) return 0f;
            return thrustCoefficient * (currentRPM * currentRPM);
        }

        /// <summary>
        /// Calculates counter-torque produced by rotor drag in Newton-meters (N*m).
        /// Positive value resists clockwise spin, negative resists counter-clockwise spin.
        /// </summary>
        /// <returns>Torque vector along local Z axis in N*m.</returns>
        public Vector3 CalculateTorqueVector()
        {
            if (!isOperational) return Vector3.zero;
            float torqueMagnitude = torqueCoefficient * (currentRPM * currentRPM) * rotationDirection;
            return new Vector3(0f, torqueMagnitude, 0f);
        }

        /// <summary>
        /// Simulates a mechanical or electrical failure condition for this motor.
        /// </summary>
        /// <param name="faulted">True to trigger a motor fault, false to restore normal operation.</param>
        public void SetFaultState(bool faulted)
        {
            isOperational = !faulted;
            if (faulted)
            {
                targetThrottle = 0f;
            }
        }

        /// <summary>
        /// Resets motor telemetry and operational state to defaults.
        /// </summary>
        public void ResetMotor()
        {
            targetThrottle = 0f;
            currentRPM = 0f;
            motorTemperatureCelsius = 25f;
            isOperational = true;
        }
    }
}





