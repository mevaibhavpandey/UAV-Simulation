using UnityEngine;
using UnityEngine.InputSystem;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Smooth inspection camera allowing WASD movement, Shift speed boost, mouse look, and scroll wheel zoom.
    /// Used for free inspection of the UAV testing facility.
    /// </summary>
    public class FreeCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 15.0f;
        [SerializeField] private float boostMultiplier = 2.5f;
        [SerializeField] private float lookSensitivity = 2.0f;
        [SerializeField] private float moveSmoothing = 10.0f;
        [SerializeField] private float zoomSensitivity = 5.0f;

        [Header("Limits")]
        [SerializeField] private float minHeight = 2.0f;
        [SerializeField] private float maxHeight = 200.0f;

        private Vector3 targetPosition;
        private Vector3 currentVelocity;
        private float pitch = 20.0f;
        private float yaw = 0.0f;
        private bool isMouseLookActive = false;

        private void Start()
        {
            targetPosition = transform.position;
            Vector3 euler = transform.eulerAngles;
            pitch = euler.x;
            yaw = euler.y;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleZoom();
        }

        private void HandleMouseLook()
        {
            // Right mouse button holds look active
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                isMouseLookActive = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
            {
                isMouseLookActive = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (isMouseLookActive && Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue() * (lookSensitivity * 0.1f);
                yaw += mouseDelta.x;
                pitch -= mouseDelta.y;
                pitch = Mathf.Clamp(pitch, -85.0f, 85.0f);

                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        private void HandleMovement()
        {
            Vector3 inputDir = Vector3.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) inputDir += transform.forward;
                if (Keyboard.current.sKey.isPressed) inputDir -= transform.forward;
                if (Keyboard.current.aKey.isPressed) inputDir -= transform.right;
                if (Keyboard.current.dKey.isPressed) inputDir += transform.right;
                if (Keyboard.current.eKey.isPressed) inputDir += Vector3.up;
                if (Keyboard.current.qKey.isPressed) inputDir -= Vector3.up;
            }

            float currentSpeed = moveSpeed;
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                currentSpeed *= boostMultiplier;
            }

            targetPosition += inputDir.normalized * (currentSpeed * Time.deltaTime);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1.0f / moveSmoothing);
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    targetPosition += transform.forward * (scroll * zoomSensitivity * 0.01f * moveSpeed);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
                }
            }
        }
    }
}


