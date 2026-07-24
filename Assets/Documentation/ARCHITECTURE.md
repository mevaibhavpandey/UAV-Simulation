# ASTRA UAV Architecture Specification

## Architecture Overview
The ASTRA UAV simulation system is designed using a highly decoupled, modular layer architecture based on Dependency Inversion and Interface Segregation principles. The system isolates physics calculation, flight control algorithms, hardware telemetry interfaces, and rendering.

```
+-----------------------------------------------------------------------+
|                               UI / HUD                                |
+-----------------------------------------------------------------------+
        |                                                 |
        v                                                 v
+-----------------------+                         +---------------------+
|   Mission Manager     |                         |  Telemetry Provider |
+-----------------------+                         +---------------------+
        |                                                 ^
        v                                                 |
+-----------------------------------------------------------------------+
|                    IDroneController Interface                          |
+-----------------------------------------------------------------------+
        |                                                 |
        +-------------------------+                       |
        v                         v                       v
+-------------------+   +------------------+    +-------------------+
| Physics Dynamics  |   | Hardware Adapter |    | AI Navigation     |
|   (Simulated)     |   |   (HITL Mode)    |    |   (Avoidance)     |
+-------------------+   +------------------+    +-------------------+
```

## Layer Descriptions

### 1. Interfaces Layer (`ASTRA.UAV.Interfaces`)
Defines the strict contracts between simulation components:
- `IDroneController`: Primary contract for commanding flight modes, target vectors, thrust, and reading state.
- `IMissionModule`: Flight plan manager for waypoint sequencing, pattern flight, and mission execution.
- `ITelemetryProvider`: Data source contract for real-time telemetry streaming (GPS, IMU, battery, status).
- `ICameraController`: Camera tracking system (Follow, Gimbal, FPV, Orbit).
- `IAIModule`: Pluggable artificial intelligence module interface for pathfinding and autonomous decisions.
- `IHardwareAdapter`: Communication adapter for real-world flight controllers (Pixhawk, Betaflight, ArduPilot).
- `IObstacleDetector`: Sensor abstraction for distance sensors, LiDAR, and depth-camera obstacle detection.
- `ISLAMProvider`: Interface for Simultaneous Localization and Mapping sensor feeds and state estimation.

### 2. Core & Managers (`ASTRA.UAV.Core`, `ASTRA.UAV.Managers`)
- `SimulationManager`: Coordinates global frame updates, fixed physics stepping, time scaling, and mode selection (SIM vs HITL).
- `MissionManager`: Orchestrates flight mission execution against `IDroneController`.

### 3. Drone & Physics (`ASTRA.UAV.Drone`, `ASTRA.UAV.Physics`)
- Computes rigid body forces, rotor torque, motor spool time, wind interaction vectors, and altitude dynamics.

### 4. Telemetry (`ASTRA.UAV.Telemetry`)
- Packaging, serializing, and transmitting flight logs and telemetry payloads.

### 5. Utilities (`ASTRA.UAV.Utilities`)
- Generic thread-safe `Singleton<T>` pattern.
- WGS84 Geodetic to local Cartesian conversions (`GeoUtilities`).
- North-East-Down (NED) to East-North-Up (ENU Unity) conversion helpers (`MathUtilities`).
- Centralized logging wrapper (`Logger`).
- Concise C# extension methods for Unity data structures (`ExtensionMethods`).
