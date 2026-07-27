//-----------------------------------------------------------------------
// <copyright file="DroneComponentRegistry.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Categorizes hardware components installed on the UAV platform.
    /// </summary>
    public enum HardwareCategory
    {
        /// <summary>Flight Controller hardware (e.g., Pixhawk, Cube Orange).</summary>
        FlightController,

        /// <summary>Power Distribution Board, battery monitors, and BEC units.</summary>
        PowerSystem,

        /// <summary>Onboard companion computer (e.g., Raspberry Pi 5, NVIDIA Jetson).</summary>
        CompanionComputer,

        /// <summary>Optical navigation or payload camera systems.</summary>
        VisionCamera,

        /// <summary>LiDAR or optical flow rangefinding sensors.</summary>
        DistanceSensor,

        /// <summary>Electronic Speed Controllers (ESCs).</summary>
        ESC,

        /// <summary>Radio control / telemetry receiver links.</summary>
        RadioReceiver,

        /// <summary>Custom payload or actuator attachments.</summary>
        Payload
    }

    /// <summary>
    /// Contract that all UAV hardware component representations must implement.
    /// </summary>
    public interface IDroneHardwareComponent
    {
        /// <summary>Gets unique hardware component string identifier.</summary>
        string ComponentId { get; }

        /// <summary>Gets user-friendly display name of the hardware part.</summary>
        string DisplayName { get; }

        /// <summary>Gets hardware classification category.</summary>
        HardwareCategory Category { get; }

        /// <summary>Gets electrical power draw in Watts.</summary>
        float PowerConsumptionWatts { get; }

        /// <summary>Gets physical component mass in grams.</summary>
        float WeightGrams { get; }

        /// <summary>Gets a value indicating whether component is currently powered and operational.</summary>
        bool IsEnabled { get; }

        /// <summary>Initializes hardware component routines.</summary>
        bool InitializeComponent();

        /// <summary>Shuts down hardware component safely.</summary>
        void ShutdownComponent();
    }

    /// <summary>
    /// Default concrete implementation of IDroneHardwareComponent for registry modularity.
    /// </summary>
    [Serializable]
    public class GenericHardwareComponent : IDroneHardwareComponent
    {
        [SerializeField] private string componentId;
        [SerializeField] private string displayName;
        [SerializeField] private HardwareCategory category;
        [SerializeField] private float powerConsumptionWatts;
        [SerializeField] private float weightGrams;
        [SerializeField] private bool isEnabled = true;

        public string ComponentId => componentId;
        public string DisplayName => displayName;
        public HardwareCategory Category => category;
        public float PowerConsumptionWatts => powerConsumptionWatts;
        public float WeightGrams => weightGrams;
        public bool IsEnabled => isEnabled;

        /// <summary>
        /// Initializes a new instance of GenericHardwareComponent.
        /// </summary>
        public GenericHardwareComponent(string id, string name, HardwareCategory category, float powerWatts, float weightGrams)
        {
            this.componentId = id;
            this.displayName = name;
            this.category = category;
            this.powerConsumptionWatts = powerWatts;
            this.weightGrams = weightGrams;
            this.isEnabled = true;
        }

        public bool InitializeComponent()
        {
            isEnabled = true;
            return true;
        }

        public void ShutdownComponent()
        {
            isEnabled = false;
        }
    }

    /// <summary>
    /// Central component registry managing all modular hardware components (Pixhawk, PDB, Pi5, Cameras, sensors)
    /// installed on the quadcopter frame, tracking total power requirements and hardware status.
    /// </summary>
    public class DroneComponentRegistry : MonoBehaviour
    {
        private readonly Dictionary<string, IDroneHardwareComponent> registeredComponents = new Dictionary<string, IDroneHardwareComponent>();

        /// <summary>
        /// Gets the total number of currently registered hardware components.
        /// </summary>
        public int ComponentCount => registeredComponents.Count;

        /// <summary>
        /// Fired when a new hardware component is registered.
        /// </summary>
        public event Action<IDroneHardwareComponent> OnComponentRegistered;

        /// <summary>
        /// Fired when a hardware component is unregistered.
        /// </summary>
        public event Action<string> OnComponentUnregistered;

        /// <summary>
        /// Unity Start initialization. Registers standard stock hardware components (Pixhawk, PDB, Pi5, Camera).
        /// </summary>
        private void Start()
        {
            RegisterDefaultHardwareComponents();
        }

        /// <summary>
        /// Registers default quadcopter hardware suite.
        /// </summary>
        private void RegisterDefaultHardwareComponents()
        {
            RegisterComponent(new GenericHardwareComponent("FC_PIXHAWK6X", "Pixhawk 6X Autopilot", HardwareCategory.FlightController, 3.5f, 38f));
            RegisterComponent(new GenericHardwareComponent("PDB_HOLYBRO_PM02", "Holybro Power Module 02", HardwareCategory.PowerSystem, 0.5f, 22f));
            RegisterComponent(new GenericHardwareComponent("COMP_RASPBERRYPI5", "Raspberry Pi 5 Companion Computer", HardwareCategory.CompanionComputer, 12.0f, 46f));
            RegisterComponent(new GenericHardwareComponent("CAM_SIYI_A8", "SIYI A8 Mini 4K Gimbal Camera", HardwareCategory.VisionCamera, 5.0f, 150f));
            RegisterComponent(new GenericHardwareComponent("RC_EXPRESSLRS_24", "ExpressLRS 2.4GHz RX", HardwareCategory.RadioReceiver, 0.4f, 4f));
        }

        /// <summary>
        /// Registers a hardware component into the central registry.
        /// </summary>
        /// <param name="component">Component instance implementing IDroneHardwareComponent.</param>
        /// <returns>True if registration succeeded, false if component is null or already exists.</returns>
        public bool RegisterComponent(IDroneHardwareComponent component)
        {
            if (component == null || string.IsNullOrEmpty(component.ComponentId))
            {
                Debug.LogWarning("[DroneComponentRegistry] Cannot register invalid component.");
                return false;
            }

            if (registeredComponents.ContainsKey(component.ComponentId))
            {
                Debug.LogWarning($"[DroneComponentRegistry] Component ID '{component.ComponentId}' is already registered.");
                return false;
            }

            registeredComponents.Add(component.ComponentId, component);
            component.InitializeComponent();
            OnComponentRegistered?.Invoke(component);
            return true;
        }

        /// <summary>
        /// Unregisters a hardware component by its unique string identifier.
        /// </summary>
        /// <param name="componentId">Unique identifier of the component to remove.</param>
        /// <returns>True if component was found and unregistered, false otherwise.</returns>
        public bool UnregisterComponent(string componentId)
        {
            if (string.IsNullOrEmpty(componentId) || !registeredComponents.TryGetValue(componentId, out var component))
            {
                return false;
            }

            component.ShutdownComponent();
            registeredComponents.Remove(componentId);
            OnComponentUnregistered?.Invoke(componentId);
            return true;
        }

        /// <summary>
        /// Gets a registered component by its ID.
        /// </summary>
        /// <param name="componentId">Target component ID.</param>
        /// <returns>Component instance or null if not found.</returns>
        public IDroneHardwareComponent GetComponentById(string componentId)
        {
            if (string.IsNullOrEmpty(componentId)) return null;
            registeredComponents.TryGetValue(componentId, out var component);
            return component;
        }

        /// <summary>
        /// Returns all registered components matching a specific HardwareCategory.
        /// </summary>
        /// <param name="category">Hardware category filter.</param>
        /// <returns>List of matching hardware components.</returns>
        public List<IDroneHardwareComponent> GetComponentsByCategory(HardwareCategory category)
        {
            return registeredComponents.Values.Where(c => c.Category == category).ToList();
        }

        /// <summary>
        /// Calculates total electrical power consumed by all currently enabled hardware components in Watts.
        /// </summary>
        /// <returns>Total active power consumption in Watts (W).</returns>
        public float GetTotalPowerConsumptionWatts()
        {
            return registeredComponents.Values.Where(c => c.IsEnabled).Sum(c => c.PowerConsumptionWatts);
        }

        /// <summary>
        /// Calculates total dry mass of all registered hardware components in grams.
        /// </summary>
        /// <returns>Total weight in grams (g).</returns>
        public float GetTotalWeightGrams()
        {
            return registeredComponents.Values.Sum(c => c.WeightGrams);
        }
    }
}



