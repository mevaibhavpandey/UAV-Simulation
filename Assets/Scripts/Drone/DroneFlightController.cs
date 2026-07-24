using System;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Flight controller implementation handling pilot input mixing, axis rate regulation,
    /// PID control feedback loops, and motor command distributions for quadcopters.
    /// </summary>
    public class DroneFlightController : MonoBehaviour, IDroneController
    {
        [Header("Control Axes Input Raw")]
        [SerializeField, Range(-1f, 1f)] private float rollInput = 0f;
        [SerializeField, Range(-1f, 1f)] private float pitchInput = 0f;
        [SerializeField, Range(-1f, 1f)] private float yawInput = 0f;
        [SerializeField, Range(0f, 1f)] private float throttleInput = 0f;

        [Header("Flight Parameters & Limits")]
        [SerializeField] private float maxBankAngleDegrees = 45f;
        [SerializeField] private float maxYawRateDegPerSec = 180f;
        [SerializeField] private float inputDeadzone = 0.05f;
        [SerializeField] private float inputExpo = 0.2f;

        [Header("PID Controller Parameters")]
        [SerializeField] private float kpAngle = 4.5f;
        [SerializeField] private float kpRate = 0.15f;
        [SerializeField] private float kiRate = 0.05f;
        [SerializeField] private float kdRate = 0.01f;

        // Internal State
        private Vector3 rateIntegralError = Vector3.zero;
        private Vector3 previousRateError = Vector3.zero;
        private bool isActive = true;
        private bool isArmed = false;
        private FlightMode currentFlightMode = FlightMode.Angle;
        private FlightState currentFlightState = FlightState.Disarmed;

        public float RollInput { get => rollInput; set => rollInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f); }
        public float PitchInput { get => pitchInput; set => pitchInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f); }
        public float YawInput { get => yawInput; set => yawInput = Mathf.Clamp(ApplyDeadzone(value), -1f, 1f); }
        public float ThrottleInput { get => throttleInput; set => throttleInput = Mathf.Clamp01(value); }

        public bool IsActive => isActive;
        public bool IsArmed => isArmed;
        public bool IsGrounded => transform.position.y < 0.3f;
        public FlightState CurrentFlightState => currentFlightState;
        public FlightMode CurrentFlightMode => currentFlightMode;
        public Vector3 Position => transform.position;
        public Vector3 Velocity => GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().linearVelocity : Vector3.zero;
        public Quaternion Attitude => transform.rotation;
        public Vector3 AngularVelocity => GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().angularVelocity : Vector3.zero;
        public float AltitudeAGL => transform.position.y;
        public float BatteryNormalized => 1.0f;

        public event Action<FlightState> OnFlightStateChanged;
        public event Action<FlightMode> OnFlightModeChanged;
        public event Action<string> OnFlightError;

        public bool Arm()
        {
            isArmed = true;
            currentFlightState = FlightState.Armed;
            OnFlightStateChanged?.Invoke(currentFlightState);
            return true;
        }

        public bool Disarm()
        {
            isArmed = false;
            currentFlightState = FlightState.Disarmed;
            OnFlightStateChanged?.Invoke(currentFlightState);
            return true;
        }

        public void Takeoff(float targetAltitude)
        {
            Arm();
            currentFlightState = FlightState.TakingOff;
            OnFlightStateChanged?.Invoke(currentFlightState);
        }

        public void Land()
        {
            currentFlightState = FlightState.Landing;
            OnFlightStateChanged?.Invoke(currentFlightState);
        }

        public void ReturnToHome()
        {
            currentFlightState = FlightState.ReturningToHome;
            OnFlightStateChanged?.Invoke(currentFlightState);
        }

        public void EmergencyStop()
        {
            Disarm();
            currentFlightState = FlightState.EmergencyStop;
            OnFlightStateChanged?.Invoke(currentFlightState);
        }

        public void SetControlInputs(float pitch, float roll, float yaw, float throttle)
        {
            PitchInput = pitch;
            RollInput = roll;
            YawInput = yaw;
            ThrottleInput = throttle;
        }

        public void ResetInputs()
        {
            pitchInput = 0f;
            rollInput = 0f;
            yawInput = 0f;
            throttleInput = 0f;
            ResetPIDState();
        }

        public void SetVelocity(Vector3 velocity, float yawRate)
        {
            // Direct velocity command stub
        }

        public void SetTargetPosition(Vector3 position, float targetYaw)
        {
            // Direct target position command stub
        }

        public void SetFlightMode(FlightMode mode)
        {
            currentFlightMode = mode;
            OnFlightModeChanged?.Invoke(mode);
        }

        public float[] CalculateMotorOutputs(Quaternion currentAttitude, Vector3 currentAngularVelocity, float deltaTime)
        {
            float[] outputs = new float[4];
            if (!isArmed) return outputs;

            Vector3 pidTorque = CalculatePIDTorque(currentAttitude, currentAngularVelocity, deltaTime);

            float pitchCorrection = pidTorque.y;
            float rollCorrection = pidTorque.x;
            float yawCorrection = pidTorque.z;

            outputs[0] = Mathf.Clamp01(throttleInput + pitchCorrection - rollCorrection + yawCorrection);
            outputs[1] = Mathf.Clamp01(throttleInput + pitchCorrection + rollCorrection - yawCorrection);
            outputs[2] = Mathf.Clamp01(throttleInput - pitchCorrection + rollCorrection + yawCorrection);
            outputs[3] = Mathf.Clamp01(throttleInput - pitchCorrection - rollCorrection - yawCorrection);

            return outputs;
        }

        private Vector3 CalculatePIDTorque(Quaternion currentAttitude, Vector3 currentAngularVelocity, float deltaTime)
        {
            if (deltaTime <= 0.0001f) return Vector3.zero;

            Vector3 targetAngularRates = Vector3.zero;

            if (currentFlightMode == FlightMode.Angle)
            {
                float targetRollAngle = ApplyExpo(rollInput) * maxBankAngleDegrees;
                float targetPitchAngle = ApplyExpo(pitchInput) * maxBankAngleDegrees;

                Vector3 currentEuler = currentAttitude.eulerAngles;
                float currentRoll = NormalizeAngle(currentEuler.z);
                float currentPitch = NormalizeAngle(currentEuler.x);

                float errorRoll = targetRollAngle - currentRoll;
                float errorPitch = targetPitchAngle - currentPitch;

                targetAngularRates.x = errorRoll * kpAngle;
                targetAngularRates.y = errorPitch * kpAngle;
                targetAngularRates.z = ApplyExpo(yawInput) * maxYawRateDegPerSec;
            }
            else
            {
                targetAngularRates.x = ApplyExpo(rollInput) * maxBankAngleDegrees * 4f;
                targetAngularRates.y = ApplyExpo(pitchInput) * maxBankAngleDegrees * 4f;
                targetAngularRates.z = ApplyExpo(yawInput) * maxYawRateDegPerSec;
            }

            Vector3 rateError = targetAngularRates - currentAngularVelocity;
            rateIntegralError += rateError * deltaTime;
            rateIntegralError = Vector3.ClampMagnitude(rateIntegralError, 10f);

            Vector3 rateDerivative = (rateError - previousRateError) / deltaTime;
            previousRateError = rateError;

            return (rateError * kpRate) + (rateIntegralError * kiRate) + (rateDerivative * kdRate);
        }

        private void ResetPIDState()
        {
            rateIntegralError = Vector3.zero;
            previousRateError = Vector3.zero;
        }

        private float ApplyDeadzone(float input)
        {
            if (Mathf.Abs(input) < inputDeadzone) return 0f;
            return Mathf.Sign(input) * ((Mathf.Abs(input) - inputDeadzone) / (1f - inputDeadzone));
        }

        private float ApplyExpo(float input)
        {
            return input * (1f - inputExpo + inputExpo * input * input);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
