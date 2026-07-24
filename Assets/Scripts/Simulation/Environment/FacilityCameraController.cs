using UnityEngine;
using UnityEngine.InputSystem;
using ASTRA.UAV.Core;
using ASTRA.UAV.Interfaces;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Master controller for switching camera observation perspectives in the facility.
    /// Maps key bindings [1-6] to Third Person, Orbit, Top, FPV, Free Cam, and Cinematic modes.
    /// </summary>
    public class FacilityCameraController : Singleton<FacilityCameraController>
    {
        [Header("Camera Mode Components")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private FreeCameraController freeCamera;
        [SerializeField] private CinematicCameraController cinematicCamera;

        [Header("State")]
        [SerializeField] private CameraMode activeMode = CameraMode.Cinematic;

        public CameraMode ActiveMode => activeMode;

        protected override void Awake()
        {
            base.Awake();
            if (mainCamera == null) mainCamera = Camera.main;
            if (freeCamera == null) freeCamera = GetComponent<FreeCameraController>();
            if (cinematicCamera == null) cinematicCamera = GetComponent<CinematicCameraController>();
        }

        private void Start()
        {
            SwitchCameraMode(activeMode);
        }

        private void Update()
        {
            HandleModeHotkeys();
        }

        private void HandleModeHotkeys()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.ThirdPerson);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.Orbit);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.TopView);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.FPV);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.Free);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) SwitchCameraMode(CameraMode.Cinematic);
        }

        /// <summary>
        /// Switches active camera observation perspective.
        /// </summary>
        /// <param name="newMode">Target camera mode.</param>
        public void SwitchCameraMode(CameraMode newMode)
        {
            activeMode = newMode;

            if (freeCamera != null) freeCamera.enabled = (newMode == CameraMode.Free);
            if (cinematicCamera != null)
            {
                cinematicCamera.enabled = (newMode == CameraMode.Cinematic);
                if (newMode == CameraMode.Cinematic)
                {
                    cinematicCamera.StartCinematicSequence();
                }
                else
                {
                    cinematicCamera.StopCinematicSequence();
                }
            }

            Logger.Log($"Switched Facility Camera Mode to: {newMode}", LogCategory.Simulation);
        }
    }
}
