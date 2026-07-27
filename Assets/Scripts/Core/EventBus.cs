using System;
using System.Collections.Generic;

namespace ASTRA.UAV.Core
{
    /// <summary>
    /// Base marker interface for all strongly-typed event payloads passed via EventBus.
    /// </summary>
    public interface IEvent { }

    /// <summary>
    /// Lightweight global publish-subscribe Event Bus for decoupling system events across UAV layers.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribes a callback to events of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="callback">Action to invoke when event occurs.</param>
        public static void Subscribe<T>(Action<T> callback)
        {
            if (callback == null) return;
            Type eventType = typeof(T);

            if (!Subscribers.ContainsKey(eventType))
            {
                Subscribers[eventType] = new List<Delegate>();
            }

            if (!Subscribers[eventType].Contains(callback))
            {
                Subscribers[eventType].Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribes a callback from events of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="callback">Action to remove.</param>
        public static void Unsubscribe<T>(Action<T> callback)
        {
            if (callback == null) return;
            Type eventType = typeof(T);

            if (Subscribers.ContainsKey(eventType))
            {
                Subscribers[eventType].Remove(callback);
                if (Subscribers[eventType].Count == 0)
                {
                    Subscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publishes an event instance of type <typeparamref name="T"/> to all registered subscribers.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="eventData">Event data instance.</param>
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (Subscribers.TryGetValue(eventType, out var list))
            {
                // Copy array to allow unsubscription during iteration safely
                Delegate[] targets = list.ToArray();
                foreach (Delegate del in targets)
                {
                    if (del is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// </summary>
        public static void ClearAllSubscribers()
        {
            Subscribers.Clear();
        }
    }
}


