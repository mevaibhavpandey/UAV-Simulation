//-----------------------------------------------------------------------
// <copyright file="DroneCore.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using ASTRA.UAV.Physics;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Central quadcopter aggregator MonoBehaviour that binds together flight control, navigation,
    /// sensor telemetry, power management, physics model, and motor sub-systems into a unified UAV system.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DroneCore : MonoBehaviour
    {
        [Header("Sub-System Aggregation References")]
        [SerializeField, Tooltip("Flight controller module implementing axis PID mixing.")]
        private DroneFlightController flightController;

        [SerializeField, Tooltip("Autonomous guidance and waypoint navigator.")]
        private DroneNavigator navigator;

        [SerializeField, Tooltip("Simulated GPS satellite positioning sensor.")]
        private DroneGPS gpsSensor;

        [SerializeField, Tooltip("Simulated 6/9-DOF IMU sensor.")]
        private DroneIMU imuSensor;

        [SerializeField, Tooltip("Modular hardware component registry.")]
        private DroneComponentRegistry componentRegistry;

        [SerializeField, Tooltip("Physics aerodynamics and motor force calculator.")]
        private DronePhysicsModel physicsModel;

        [SerializeField, Tooltip("Four quadcopter rotor motor components (FL, FR, RR, RL).")]
        private DroneMotor[] motors = new DroneMotor[4];

        [Header("Battery & Power Simulation")]
        [SerializeField, Tooltip("Nominal LiPo battery pack capacity in milliamp-hours (mAh).")]
        private float batteryCapacitymAh = 5000f;

        [SerializeField, Tooltip("Fully charged battery voltage in Volts (e.g. 4S = 16.8V).")]
        private float maxBatteryVoltage = 16.8f;

        [SerializeField, Tooltip("Minimum cutoff battery voltage in Volts (e.g. 3.3V per cell).")]
        private float minBatteryVoltage = 13.2f;

        [Header("Live Operational Telemetry")]
        [SerializeField]
        private DroneArmState armState = DroneArmState.Disarmed;

        [SerializeField]
        private float remainingBatterymAh;

        [SerializeField]
        private float currentVoltage;

        private Rigidbody droneRigidbody;
        private Vector3 previousVelocity;
        private Vector3 currentAcceleration;

        /// <summary>
        /// Gets the current arming state of the quadcopter platform.
        /// </summary>
        public DroneArmState ArmState => armState;

        /// <summary>
        /// Gets the active flight controller instance.
        /// </summary>
        public IDroneController Controller => flightController;

        /// <summary>
        /// Gets the autonomous waypoint navigator module.
        /// </summary>
        public DroneNavigator Navigator => navigator;

        /// <summary>
        /// Gets the onboard simulated GPS sensor.
        /// </summary>
        public DroneGPS GPS => gpsSensor;

        /// <summary>
        /// Gets the onboard simulated IMU sensor.
        /// </summary>
        public DroneIMU IMU => imuSensor;

        /// <summary>
        /// Gets the hardware component registry.
        /// </summary>
        public DroneComponentRegistry ComponentRegistry => componentRegistry;

        /// <summary>
        /// Gets the quadcopter physics model.
        /// </summary>
        public DronePhysicsModel PhysicsModel => physicsModel;

        /// <summary>
        /// Gets the battery capacity remaining percentage [0.0 - 100.0%].
        /// </summary>
        public float BatteryPercentage => Mathf.Clamp01(remainingBatterymAh / Mathf.Max(1f, batteryCapacitymAh)) * 100f;

        /// <summary>
        /// Fired when the arm state changes.
        /// </summary>
        public event Action<DroneArmState> OnArmStateChanged;

        /// <summary>
        /// Fired on every frame with updated flight state telemetry.
        /// </summary>
        public event Action<DroneFlightStateData> OnTelemetryUpdated;

        /// <summary>
        /// Fired when battery level drops below safety threshold (20%).
        /// </summary>
        public event Action<float> OnBatteryWarning;

        /// <summary>
        /// Unity Awake lifecycle callback. Auto-binds components if unassigned.
        /// </summary>
        private void Awake()
        {
            droneRigidbody = GetComponent<Rigidbody>();
            remainingBatterymAh = batteryCapacitymAh;
            currentVoltage = maxBatteryVoltage;

            EnsureComponentReferences();
        }

        /// <summary>
        /// Auto-binds missing component references from child or sibling GameObjects.
        /// </summary>
        private void EnsureComponentReferences()
        {
            if (flightController == null) flightController = GetComponent<DroneFlightController>();
            if (navigator == null) navigator = GetComponent<DroneNavigator>();
            if (gpsSensor == null) gpsSensor = GetComponent<DroneGPS>();
            if (imuSensor == null) imuSensor = GetComponent<DroneIMU>();
            if (componentRegistry == null) componentRegistry = GetComponent<DroneComponentRegistry>();
            if (physicsModel == null) physicsModel = GetComponent<DronePhysicsModel>();

            if (motors == null || motors.Length < 4)
            {
                motors = GetComponentsInChildren<DroneMotor>();
            }
        }

        /// <summary>
        /// Unity Start lifecycle callback.
        /// </summary>
        private void Start()
        {
            Disarm();
        }

        /// <summary>
        /// Initiates pre-arm safety checks and arms the quadcopter if safety checks pass.
        /// </summary>
        /// <returns>True if drone successfully armed, false if pre-arm safety checks failed.</returns>
        public bool Arm()
        {
            if (armState == DroneArmState.Armed) return true;

            // Pre-Arm Safety Checks:
            // 1. Battery must be > 15%
            if (BatteryPercentage < 15f)
            {
                Debug.LogWarning("[DroneCore] Arming failed: Battery level critically low.");
                SetArmState(DroneArmState.ArmingError);
                return false;
            }

            // 2. Throttle must be zero
            if (flightController != null && flightController.ThrottleInput > 0.05f)
            {
                Debug.LogWarning("[DroneCore] Arming failed: Throttle input must be at zero.");
                SetArmState(DroneArmState.ArmingError);
                return false;
            }

            SetArmState(DroneArmState.Arming);

            if (flightController != null)
            {
                flightController.SetActive(true);
            }

            SetArmState(DroneArmState.Armed);
            Debug.Log("[DroneCore] System ARMED successfully.");
            return true;
        }

        /// <summary>
        /// Disarms the quadcopter platform immediately and cuts motor throttle.
        /// </summary>
        public void Disarm()
        {
            SetArmState(DroneArmState.Disarming);

            if (flightController != null)
            {
                flightController.ResetInputs();
                flightController.SetActive(false);
            }

            if (motors != null)
            {
                foreach (var motor in motors)
                {
                    if (motor != null) motor.SetThrottleInput(0f);
                }
            }

            SetArmState(DroneArmState.Disarmed);
            Debug.Log("[DroneCore] System DISARMED.");
        }

        /// <summary>
        /// Unity Update lifecycle callback for telemetry and autonomy state execution.
        /// </summary>
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            // Update Autonomous Navigation vector if in Autonomous mode
            if (armState == DroneArmState.Armed && flightController != null && navigator != null)
            {
                if (flightController.CurrentFlightMode == FlightMode.AutonomousWaypoint)
                {
                    navigator.CalculateGuidanceCommands(
                        droneRigidbody.position,
                        droneRigidbody.linearVelocity,
                        deltaTime,
                        out float navPitch, out float navRoll, out float navYaw, out float navThrottle
                    );

                    flightController.SetControlInputs(navPitch, navRoll, navYaw, navThrottle);
                }
            }

            // Execute Motor Output Distribution
            if (armState == DroneArmState.Armed && flightController != null && motors != null && motors.Length >= 4)
            {
                float[] outputs = flightController.CalculateMotorMixingOutputs(droneRigidbody.rotation, droneRigidbody.angularVelocity, deltaTime);
                for (int i = 0; i < 4; i++)
                {
                    if (motors[i] != null)
                    {
                        motors[i].SetThrottleInput(outputs[i]);
                        motors[i].UpdateMotorDynamics(deltaTime);
                    }
                }
            }

            // Power Drain Simulation
            SimulatePowerConsumption(deltaTime);

            // Process Sensor Data Updates
            ProcessSensors(deltaTime);

            // Broadcast Flight Telemetry Snapshot
            BroadcastTelemetry();
        }

        /// <summary>
        /// Unity FixedUpdate lifecycle callback for physical acceleration calculation.
        /// </summary>
        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            if (fixedDeltaTime <= 0f) return;

            Vector3 currentVelocity = droneRigidbody.linearVelocity;
            currentAcceleration = (currentVelocity - previousVelocity) / fixedDeltaTime;
            previousVelocity = currentVelocity;
        }

        /// <summary>
        /// Simulates battery capacity drain based on motor loads and hardware registry draw.
        /// </summary>
        private void SimulatePowerConsumption(float deltaTime)
        {
            if (remainingBatterymAh <= 0f) return;

            float motorAmps = 0f;
            if (motors != null)
            {
                foreach (var motor in motors)
                {
                    if (motor != null) motorAmps += motor.NormalizedThrust * 15.0f; // Max 15A per motor
                }
            }

            float componentWatts = (componentRegistry != null) ? componentRegistry.GetTotalPowerConsumptionWatts() : 15.0f;
            float componentAmps = componentWatts / Mathf.Max(1f, currentVoltage);

            float totalAmps = motorAmps + componentAmps;
            float consumedmAh = (totalAmps * 1000f) * (deltaTime / 3600f);

            remainingBatterymAh = Mathf.Max(0f, remainingBatterymAh - consumedmAh);
            float batteryRatio = BatteryPercentage / 100f;
            currentVoltage = Mathf.Lerp(minBatteryVoltage, maxBatteryVoltage, batteryRatio);

            if (BatteryPercentage <= 20f)
            {
                OnBatteryWarning?.Invoke(BatteryPercentage);
            }

            if (remainingBatterymAh <= 0f && armState == DroneArmState.Armed)
            {
                Debug.LogError("[DroneCore] Battery completely depleted! Initiating forced disarm.");
                Disarm();
            }
        }

        /// <summary>
        /// Updates simulated onboard sensors with true rigid body state.
        /// </summary>
        private void ProcessSensors(float deltaTime)
        {
            if (gpsSensor != null)
            {
                gpsSensor.ProcessSensorSimulation(droneRigidbody.position, droneRigidbody.linearVelocity, deltaTime);
            }

            if (imuSensor != null)
            {
                imuSensor.ProcessSensorSimulation(currentAcceleration, droneRigidbody.angularVelocity, droneRigidbody.rotation, deltaTime);
            }
        }

        /// <summary>
        /// Constructs and broadcasts instantaneous telemetry snapshot event.
        /// </summary>
        private void BroadcastTelemetry()
        {
            DroneFlightStateData stateData = new DroneFlightStateData
            {
                ArmState = armState,
                CurrentFlightMode = (flightController != null) ? flightController.CurrentFlightMode : FlightMode.Disarmed,
                Position = droneRigidbody.position,
                Velocity = droneRigidbody.linearVelocity,
                Acceleration = currentAcceleration,
                Orientation = droneRigidbody.rotation,
                AngularVelocity = droneRigidbody.angularVelocity,
                AltitudeAGL = droneRigidbody.position.y,
                AltitudeMSL = (gpsSensor != null) ? gpsSensor.CurrentData.AltitudeMSL : droneRigidbody.position.y + 50f,
                BatteryPercentage = BatteryPercentage,
                BatteryVoltage = currentVoltage,
                IsGrounded = Mathf.Abs(droneRigidbody.position.y) < 0.1f,
                Timestamp = Time.doubleValue
            };

            OnTelemetryUpdated?.Invoke(stateData);
        }

        /// <summary>
        /// Changes internal arm state and dispatches events.
        /// </summary>
        private void SetArmState(DroneArmState newState)
        {
            if (armState == newState) return;
            armState = newState;
            OnArmStateChanged?.Invoke(armState);
        }
    }
}
