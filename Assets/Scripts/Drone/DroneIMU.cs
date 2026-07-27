//-----------------------------------------------------------------------
// <copyright file="DroneIMU.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Telemetry container for 6-DOF / 9-DOF Inertial Measurement Unit (IMU) readings.
    /// </summary>
    [Serializable]
    public struct IMUData
    {
        /// <summary>3-Axis acceleration vector in body frame (m/s²), includes gravity component.</summary>
        public Vector3 Accelerometer;

        /// <summary>3-Axis angular velocity vector in body frame (rad/s).</summary>
        public Vector3 Gyroscope;

        /// <summary>3-Axis magnetic field vector in body frame (microTesla uT).</summary>
        public Vector3 Magnetometer;

        /// <summary>IMU die internal temperature in degrees Celsius.</summary>
        public float TemperatureCelsius;

        /// <summary>IMU timestamp in seconds.</summary>
        public double Timestamp;
    }

    /// <summary>
    /// Simulated Inertial Measurement Unit sensor provider simulating 3-axis accelerometer,
    /// 3-axis gyroscope, and 3-axis magnetometer readings with noise injection and bias drift models.
    /// </summary>
    public class DroneIMU : MonoBehaviour
    {
        [Header("Sensor Sampling Settings")]
        [SerializeField, Tooltip("IMU update sample rate frequency in Hertz (Hz).")]
        private float sampleRateHz = 200f;

        [Header("Noise Configuration")]
        [SerializeField, Tooltip("Accelerometer noise spectral density (m/s²).")]
        private float accelNoiseStdDev = 0.08f;

        [SerializeField, Tooltip("Gyroscope noise spectral density (rad/s).")]
        private float gyroNoiseStdDev = 0.005f;

        [SerializeField, Tooltip("Magnetometer noise spectral density (uT).")]
        private float magNoiseStdDev = 0.2f;

        [Header("Bias & Drift Parameters")]
        [SerializeField, Tooltip("Accelerometer static sensor bias offset vector.")]
        private Vector3 accelBias = Vector3.zero;

        [SerializeField, Tooltip("Gyroscope static sensor bias offset vector.")]
        private Vector3 gyroBias = Vector3.zero;

        [Header("Live IMU Data")]
        [SerializeField]
        private IMUData currentData;

        private float timeSinceLastSample = 0f;
        private SensorHealthStatus healthStatus = SensorHealthStatus.Healthy;

        /// <summary>
        /// Gets the current live IMU telemetry sample data.
        /// </summary>
        public IMUData CurrentData => currentData;

        /// <summary>
        /// Gets the operational health status of the IMU sensor.
        /// </summary>
        public SensorHealthStatus HealthStatus => healthStatus;

        /// <summary>
        /// Fired whenever a new IMU sample frame is computed.
        /// </summary>
        public event Action<IMUData> OnIMUUpdated;

        /// <summary>
        /// Performs gyroscope zero-bias calibration based on stationary assumption.
        /// </summary>
        public void CalibrateGyroscope()
        {
            gyroBias = new Vector3(
                GenerateGaussianNoise(0f, gyroNoiseStdDev * 0.1f),
                GenerateGaussianNoise(0f, gyroNoiseStdDev * 0.1f),
                GenerateGaussianNoise(0f, gyroNoiseStdDev * 0.1f)
            );
        }

        /// <summary>
        /// Performs accelerometer zero-g level calibration.
        /// </summary>
        public void CalibrateAccelerometer()
        {
            accelBias = Vector3.zero;
        }

        /// <summary>
        /// Processes and updates IMU simulated sensor channels from ground-truth physics state.
        /// </summary>
        /// <param name="trueLinearAccelWorld">True world space linear acceleration in m/s².</param>
        /// <param name="trueAngularVelocityBody">True body space angular velocity in rad/s.</param>
        /// <param name="trueOrientation">True orientation quaternion.</param>
        /// <param name="deltaTime">Time delta step in seconds.</param>
        public void ProcessSensorSimulation(Vector3 trueLinearAccelWorld, Vector3 trueAngularVelocityBody, Quaternion trueOrientation, float deltaTime)
        {
            if (healthStatus == SensorHealthStatus.Failed) return;

            timeSinceLastSample += deltaTime;
            float samplePeriod = 1.0f / Mathf.Max(1f, sampleRateHz);

            if (timeSinceLastSample < samplePeriod) return;
            timeSinceLastSample = 0f;

            // Include gravity vector (9.81 m/s² pointing down in world space)
            Vector3 gravityWorld = new Vector3(0f, 9.81f, 0f);
            Vector3 totalAccelWorld = trueLinearAccelWorld + gravityWorld;

            // Transform acceleration vector to body frame
            Quaternion inverseRot = Quaternion.Inverse(trueOrientation);
            Vector3 accelBody = inverseRot * totalAccelWorld;

            // Add Accelerometer noise & bias
            Vector3 noisyAccel = accelBody + accelBias + new Vector3(
                GenerateGaussianNoise(0f, accelNoiseStdDev),
                GenerateGaussianNoise(0f, accelNoiseStdDev),
                GenerateGaussianNoise(0f, accelNoiseStdDev)
            );

            // Add Gyroscope noise & bias
            Vector3 noisyGyro = trueAngularVelocityBody + gyroBias + new Vector3(
                GenerateGaussianNoise(0f, gyroNoiseStdDev),
                GenerateGaussianNoise(0f, gyroNoiseStdDev),
                GenerateGaussianNoise(0f, gyroNoiseStdDev)
            );

            // Calculate simulated Magnetometer vector (Magnetic North vector in body frame)
            Vector3 magneticNorthWorld = new Vector3(0f, 0.4f, 0.9f).normalized * 45f; // ~45 uT field
            Vector3 magBody = inverseRot * magneticNorthWorld + new Vector3(
                GenerateGaussianNoise(0f, magNoiseStdDev),
                GenerateGaussianNoise(0f, magNoiseStdDev),
                GenerateGaussianNoise(0f, magNoiseStdDev)
            );

            currentData = new IMUData
            {
                Accelerometer = noisyAccel,
                Gyroscope = noisyGyro,
                Magnetometer = magBody,
                TemperatureCelsius = 35f,
                Timestamp = (double)Time.time
            };

            OnIMUUpdated?.Invoke(currentData);
        }

        /// <summary>
        /// Generates Gaussian normally distributed random samples for sensor noise simulation.
        /// </summary>
        private float GenerateGaussianNoise(float mean, float stdDev)
        {
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}




