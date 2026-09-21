# Odd & Even Balloons

A Unity and C# number-recognition game. Catch the moving balloons that match the current odd or even target.

**Game design and development: [Ufuk Bayhan](https://github.com/UfukBayhan)**

The in-game interface remains in Turkish. This repository contains a standalone portfolio edition with simple UI artwork and no account or server dependency.

![Mode selection](Docs/menu.png)

## Gameplay

Read the odd/even instruction and click matching balloons as they rise. Correct catches reduce the remaining goal; incorrect catches increase the mistake counter. Complete the goal to advance. The mixed mode can change its target each level.

The HUD shows the current level, elapsed time and mistakes. **Modlar** (Modes) or **Escape** returns to mode selection. **Yeniden başla** (Restart) resets the current mode to its first level.

## Modes

| Mode | Scene |
| --- | --- |
| Even numbers | `CiftSayilar` |
| Mixed parity | `CiftTekSayilar` |
| Odd numbers | `TekSayilar` |

## Screenshots

Captured from the running Windows development build. These show actual gameplay in different modes and levels.

### Even numbers

![ Even numbers gameplay](Docs/CiftSayilar-level1.png)

![ Even numbers — level 2](Docs/CiftSayilar-level2.png)

### Mixed parity

![ Mixed parity gameplay](Docs/CiftTekSayilar-level1.png)

### Odd numbers

![ Odd numbers gameplay](Docs/TekSayilar-level1.png)

## Getting started

1. Clone this repository or download it as a ZIP archive.
2. Add the project folder through Unity Hub.
3. Open it with **Unity 6000.3.9f1** and allow package imports to finish.
4. Open `Assets/Game/Scenes/ModeSelect.unity` and press **Play**.
5. Select a mode and follow its instructions.

The editor opens the mode-selection scene automatically when starting from a clean, unnamed empty scene. You can also use **Portfolio → Open Mode Selection**. If the Game tab is missing, use **Window → General → Game**.

## Project structure

| File / folder | Purpose |
| --- | --- |
| `Assets/Game/Scenes` | Mode selection and individual game scenes |
| `OddOrEvenGame.cs` | Core game rules, generation and progression |
| `StemGameManager.cs` | Shared local session management |
| `SessionHud.cs` | Level, time, mistakes and restart |
| `ModeNavigation.cs` | Mode selection links |
| `FeedbackEffect.cs` | Answer feedback and short generated tones |
| `Assets/Resources/PortfolioConfig.json` | Mode settings and labels |
| `Assets/Editor/PortfolioBuilder.cs` | Scene generation and Windows build tool |

The original parity rules, spawn selection and difficulty curves are retained. Movement is adapted to canvas coordinates, and old balloons are cleared at level transitions.

UI elements are built with Unity UI and TextMesh Pro. Scene and script metadata is included for reproducible Unity imports.

## Build and validation

Use **File → Build Profiles** with the Windows target. The enabled scene list includes `ModeSelect` first, followed by all game modes. Build outputs, local logs and Unity caches are excluded from Git.

`PortfolioBuilder.Build` generates scenes and creates `Builds/Windows/Game.exe`. **It overwrites generated scenes**, so commit or back up manual scene edits before using it.

Run the development player with `--portfolio-checks` to execute the opt-in checks and capture the gallery under `Docs` when using the default build location. The checks exercise UI callbacks, accelerate time where needed, and do not run during normal gameplay. They are not a substitute for testing physical touch or keyboard input on every device.

Automated Windows-player checks passed for all 3 modes (55 assertions). Checks cover mode navigation, answer behavior, repeated input, level progression, restart and menu return.

Validation targets Windows. Mobile-device testing has not been performed.

## Third-party components

Liberation Sans is distributed under the SIL Open Font License. Its license is included at `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. Existing Unity and TextMesh Pro notices are preserved.

This repository is shared for portfolio presentation and review. No separate open-source license has been granted.
