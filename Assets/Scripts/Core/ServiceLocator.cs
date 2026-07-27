using System;
using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// Lightweight dependency injection service locator for managing global application services and managers.
    /// Enables loose coupling between managers and core systems.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers a service instance under the specified interface or class type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The key type to register the service under.</typeparam>
        /// <param name="service">The service instance.</param>
        /// <param name="overwrite">If true, overwrites any existing registration for type T.</param>
        public static void Register<T>(T service, bool overwrite = true) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Cannot register null instance for service type {typeof(T).Name}.");
                return;
            }

            lock (_lock)
            {
                Type serviceType = typeof(T);
                if (_services.ContainsKey(serviceType))
                {
                    if (overwrite)
                    {
                        _services[serviceType] = service;
                        Debug.Log($"[ServiceLocator] Re-registered service for type: {serviceType.Name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ServiceLocator] Service of type {serviceType.Name} already registered.");
                    }
                }
                else
                {
                    _services.Add(serviceType, service);
                    Debug.Log($"[ServiceLocator] Registered service: {serviceType.Name}");
                }
            }
        }

        /// <summary>
        /// Unregisters a service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to unregister.</typeparam>
        public static void Unregister<T>() where T : class
        {
            lock (_lock)
            {
                Type serviceType = typeof(T);
                if (_services.Remove(serviceType))
                {
                    Debug.Log($"[ServiceLocator] Unregistered service: {serviceType.Name}");
                }
            }
        }

        /// <summary>
        /// Retrieves the registered service instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service requested.</typeparam>
        /// <returns>The registered service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
        public static T Get<T>() where T : class
        {
            lock (_lock)
            {
                Type serviceType = typeof(T);
                if (_services.TryGetValue(serviceType, out var service))
                {
                    return (T)service;
                }
            }

            Debug.LogError($"[ServiceLocator] Requested service of type {typeof(T).Name} is not registered!");
            throw new InvalidOperationException($"Service of type {typeof(T).Name} not found in ServiceLocator.");
        }

        /// <summary>
        /// Tries to retrieve the registered service instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service requested.</typeparam>
        /// <param name="service">Outputs the service instance if found, or null otherwise.</param>
        /// <returns>True if service exists and was retrieved, false otherwise.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            lock (_lock)
            {
                Type serviceType = typeof(T);
                if (_services.TryGetValue(serviceType, out var instance))
                {
                    service = (T)instance;
                    return true;
                }
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Clears all registered services from the locator.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _services.Clear();
                Debug.Log("[ServiceLocator] All registered services cleared.");
            }
        }
    }
}


