# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Start

This is a Fire Emblem-style tactical RPG built with **Unity 6** (C#). 
It uses **Domain-Driven Design** with **Hexagonal Architecture** — pure domain logic separated from Unity adapters, with all game rules testable without the runtime. And when building we use TDD and SOLID principles.

## Build & Test Commands

### Running Tests
```bash
# In Unity Editor: Window → Testing → Test Runner (or Ctrl+Alt+T)
# Click "EditMode" tab → "Run All"
# All 46+ NUnit tests run without the Unity runtime

# Command line (optional):
unity -projectPath /Users/r2d2/Projects/fire-emblem/tactic-fantasy -runTests -testCategory EditMode
```

### Opening in Unity
```bash
# Open project in Unity 6 via Hub, or:
open -a "Unity" /Users/r2d2/Projects/fire-emblem/tactic-fantasy
```

### Building Standalone
```bash
# Via Unity Editor:
# File → Build Settings → Select platform (macOS/WebGL) → Build
```

## High-Level Architecture

The codebase is split into three layers with **zero circular dependencies**:

### 1. Domain Layer (`Assets/Scripts/Domain/`)
**Pure C# with zero Unity dependencies.** All game rules, formulas, and logic live here. Contains:

- **Units/** — Character stats, classes, team, experience/level system
- **Weapons/** — Weapon stats, types, damage types, weapon triangle (advantage/disadvantage)
- **Map/** — 16x16 procedural grid, terrain types, A* pathfinding with movement costs
- **Combat/** — All combat formulas (damage, hit rate, critical, attack speed, doubles, counters)
- **Turn/** — Phase state machine (PlayerPhase → EnemyPhase → GameOver)
- **AI/** — Heuristic AI for enemy movement and targeting

**Key Design Patterns:**
- Interfaces for all major types (`IUnit`, `IGameMap`, `ICombatResolver`)
- Immutable value objects where possible (CharacterStats, CombatResult)
- Constructor injection for testability
- No `using UnityEngine` statements anywhere

### 2. Adapter Layer (`Assets/Scripts/Adapters/`)
**MonoBehaviour-based bridges** that connect domain logic to Unity:

- **GameController.cs** — Main orchestrator. Handles initialization, player input, enemy AI execution, phase transitions, win/loss conditions. Never duplicates business logic.
- **InputHandler.cs** — Mouse click detection with raycast-based tile/unit selection.
- **GamepadCursorController.cs** — Gamepad input handling (Nintendo Switch Pro compatible). Emits events for cursor movement, confirm, cancel, end turn, toggle attack range.
- **MapRenderer.cs** — Renders tiles as quads with color-coded terrain. Highlights movement/attack range dynamically.
- **UnitRenderer.cs** — Renders units as spheres (Blue=Player, Red=Enemy) with world-space HP bars.
- **CursorRenderer.cs** — Pulsating yellow quad for gamepad cursor position.
- **UIManager.cs** — HUD (turn counter, phase indicator, selected unit info, combat results, End Turn button, game over screen).
- **GameSceneSetup.cs** — Auto-initializes GameController and all components when placed in a scene.

**Architecture Principle:** Domain is completely independent. Adapters depend on Domain, never the reverse.

### 3. Tests (`Assets/Scripts/Tests/`)
**NUnit tests for domain logic only.** No Unity runtime required — tests compile and run instantly.

- CombatResolverTests.cs
- WeaponTriangleTests.cs
- PathFinderTests.cs
- TurnManagerTests.cs
- StatusEffectTests.cs
- ExperienceSystemTests.cs
- GameSaveServiceTests.cs
- AIControllerTests.cs
- GamepadCursorControllerTests.cs
- UnitDisplayFormatterTests.cs

## Core Game Systems

### Combat System
Located in `Domain/Combat/CombatResolver.cs`. Key formulas:

- **Damage (Physical)**: `max(0, (STR + Weapon.Might ± Triangle) - (Enemy.DEF ± Terrain))`
- **Damage (Magical)**: `max(0, (MAG + Weapon.Might ± Triangle) - (Enemy.RES ± Terrain))`
- **Hit Rate**: `(SKL * 2) + (LCK / 2) + Weapon.Hit ± Triangle - (Enemy.AS * 2) - Enemy.LCK ± Terrain`
  - Uses **True Hit**: average of 2 RNG rolls (0-99)
- **Critical**: `(SKL / 2) + Weapon.Crit - Enemy.LCK` (3x damage multiplier)
- **Attack Speed**: `SPD - max(0, Weapon.Weight - STR)`
- **Double Attack**: Triggers when `Attacker.AS - Defender.AS ≥ 4`
- **Weapon Triangle**: Sword > Axe > Lance (only physical). Advantage = +1 damage, +10 hit; Disadvantage = -1 damage, -10 hit.

### Terrain System
Located in `Domain/Map/TerrainType.cs`. Properties per terrain:

- **Plain** — 1 move cost, no defense bonus
- **Forest** — 2 move cost, +15 avoid
- **Fort** — 1 move cost, +2 defense, heals 20% HP per turn
- **Mountain** — 3 move cost (impassable for cavalry)
- **Wall** — Impassable

Pathfinding respects these costs via `PathFinder.cs` (A* with movement cost awareness).

### Unit Classes & Stats
Located in `Domain/Units/ClassData.cs`. 6 base classes (Myrmidon, Soldier, Fighter, Mage, Archer, Cleric) with:
- Base stats (HP, STR, MAG, SKL, SPD, LCK, DEF, RES, MOV)
- Cap stats (level 20 maximum)
- Growth rates (probabilistic stat gains on level-up via `Unit.GainExperience()`)

### Experience & Leveling
Located in `Domain/Units/Unit.cs` and `Domain/Combat/CombatXp.cs`:
- Units gain XP from combat (kill bonus: 60, damage bonus: 20, survived bonus: 10)
- On level-up, stats grow probabilistically capped by CapStats
- MaxHP updates; CurrentHP heals by the HP gain
- Level cap: 20

### Status Effects
Located in `Domain/Units/StatusEffect.cs`. Types: Poison, Sleep, Stun. Applied via `Unit.ApplyStatus()`, tick down at end of EnemyPhase.

### AI System
Located in `Domain/AI/AIController.cs`. Heuristic strategy:
- Finds nearest player unit within attack range
- Moves to position with best attack option (considering terrain defense bonus)
- Targets lowest HP enemy (maximizes threat elimination)
- Healers prioritize most injured allies
- No multi-turn planning; decisions made each turn

## Important Architectural Rules

1. **No Domain-to-Adapter Dependencies**: Domain classes never reference MonoBehaviour, Renderer, Camera, or any Unity types.
2. **Adapters Are Thin**: All business logic in Domain. Adapters only handle rendering, input, and orchestration.
3. **Interfaces Over Implementations**: Domain exposes `IUnit`, `IGameMap`, `ICombatResolver`, etc. Tests inject mocks.
4. **Constructor Injection**: All domain dependencies injected at construction (e.g., `CombatResolver` takes no constructor args; `PathFinder(IGameMap)` takes a map).
5. **No Static Singletons in Domain**: All instances passed via constructor.
6. **Tests Are Domain-Only**: No Unity scene setup, no PlayMode tests. EditMode with NUnit.

## Common Development Patterns

### Adding a New Stat or Property to Units
1. Modify `CharacterStats.cs` (value object with 9 fields)
2. Update `ClassData.cs` base/cap/growth for each class
3. Update `CombatResolver.cs` to use the new stat in formulas
4. Add tests in `CombatResolverTests.cs`
5. Update `UnitDisplayFormatter.cs` if needed for UI display

### Adding a New Terrain Type
1. Add to `TerrainType.cs` enum
2. Add properties in `TerrainProperties` static class (movement cost, defense, avoid, healing)
3. Update `GameMap.cs` terrain generation if needed
4. Test pathfinding with new costs in `PathFinderTests.cs`

### Adding a New Unit Class
1. Add static method to `ClassData.cs` (e.g., `public static ClassData Paladin()`)
2. Set base/cap/growth stats
3. Update `UnitFactory.cs` if needed for quick creation
4. Add to test fixtures if needed

### Extending Combat Logic
1. Modify `CombatResolver.ResolveCombat()` or add helper methods
2. Update `CombatResult.cs` to track new outcomes if needed
3. Add tests in `CombatResolverTests.cs` (use `new Unit()` with controlled stats)

### Extending AI Behavior
1. Modify `AIController.DecideAction()` or add helper methods
2. Add heuristics in scoring functions
3. Add tests in `AIControllerTests.cs` (mock the map/units)

## File Naming & Conventions

- **Interfaces**: `IUnit`, `IGameMap`, `ICombatResolver` (I-prefix)
- **Domain classes**: PascalCase (e.g., `CombatResolver`, `WeaponTriangle`)
- **Enums**: PascalCase (e.g., `TerrainType`, `Team`, `Phase`)
- **Value Objects**: PascalCase (e.g., `CharacterStats`, `CombatResult`)
- **MonoBehaviours**: PascalCase (e.g., `GameController`, `MapRenderer`)
- **Tests**: `*Tests.cs` (e.g., `CombatResolverTests.cs`)
- **Constants**: ALL_CAPS (e.g., `ADVANTAGE_DAMAGE = 1`)

## Key Classes to Understand First

Start reading in this order for a mental model:

1. **CharacterStats.cs** — 9-stat value object (HP, STR, MAG, SKL, SPD, LCK, DEF, RES, MOV)
2. **Unit.cs** — Game unit with position, HP, team, weapon, status, level, XP
3. **CombatResolver.cs** — All combat math; start with `ResolveCombat()`
4. **GameMap.cs** — 16x16 grid; procedural terrain generation
5. **PathFinder.cs** — A* algorithm with terrain movement costs
6. **AIController.cs** — Enemy decision-making heuristics
7. **TurnManager.cs** — Phase state machine
8. **GameController.cs** — How everything wires together; see `Awake()` and `Update()`

## Testing Strategy

- **EditMode Only**: Domain has zero Unity dependencies, so all tests are EditMode (no runtime).
- **Arrange-Act-Assert**: All tests follow AAA pattern.
- **Mocks via Constructor**: Tests inject mock `IGameMap`, mock `IUnit`, etc.
- **No Scene Setup**: Tests create plain C# objects; no GameObjects, no Scenes.
- **Example**: `CombatResolverTests.cs` creates `Unit` instances directly and calls `CombatResolver.ResolveCombat()`.

## Documentation Structure

- **README.md** — User guide (how to play, controls, game features, build instructions)
- **IMPLEMENTATION_SUMMARY.md** — Deep technical dive (every system, design decisions, file structure)
- **GAMEPAD_SETUP.md** — Gamepad control mapping and implementation details
- **CLAUDE.md** (this file) — Guidance for Claude Code and future developers

## Code Style Notes

- **No comments on obvious code** — `int damage = maxOf0(...);` is clear; doesn't need "// calculate damage".
- **Complex algorithms get comments** — True Hit formula, A* heuristic, terrain bias scoring.
- **Meaningful variable names** — `attackerCurrentHP`, `defenderDodgeChance`, not `a`, `d`, `c`.
- **Small methods** — `CalculateDamage()`, `CanDoubleAttack()` each do one thing.
- **Immutable parameters** — Methods don't modify input parameters; return new values.

## Potential Pitfalls

1. **Modifying Domain from Adapters** — Adapters call Domain methods; they never modify Domain objects directly. Example: Don't do `unit.currentHP -= 5` in GameController; call `combatResolver.ResolveCombat()` instead.

2. **Adding Unity Imports to Domain** — Any `using UnityEngine;` in Domain is a red flag. Use interfaces instead.

3. **Magic Numbers** — All constants must be named. Search the codebase for `const` to see patterns.

4. **Mutable Value Objects** — `CharacterStats` is a struct; keep it immutable. Create new instances for modifications.

5. **Circular Dependencies** — Domain → Adapters → Unity is the correct flow. Never Adapters → Domain with callbacks.

## Recent Changes (v1.4)

- **Terrain-Aware AI** — AIController now factors terrain defense bonus when scoring attack positions. Fort > Forest > Plain preference.
- **Experience & Leveling** — Units gain XP from combat; probabilistic stat growth on level-up capped by CapStats.
- **Status Effects** — Poison, Sleep, Stun with duration tracking; tick down at end of phase.
- **Save/Load Framework** — `GameSnapshot`, `UnitSnapshot`, `IGameRepository` port, `GameSaveService` (domain not yet persisting to disk).

## Gamepad Support

- **GamepadCursorController.cs** — Reads stick/D-pad input, validates against map bounds, emits events.
- **CursorRenderer.cs** — Renders yellow pulsating quad at cursor position.
- **Nintendo Switch Pro** — Fully compatible via Unity Input System.
- **Controls** — Analog stick or D-pad to move; A=confirm, B=cancel, X=end turn, Y=toggle enemy range display.
- **Coexistence** — Mouse and gamepad both work simultaneously without conflict.

## Performance Notes

- **Pathfinding** — A* runs at ~100k nodes/sec; 16x16 grid is negligible.
- **Combat Resolution** — Real-time; no turn delays or animation loops.
- **AI Decisions** — ~50ms per enemy unit (greedy heuristic).
- **Frame Rate** — Target 60 FPS; all rendering is simple (quads, spheres, world-space UI).

---

**Last Updated**: 2026-04-12
**Version**: v1.4
**Status**: Feature Complete — Terrain-aware AI, Experience/Leveling, Status Effects, Save/Load Framework
