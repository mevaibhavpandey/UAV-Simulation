using System;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Event fired when a drone is spawned into the simulation environment.
    /// </summary>
    public struct DroneSpawnedEvent : IEvent
    {
        public GameObject DroneInstance { get; }
        public DroneSpawnedEvent(GameObject droneInstance) => DroneInstance = droneInstance;
    }

    /// <summary>
    /// Event fired when a drone is despawned or destroyed.
    /// </summary>
    public struct DroneDespawnedEvent : IEvent
    {
        public GameObject DroneInstance { get; }
        public DroneDespawnedEvent(GameObject droneInstance) => DroneInstance = droneInstance;
    }

    /// <summary>
    /// Event fired when the currently active control drone changes.
    /// </summary>
    public struct ActiveDroneChangedEvent : IEvent
    {
        public GameObject NewActiveDrone { get; }
        public ActiveDroneChangedEvent(GameObject newActiveDrone) => NewActiveDrone = newActiveDrone;
    }

    /// <summary>
    /// Manages active drone instances, handles drone registration, spawning, despawning stubs, and lifecycle events.
    /// </summary>
    public class DroneManager : MonoBehaviour
    {
        [Header("Default Spawning Configuration")]
        [SerializeField] private GameObject _defaultDronePrefab;
        [SerializeField] private Vector3 _defaultSpawnPosition = new Vector3(0f, 1f, 0f);
        [SerializeField] private Quaternion _defaultSpawnRotation = Quaternion.identity;

        /// <summary>
        /// Gets the current active drone GameObject instance.
        /// </summary>
        public GameObject ActiveDrone { get; private set; }

        /// <summary>
        /// Gets the Transform component of the active drone, or null if none exists.
        /// </summary>
        public Transform ActiveDroneTransform => ActiveDrone != null ? ActiveDrone.transform : null;

        /// <summary>
        /// Returns true if an active drone is currently loaded and registered.
        /// </summary>
        public bool HasActiveDrone => ActiveDrone != null;

        /// <summary>
        /// Action callback when an active drone is registered or spawned.
        /// </summary>
        public event Action<GameObject> OnDroneSpawned;

        /// <summary>
        /// Action callback when active drone is despawned or removed.
        /// </summary>
        public event Action<GameObject> OnDroneDespawned;

        private void Awake()
        {
            ServiceLocator.Register<DroneManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<DroneManager>();
        }

        /// <summary>
        /// Spawns a drone instance from the provided prefab at the given location.
        /// </summary>
        /// <param name="prefab">Drone prefab to instantiate. Uses default if null.</param>
        /// <param name="position">World position for spawn.</param>
        /// <param name="rotation">World rotation for spawn.</param>
        /// <returns>The spawned drone GameObject.</returns>
        public GameObject SpawnDrone(GameObject prefab = null, Vector3? position = null, Quaternion? rotation = null)
        {
            GameObject prefabToSpawn = prefab != null ? prefab : _defaultDronePrefab;
            if (prefabToSpawn == null)
            {
                Debug.LogError("[DroneManager] Cannot spawn drone: No prefab assigned or provided.");
                return null;
            }

            if (ActiveDrone != null)
            {
                DespawnDrone();
            }

            Vector3 spawnPos = position ?? _defaultSpawnPosition;
            Quaternion spawnRot = rotation ?? _defaultSpawnRotation;

            ActiveDrone = Instantiate(prefabToSpawn, spawnPos, spawnRot);
            ActiveDrone.name = "ASTRA_Active_Drone";

            Debug.Log($"[DroneManager] Drone spawned successfully at {spawnPos}.");

            OnDroneSpawned?.Invoke(ActiveDrone);
            EventBus.Publish(new DroneSpawnedEvent(ActiveDrone));
            EventBus.Publish(new ActiveDroneChangedEvent(ActiveDrone));

            return ActiveDrone;
        }

        /// <summary>
        /// Manually registers an existing in-scene GameObject as the active drone.
        /// </summary>
        /// <param name="drone">GameObject representing the drone.</param>
        public void RegisterDrone(GameObject drone)
        {
            if (drone == null)
            {
                Debug.LogError("[DroneManager] Cannot register null drone GameObject.");
                return;
            }

            ActiveDrone = drone;
            Debug.Log($"[DroneManager] Registered drone: {drone.name}");

            OnDroneSpawned?.Invoke(ActiveDrone);
            EventBus.Publish(new DroneSpawnedEvent(ActiveDrone));
            EventBus.Publish(new ActiveDroneChangedEvent(ActiveDrone));
        }

        /// <summary>
        /// Despawns and destroys the currently active drone instance.
        /// </summary>
        public void DespawnDrone()
        {
            if (ActiveDrone == null) return;

            GameObject instanceToDestroy = ActiveDrone;
            ActiveDrone = null;

            OnDroneDespawned?.Invoke(instanceToDestroy);
            EventBus.Publish(new DroneDespawnedEvent(instanceToDestroy));
            EventBus.Publish(new ActiveDroneChangedEvent(null));

            Destroy(instanceToDestroy);
            Debug.Log("[DroneManager] Active drone despawned and destroyed.");
        }
    }
}



