using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Math and coordinate system helper functions for UAV aerospace dynamics and vector transforms.
    /// Handles NED (North-East-Down) to ENU (East-North-Up / Unity) conversions.
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Converts a position or velocity vector from NED (North-East-Down) to Unity ENU (East-North-Up) coordinates.
        /// </summary>
        /// <param name="ned">Vector in NED coordinate frame.</param>
        /// <returns>Vector in ENU (Unity) coordinate frame.</returns>
        public static Vector3 NEDToENU(Vector3 ned)
        {
            // NED: X = North, Y = East, Z = Down
            // ENU: X = East,  Y = Up,   Z = North
            return new Vector3(ned.y, -ned.z, ned.x);
        }

        /// <summary>
        /// Converts a position or velocity vector from Unity ENU (East-North-Up) to NED (North-East-Down) coordinates.
        /// </summary>
        /// <param name="enu">Vector in ENU (Unity) coordinate frame.</param>
        /// <returns>Vector in NED coordinate frame.</returns>
        public static Vector3 ENUToNED(Vector3 enu)
        {
            // ENU: X = East,  Y = Up,   Z = North
            // NED: X = North, Y = East, Z = Down
            return new Vector3(enu.z, enu.x, -enu.y);
        }

        /// <summary>
        /// Converts aircraft attitude orientation from Aerospace NED frame to Unity ENU frame.
        /// </summary>
        /// <param name="nedRotation">Quaternion in NED frame.</param>
        /// <returns>Quaternion in ENU Unity frame.</returns>
        public static Quaternion NEDToENURotation(Quaternion nedRotation)
        {
            Vector3 eulerNed = nedRotation.eulerAngles;
            // Roll (around X_ned -> Z_enu), Pitch (around Y_ned -> X_enu), Yaw (around Z_ned -> Y_enu)
            return Quaternion.Euler(-eulerNed.y, -eulerNed.z, eulerNed.x);
        }

        /// <summary>
        /// Converts aircraft attitude orientation from Unity ENU frame to Aerospace NED frame.
        /// </summary>
        /// <param name="enuRotation">Quaternion in ENU frame.</param>
        /// <returns>Quaternion in NED frame.</returns>
        public static Quaternion ENUToNEDRotation(Quaternion enuRotation)
        {
            Vector3 eulerEnu = enuRotation.eulerAngles;
            return Quaternion.Euler(eulerEnu.z, -eulerEnu.x, -eulerEnu.y);
        }

        /// <summary>
        /// Constructs a Quaternion orientation from Pitch, Roll, and Yaw angles in degrees.
        /// </summary>
        /// <param name="pitch">Pitch angle in degrees (nose up/down).</param>
        /// <param name="roll">Roll angle in degrees (wing tilt).</param>
        /// <param name="yaw">Yaw heading angle in degrees.</param>
        /// <returns>Constructed Quaternion.</returns>
        public static Quaternion QuaternionFromEulerPRY(float pitch, float roll, float yaw)
        {
            return Quaternion.Euler(pitch, yaw, roll);
        }

        /// <summary>
        /// Extracts Pitch, Roll, and Yaw angles in degrees from a Quaternion orientation.
        /// </summary>
        /// <param name="rotation">Aircraft orientation quaternion.</param>
        /// <param name="pitch">Extracted pitch angle in degrees.</param>
        /// <param name="roll">Extracted roll angle in degrees.</param>
        /// <param name="yaw">Extracted yaw angle in degrees.</param>
        public static void QuaternionToEulerPRY(Quaternion rotation, out float pitch, out float roll, out float yaw)
        {
            Vector3 euler = rotation.eulerAngles;
            pitch = NormalizeAngle180(euler.x);
            roll = NormalizeAngle180(euler.z);
            yaw = NormalizeAngle360(euler.y);
        }

        /// <summary>
        /// Normalizes an angle in degrees to the [-180, +180] range.
        /// </summary>
        /// <param name="angleDegrees">Angle in degrees.</param>
        /// <returns>Normalized angle [-180..+180].</returns>
        public static float NormalizeAngle180(float angleDegrees)
        {
            angleDegrees %= 360f;
            if (angleDegrees > 180f) angleDegrees -= 360f;
            if (angleDegrees < -180f) angleDegrees += 360f;
            return angleDegrees;
        }

        /// <summary>
        /// Normalizes an angle in degrees to the [0, 360) range.
        /// </summary>
        /// <param name="angleDegrees">Angle in degrees.</param>
        /// <returns>Normalized angle [0..360).</returns>
        public static float NormalizeAngle360(float angleDegrees)
        {
            angleDegrees %= 360f;
            if (angleDegrees < 0f) angleDegrees += 360f;
            return angleDegrees;
        }

        /// <summary>
        /// Applies a deadband threshold to a scalar value.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="threshold">Threshold cutoff value.</param>
        /// <returns>0 if within deadband; adjusted input value otherwise.</returns>
        public static float ApplyDeadband(float value, float threshold)
        {
            if (Mathf.Abs(value) < threshold) return 0f;
            return Mathf.Sign(value) * ((Mathf.Abs(value) - threshold) / (1f - threshold));
        }

        /// <summary>
        /// Remaps a value from one range [fromMin, fromMax] to another range [toMin, toMax].
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// First-order exponential low pass filter for vector smoothing.
        /// </summary>
        /// <param name="current">Current raw measurement.</param>
        /// <param name="previous">Previous filtered value.</param>
        /// <param name="alpha">Filter coefficient [0..1]. Higher = faster response, less smoothing.</param>
        /// <returns>Filtered Vector3 value.</returns>
        public static Vector3 LowPassFilter(Vector3 current, Vector3 previous, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            return alpha * current + (1f - alpha) * previous;
        }
    }
}



