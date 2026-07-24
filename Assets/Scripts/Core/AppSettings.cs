using UnityEngine;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// Global application settings and runtime configuration container for the ASTRA UAV system.
    /// Manages graphics, audio, simulation parameters, and network connection profiles.
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettings", menuName = "ASTRA/Core/App Settings")]
    public class AppSettings : ScriptableObject
    {
        private static AppSettings _instance;

        /// <summary>
        /// Gets the active runtime AppSettings instance, loading a default from Resources if necessary.
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AppSettings>("AppSettings");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<AppSettings>();
                        Debug.LogWarning("[AppSettings] No AppSettings asset found in Resources folder. Created temporary runtime instance.");
                    }
                }
                return _instance;
            }
            set => _instance = value;
        }

        [Header("Graphics & Performance")]
        [Tooltip("Target frame rate for the application.")]
        [SerializeField] private int _targetFrameRate = 60;

        [Tooltip("VSync count (0 = Disabled, 1 = Every VSync, 2 = Every Second VSync).")]
        [SerializeField] private int _vSyncCount = 0;

        [Tooltip("Enable or disable full screen mode.")]
        [SerializeField] private bool _fullScreen = true;

        [Header("Audio Settings")]
        [Tooltip("Master audio volume level (0.0 to 1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float _masterVolume = 1.0f;

        [Tooltip("Sound effects audio volume level (0.0 to 1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.8f;

        [Tooltip("Music audio volume level (0.0 to 1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float _musicVolume = 0.6f;

        [Header("Simulation Defaults")]
        [Tooltip("Default simulation time scale on start.")]
        [Range(0.1f, 5f)]
        [SerializeField] private float _defaultTimeScale = 1.0f;

        [Tooltip("Fixed update delta time (seconds per physics step).")]
        [SerializeField] private float _physicsFixedDeltaTime = 0.02f; // 50 Hz

        [Header("Hardware & Network Bridge")]
        [Tooltip("Default hardware connection address (IP or COM port).")]
        [SerializeField] private string _connectionAddress = "127.0.0.1";

        [Tooltip("Default MAVLink telemetry port.")]
        [SerializeField] private int _mavlinkPort = 14550;

        [Tooltip("ROS2 Domain ID.")]
        [SerializeField] private int _ros2DomainId = 0;

        /// <summary>Target frame rate for the engine render loop.</summary>
        public int TargetFrameRate { get => _targetFrameRate; set => _targetFrameRate = value; }

        /// <summary>VSync count parameter.</summary>
        public int VSyncCount { get => _vSyncCount; set => _vSyncCount = value; }

        /// <summary>Fullscreen display state toggle.</summary>
        public bool FullScreen { get => _fullScreen; set => _fullScreen = value; }

        /// <summary>Master audio volume (0..1 range).</summary>
        public float MasterVolume { get => _masterVolume; set => _sfxVolume = value; }

        /// <summary>Sound effect audio volume (0..1 range).</summary>
        public float SfxVolume { get => _sfxVolume; set => _sfxVolume = value; }

        /// <summary>Background music volume (0..1 range).</summary>
        public float MusicVolume { get => _musicVolume; set => _musicVolume = value; }

        /// <summary>Default target simulation time scale.</summary>
        public float DefaultTimeScale { get => _defaultTimeScale; set => _defaultTimeScale = value; }

        /// <summary>Physics tick interval in seconds.</summary>
        public float PhysicsFixedDeltaTime { get => _physicsFixedDeltaTime; set => _physicsFixedDeltaTime = value; }

        /// <summary>Target hardware bridge network address or serial port identifier.</summary>
        public string ConnectionAddress { get => _connectionAddress; set => _connectionAddress = value; }

        /// <summary>MAVLink UDP/TCP communication port.</summary>
        public int MavlinkPort { get => _mavlinkPort; set => _mavlinkPort = value; }

        /// <summary>ROS2 Domain identification number.</summary>
        public int Ros2DomainId { get => _ros2DomainId; set => _ros2DomainId = value; }

        /// <summary>
        /// Applies current settings to Unity runtime subsystems (Application, QualitySettings, Time).
        /// </summary>
        public void ApplySettings()
        {
            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = _vSyncCount;
            Screen.fullScreen = _fullScreen;
            Time.fixedDeltaTime = _physicsFixedDeltaTime;
            Debug.Log("[AppSettings] Applied graphics and simulation settings successfully.");
        }
    }
}
