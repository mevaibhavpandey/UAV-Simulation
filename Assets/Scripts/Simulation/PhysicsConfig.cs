using UnityEngine;

namespace ASTRA.UAV.Simulation
{
    /// <summary>
    /// ScriptableObject defining physical vehicle dynamics, mass properties, aerodynamic drag, motor characteristics, and thrust coefficients.
    /// </summary>
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "ASTRA/UAV/Physics Config", order = 11)]
    public class PhysicsConfig : ScriptableObject
    {
        [Header("Mass & Inertia")]
        [Tooltip("Total mass of UAV vehicle including battery and payload in kilograms.")]
        [Range(0.1f, 50f)]
        [SerializeField] private float massKg = 1.5f;

        [Tooltip("Diagonal moment of inertia tensor (Ix, Iy, Iz) in kg*m^2.")]
        [SerializeField] private Vector3 inertiaTensor = new Vector3(0.02f, 0.02f, 0.04f);

        [Header("Aerodynamics")]
        [Tooltip("Aerodynamic drag coefficient (Cd).")]
        [Range(0.01f, 1.5f)]
        [SerializeField] private float dragCoefficient = 0.35f;

        [Tooltip("Frontal cross-sectional surface area in square meters.")]
        [Range(0.01f, 2.0f)]
        [SerializeField] private float crossSectionalAreaSquareMeters = 0.12f;

        [Header("Propulsion System")]
        [Tooltip("Number of electric motors/rotors.")]
        [Range(1, 12)]
        [SerializeField] private int motorCount = 4;

        [Tooltip("Maximum achievable motor rotational velocity in RPM.")]
        [Range(1000f, 30000f)]
        [SerializeField] private float maxMotorRPM = 12000f;

        [Tooltip("Rotor thrust coefficient (Ct) converting RPM^2 to thrust force (N).")]
        [SerializeField] private float motorThrustCoefficient = 1.2e-5f;

        [Tooltip("Rotor torque coefficient (Cq) converting RPM^2 to reaction torque (N*m).")]
        [SerializeField] private float motorTorqueCoefficient = 2.5e-7f;

        [Tooltip("First-order motor spool response time constant in seconds.")]
        [Range(0.005f, 0.5f)]
        [SerializeField] private float motorTimeConstant = 0.04f;

        [Header("Flight Limits")]
        [Tooltip("Maximum allowed pitch/roll tilt angle in degrees.")]
        [Range(10f, 60f)]
        [SerializeField] private float maxTiltAngleDegrees = 35.0f;

        /// <summary>Gets mass in kilograms.</summary>
        public float MassKg => massKg;

        /// <summary>Gets diagonal inertia tensor.</summary>
        public Vector3 InertiaTensor => inertiaTensor;

        /// <summary>Gets drag coefficient.</summary>
        public float DragCoefficient => dragCoefficient;

        /// <summary>Gets cross-sectional surface area.</summary>
        public float CrossSectionalAreaSquareMeters => crossSectionalAreaSquareMeters;

        /// <summary>Gets number of propulsion motors.</summary>
        public int MotorCount => motorCount;

        /// <summary>Gets max RPM per motor.</summary>
        public float MaxMotorRPM => maxMotorRPM;

        /// <summary>Gets thrust coefficient Ct.</summary>
        public float MotorThrustCoefficient => motorThrustCoefficient;

        /// <summary>Gets torque coefficient Cq.</summary>
        public float MotorTorqueCoefficient => motorTorqueCoefficient;

        /// <summary>Gets motor time constant.</summary>
        public float MotorTimeConstant => motorTimeConstant;

        /// <summary>Gets maximum allowed tilt angle in degrees.</summary>
        public float MaxTiltAngleDegrees => maxTiltAngleDegrees;

        /// <summary>
        /// Calculates the maximum theoretical total vertical thrust force in Newtons across all motors.
        /// </summary>
        /// <returns>Max thrust in Newtons.</returns>
        public float CalculateMaxTotalThrust()
        {
            return motorCount * motorThrustCoefficient * (maxMotorRPM * maxMotorRPM);
        }
    }
}





