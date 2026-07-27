using UnityEngine;

namespace ASTRA.UAV.Simulation
{
    /// <summary>
    /// ScriptableObject defining physical vehicle specs, model name, battery specifications, and payload configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "DroneConfig", menuName = "ASTRA/UAV/Drone Config", order = 12)]
    public class DroneConfig : ScriptableObject
    {
        [Header("General Identity")]
        [SerializeField] private string droneModelName = "ASTRA Quad-X Pro";
        [SerializeField] private string firmwareVersion = "v2.4.0-release";

        [Header("Physical Specs")]
        [Tooltip("Wheelbase diagonal motor-to-motor distance in millimeters.")]
        [SerializeField] private float frameSizeMm = 450f;

        [Tooltip("Propeller diameter in inches.")]
        [SerializeField] private float propellerDiameterInches = 10f;

        [Header("Battery Specifications")]
        [Tooltip("Battery nominal voltage in Volts.")]
        [SerializeField] private float batteryVoltageVolts = 14.8f; // 4S LiPo

        [Tooltip("Battery capacity in milliampere-hours (mAh).")]
        [SerializeField] private float batteryCapacityMah = 5200f;

        [Header("Payload Capabilities")]
        [Tooltip("Maximum payload weight capacity in kilograms.")]
        [SerializeField] private float maxPayloadWeightKg = 0.8f;

        /// <summary>Gets model name.</summary>
        public string DroneModelName => droneModelName;

        /// <summary>Gets firmware version string.</summary>
        public string FirmwareVersion => firmwareVersion;

        /// <summary>Gets frame size in mm.</summary>
        public float FrameSizeMm => frameSizeMm;

        /// <summary>Gets prop diameter in inches.</summary>
        public float PropellerDiameterInches => propellerDiameterInches;

        /// <summary>Gets battery voltage in Volts.</summary>
        public float BatteryVoltageVolts => batteryVoltageVolts;

        /// <summary>Gets battery capacity in mAh.</summary>
        public float BatteryCapacityMah => batteryCapacityMah;

        /// <summary>Gets max payload weight capacity in kg.</summary>
        public float MaxPayloadWeightKg => maxPayloadWeightKg;
    }
}





