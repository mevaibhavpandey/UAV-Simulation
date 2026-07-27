using System;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Legacy flight controller — superseded by ASTRASimulation.
    /// Kept for reference. Interface removed to prevent compile conflicts.
    /// </summary>
    public class DroneFlightController : MonoBehaviour
    {
        [Header("Control Axes")]
        [SerializeField, Range(-1f, 1f)] public float rollInput = 0f;
        [SerializeField, Range(-1f, 1f)] public float pitchInput = 0f;
        [SerializeField, Range(-1f, 1f)] public float yawInput = 0f;
        [SerializeField, Range(0f, 1f)]  public float throttleInput = 0f;

        [Header("PID Gains")]
        [SerializeField] private float maxBankAngleDegrees = 45f;
        [SerializeField] private float maxYawRateDegPerSec = 180f;
        [SerializeField] private float inputDeadzone = 0.05f;
        [SerializeField] private float inputExpo = 0.2f;
        [SerializeField] private float kpAngle = 4.5f;
        [SerializeField] private float kpRate  = 0.15f;
        [SerializeField] private float kiRate  = 0.05f;
        [SerializeField] private float kdRate  = 0.01f;

        private Vector3 rateIntegralError  = Vector3.zero;
        private Vector3 previousRateError  = Vector3.zero;
        private bool    isArmed            = false;

        public bool IsArmed    => isArmed;
        public bool IsGrounded => transform.position.y < 0.3f;
        public float ThrottleInput => throttleInput;
        public FlightMode CurrentFlightMode => FlightMode.Manual;

        public bool Arm()   { isArmed = true;  return true; }
        public bool Disarm(){ isArmed = false; return true; }
        public void SetActive(bool active) { if (active) Arm(); else Disarm(); }

        public void SetControlInputs(float pitch, float roll, float yaw, float throttle)
        {
            pitchInput    = Mathf.Clamp(pitch,    -1f, 1f);
            rollInput     = Mathf.Clamp(roll,     -1f, 1f);
            yawInput      = Mathf.Clamp(yaw,      -1f, 1f);
            throttleInput = Mathf.Clamp01(throttle);
        }

        public void ResetInputs()
        {
            pitchInput = rollInput = yawInput = 0f;
            throttleInput = 0f;
            rateIntegralError = previousRateError = Vector3.zero;
        }

        public float[] CalculateMotorOutputs(Quaternion attitude, Vector3 angularVelocity, float dt)
        {
            float[] outputs = new float[4];
            if (!isArmed || dt <= 0f) return outputs;

            Vector3 pid = CalculatePID(attitude, angularVelocity, dt);

            outputs[0] = Mathf.Clamp01(throttleInput + pid.y - pid.x + pid.z);
            outputs[1] = Mathf.Clamp01(throttleInput + pid.y + pid.x - pid.z);
            outputs[2] = Mathf.Clamp01(throttleInput - pid.y + pid.x + pid.z);
            outputs[3] = Mathf.Clamp01(throttleInput - pid.y - pid.x - pid.z);
            return outputs;
        }

        public float[] CalculateMotorMixingOutputs(Quaternion attitude, Vector3 angularVelocity, float dt)
        {
            return CalculateMotorOutputs(attitude, angularVelocity, dt);
        }

        private Vector3 CalculatePID(Quaternion attitude, Vector3 angularVelocity, float dt)
        {
            Vector3 targetRates = Vector3.zero;
            Vector3 euler = attitude.eulerAngles;

            float targetRoll  = ApplyExpo(rollInput)  * maxBankAngleDegrees;
            float targetPitch = ApplyExpo(pitchInput) * maxBankAngleDegrees;

            float errRoll  = targetRoll  - NormalizeAngle(euler.z);
            float errPitch = targetPitch - NormalizeAngle(euler.x);

            targetRates.x = errRoll  * kpAngle;
            targetRates.y = errPitch * kpAngle;
            targetRates.z = ApplyExpo(yawInput) * maxYawRateDegPerSec;

            Vector3 rateError   = targetRates - angularVelocity;
            rateIntegralError  += rateError * dt;
            rateIntegralError   = Vector3.ClampMagnitude(rateIntegralError, 10f);
            Vector3 rateD       = (rateError - previousRateError) / dt;
            previousRateError   = rateError;

            return rateError * kpRate + rateIntegralError * kiRate + rateD * kdRate;
        }

        private float ApplyDeadzone(float v)
        {
            if (Mathf.Abs(v) < inputDeadzone) return 0f;
            return Mathf.Sign(v) * ((Mathf.Abs(v) - inputDeadzone) / (1f - inputDeadzone));
        }

        private float ApplyExpo(float v) => v * (1f - inputExpo + inputExpo * v * v);

        private float NormalizeAngle(float a)
        {
            while (a >  180f) a -= 360f;
            while (a < -180f) a += 360f;
            return a;
        }
    }
}





