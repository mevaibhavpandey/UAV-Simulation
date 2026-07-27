using System;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Telemetry data packet containing full vehicle attitude, position, battery, and status metrics.
    /// </summary>
    [Serializable]
    public struct TelemetryData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 PitchRollYaw; // Vector3(Pitch, Roll, Yaw) in degrees
        public float AltitudeAGL;
        public float AltitudeMSL;
        public Vector3 Velocity;
        public float GroundSpeed;
        public float VerticalSpeed;
        public float BatteryPercentage;
        public float BatteryVoltage;
        public int GpsSatellites;
        public bool IsArmed;
        public string FlightMode;
        public double Latitude;
        public double Longitude;
        public long TimestampTicks;
    }

    /// <summary>
    /// Event broadcast when telemetry data is updated.
    /// </summary>
    public struct TelemetryUpdatedEvent : IEvent
    {
        public TelemetryData Data { get; }
        public TelemetryUpdatedEvent(TelemetryData data) => Data = data;
    }

    /// <summary>
    /// Aggregates UAV telemetry from hardware bridges or local simulation and broadcasts telemetry updates via EventBus.
    /// </summary>
    public class TelemetryManager : MonoBehaviour
    {
        [Header("Telemetry Broadcast Settings")]
        [Tooltip("Update broadcast frequency in seconds (e.g. 0.05 = 20 Hz).")]
        [SerializeField] private float _updateInterval = 0.05f;

        private float _timer;
        private DroneManager _droneManager;

        /// <summary>
        /// Gets the most recent telemetry packet received or computed.
        /// </summary>
        public TelemetryData LatestTelemetry { get; private set; }

        /// <summary>
        /// Action callback invoked on every telemetry update interval.
        /// </summary>
        public event Action<TelemetryData> OnTelemetryUpdated;

        private void Awake()
        {
            ServiceLocator.Register<TelemetryManager>(this);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _droneManager);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TelemetryManager>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _updateInterval)
            {
                _timer = 0f;

                // If drone manager available and no manual telemetry push, sample local simulated drone transform
                if (_droneManager == null)
                {
                    ServiceLocator.TryGet(out _droneManager);
                }

                if (_droneManager != null && _droneManager.HasActiveDrone)
                {
                    SampleLocalDroneTelemetry(_droneManager.ActiveDrone);
                }

                OnTelemetryUpdated?.Invoke(LatestTelemetry);
                EventBus.Publish(new TelemetryUpdatedEvent(LatestTelemetry));
            }
        }

        /// <summary>
        /// Manually feeds updated telemetry into the manager (e.g. from MAVLink/ROS2 network streams).
        /// </summary>
        /// <param name="data">Telemetry packet.</param>
        public void UpdateTelemetry(TelemetryData data)
        {
            LatestTelemetry = data;
        }

        /// <summary>
        /// Sets the telemetry broadcast interval rate.
        /// </summary>
        /// <param name="intervalSeconds">Interval in seconds.</param>
        public void SetUpdateInterval(float intervalSeconds)
        {
            _updateInterval = Mathf.Max(0.01f, intervalSeconds);
        }

        private void SampleLocalDroneTelemetry(GameObject droneObj)
        {
            Transform t = droneObj.transform;
            Vector3 pos = t.position;
            Quaternion rot = t.rotation;
            Vector3 euler = rot.eulerAngles;

            Rigidbody rb = droneObj.GetComponent<Rigidbody>();
            Vector3 velocity = rb != null ? rb.linearVelocity : Vector3.zero;

            TelemetryData data = LatestTelemetry;
            data.Position = pos;
            data.Rotation = rot;
            data.PitchRollYaw = new Vector3(euler.x, euler.z, euler.y);
            data.AltitudeAGL = Mathf.Max(0f, pos.y);
            data.AltitudeMSL = pos.y + 100f; // Dummy sea level baseline
            data.Velocity = velocity;
            data.GroundSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            data.VerticalSpeed = velocity.y;
            data.TimestampTicks = DateTime.UtcNow.Ticks;

            LatestTelemetry = data;
        }
    }
}



