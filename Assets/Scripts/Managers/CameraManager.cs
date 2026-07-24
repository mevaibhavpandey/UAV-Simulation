using System;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Available camera perspectives and control modes for UAV observation.
    /// </summary>
    public enum CameraMode
    {
        ThirdPerson,
        FPV,
        TopView,
        Orbit,
        Free,
        Cinematic
    }

    /// <summary>
    /// Event broadcast when the active camera mode changes.
    /// </summary>
    public struct CameraModeChangedEvent : IEvent
    {
        public CameraMode PreviousMode { get; }
        public CameraMode NewMode { get; }

        public CameraModeChangedEvent(CameraMode previousMode, CameraMode newMode)
        {
            PreviousMode = previousMode;
            NewMode = newMode;
        }
    }

    /// <summary>
    /// Manages application camera modes (Third Person, FPV, Top View, Orbit, Free, Cinematic) and smooth target tracking.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Target & Camera References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _target;

        [Header("Camera Mode Configurations")]
        [SerializeField] private CameraMode _currentMode = CameraMode.ThirdPerson;
        [SerializeField] private Vector3 _thirdPersonOffset = new Vector3(0f, 2.5f, -6f);
        [SerializeField] private Vector3 _fpvOffset = new Vector3(0f, 0.2f, 0.4f);
        [SerializeField] private Vector3 _topViewOffset = new Vector3(0f, 20f, 0f);
        [SerializeField] private float _orbitDistance = 8f;
        [SerializeField] private float _orbitHeight = 3f;
        [SerializeField] private float _smoothSpeed = 10f;

        private float _orbitAngle;
        private DroneManager _droneManager;

        /// <summary>
        /// Gets the current active CameraMode.
        /// </summary>
        public CameraMode CurrentMode => _currentMode;

        /// <summary>
        /// Gets the active Camera instance being managed.
        /// </summary>
        public Camera MainCamera => _mainCamera;

        /// <summary>
        /// Action callback when camera mode changes.
        /// </summary>
        public event Action<CameraMode, CameraMode> OnCameraModeChanged;

        private void Awake()
        {
            ServiceLocator.Register<CameraManager>(this);
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<DroneManager>(out _droneManager))
            {
                if (_droneManager.HasActiveDrone)
                {
                    SetTarget(_droneManager.ActiveDroneTransform);
                }
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<CameraManager>();
        }

        private void LateUpdate()
        {
            if (_target == null && _droneManager != null && _droneManager.HasActiveDrone)
            {
                SetTarget(_droneManager.ActiveDroneTransform);
            }

            if (_mainCamera == null) return;

            UpdateCameraTransform();
        }

        /// <summary>
        /// Changes the active camera viewing mode.
        /// </summary>
        /// <param name="mode">Target camera mode.</param>
        public void SetCameraMode(CameraMode mode)
        {
            if (_currentMode == mode) return;

            CameraMode prevMode = _currentMode;
            _currentMode = mode;

            Debug.Log($"[CameraManager] Camera Mode changed to {mode}");

            OnCameraModeChanged?.Invoke(prevMode, mode);
            EventBus.Publish(new CameraModeChangedEvent(prevMode, mode));
        }

        /// <summary>
        /// Sets the transform target for camera tracking.
        /// </summary>
        /// <param name="target">Target transform.</param>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// Sets field of view for the main camera.
        /// </summary>
        /// <param name="fov">Field of view in degrees (10 to 140).</param>
        public void SetFieldOfView(float fov)
        {
            if (_mainCamera != null)
            {
                _mainCamera.fieldOfView = Mathf.Clamp(fov, 10f, 140f);
            }
        }

        private void UpdateCameraTransform()
        {
            switch (_currentMode)
            {
                case CameraMode.ThirdPerson:
                    if (_target != null)
                    {
                        Vector3 targetPosition = _target.TransformPoint(_thirdPersonOffset);
                        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetPosition, Time.deltaTime * _smoothSpeed);
                        _mainCamera.transform.LookAt(_target.position + Vector3.up * 1f);
                    }
                    break;

                case CameraMode.FPV:
                    if (_target != null)
                    {
                        _mainCamera.transform.position = _target.TransformPoint(_fpvOffset);
                        _mainCamera.transform.rotation = Quaternion.Slerp(_mainCamera.transform.rotation, _target.rotation, Time.deltaTime * _smoothSpeed * 2f);
                    }
                    break;

                case CameraMode.TopView:
                    if (_target != null)
                    {
                        Vector3 targetPos = _target.position + _topViewOffset;
                        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetPos, Time.deltaTime * _smoothSpeed);
                        _mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    }
                    break;

                case CameraMode.Orbit:
                    if (_target != null)
                    {
                        _orbitAngle += Time.deltaTime * 20f;
                        Quaternion rot = Quaternion.Euler(_orbitHeight * 5f, _orbitAngle, 0f);
                        Vector3 offset = rot * new Vector3(0f, 0f, -_orbitDistance);
                        _mainCamera.transform.position = _target.position + offset;
                        _mainCamera.transform.LookAt(_target.position);
                    }
                    break;

                case CameraMode.Free:
                    // Free camera movement handled by user input
                    break;

                case CameraMode.Cinematic:
                    if (_target != null)
                    {
                        _orbitAngle += Time.deltaTime * 10f;
                        Vector3 cinPos = _target.position + new Vector3(Mathf.Sin(_orbitAngle * 0.05f) * 12f, 4f, Mathf.Cos(_orbitAngle * 0.05f) * 12f);
                        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, cinPos, Time.deltaTime * 2f);
                        _mainCamera.transform.LookAt(_target.position);
                    }
                    break;
            }
        }
    }
}
