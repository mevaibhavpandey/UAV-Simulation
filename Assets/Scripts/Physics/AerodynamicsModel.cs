//-----------------------------------------------------------------------
// <copyright file="AerodynamicsModel.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Physics
{
    /// <summary>
    /// Evaluates aerodynamic forces acting on the UAV platform including wind gusts,
    /// translational parasite drag, angular damping, and ground effect thrust augmentation.
    /// </summary>
    [Serializable]
    public class AerodynamicsModel
    {
        [Header("Environmental Properties")]
        [SerializeField, Tooltip("Air density rho in kg/m³ at sea level (standard atmosphere ~1.225).")]
        private float airDensityKgM3 = 1.225f;

        [SerializeField, Tooltip("Base constant global wind velocity vector in m/s.")]
        private Vector3 baseWindVelocity = new Vector3(2.0f, 0.0f, 1.0f);

        [SerializeField, Tooltip("Turbulence intensity scaling factor for wind gusts.")]
        private float turbulenceIntensity = 1.5f;

        [Header("Aerodynamic Drag Coefficients")]
        [SerializeField, Tooltip("Frontal / lateral drag coefficient (C_d).")]
        private float translationalDragCoefficient = 0.45f;

        [SerializeField, Tooltip("Effective frontal cross-sectional area of quadcopter frame in m².")]
        private float frontalAreaM2 = 0.08f;

        [SerializeField, Tooltip("Angular rotational drag coefficient factor.")]
        private float rotationalDragCoefficient = 0.02f;

        [Header("Ground Effect Physics")]
        [SerializeField, Tooltip("Maximum thrust multiplier ratio achieved in close ground proximity.")]
        private float maxGroundEffectMultiplier = 1.25f;

        /// <summary>
        /// Gets or sets the base environmental wind velocity vector in m/s.
        /// </summary>
        public Vector3 BaseWindVelocity
        {
            get => baseWindVelocity;
            set => baseWindVelocity = value;
        }

        /// <summary>
        /// Computes effective environmental wind vector at a specific spatial location including turbulence.
        /// </summary>
        /// <param name="position">Spatial world position in meters.</param>
        /// <param name="time">Current simulation time in seconds.</param>
        /// <returns>Combined wind velocity vector in m/s.</returns>
        public Vector3 CalculateWindVector(Vector3 position, float time)
        {
            float noiseX = (Mathf.PerlinNoise(position.x * 0.1f + time * 0.5f, position.z * 0.1f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(position.y * 0.1f, time * 0.3f) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(position.z * 0.1f, position.x * 0.1f + time * 0.5f) - 0.5f) * 2f;

            Vector3 gust = new Vector3(noiseX, noiseY, noiseZ) * turbulenceIntensity;
            return baseWindVelocity + gust;
        }

        /// <summary>
        /// Calculates aerodynamic translational drag force opposing relative airspeed motion.
        /// Formula: F_drag = 0.5 * rho * C_d * Area * |V_rel| * V_rel
        /// </summary>
        /// <param name="droneVelocity">True velocity of quadcopter frame in world space (m/s).</param>
        /// <param name="windVector">Environmental wind vector in world space (m/s).</param>
        /// <returns>Translational drag force vector in Newtons (N).</returns>
        public Vector3 CalculateTranslationalDrag(Vector3 droneVelocity, Vector3 windVector)
        {
            // Relative air velocity vector
            Vector3 relativeAirVelocity = droneVelocity - windVector;
            float speedSqr = relativeAirVelocity.sqrMagnitude;
            if (speedSqr < 0.0001f) return Vector3.zero;

            float speed = Mathf.Sqrt(speedSqr);
            float dragMagnitude = 0.5f * airDensityKgM3 * translationalDragCoefficient * frontalAreaM2 * speedSqr;

            // Drag opposes direction of relative motion
            return -relativeAirVelocity.normalized * dragMagnitude;
        }

        /// <summary>
        /// Calculates rotational aerodynamic drag torque resisting rapid body rotation.
        /// </summary>
        /// <param name="angularVelocity">Angular rate vector in radians/sec.</param>
        /// <returns>Rotational damping torque vector in N*m.</returns>
        public Vector3 CalculateRotationalDrag(Vector3 angularVelocity)
        {
            float sqrMag = angularVelocity.sqrMagnitude;
            if (sqrMag < 0.0001f) return Vector3.zero;

            return -angularVelocity.normalized * (rotationalDragCoefficient * sqrMag);
        }

        /// <summary>
        /// Calculates ground effect thrust amplification ratio when operating close to ground surface.
        /// Based on Cheeseman and Bennett ground effect model: T_ge / T_inf = 1 / (1 - (R / 4h)^2)
        /// </summary>
        /// <param name="altitudeAGL">Altitude Above Ground Level in meters.</param>
        /// <param name="rotorRadiusMeters">Rotor propeller radius in meters.</param>
        /// <returns>Thrust scaling multiplier bounded between 1.0 and maxGroundEffectMultiplier.</returns>
        public float CalculateGroundEffectMultiplier(float altitudeAGL, float rotorRadiusMeters)
        {
            if (altitudeAGL <= 0.05f) return maxGroundEffectMultiplier;

            // Ground effect is negligible above 2.0 * rotor diameter (4.0 * radius)
            float maxEffectAltitude = 4.0f * rotorRadiusMeters;
            if (altitudeAGL >= maxEffectAltitude) return 1.0f;

            float ratio = rotorRadiusMeters / (4.0f * altitudeAGL);
            float multiplier = 1.0f / (1.0f - (ratio * ratio));
            return Mathf.Clamp(multiplier, 1.0f, maxGroundEffectMultiplier);
        }
    }
}




