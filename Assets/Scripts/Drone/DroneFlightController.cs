//-----------------------------------------------------------------------
// <copyright file="DroneFlightController.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Flight controller implementation handling pilot input mixing, axis rate regulation,
    /// PID control feedback loops, and motor command distributions for quadcopters.
    /// </summary>
    public class DroneFlightController : MonoBehaviour, IDroneController
    {
        [Header("Control Axes Input Raw")]
        [SerializeField, Range(-1f, 1f), Tooltip("Roll axis input [-1.0 Left, +1.0 Right].")]
        private float rollInput = 0f;

        [SerializeField, Range(-1f, 1f), Tooltip("Pitch axis input [-1.0 Back, +1.0 Forward].")]
        private float pitchInput = 0f;

        [SerializeField, Range(-1f, 1f), Tooltip("Yaw axis input [-1.0 CCW, +1.0 CW].")]
        private float yawInput = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("Throttle axis input [0.0 Min, 1.0 Max].")]
        private float throttleInput = 0f;

        [Header("Flight Parameters & Limits")]
        [SerializeField, Tooltip("Maximum achievable bank angle in degrees during Angle flight mode.")]
        private float maxBankAngleDegrees = 45f;

        [SerializeField, Tooltip("Maximum target yaw rotation rate in degrees per second.")]
        private float maxYawRateDegPerSec = 180f;

        [SerializeField, Tooltip("Control input deadzone threshold to suppress stick drift.")]
        private float inputDeadzone = 0.05f;

        [SerializeField, Tooltip("Input expo factor for smoothing stick responsiveness near center.")]
        private float inputExpo = 0.2f;

        [Header("PID Controller Parameters")]
        [SerializeField, Tooltip("Proportional gain for pitch/roll angle control loop.")]
        private float kpAngle = 4.5f;

        [SerializeField, Tooltip("Proportional gain for angular rate control loop.")]
        private float kpRate = 0.15f;

        [SerializeField, Tooltip("Integral gain for rate control loop.")]
        private float kiRate = 0.05f;

        [SerializeField, Tooltip("Derivative gain for rate control loop.")]
        private float kdRate = 0.01f;

        // Internal PID State Tracking
        private Vector3 rateIntegralError = Vector3.zero;
        private Vector3 previousRateError = Vector3.zero;
        private bool isActive = false;
        private FlightMode currentFlightMode = FlightMode.Angle;

        /// <summary>
        /// Gets or sets the normalized roll control input [-1.0, 1.0].
        /// </summary>
        public float RollInput
        {
            get => rollInput;
            set => rollInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f);
        }

        /// <summary>
        /// Gets or sets the normalized pitch control input [-1.0, 1.0].
        /// </summary>
        public float PitchInput
        {
            get => pitchInput;
            set => pitchInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f);
        }

        /// <summary>
        /// Gets or sets the normalized yaw control input [-1.0, 1.0].
        /// </summary>
        public float YawInput
        {
            get => yawInput;
            set => yawInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f);
        }

        /// <summary>
        /// Gets or sets the normalized throttle control input [0.0, 1.0].
        /// </summary>
        public float ThrottleInput
        {
            get => throttleInput;
            set => throttleInput = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Gets a value indicating whether the flight controller is actively executing control logic.
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Gets the current operational flight mode.
        /// </summary>
        public FlightMode CurrentFlightMode => currentFlightMode;

        /// <summary>
        /// Event fired whenever flight control axis inputs are modified.
        /// </summary>
        public event Action<float, float, float, float> OnControlInputsChanged;

        /// <summary>
        /// Event fired whenever the flight mode transitions.
        /// </summary>
        public event Action<FlightMode> OnFlightModeChanged;

        /// <summary>
        /// Activates or deactivates the flight control processing pipeline.
        /// </summary>
        /// <param name="active">True to enable flight control calculations, false to disable.</param>
        public void SetActive(bool active)
        {
            isActive = active;
            if (!isActive)
            {
                ResetInputs();
                ResetPIDState();
            }
        }

        /// <summary>
        /// Switches the active quadcopter flight mode.
        /// </summary>
        /// <param name="newMode">Target flight mode to activate.</param>
        public void SetFlightMode(FlightMode newMode)
        {
            if (currentFlightMode == newMode) return;
            currentFlightMode = newMode;
            ResetPIDState();
            OnFlightModeChanged?.Invoke(currentFlightMode);
        }

        /// <summary>
        /// Sets all flight control axis values simultaneously with safety checks and notifications.
        /// </summary>
        /// <param name="pitch">Pitch input [-1.0, 1.0].</param>
        /// <param name="roll">Roll input [-1.0, 1.0].</param>
        /// <param name="yaw">Yaw input [-1.0, 1.0].</param>
        /// <param name="throttle">Throttle input [0.0, 1.0].</param>
        public void SetControlInputs(float pitch, float roll, float yaw, float throttle)
        {
            PitchInput = pitch;
            RollInput = roll;
            YawInput = yaw;
            ThrottleInput = throttle;

            OnControlInputsChanged?.Invoke(pitchInput, rollInput, yawInput, throttleInput);
        }

        /// <summary>
        /// Resets all control inputs back to zero/baseline settings.
        /// </summary>
        public void ResetInputs()
        {
            rollInput = 0f;
            pitchInput = 0f;
            yawInput = 0f;
            throttleInput = 0f;
        }

        /// <summary>
        /// Calculates individual motor throttle outputs [0.0 - 1.0] for standard X quadcopter geometry.
        /// Array indices correspond to: 0 = Front-Left, 1 = Front-Right, 2 = Rear-Right, 3 = Rear-Left.
        /// </summary>
        /// <param name="currentAttitude">Current estimated quadcopter attitude quaternion.</param>
        /// <param name="currentAngularVelocity">Current angular rate vector in deg/sec.</param>
        /// <param name="deltaTime">Physics update step duration in seconds.</param>
        /// <returns>Array of 4 normalized throttle values for quadcopter rotors.</returns>
        public float[] CalculateMotorMixingOutputs(Quaternion currentAttitude, Vector3 currentAngularVelocity, float deltaTime)
        {
            float[] outputs = new float[4];
            if (!isActive || throttleInput <= 0.01f)
            {
                return outputs;
            }

            Vector3 torqueCorrection = CalculatePIDTorque(currentAttitude, currentAngularVelocity, deltaTime);

            float rollCorrection = torqueCorrection.x;
            float pitchCorrection = torqueCorrection.y;
            float yawCorrection = torqueCorrection.z;

            // X-Quadcopter Motor Output Mixing Formula:
            // Motor 0 (FL - CW):  Throttle + Pitch - Roll + Yaw
            // Motor 1 (FR - CCW): Throttle + Pitch + Roll - Yaw
            // Motor 2 (RR - CW):  Throttle - Pitch + Roll + Yaw
            // Motor 3 (RL - CCW): Throttle - Pitch - Roll - Yaw

            outputs[0] = Mathf.Clamp01(throttleInput + pitchCorrection - rollCorrection + yawCorrection);
            outputs[1] = Mathf.Clamp01(throttleInput + pitchCorrection + rollCorrection - yawCorrection);
            outputs[2] = Mathf.Clamp01(throttleInput - pitchCorrection + rollCorrection + yawCorrection);
            outputs[3] = Mathf.Clamp01(throttleInput - pitchCorrection - rollCorrection - yawCorrection);

            return outputs;
        }

        /// <summary>
        /// Computes PID torque correction feedback signals across pitch, roll, and yaw axes.
        /// </summary>
        private Vector3 CalculatePIDTorque(Quaternion currentAttitude, Vector3 currentAngularVelocity, float deltaTime)
        {
            if (deltaTime <= 0.0001f) return Vector3.zero;

            Vector3 targetAngularRates = Vector3.zero;

            if (currentFlightMode == FlightMode.Angle)
            {
                // Angle Mode: Convert stick position to target bank angles
                float targetRollAngle = ApplyExpo(rollInput) * maxBankAngleDegrees;
                float targetPitchAngle = ApplyExpo(pitchInput) * maxBankAngleDegrees;

                // Extract current Roll and Pitch angles from quaternion
                Vector3 currentEuler = currentAttitude.eulerAngles;
                float currentRoll = NormalizeAngle(currentEuler.z);
                float currentPitch = NormalizeAngle(currentEuler.x);

                // Angle controller error
                float errorRoll = targetRollAngle - currentRoll;
                float errorPitch = targetPitchAngle - currentPitch;

                targetAngularRates.x = errorRoll * kpAngle;
                targetAngularRates.y = errorPitch * kpAngle;
                targetAngularRates.z = ApplyExpo(yawInput) * maxYawRateDegPerSec;
            }
            else
            {
                // Acro Mode: Direct stick rate control
                targetAngularRates.x = ApplyExpo(rollInput) * maxBankAngleDegrees * 4f;
                targetAngularRates.y = ApplyExpo(pitchInput) * maxBankAngleDegrees * 4f;
                targetAngularRates.z = ApplyExpo(yawInput) * maxYawRateDegPerSec;
            }

            // Calculate rate loop error
            Vector3 rateError = targetAngularRates - currentAngularVelocity;
            rateIntegralError += rateError * deltaTime;
            rateIntegralError = Vector3.ClampMagnitude(rateIntegralError, 10f); // Anti-windup limit

            Vector3 rateDerivative = (rateError - previousRateError) / deltaTime;
            previousRateError = rateError;

            // PID torque output sum
            Vector3 pidOutput = (rateError * kpRate) + (rateIntegralError * kiRate) + (rateDerivative * kdRate);
            return pidOutput;
        }

        /// <summary>
        /// Resets internal PID error accumulators.
        /// </summary>
        private void ResetPIDState()
        {
            rateIntegralError = Vector3.zero;
            previousRateError = Vector3.zero;
        }

        /// <summary>
        /// Applies stick deadzone logic to input values.
        /// </summary>
        private float ApplyDeadzone(float input)
        {
            if (Mathf.Abs(input) < inputDeadzone) return 0f;
            return Mathf.Sign(input) * ((Mathf.Abs(input) - inputDeadzone) / (1f - inputDeadzone));
        }

        /// <summary>
        /// Applies exponential curve mapping to smooth control input responsiveness.
        /// </summary>
        private float ApplyExpo(float input)
        {
            return input * (1f - inputExpo + inputExpo * input * input);
        }

        /// <summary>
        /// Normalizes Euler angle degrees into [-180, 180] domain.
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
