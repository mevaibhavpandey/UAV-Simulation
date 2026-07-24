namespace ASTRA.UAV.AI
{
    /// <summary>
    /// Contract interface for modular autonomous AI subsystems (perception, planning, SLAM, target tracking).
    /// </summary>
    public interface IAIModule
    {
        /// <summary>Gets the human-readable name of the AI subsystem module.</summary>
        string ModuleName { get; }

        /// <summary>Gets whether the AI module is initialized and actively processing.</summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initializes AI module resources, models, and worker threads.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Update loop step invoked per frame or update tick.
        /// </summary>
        /// <param name="deltaTime">Delta time since last update step.</param>
        void UpdateModule(float deltaTime);

        /// <summary>
        /// Shuts down module execution and releases native inference resources.
        /// </summary>
        void Shutdown();
    }
}
