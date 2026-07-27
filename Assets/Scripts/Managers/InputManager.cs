using System;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Event broadcast when drone flight control axes change.
    /// </summary>
    public struct FlightInputEvent : IEvent
    {
        /// <summary>Vector4 containing (Roll, Pitch, Yaw, Throttle) normalized between -1.0 and 1.0 (Throttle 0 to 1).</summary>
        public Vector4 Controls { get; }

        public FlightInputEvent(Vector4 controls) => Controls = controls;
    }

    /// <summary>
    /// Wraps user input actions for drone flight stick axes (Pitch, Roll, Yaw, Throttle) and camera controls.
    /// Supports Unity 6 Input system abstraction.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Flight Input Sensitivity")]
        [SerializeField] private float _pitchSensitivity = 1.0f;
        [SerializeField] private float _rollSensitivity = 1.0f;
        [SerializeField] private float _yawSensitivity = 1.0f;
        [SerializeField] private float _throttleSensitivity = 1.0f;

        [Header("Invert Axes")]
        [SerializeField] private bool _invertPitch = false;
        [SerializeField] private bool _invertRoll = false;

        /// <summary>
        /// Gets current flight control vector: X = Roll (-1..1), Y = Pitch (-1..1), Z = Yaw (-1..1), W = Throttle (0..1).
        /// </summary>
        public Vector4 FlightControls { get; private set; }

        /// <summary>
        /// Gets camera orbit input delta (X = Pitch, Y = Yaw).
        /// </summary>
        public Vector2 CameraOrbitInput { get; private set; }

        /// <summary>
        /// Gets camera zoom scroll axis.
        /// </summary>
        public float CameraZoomInput { get; private set; }

        /// <summary>
        /// Invoked on every frame update with flight control stick axis values.
        /// </summary>
        public event Action<Vector4> OnFlightControlsChanged;

        /// <summary>
        /// Invoked when pause button is pressed.
        /// </summary>
        public event Action OnPausePressed;

        /// <summary>
        /// Invoked when emergency stop key is pressed.
        /// </summary>
        public event Action OnEmergencyStopPressed;

        private void Awake()
        {
            ServiceLocator.Register<InputManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InputManager>();
        }

        private void Update()
        {
            ReadInputAxes();
            HandleHotkeys();
        }

        /// <summary>
        /// Reads flight control stick inputs and updates FlightControls property.
        /// </summary>
        private void ReadInputAxes()
        {
            float roll = Input.GetAxis("Horizontal") * _rollSensitivity * (_invertRoll ? -1f : 1f);
            float pitch = Input.GetAxis("Vertical") * _pitchSensitivity * (_invertPitch ? -1f : 1f);

            float yaw = 0f;
            if (Input.GetKey(KeyCode.Q)) yaw -= 1f;
            if (Input.GetKey(KeyCode.E)) yaw += 1f;
            yaw *= _yawSensitivity;

            float throttle = 0f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) throttle += 1f;
            if (Input.GetKey(KeyCode.LeftControl)) throttle -= 1f;
            throttle *= _throttleSensitivity;

            FlightControls = new Vector4(
                Mathf.Clamp(roll, -1f, 1f),
                Mathf.Clamp(pitch, -1f, 1f),
                Mathf.Clamp(yaw, -1f, 1f),
                Mathf.Clamp(throttle, -1f, 1f)
            );

            // Read Camera Input
            float camX = Input.GetAxis("Mouse X");
            float camY = Input.GetAxis("Mouse Y");
            CameraOrbitInput = new Vector2(camX, camY);
            CameraZoomInput = Input.GetAxis("Mouse ScrollWheel");

            OnFlightControlsChanged?.Invoke(FlightControls);
            EventBus.Publish(new FlightInputEvent(FlightControls));
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                OnPausePressed?.Invoke();
                if (ServiceLocator.TryGet<GameManager>(out var gm))
                {
                    if (gm.CurrentState == GameState.Simulating) gm.PauseGame();
                    else if (gm.CurrentState == GameState.Paused) gm.ResumeGame();
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X))
            {
                OnEmergencyStopPressed?.Invoke();
                if (ServiceLocator.TryGet<GameManager>(out var gm))
                {
                    gm.TriggerEmergencyStop();
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (ServiceLocator.TryGet<CameraManager>(out var camManager))
                {
                    int nextModeIndex = ((int)camManager.CurrentMode + 1) % Enum.GetValues(typeof(CameraMode)).Length;
                    camManager.SetCameraMode((CameraMode)nextModeIndex);
                }
            }
        }
    }
}





