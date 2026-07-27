using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Supported state machine operational states.
    /// </summary>
    public enum DroneOperationalState
    {
        Idle,
        Initializing,
        Armed,
        Takeoff,
        Hover,
        Flying,
        Landing,
        Disarmed,
        EmergencyStop
    }

    /// <summary>
    /// Event broadcast when drone state machine transitions.
    /// </summary>
    public struct DroneStateChangedEvent : IEvent
    {
        public DroneOperationalState PreviousState;
        public DroneOperationalState NewState;
    }

    /// <summary>
    /// Event-driven state machine driving drone lifecycle (Idle, Armed, Takeoff, Hover, Flying, Landing, EmergencyStop).
    /// </summary>
    public class DroneStateMachine : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private DroneOperationalState currentState = DroneOperationalState.Idle;

        public DroneOperationalState CurrentState => currentState;

        private void Start()
        {
            SetState(DroneOperationalState.Idle);
        }

        /// <summary>
        /// Transitions to a new operational state.
        /// </summary>
        public void SetState(DroneOperationalState newState)
        {
            if (currentState == newState) return;

            DroneOperationalState oldState = currentState;
            currentState = newState;

            EventBus.Publish(new DroneStateChangedEvent
            {
                PreviousState = oldState,
                NewState = newState
            });

            Debug.Log($"Drone State transitioned: {oldState} -> {newState}", LogCategory.Drone);
        }
    }
}



