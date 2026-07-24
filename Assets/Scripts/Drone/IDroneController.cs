//-----------------------------------------------------------------------
// <copyright file="IDroneController.cs" company="ASTRA UAV">
//     Copyright (c) ASTRA UAV. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ASTRA.UAV.Drone
{
    /// <summary>
    /// Interface defining flight control input contracts and operational state management for quadcopter controllers.
    /// </summary>
    public interface IDroneController
    {
        /// <summary>
        /// Gets or sets the target roll control input normalized between [-1.0, 1.0].
        /// Positive values roll right, negative values roll left.
        /// </summary>
        float RollInput { get; set; }

        /// <summary>
        /// Gets or sets the target pitch control input normalized between [-1.0, 1.0].
        /// Positive values pitch forward, negative values pitch backward.
        /// </summary>
        float PitchInput { get; set; }

        /// <summary>
        /// Gets or sets the target yaw control input normalized between [-1.0, 1.0].
        /// Positive values yaw clockwise (yaw right), negative values yaw counter-clockwise (yaw left).
        /// </summary>
        float YawInput { get; set; }

        /// <summary>
        /// Gets or sets the target throttle control input normalized between [0.0, 1.0].
        /// 0 represents zero thrust, 1 represents maximum thrust.
        /// </summary>
        float ThrottleInput { get; set; }

        /// <summary>
        /// Gets a value indicating whether the flight controller is active and processing inputs.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Sets all flight control axes simultaneously with safety bounds checking.
        /// </summary>
        /// <param name="pitch">Pitch input [-1.0, 1.0].</param>
        /// <param name="roll">Roll input [-1.0, 1.0].</param>
        /// <param name="yaw">Yaw input [-1.0, 1.0].</param>
        /// <param name="throttle">Throttle input [0.0, 1.0].</param>
        void SetControlInputs(float pitch, float roll, float yaw, float throttle);

        /// <summary>
        /// Resets all control axis inputs to their neutral baseline state.
        /// </summary>
        void ResetInputs();
    }
}
