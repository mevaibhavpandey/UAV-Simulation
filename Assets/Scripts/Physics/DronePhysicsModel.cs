//-----------------------------------------------------------------------
// <copyright file="DronePhysicsModel.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using ASTRA.UAV.Drone;

namespace ASTRA.UAV.Physics
{
    /// <summary>
    /// Computes full 6-DOF quadcopter dynamics physics including rotor thrust forces,
    /// differential roll/pitch/yaw torque mixing, aerodynamic drag, and ground effect cushions,
    /// applying forces directly to the underlying Unity Rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DronePhysicsModel : MonoBehaviour
    {
        [Header("Frame Mass & Inertia Properties")]
        [SerializeField, Tooltip("Total frame mass in kilograms (kg).")]
        private float frameMassKg = 1.8f;

        [SerializeField, Tooltip("Rotor arm distance from center of mass in meters.")]
        private float armLengthMeters = 0.225f;

        [SerializeField, Tooltip("Moment of inertia vector (Ixx, Iyy, Izz) in kg*m².")]
        private Vector3 momentOfInertia = new Vector3(0.015f, 0.015f, 0.025f);

        [SerializeField, Tooltip("Center of mass offset relative to transform origin.")]
        private Vector3 centerOfMassOffset = Vector3.zero;

        [Header("Physics Sub-Models")]
        [SerializeField, Tooltip("Motor dynamics sub-model for spool-up lag and thermal limits.")]
        private MotorDynamics motorDynamics = new MotorDynamics();

        [SerializeField, Tooltip("Aerodynamics model for wind, drag, and ground effect physics.")]
        private AerodynamicsModel aerodynamicsModel = new AerodynamicsModel();

        [Header("Live Physics Diagnostics")]
        [SerializeField] private float totalThrustForceN = 0f;
        [SerializeField] private Vector3 totalTorqueNm = Vector3.zero;

        private Rigidbody droneRigidbody;
        private DroneMotor[] motorComponents = new DroneMotor[4];

        /// <summary>
        /// Gets the active aerodynamics simulation model.
        /// </summary>
        public AerodynamicsModel Aerodynamics => aerodynamicsModel;

        /// <summary>
        /// Gets the active motor dynamics model.
        /// </summary>
        public MotorDynamics Dynamics => motorDynamics;

        /// <summary>
        /// Gets the instantaneous combined thrust output across all rotors in Newtons.
        /// </summary>
        public float TotalThrustForceN => totalThrustForceN;

        /// <summary>
        /// Gets the instantaneous net torque vector acting on the frame in N*m.
        /// </summary>
        public Vector3 TotalTorqueNm => totalTorqueNm;

        /// <summary>
        /// Unity Awake initialization. Configures Rigidbody mass and inertia properties.
        /// </summary>
        private void Awake()
        {
            droneRigidbody = GetComponent<Rigidbody>();
            ConfigureRigidbody();
            motorComponents = GetComponentsInChildren<DroneMotor>();
        }

        /// <summary>
        /// Configures Unity Rigidbody properties matching real physical frame specifications.
        /// </summary>
        public void ConfigureRigidbody()
        {
            if (droneRigidbody == null) return;

            droneRigidbody.mass = Mathf.Max(0.1f, frameMassKg);
            droneRigidbody.centerOfMass = centerOfMassOffset;
            droneRigidbody.inertiaTensor = momentOfInertia;
            droneRigidbody.useGravity = true;
        }

        /// <summary>
        /// Unity FixedUpdate lifecycle callback for rigid body physics simulation step.
        /// </summary>
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            if (deltaTime <= 0f) return;

            EvaluateQuadcopterPhysics(deltaTime);
        }

        /// <summary>
        /// Evaluates net forces and torques acting on quadcopter and applies them to Rigidbody.
        /// </summary>
        /// <param name="deltaTime">Physics time step step duration in seconds.</param>
        public void EvaluateQuadcopterPhysics(float deltaTime)
        {
            if (droneRigidbody == null) return;

            totalThrustForceN = 0f;
            totalTorqueNm = Vector3.zero;

            // Calculate ground effect coefficient based on current height AGL
            float altitudeAGL = Mathf.Max(0.01f, transform.position.y);
            float groundEffectMultiplier = aerodynamicsModel.CalculateGroundEffectMultiplier(altitudeAGL, 0.127f); // 5-inch prop radius ~0.127m

            // Rotor offset positions relative to body center in X-configuration (+45 deg arm offsets)
            Vector3[] motorOffsets = new Vector3[]
            {
                new Vector3(-armLengthMeters * 0.7071f, 0f,  armLengthMeters * 0.7071f), // Motor 0: Front-Left
                new Vector3( armLengthMeters * 0.7071f, 0f,  armLengthMeters * 0.7071f), // Motor 1: Front-Right
                new Vector3( armLengthMeters * 0.7071f, 0f, -armLengthMeters * 0.7071f), // Motor 2: Rear-Right
                new Vector3(-armLengthMeters * 0.7071f, 0f, -armLengthMeters * 0.7071f)  // Motor 3: Rear-Left
            };

            // Evaluate motor forces
            if (motorComponents != null && motorComponents.Length >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (motorComponents[i] == null) continue;

                    float rawThrust = motorComponents[i].CalculateThrustForce();
                    float effectiveThrust = rawThrust * groundEffectMultiplier;
                    totalThrustForceN += effectiveThrust;

                    // Rotor local thrust force vector (pointing UP along transform.up)
                    Vector3 thrustForceVectorWorld = transform.up * effectiveThrust;
                    Vector3 motorPositionWorld = transform.TransformPoint(motorOffsets[i]);

                    // Apply thrust force at motor position (produces pitch/roll torque automatically)
                    droneRigidbody.AddForceAtPosition(thrustForceVectorWorld, motorPositionWorld, ForceMode.Force);

                    // Rotor counter-torque reaction vector
                    Vector3 motorTorqueLocal = motorComponents[i].CalculateTorqueVector();
                    Vector3 motorTorqueWorld = transform.TransformDirection(motorTorqueLocal);
                    droneRigidbody.AddTorque(motorTorqueWorld, ForceMode.Force);

                    totalTorqueNm += motorTorqueWorld;
                }
            }

            // Evaluate Aerodynamic Translational Drag
            Vector3 currentWind = aerodynamicsModel.CalculateWindVector(transform.position, Time.time);
            Vector3 dragForce = aerodynamicsModel.CalculateTranslationalDrag(droneRigidbody.linearVelocity, currentWind);
            droneRigidbody.AddForce(dragForce, ForceMode.Force);

            // Evaluate Aerodynamic Angular Damping Torque
            Vector3 rotationalDamping = aerodynamicsModel.CalculateRotationalDrag(droneRigidbody.angularVelocity);
            droneRigidbody.AddTorque(rotationalDamping, ForceMode.Force);
            totalTorqueNm += rotationalDamping;
        }
    }
}




