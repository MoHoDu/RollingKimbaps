# Development Guidelines

## 1. Project Overview

- **Project Name:** Rolling Kimbaps
- **Engine:** Unity
- **Platform:** Mobile (Android)
- **Core Gameplay:** A 2D platformer game where the player controls a "Kimbap" character to overcome obstacles and reach a goal.

## 2. Project Architecture

- **`Assets/`**: Main directory for all game assets.
  - **`Assets/Scenes/`**: Contains all game scenes. The main game scene is `GameScene`.
  - **`Assets/Scripts/`**: Contains all C# scripts.
    - **`Assets/Scripts/Player/`**: Scripts related to player behavior.
    - **`Assets/Scripts/Enemies/`**: Scripts for enemy characters.
    - **`Assets/Scripts/Managers/`**: Game manager scripts (e.g., `GameManager`, `UIManager`).
  - **`Assets/Prefabs/`**: Contains all prefabs.
  - **`Assets/Sprites/`**: Contains all 2D sprites.
  - **`Assets/Animations/`**: Contains all animation files.
- **`ProjectSettings/`**: Unity project settings. Do not modify files in this directory directly unless you have a clear understanding of the impact.

## 3. Code Standards

- **Naming Conventions:**
  - Use PascalCase for class and method names (e.g., `PlayerController`, `MovePlayer`).
  - Use camelCase for variable names (e.g., `playerSpeed`, `isJumping`).
  - Prepend an underscore `_` to private member variables (e.g., `_playerHealth`).
- **Comments:**
  - Add comments to explain complex logic.
  - Do not use comments to state the obvious.
- **Logging:**
  - Use `Debug.Log()` for general information.
  - Use `Debug.LogWarning()` for potential issues.
  - Use `Debug.LogError()` for critical errors.
  - All log messages should be written in English.

## 4. Functionality Implementation Standards

- **Player Movement:**
  - Player movement logic is handled in `PlayerController.cs`.
  - Use the new Input System for player input.
- **UI:**
  - UI elements are managed by `UIManager.cs`.
  - Use prefabs for UI elements.
- **Game State:**
  - The game state (e.g., score, lives) is managed by `GameManager.cs`.

## 5. Framework/Plugin/Third-party Library Usage Standards

- **UniTask:**
  - Use UniTask for asynchronous operations to improve performance.
- **DOTween:**
  - Use DOTween for all animations and visual effects.

## 6. Workflow Standards

- **Scene Management:**
  - Do not make direct changes to the `GameScene`.
  - Create a new scene for testing and development.
- **Prefab Workflow:**
  - Create prefabs for all reusable GameObjects.
  - Apply changes to the prefab, not the instance in the scene.

## 7. Key File Interaction Standards

- When creating a new script that requires input, add it to the `PlayerActions` Input Action Asset.
- When adding a new UI element, update the `UIManager.cs` script to handle its behavior.

## 8. AI Decision-making Standards

- **Error Handling:**
  - If you encounter a compile error, first try to understand the error message and fix it.
  - If you cannot fix the error, revert the changes and ask for help.
- **Asset Creation:**
  - When creating new sprites or animations, ensure they match the existing art style.

## 9. Prohibited Actions

- **Do not** use `GameObject.Find()` in `Update()` or other frequently called methods. Use a reference set in the Inspector or `Start()`.
- **Do not** commit changes that break the build.
- **Do not** add large assets directly to the Git repository. Use Git LFS.
- **Do not** modify the `ProjectSettings/` directory without prior discussion.
