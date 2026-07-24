using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Extension methods for common Unity structures (Vector3, Quaternion, Transform, GameObject, float).
    /// </summary>
    public static class ExtensionMethods
    {
        #region Vector3 Extensions

        /// <summary>
        /// Returns a copy of the Vector3 with the X component modified.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a copy of the Vector3 with the Y component modified.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Returns a copy of the Vector3 with the Z component modified.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Converts a 3D vector to a 2D horizontal plane vector (X, Z).
        /// </summary>
        public static Vector2 ToVector2XZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        /// <summary>
        /// Calculates the horizontal distance (ignoring Y/altitude) between two 3D positions.
        /// </summary>
        public static float HorizontalDistanceTo(this Vector3 position, Vector3 target)
        {
            Vector2 p1 = new Vector2(position.x, position.z);
            Vector2 p2 = new Vector2(target.x, target.z);
            return Vector2.Distance(p1, p2);
        }

        /// <summary>
        /// Checks whether a vector is contained inside a 3D bounding box (min to max).
        /// </summary>
        public static bool IsWithinBounds(this Vector3 position, Vector3 minBounds, Vector3 maxBounds)
        {
            return position.x >= minBounds.x && position.x <= maxBounds.x &&
                   position.y >= minBounds.y && position.y <= maxBounds.y &&
                   position.z >= minBounds.z && position.z <= maxBounds.z;
        }

        #endregion

        #region Quaternion Extensions

        /// <summary>
        /// Ensures a quaternion is normalized to prevent NaN rotation propagation.
        /// </summary>
        public static Quaternion EnsureNormalized(this Quaternion rotation)
        {
            float lengthSq = rotation.x * rotation.x + rotation.y * rotation.y + rotation.z * rotation.z + rotation.w * rotation.w;
            if (Mathf.Approximately(lengthSq, 1f) || lengthSq <= 0f) return rotation;

            float lengthInv = 1f / Mathf.Sqrt(lengthSq);
            return new Quaternion(rotation.x * lengthInv, rotation.y * lengthInv, rotation.z * lengthInv, rotation.w * lengthInv);
        }

        #endregion

        #region Transform Extensions

        /// <summary>
        /// Resets position to Vector3.zero, localRotation to Identity, and localScale to Vector3.one.
        /// </summary>
        public static void ResetLocalTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Destroys all immediate child GameObjects of this transform.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Returns all immediate child Transforms of this parent transform.
        /// </summary>
        public static List<Transform> GetDirectChildren(this Transform parent)
        {
            var children = new List<Transform>(parent.childCount);
            for (int i = 0; i < parent.childCount; i++)
            {
                children.Add(parent.GetChild(i));
            }
            return children;
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// Returns the specified component if attached, or adds it if missing.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out T existingComponent))
            {
                return existingComponent;
            }
            return gameObject.AddComponent<T>();
        }

        #endregion

        #region Float Extensions

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static float ToDegrees(this float radians)
        {
            return radians * Mathf.Rad2Deg;
        }

        #endregion
    }
}
