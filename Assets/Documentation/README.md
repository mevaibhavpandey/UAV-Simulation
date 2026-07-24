# ASTRA UAV Simulation System

## Overview
ASTRA UAV (Unmanned Aerial Vehicle) Simulation System is a high-fidelity, extensible Unity 6 simulation environment designed for autonomous drone flight dynamics, telemetry monitoring, mission planning, hardware-in-the-loop (HITL) integration, and AI-driven navigation.

## Key Features
- **Realistic Physics & Flight Controller**: Multi-rotor thrust dynamics, aerodynamic drag, ground effect, and PID/state-space attitude stabilization.
- **Geographic & Spatial Navigation**: WGS84 Geodetic (Lat/Lon/Alt) to Local Cartesian coordinate transforms and NED/ENU conversions.
- **Mission Engine**: Waypoint navigation, automated survey grid generation, failsafe execution (Return to Home, Land, Hover).
- **Telemetry & Ground Control System**: Real-time broadcast and logging of IMU, GPS, battery, state data over MAVLink/Custom protocols.
- **Computer Vision & SLAM Simulation**: Simulated depth cameras, LiDAR obstacle detection, visual-inertial odometry interfaces.
- **Universal Render Pipeline (URP)**: Optimized for performance across high-end desktop workstations and simulation nodes.

## Prerequisites
- **Unity Editor**: `6000.0.0f1` (Unity 6) or higher.
- **Render Pipeline**: Universal Render Pipeline (URP).
- **Input System**: Unity New Input System Package `com.unity.inputsystem`.

## Getting Started
1. Open the project in Unity 6 (`6000.0.0f1`).
2. Open the main simulation scene in `Assets/Scenes/MainSimulation.unity`.
3. Press **Play** to run the drone flight controller and telemetry stream.

## Directory Structure
- `Assets/Scripts/Core`: Application bootstrapper, core system lifecycles.
- `Assets/Scripts/Drone`: Flight physics, motor control, state estimation.
- `Assets/Scripts/Interfaces`: Contract interfaces for modular system swapping.
- `Assets/Scripts/Managers`: Systems managers (Simulation, Mission, Audio, Input).
- `Assets/Scripts/Mission`: Mission planner, waypoint navigation, failsafe handlers.
- `Assets/Scripts/Physics`: Aerodynamics, wind vector models, ground effect.
- `Assets/Scripts/Simulation`: Time scaling, hardware-in-the-loop adapters.
- `Assets/Scripts/Telemetry`: Telemetry providers, data loggers, network bridges.
- `Assets/Scripts/UI`: Flight Heads-Up Display (HUD), Ground Control Station (GCS) panels.
- `Assets/Scripts/Utilities`: Math, Geo, Logger, and C# extension utilities.

## License
Internal Architectural System for ASTRA UAV Framework.
