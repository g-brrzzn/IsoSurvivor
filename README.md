# 🛡️ Isometric Survivor (MonoGame)

A 2D Action-Survival game (Survivor-like) built with **C# and MonoGame** featuring an isometric perspective. This project demonstrates a complete game loop including procedural map generation, spatial partitioning for physics, a custom map editor, and a scalable entity component structure.

## 🎥 Demonstration

<img width="1917" height="1073" alt="p4" src="https://github.com/user-attachments/assets/4373e298-1cd8-444a-8af2-8559f312ae10" />
<img width="1901" height="1052" alt="p3" src="https://github.com/user-attachments/assets/f94240a2-bf99-4ddf-a180-994072a40ca3" />
<img width="1912" height="1067" alt="p2" src="https://github.com/user-attachments/assets/707fc7da-3a35-4fda-8414-e09144d0ac87" />
<img width="1911" height="1071" alt="p1" src="https://github.com/user-attachments/assets/f851191b-f8c9-4522-897c-88e4f434d01a" />


## 🚀 Features

- **Isometric Engine:** Custom rendering logic converting world coordinates to isometric screen space with depth sorting (`IsoMath`).
- **Procedural Generation:** Algorithms to generate infinite-feeling arenas with lakes, dirt patches, and walls.
- **Custom Map Editor:** A built-in tool to create, edit, and save maps (`.json`) with tile placement and trigger zones.
- **Optimization:** Implements **Spatial Hashing** (`SpatialGrid`) to handle collision detection for hundreds of entities efficiently.
- **Gameplay Systems:**
  - **Upgrade System:** Level-up logic with random weapon/passive upgrades (RNG).
  - **Weapons:** Auto-firing wands, orbiting shields, and projectile logic.
  - **Pathfinding:** A* algorithm implementation for enemy navigation.
  - **State Management:** Robust Finite State Machine handling Menu, Game, Editor, Pause, and Level Up states.

## 🎮 Controls

### Gameplay
- **WASD / Arrows:** Move Character.
- **ESC:** Pause Game.
- **Space/Enter:** Confirm selections (Level Up).
- *Attacks are automatic based on weapon cooldowns.*

### Map Editor
- **Tab:** Switch between **Tile Mode** and **Trigger Mode**.
- **WASD / Arrows:** Pan Camera.
- **Left Click:** Place Tile / Select Trigger.
- **Right Click:** Erase Tile / Add Trigger.
- **Q / E:** Cycle through Tile Palette.
- **Ctrl + S:** Save Map.
- **Ctrl + N:** New Map.
- **Scroll:** Zoom In/Out.

## 📦 Installation & Build

1. **Prerequisites**
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) (with .NET Desktop Development workload).

2. **Clone and Open**
   ```bash
   git clone https://github.com/g-brrzzn/IsometricGame-MonoGame.git
   Open the .sln file in Visual Studio.
   Build
   Press F5 to build and run the project.
   Ensure Content is built correctly via the MonoGame Content Pipeline if modifying assets.
   
## 🛠️ Tech Stack
- Language: C# (.NET 8)
- Framework: MonoGame
- Data: JSON (Newtonsoft) for Map serialization.
- Physics: Custom AABB implementation with Isometric projection.

<br>
Developed as a personal project to explore game architecture patterns and MonoGame capabilities.
