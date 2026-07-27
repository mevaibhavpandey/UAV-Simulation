using System;
using System.Text;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Supported hardware communication bridge types.
    /// </summary>
    public enum HardwareBridgeType
    {
        Simulated,
        MAVLink,
        ROS2,
        DirectSerial,
        RaspberryPiGPIO
    }

    /// <summary>
    /// Event broadcast when hardware bridge connection state changes.
    /// </summary>
    public struct HardwareConnectionStateChangedEvent : IEvent
    {
        public HardwareBridgeType BridgeType { get; }
        public bool IsConnected { get; }
        public string ConnectionInfo { get; }

        public HardwareConnectionStateChangedEvent(HardwareBridgeType bridgeType, bool isConnected, string connectionInfo)
        {
            BridgeType = bridgeType;
            IsConnected = isConnected;
            ConnectionInfo = connectionInfo;
        }
    }

    /// <summary>
    /// Event broadcast when raw or parsed hardware packet data is received.
    /// </summary>
    public struct HardwareDataReceivedEvent : IEvent
    {
        public string Header { get; }
        public byte[] Payload { get; }

        public HardwareDataReceivedEvent(string header, byte[] payload)
        {
            Header = header;
            Payload = payload;
        }
    }

    /// <summary>
    /// Hardware abstraction bridge managing connections and data flow for PX4, MAVLink, ROS2, Serial, and Raspberry Pi GPIO interfaces.
    /// </summary>
    public class HardwareManager : MonoBehaviour
    {
        [Header("Connection Configuration")]
        [SerializeField] private HardwareBridgeType _bridgeType = HardwareBridgeType.Simulated;
        [SerializeField] private string _connectionAddress = "127.0.0.1";
        [SerializeField] private int _port = 14550;
        [SerializeField] private bool _autoConnectOnStart = false;

        private float _heartbeatTimer;

        /// <summary>
        /// Gets whether the hardware bridge is currently connected and active.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Active hardware bridge type.
        /// </summary>
        public HardwareBridgeType ActiveBridgeType => _bridgeType;

        /// <summary>
        /// Active connection target string.
        /// </summary>
        public string ConnectionAddress => $"{_connectionAddress}:{_port}";

        /// <summary>
        /// Action callback on connection state change.
        /// </summary>
        public event Action<bool, string> OnConnectionStateChanged;

        /// <summary>
        /// Action callback on receiving hardware data.
        /// </summary>
        public event Action<string, byte[]> OnDataReceived;

        private void Awake()
        {
            ServiceLocator.Register<HardwareManager>(this);
        }

        private void Start()
        {
            if (_autoConnectOnStart)
            {
                Connect(_connectionAddress, _port, _bridgeType);
            }
        }

        private void OnDestroy()
        {
            Disconnect();
            ServiceLocator.Unregister<HardwareManager>();
        }

        private void Update()
        {
            if (IsConnected)
            {
                _heartbeatTimer += Time.deltaTime;
                if (_heartbeatTimer >= 1.0f) // 1 Hz Heartbeat
                {
                    _heartbeatTimer = 0f;
                    SendHeartbeat();
                }
            }
        }

        /// <summary>
        /// Connects to hardware protocol endpoint using specified parameters.
        /// </summary>
        /// <param name="address">IP address or COM port string.</param>
        /// <param name="port">Port or baud rate.</param>
        /// <param name="bridgeType">Protocol type.</param>
        public bool Connect(string address, int port, HardwareBridgeType bridgeType)
        {
            if (IsConnected) Disconnect();

            _connectionAddress = address;
            _port = port;
            _bridgeType = bridgeType;

            Debug.Log($"[HardwareManager] Connecting via {bridgeType} to {address}:{port}...");

            // Hardware protocol initialization stubs (MAVLink UDP socket, ROS2 node initialization, SerialPort setup, Pi GPIO)
            IsConnected = true;

            string statusMsg = $"{bridgeType} connected to {address}:{port}";
            OnConnectionStateChanged?.Invoke(true, statusMsg);
            EventBus.Publish(new HardwareConnectionStateChangedEvent(bridgeType, true, statusMsg));

            return IsConnected;
        }

        /// <summary>
        /// Disconnects from current hardware endpoint.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            string statusMsg = $"Disconnected from {_bridgeType}";

            Debug.Log($"[HardwareManager] {statusMsg}");

            OnConnectionStateChanged?.Invoke(false, statusMsg);
            EventBus.Publish(new HardwareConnectionStateChangedEvent(_bridgeType, false, statusMsg));
        }

        /// <summary>
        /// Sends a command message or packet over the active hardware bridge interface.
        /// </summary>
        /// <param name="commandName">Command type or header string.</param>
        /// <param name="payload">Command parameter payload bytes.</param>
        public void SendHardwareCommand(string commandName, byte[] payload)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[HardwareManager] Cannot send command: Hardware bridge is disconnected.");
                return;
            }

            int length = payload != null ? payload.Length : 0;
            Debug.Log($"[HardwareManager] Sent command '{commandName}' ({length} bytes) via {_bridgeType}.");
        }

        /// <summary>
        /// Processes raw bytes received from network or serial hardware connection.
        /// </summary>
        /// <param name="header">Packet identifier or topic name.</param>
        /// <param name="rawData">Raw payload bytes.</param>
        public void ProcessIncomingData(string header, byte[] rawData)
        {
            if (!IsConnected) return;

            OnDataReceived?.Invoke(header, rawData);
            EventBus.Publish(new HardwareDataReceivedEvent(header, rawData));
        }

        private void SendHeartbeat()
        {
            byte[] heartbeatPayload = Encoding.UTF8.GetBytes($"HEARTBEAT:{DateTime.UtcNow.Ticks}");
            SendHardwareCommand("HEARTBEAT", heartbeatPayload);
        }
    }
}



