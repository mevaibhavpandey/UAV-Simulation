using System;

namespace ASTRA.UAV.Interfaces
{
    /// <summary>
    /// Contract for communicating with physical or external hardware (e.g., PX4/ArduPilot flight controller over UDP/Serial).
    /// </summary>
    public interface IHardwareAdapter
    {
        /// <summary>
        /// Gets the name of the protocol implemented by this adapter (e.g. "MAVLink 2.0", "MSP").
        /// </summary>
        string ProtocolName { get; }

        /// <summary>
        /// Gets the connection endpoint descriptor (e.g. "127.0.0.1:14550" or "COM3:115200").
        /// </summary>
        string ConnectionEndpoint { get; }

        /// <summary>
        /// Gets a value indicating whether a active connection to the hardware endpoint exists.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the total number of packets received since connection established.
        /// </summary>
        ulong PacketsReceived { get; }

        /// <summary>
        /// Gets the total number of packets transmitted since connection established.
        /// </summary>
        ulong PacketsSent { get; }

        /// <summary>
        /// Gets normalized hardware signal and connection quality [0.0 to 1.0].
        /// </summary>
        float ConnectionQualityNormalized { get; }

        /// <summary>
        /// Fired when hardware connection is established.
        /// </summary>
        event Action OnConnected;

        /// <summary>
        /// Fired when hardware connection is terminated.
        /// </summary>
        event Action OnDisconnected;

        /// <summary>
        /// Fired when raw binary byte frames are received from hardware.
        /// </summary>
        event Action<byte[]> OnDataReceived;

        /// <summary>
        /// Fired when a communication or hardware protocol error occurs.
        /// </summary>
        event Action<string> OnHardwareError;

        /// <summary>
        /// Establishes connection to the specified hardware endpoint string.
        /// </summary>
        /// <param name="endpoint">Endpoint address (IP, Port, or COM port).</param>
        /// <returns>True if connection initiated successfully.</returns>
        bool Connect(string endpoint);

        /// <summary>
        /// Terminates active hardware connection cleanly.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Transmits raw byte payload to physical flight hardware.
        /// </summary>
        /// <param name="payload">Byte array payload.</param>
        /// <returns>True if data was written to buffer successfully.</returns>
        bool SendRawData(byte[] payload);
    }
}
