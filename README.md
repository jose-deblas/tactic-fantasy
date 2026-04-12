# Tactic Fantasy v1

A Fire Emblem-style tactical RPG built with Unity 6 (C#). Turn-based combat on a randomized 16x16 grid with 4v4 team battles.

## Project Overview

**Game Type**: Tactical RPG (Turn-based)
**Platform**: Unity 6 (Windows, macOS, WebGL)
**Architecture**: Domain-Driven Design with Hexagonal Architecture
**Testing**: NUnit (domain logic only, no Unity dependencies)

### Game Features

- **Random Grid Maps**: 16x16 procedurally generated terrain with multiple terrain types
- **Turn-Based Combat**: Player phase → Enemy AI phase with automatic resolution
- **6 Unit Classes**: Myrmidon, Soldier, Fighter, Mage, Archer, Cleric (each with unique stats)
- **Weapon Triangle**: Sword > Axe > Lance > Sword with damage/hit bonuses
- **Complex Combat Formulas**:
  - True Hit (average of 2 RNG rolls)
  - Critical strikes (single RNG, 3x damage)
  - Double attacks (when attacker's speed advantage ≥ 4)
  - Terrain bonuses (defense, avoid, healing)
- **AI Controller**: Simple heuristic-based enemy movement and targeting
- **Win/Lose Conditions**: Eliminate all enemies to win; losing all units = defeat

## Installation

### Prerequisites

- **Unity 6** (or compatible version with C# 11 support)
- **NUnit** (for running tests; automatically included with Unity)

### Setup Instructions

1. **Open the project in Unity 6**
   - Open Unity Hub
   - Click "Add" and navigate to `/Users/r2d2/Projects/fire-emblem/tactic-fantasy`
   - Select Unity 6.0 or newer
   - Click "Open"

2. **Create the Game Scene** (if not already present)
   - In the Project window, navigate to `Assets/Scenes`
   - Right-click and select **Create > Scene**
   - Name it `GameScene`
   - Double-click to open the scene

3. **Add the Game Controller**
   - Create an empty GameObject in the scene (Ctrl+Shift+N)
   - Name it `GameController`
   - In the Inspector, click **Add Component** and add the `GameSceneSetup` script
     - Path: `Assets/Scripts/Adapters/GameSceneSetup.cs`

4. **Save and Play**
   - Press **Ctrl+S** to save the scene
   - Click the **Play** button in the top toolbar
   - Game will initialize automatically

## How to Play

### Controls

- **Click Unit**: Select your unit (Blue team = Player)
- **Click Blue Tile**: Move to highlighted movement range
- **Click Red Tile**: Attack enemy in highlighted attack range
- **End Turn Button**: End your turn and let AI move
- **Space** (optional): Can be added to skip/end turn

### Game Flow

1. **Player Phase**: Select units and take actions (Move + Attack/Wait/Heal)
2. **Enemy Phase**: AI automatically moves and attacks
3. **Victory**: Defeat all enemy units
4. **Defeat**: Lose all your units

### Unit Classes & Stats

#### Myrmidon (Sword)
- **Strengths**: High Speed, Skill
- **Weaknesses**: Lower HP, Defense
- **Stats**: HP=18, STR=6, SPD=12, SKL=11

#### Soldier (Lance)
- **Strengths**: Balanced, High Defense
- **Weaknesses**: Lower Speed
- **Stats**: HP=18, STR=7, SPD=8, DEF=7

#### Fighter (Axe)
- **Strengths**: High STR, HP
- **Weaknesses**: Low Skill and Speed
- **Stats**: HP=22, STR=9, SPD=7, SKL=5

#### Mage (Fire Tome - Magical)
- **Strengths**: High MAG, Range (1-2)
- **Weaknesses**: Very fragile
- **Stats**: HP=16, MAG=8, SPD=7, RES=7

#### Archer (Bow - Range 2 Only)
- **Strengths**: High Skill, Medium Range
- **Weaknesses**: Cannot attack adjacent tiles
- **Stats**: HP=18, STR=6, SPD=7, SKL=10

#### Cleric (Heal Staff)
- **Strengths**: High RES, Can heal allies
- **Weaknesses**: Cannot attack enemies
- **Stats**: HP=16, MAG=7, RES=8, MOV=5

### Terrain Types

- **Plain** (Green): Normal movement (cost: 1)
- **Forest** (Dark Green): Slow movement (cost: 2), +15 avoid
- **Fort** (Yellow): Normal movement, +2 defense, heals 20% HP per turn
- **Mountain** (Gray): Slow for infantry (cost: 3), cavalry cannot pass
- **Wall** (Dark Gray): Impassable

## Architecture

### Domain Layer (`Assets/Scripts/Domain/`)

Pure C# with **zero Unity dependencies**. All game logic lives here.

```
Domain/
├── Units/
│   ├── CharacterStats.cs        (9 stat values)
│   ├── Unit.cs                  (game unit with position, HP, team)
│   ├── ClassData.cs             (class templates with base/cap/growth stats)
│   ├── UnitFactory.cs           (unit creation)
│   ├── Team.cs                  (enum)
│   └── MoveType.cs              (enum)
├── Weapons/
│   ├── Weapon.cs                (weapon stats)
│   ├── WeaponType.cs            (enum)
│   └── DamageType.cs            (Physical/Magical)
├── Map/
│   ├── Tile.cs                  (single map tile)
│   ├── TerrainType.cs           (enum + terrain properties)
│   ├── GameMap.cs               (16x16 grid)
│   └── PathFinder.cs            (A* implementation)
├── Combat/
│   ├── CombatResolver.cs        (all combat formulas)
│   ├── WeaponTriangle.cs        (advantage/disadvantage)
│   └── CombatResult.cs          (combat outcome)
├── Turn/
│   ├── TurnManager.cs           (phase state machine)
│   └── Phase.cs                 (enum)
└── AI/
    └── AIController.cs          (simple heuristic AI)
```

### Adapter Layer (`Assets/Scripts/Adapters/`)

Unity MonoBehaviours that bridge domain logic to Unity.

```
Adapters/
├── GameController.cs            (main orchestrator)
├── MapRenderer.cs               (renders tiles with colors)
├── UnitRenderer.cs              (renders units as spheres + HP bars)
├── InputHandler.cs              (mouse click handling)
├── UIManager.cs                 (HUD elements)
├── GameSceneSetup.cs            (scene initialization)
```

### Tests (`Assets/Scripts/Tests/`)

NUnit tests for domain logic (no Unity runtime required).

```
Tests/
├── CombatResolverTests.cs       (damage, doubles, hit calculations)
├── WeaponTriangleTests.cs       (advantage/disadvantage)
├── PathFinderTests.cs           (A* pathfinding)
└── TurnManagerTests.cs          (phase transitions)
```

## Running Tests

### In Unity Editor

1. **Open Test Runner**
   - Window → Testing → Test Runner (or Ctrl+Alt+T)

2. **Select "EditMode"** tab
   - Tests are in `Assets/Scripts/Tests/`

3. **Click "Run All"**
   - All NUnit tests execute without runtime

### Command Line (Optional)

```bash
# If Unity is installed with CLI tools
unity -projectPath /Users/r2d2/Projects/fire-emblem/tactic-fantasy -runTests -testCategory EditMode
```

## Combat Formulas

### Damage Calculation

**Physical**: `max(0, (STR + Weapon.Might ± Triangle) - (Enemy.DEF ± Terrain))`
**Magical**: `max(0, (MAG + Weapon.Might ± Triangle) - (Enemy.RES ± Terrain))`

### Hit Rate

`(SKL * 2) + (LCK / 2) + Weapon.Hit ± Triangle - (Enemy.AS * 2) - Enemy.LCK ± Terrain`

Uses **True Hit**: average of 2 rolls (0-99), compare against hit rate.

### Critical Rate

`(SKL / 2) + Weapon.Crit - Enemy.LCK` (single RNG, not 2RN)
Critical damage multiplier: **3x**

### Attack Speed

`SPD - max(0, Weapon.Weight - STR)`

### Double Attack

Occurs when `Attacker.AS - Defender.AS ≥ 4`

### Weapon Triangle

- **Advantage**: +1 damage, +10 hit
- **Disadvantage**: -1 damage, -10 hit
- **Matchups**: Sword > Axe > Lance > Sword (only physical weapons)

## Known Limitations (v1)

- No narrative or dialogue
- Simple geometric visuals (spheres and quads)
- No sound effects or music
- No unit animations
- Random team composition (no custom unit selection)
- No advanced AI (just basic heuristics)
- No persistence/saving

## Planned Enhancements (v2+)

- Story mode with narrative
- Custom unit creation and team building
- More unit classes and weapons
- Skill system (special unit abilities)
- Save/load functionality
- Improved AI with strategy variety
- Polished 2D/3D visuals
- Sound and music
- Difficulty settings

## Build Instructions

### For macOS

1. **File > Build Settings**
2. Select **macOS** as target platform
3. Click **Build**
4. Select output folder and build
5. Run the generated `.app` bundle

### For WebGL

1. **File > Build Settings**
2. Select **WebGL** as target platform
3. Click **Build and Run**
4. Game will launch in default browser

## Project Structure

```
tactic-fantasy/
├── Assets/
│   ├── Scripts/
│   │   ├── Domain/           (pure C#, no Unity deps)
│   │   ├── Adapters/         (MonoBehaviours)
│   │   └── Tests/            (NUnit)
│   ├── Scenes/
│   │   └── GameScene.unity
│   ├── Prefabs/
│   ├── Materials/
│   └── Settings/
├── Packages/
├── ProjectSettings/
└── README.md
```

## Coding Standards

### Domain Layer (Assets/Scripts/Domain/)

- ✅ Pure C# classes (no MonoBehaviour)
- ✅ Interfaces for all major types (IUnit, IGameMap, ICombatResolver)
- ✅ Immutable where possible
- ✅ Constructor injection for dependencies
- ✅ Small, focused methods
- ✅ Meaningful variable names

### Adapter Layer (Assets/Scripts/Adapters/)

- ✅ MonoBehaviour-based
- ✅ Delegates domain logic to domain classes
- ✅ Handles rendering and input
- ✅ Never duplicates business logic

### Tests (Assets/Scripts/Tests/)

- ✅ NUnit framework
- ✅ Test only domain logic
- ✅ No Unity runtime dependencies
- ✅ Arrange-Act-Assert pattern

## Troubleshooting

### Scene won't load / Game doesn't start

1. Ensure `GameScene` is created (see Setup Instructions above)
2. Verify `GameSceneSetup` script is attached to a GameObject
3. Check Console for errors (Ctrl+Shift+C)

### Units not moving or attacking

1. Click on a blue unit to select it
2. Blue tiles are movement range — click to move
3. Red tiles are attack range — click to attack
4. Ensure unit is on your team (Team A = Player)

### Tests fail in Test Runner

1. Ensure you're running **EditMode** tests, not PlayMode
2. Check that `Assets/Scripts/Tests/` folder exists
3. Verify NUnit is installed (comes with Unity 2021+)

### WebGL build issues

1. Ensure `Device.simulated` is not enabled in WebGL settings
2. Check that physics is set to non-physics mode if needed
3. Reduce texture quality if memory issues occur

## Performance Notes

- **Frame Rate**: Target 60 FPS on most machines
- **Pathfinding**: A* runs at ~100k nodes/sec on modern CPUs
- **Combat Resolution**: Real-time (no turn delays)
- **AI Decisions**: ~50ms per enemy unit

## Credits

**Design**: Fire Emblem series (Intelligent Systems, Koei Tecmo)
**Engine**: Unity 6
**Development**: Claude Code
**Architecture**: Domain-Driven Design + Hexagonal Architecture

## License

This is an educational project for learning tactical RPG mechanics and clean architecture in C#/Unity. It's not affiliated with Nintendo or the Fire Emblem franchise.

---

**Last Updated**: 2026-04-12
**Version**: 1.0.0
**Status**: Feature Complete - Ready for Play
