using System;
using System.Collections;
using UnityEngine;
using ASTRA.UAV.Core;

namespace ASTRA.UAV.Managers
{
    /// <summary>
    /// Event broadcast when the simulation timescale is modified.
    /// </summary>
    public struct SimulationTimeScaleChangedEvent : IEvent
    {
        public float PreviousTimeScale { get; }
        public float NewTimeScale { get; }

        public SimulationTimeScaleChangedEvent(float previousTimeScale, float newTimeScale)
        {
            PreviousTimeScale = previousTimeScale;
            NewTimeScale = newTimeScale;
        }
    }

    /// <summary>
    /// Event broadcast when simulation state is reset.
    /// </summary>
    public struct SimulationResetEvent : IEvent
    {
    }

    /// <summary>
    /// Controls simulation engine timing, time scale, physics step size, pause/resume state, and single-step execution.
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        private float _previousTimeScale = 1.0f;

        /// <summary>
        /// Gets whether simulation engine is currently paused (Time.timeScale == 0).
        /// </summary>
        public bool IsPaused => Time.timeScale == 0f;

        /// <summary>
        /// Gets or sets current simulation time scale multiplier.
        /// </summary>
        public float TimeScale
        {
            get => Time.timeScale;
            private set => SetTimeScale(value);
        }

        /// <summary>
        /// Accumulated simulation running time (unpaused seconds).
        /// </summary>
        public float TotalSimulatedTime { get; private set; }

        /// <summary>
        /// Action callback invoked on timescale changes.
        /// </summary>
        public event Action<float> OnTimeScaleChanged;

        /// <summary>
        /// Action callback invoked when simulation is reset.
        /// </summary>
        public event Action OnSimulationReset;

        private void Awake()
        {
            ServiceLocator.Register<SimulationManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SimulationManager>();
        }

        private void Update()
        {
            if (!IsPaused)
            {
                TotalSimulatedTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// Sets the simulation timescale multiplier (e.g. 0.5x, 1.0x, 2.0x).
        /// </summary>
        /// <param name="scale">Target time scale factor (0.0 to 10.0).</param>
        public void SetTimeScale(float scale)
        {
            float targetScale = Mathf.Clamp(scale, 0f, 10f);
            if (Mathf.Approximately(Time.timeScale, targetScale)) return;

            float prev = Time.timeScale;
            Time.timeScale = targetScale;

            if (targetScale > 0f)
            {
                _previousTimeScale = targetScale;
            }

            Debug.Log($"[SimulationManager] TimeScale updated to: {targetScale}x");

            OnTimeScaleChanged?.Invoke(targetScale);
            EventBus.Publish(new SimulationTimeScaleChangedEvent(prev, targetScale));
        }

        /// <summary>
        /// Pauses simulation engine by setting timeScale to 0.
        /// </summary>
        public void PauseSimulation()
        {
            if (Time.timeScale > 0f)
            {
                _previousTimeScale = Time.timeScale;
                SetTimeScale(0f);
            }
        }

        /// <summary>
        /// Resumes simulation engine to previously configured non-zero timescale.
        /// </summary>
        public void ResumeSimulation()
        {
            if (IsPaused)
            {
                SetTimeScale(_previousTimeScale > 0f ? _previousTimeScale : 1.0f);
            }
        }

        /// <summary>
        /// Toggles between paused state and running state.
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused) ResumeSimulation();
            else PauseSimulation();
        }

        /// <summary>
        /// Advances physics simulation by a single time step while paused.
        /// </summary>
        /// <param name="stepDeltaTime">Step duration in seconds (default 0.02s).</param>
        public void SingleStep(float stepDeltaTime = 0.02f)
        {
            StartCoroutine(SingleStepRoutine(stepDeltaTime));
        }

        /// <summary>
        /// Resets simulation time accumulator and notifies listeners.
        /// </summary>
        public void ResetSimulation()
        {
            TotalSimulatedTime = 0f;
            Debug.Log("[SimulationManager] Simulation state reset.");

            OnSimulationReset?.Invoke();
            EventBus.Publish(new SimulationResetEvent());
        }

        private IEnumerator SingleStepRoutine(float stepDeltaTime)
        {
            Time.timeScale = 1.0f;
            UnityEngine.Physics.Simulate(stepDeltaTime);
            yield return new WaitForFixedUpdate();
            Time.timeScale = 0.0f;
            Debug.Log($"[SimulationManager] Stepped simulation by {stepDeltaTime}s.");
        }
    }
}


