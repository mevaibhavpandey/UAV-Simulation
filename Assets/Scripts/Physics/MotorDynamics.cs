//-----------------------------------------------------------------------
// <copyright file="MotorDynamics.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Physics
{
    /// <summary>
    /// Simulates electrical and electromechanical motor dynamics including spool-up lag,
    /// back-EMF efficiency degradation, first-order time constants, and thermal derating factors.
    /// </summary>
    [Serializable]
    public class MotorDynamics
    {
        [Header("Motor Physical Properties")]
        [SerializeField, Tooltip("First-order response time constant tau in seconds.")]
        private float timeConstantTau = 0.04f;

        [SerializeField, Tooltip("Thermal derating threshold temperature in Celsius.")]
        private float thermalThresholdCelsius = 85f;

        [SerializeField, Tooltip("Maximum thermal derating power limit percentage [0.5 - 1.0].")]
        private float minThermalEfficiency = 0.7f;

        /// <summary>
        /// Gets or sets the motor time constant tau in seconds.
        /// </summary>
        public float TimeConstantTau
        {
            get => timeConstantTau;
            set => timeConstantTau = Mathf.Max(0.001f, value);
        }

        /// <summary>
        /// Computes updated motor spin rate RPM for the next time step given target RPM and first-order motor lag.
        /// </summary>
        /// <param name="currentRPM">Current instantaneous motor RPM.</param>
        /// <param name="targetRPM">Target desired motor RPM.</param>
        /// <param name="deltaTime">Time step duration in seconds.</param>
        /// <returns>Updated instantaneous motor RPM.</returns>
        public float ComputeNextRPM(float currentRPM, float targetRPM, float deltaTime)
        {
            if (deltaTime <= 0f) return currentRPM;

            // Exponential first-order dynamic filter: d(RPM)/dt = (target - current) / tau
            float alpha = 1.0f - Mathf.Exp(-deltaTime / Mathf.Max(0.001f, timeConstantTau));
            float newRPM = Mathf.Lerp(currentRPM, targetRPM, alpha);
            return Mathf.Max(0f, newRPM);
        }

        /// <summary>
        /// Calculates vertical thrust output force produced by a rotor spinning at specified RPM.
        /// Formula: F_thrust = K_t * RPM^2 * ThermalEfficiency
        /// </summary>
        /// <param name="rpm">Instantaneous motor rotational speed in RPM.</param>
        /// <param name="thrustCoefficient">Thrust constant K_t in N/(RPM^2).</param>
        /// <param name="temperatureCelsius">Current motor temperature in degrees Celsius.</param>
        /// <returns>Produced thrust magnitude in Newtons (N).</returns>
        public float CalculateThrustFromRPM(float rpm, float thrustCoefficient, float temperatureCelsius = 25f)
        {
            if (rpm <= 0f) return 0f;
            float thermalEfficiency = GetThermalEfficiency(temperatureCelsius);
            return thrustCoefficient * (rpm * rpm) * thermalEfficiency;
        }

        /// <summary>
        /// Calculates reaction counter-torque vector produced by propeller aerodynamic drag.
        /// Formula: Tau_drag = K_q * RPM^2 * spinDirection
        /// </summary>
        /// <param name="rpm">Instantaneous motor rotational speed in RPM.</param>
        /// <param name="torqueCoefficient">Torque constant K_q in N*m/(RPM^2).</param>
        /// <param name="spinDirection">Rotor rotation direction (+1 CW, -1 CCW).</param>
        /// <returns>Torque vector along vertical shaft axis in Newton-meters (N*m).</returns>
        public Vector3 CalculateTorqueFromRPM(float rpm, float torqueCoefficient, int spinDirection)
        {
            if (rpm <= 0f) return Vector3.zero;
            float torqueMagnitude = torqueCoefficient * (rpm * rpm) * Mathf.Sign(spinDirection);
            return new Vector3(0f, torqueMagnitude, 0f);
        }

        /// <summary>
        /// Calculates thermal derating factor based on current motor temperature.
        /// </summary>
        /// <param name="temperatureCelsius">Motor operating temperature in degrees Celsius.</param>
        /// <returns>Efficiency scale multiplier between minThermalEfficiency and 1.0.</returns>
        public float GetThermalEfficiency(float temperatureCelsius)
        {
            if (temperatureCelsius <= thermalThresholdCelsius) return 1.0f;

            float excessTemp = temperatureCelsius - thermalThresholdCelsius;
            float derating = excessTemp * 0.01f;
            return Mathf.Max(minThermalEfficiency, 1.0f - derating);
        }
    }
}


