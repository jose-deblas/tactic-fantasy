# COMPUTER INSTRUCTIONS

This document explains how to play the game on a computer using the keyboard and mouse (current supported inputs).

**Keyboard Controls**

- **End Turn**: `Space` or `Enter` — Ends the player's turn when it's your phase. (Implemented in [Assets/Scripts/Adapters/InputHandler.cs](Assets/Scripts/Adapters/InputHandler.cs#L20-L43)).
- **Open / Close Menu**: `Esc` — Toggles the in-game menu (Save, End Turn, Exit). (Implemented in [Assets/Scripts/Adapters/InputHandler.cs](Assets/Scripts/Adapters/InputHandler.cs#L20-L43)).

- **Notes**: There is currently no full keyboard-only cursor or confirm/cancel mapping implemented. Use the mouse (left-click) to select and confirm actions, or a gamepad for full controller support (A=confirm, B=cancel, X=end turn, Y=toggle enemy ranges) as implemented in [Assets/Scripts/Adapters/GamepadCursorController.cs](Assets/Scripts/Adapters/GamepadCursorController.cs#L40-L86).

**Mouse Controls**

- **Select / Confirm / Interact**: `Left mouse button` — Click a unit or tile to select it and show details. If a friendly unit is selected:
  - Click a highlighted movement tile to move the unit.
  - Click a highlighted attack tile (red) to attack the unit on that tile or trigger a special action (e.g., refresh).
  - Clicking the same unit again will deselect it.
  (Selection and tile click handling are in [Assets/Scripts/Adapters/GameController.cs](Assets/Scripts/Adapters/GameController.cs#L60-L160) and [Assets/Scripts/Adapters/InputHandler.cs](Assets/Scripts/Adapters/InputHandler.cs#L1-L60).)

- **Inspect Terrain**: Click any tile (even without a unit selected) to show terrain information (move cost, avoid, defense, heal) in the UI. (See `UIManager.ShowTerrainInfo` in [Assets/Scripts/Adapters/UIManager.cs](Assets/Scripts/Adapters/UIManager.cs#L1-L120)).

- **Other mouse buttons / wheel**: Not used by the current input handler. Only the left mouse button is handled by the game at the moment.

**Quick Examples**

- To move a unit: Left-click the friendly unit → left-click a highlighted movement tile.
- To attack: Left-click the friendly unit → left-click an enemy on a highlighted attack tile.
- To end your turn: Press `Space`/`Enter` or click the End Turn button in the UI.
- To open the menu: Press `Esc`.

**Tips & Known Limitations**

- Keyboard support is intentionally minimal right now (menu + end-turn). If you prefer keyboard-only play, you can use a gamepad (recommended) or I can add keyboard mappings for cursor movement and confirm/cancel on request.
- If `Space`/`Enter` doesn't end the turn, make sure the game is in the Player Phase and no modal menu or interstitial screen is open.

If you'd like, I can extend the keyboard controls to add:
- Arrow keys / WASD for cursor movement.
- `Z` / `Enter` / `Space` for confirm and `X` / `Esc` for cancel.

---
Generated from current input handlers and controller adapters in the project.
