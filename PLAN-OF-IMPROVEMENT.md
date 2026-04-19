# Plan of Improvement: Radiant Dawn Gameplay Mechanics

> **Goal**: Transform Tactic Fantasy from a functional tactical RPG into a game that captures the depth and feel of Fire Emblem: Radiant Dawn.
>
> **Architecture Rule**: All gameplay logic lives in `Domain/`. Adapters stay thin. Every feature gets TDD with EditMode NUnit tests.
>
> **Current State (v2.7)**: 6 base + 6 promoted + 6 master-tier classes, 8 Laguz shapeshifter races, full skill system (Adept, Vantage, Wrath, Resolve, Nihil, Paragon, Sol, Luna + 5 mastery skills), 7-slot inventory with consumables and stat boosters, weapon triangle (Sword>Axe>Lance), combat with True Hit/crits/doubles/counters, 3 status effects, A* pathfinding, terrain-aware heuristic AI, procedural 16x16 maps, save/load, gamepad support.

---

## ~~Phase 1: Skills System + Weapon Tiers~~ ✅ DONE (v2.0–v2.3)

> Implemented across multiple releases. Adept, Vantage, Wrath, Resolve, Nihil, Paragon, Sol, Luna all live in `Domain/Skills/SkillDatabase.cs`. `CombatResolver` runs the full skill pipeline (PreCombat → OnAttack → OnDamageDealt). Weapon tiers (IsBrave, WeaponRank) added to `IWeapon`. `WeaponFactory` has Iron/Steel/Silver/Brave variants.

---

## ~~Phase 2: Inventory + Items + Multi-Weapon Classes~~ ✅ DONE (v2.5)

> Implemented in v2.5. `IItem`, `Inventory` (7 slots), `ConsumableItem`, `StatBooster`, `IWeapon extends IItem`. All promoted classes have correct multi-weapon lists.

---

## ~~Phase 3: Third-Tier Classes + Mastery Skills~~ ✅ DONE (v2.6)

> Implemented in v2.6. `ClassData.Tier` (1/2/3), 6 master classes (Trueblade, Marshall, Reaver, Archsage, Marksman, Saint), `ClassPromotionService` extended to Tier 2→3 with mastery auto-learn. All 5 mastery skills in `SkillDatabase`.

---

## ~~Phase 4: Laguz / Shapeshifters~~ ✅ DONE (v2.7)

> Implemented in v2.7. `TransformGauge`, `LaguzClassData` for all 8 races, `LaguzWeaponFactory`, `LaguzItemFactory` (Laguz Stone, Olivi Grass), `Unit` Laguz methods, TurnManager gauge ticking, AI retreat for untransformed Laguz, Heron single-target refresh + cross-pattern refresh when transformed.

---

## Phase 1: Skills System + Weapon Tiers

**Why first**: Skills are THE defining feature of Radiant Dawn. Without Adept, Vantage, Wrath, Sol, and Luna, combat is generic. This phase transforms every battle into a moment of tension.

### 1A. Skill Framework

| New File | Purpose |
|----------|---------|
| `Domain/Skills/ISkill.cs` | Interface: `Name`, `ActivationPhase`, `CanActivate(owner, opponent, ctx)`, `Apply(ctx)` |
| `Domain/Skills/SkillActivationPhase.cs` | Enum: `PreCombat`, `OnAttack`, `OnDefend`, `OnDamageDealt`, `Passive`, `OnTurnStart` |
| `Domain/Skills/CombatContext.cs` | Mutable context object passed through the combat pipeline. Carries attacker, defender, damage, hit, crit, flags (extra attack, ignore DEF, etc.), and a `Random` reference for testability |
| `Domain/Skills/SkillDatabase.cs` | Static factory methods for each skill (same pattern as `ClassDataFactory`) |

**Changes to existing files:**

- **`IUnit` / `Unit.cs`** -- Add `IReadOnlyList<ISkill> EquippedSkills`, `IReadOnlyList<ISkill> LearnedSkills`, `int SkillCapacity` (RD limits how many skills a unit can equip), `void LearnSkill(ISkill)`, `void EquipSkill(ISkill)`, `void UnequipSkill(ISkill)`
- **`IClassData` / `ClassData.cs`** -- Add `IReadOnlyList<ISkill> IntrinsicSkills` (class-granted skills, e.g., Swordmaster gets Vantage innately in RD)
- **`CombatResolver.cs`** -- **Major refactor**. Current monolithic method becomes a pipeline:
  1. Build `CombatContext` from attacker/defender/map
  2. Run `PreCombat` skills (Vantage reorders attack order, Nihil disables opponent skills)
  3. Execute attacker strike with `OnAttack` skills (Adept triggers extra attack, Astra triggers 5 hits)
  4. Execute defender counter with `OnDefend` skills
  5. Execute follow-up attacks (doubles)
  6. Run `OnDamageDealt` skills (Sol heals attacker)
  7. Build `CombatResult` from context
- **`CombatResult.cs`** -- Add `List<string> ActivatedSkills` to display which skills fired

**Phase 1 skill batch:**

| Skill | Type | Activation | Effect |
|-------|------|-----------|--------|
| **Adept** | OnAttack | SKL% chance | Immediate extra attack |
| **Vantage** | PreCombat | HP <= 50% | Unit attacks first regardless of who initiated |
| **Wrath** | OnAttack | HP <= 50% | Guaranteed critical hit |
| **Resolve** | Passive | HP <= 50% | +7 to SKL, SPD, DEF |
| **Nihil** | PreCombat | Always | Negate all opponent skills |
| **Paragon** | Passive | Always | 2x XP gain (modifies CombatXp output) |

**Design notes:**
- Each skill is a pure C# class implementing `ISkill` -- no switch/case in CombatResolver
- `CombatContext` carries a `Random` reference so tests inject deterministic RNG
- Nihil + Adept interaction: Nihil must be checked first in PreCombat phase
- Resolve modifies `CurrentStats` via a buff system, not permanent stat change

**Tests:** `SkillActivationTests.cs`, `CombatResolverWithSkillsTests.cs` -- Each skill isolated + combination tests (Nihil blocks Adept, Vantage + Wrath combo)

### 1B. Weapon Tiers

| New File | Purpose |
|----------|---------|
| `Domain/Weapons/WeaponTier.cs` | Enum: `Iron`, `Steel`, `Silver`, `Brave`, `Legendary` |

**Changes to existing:**

- **`Weapon.cs` / `IWeapon`** -- Add `bool IsBrave` (attacks twice before counter), `WeaponRank RequiredRank` (E/D/C/B/A/S)
- **`IUnit` / `Unit.cs`** -- Add `Dictionary<WeaponType, WeaponRank> WeaponProficiency` for weapon rank system (units build proficiency over time)
- **`WeaponFactory.cs`** -- Add full weapon tiers:

| Weapon | Might | Hit | Weight | Uses | Rank |
|--------|-------|-----|--------|------|------|
| Iron Sword | 5 | 90 | 5 | 30 | E |
| Steel Sword | 8 | 80 | 8 | 25 | D |
| Silver Sword | 11 | 75 | 7 | 20 | A |
| Brave Sword | 7 | 70 | 9 | 20 (2x) | B |
| Iron Lance | 7 | 80 | 8 | 30 | E |
| Steel Lance | 10 | 70 | 11 | 25 | D |
| Silver Lance | 13 | 70 | 10 | 20 | A |
| Iron Axe | 8 | 75 | 9 | 30 | E |
| Steel Axe | 11 | 65 | 13 | 25 | D |
| Silver Axe | 14 | 65 | 12 | 20 | A |
| Brave Axe | 9 | 60 | 14 | 20 (2x) | B |

- **`CombatResolver.cs`** -- Brave weapons trigger two consecutive attacks before defender counter

**Tests:** `WeaponTierTests.cs`, `BraveWeaponTests.cs`, `WeaponRankTests.cs`

---

## Phase 2: Inventory + Items + Multi-Weapon Classes

**Why second**: RD units carry up to 7 items (weapons + consumables). Promoted classes wield multiple weapon types. Both require an inventory system.

### 2A. Inventory System

| New File | Purpose |
|----------|---------|
| `Domain/Items/IItem.cs` | Interface: `Name`, `ItemType` (Weapon/Consumable/KeyItem), `Uses`, `void Use(IUnit)` |
| `Domain/Items/Inventory.cs` | Container for up to 7 items. Methods: `Add`, `Remove`, `Swap`, `GetAll`, `IsFull`, `GetWeapons` |
| `Domain/Items/ConsumableItem.cs` | Concrete items: Vulnerary (heal 10 HP, 3 uses), Elixir (full heal, 3 uses), Antitoxin (cure Poison), Pure Water (+7 RES for map) |
| `Domain/Items/StatBooster.cs` | Permanent stat items: Energy Drop (+2 STR), Spirit Dust (+2 MAG), Speedwing (+2 SPD), etc. |

**Changes to existing:**
- **`IWeapon`** should extend `IItem` (weapons ARE items in RD's inventory)
- **`IUnit` / `Unit.cs`** -- Replace single `EquippedWeapon` with `Inventory`. Add `int EquippedWeaponIndex`. `EquippedWeapon` becomes a computed property: `Inventory[EquippedWeaponIndex] as IWeapon`
- **Migration path**: Make `IWeapon` extend `IItem`, wrap current weapon in a 1-item inventory. Convenience constructor preserves backward compat in all existing tests

**Design notes:**
- RD inventory limit is 7 slots
- Items can be used during player's turn as an action (Move -> Attack/Item/Wait)
- `GameController.cs` needs a unit action menu (adapter-level change)

### 2B. Multi-Weapon Classes

**Changes to existing:**
- **`IClassData` / `ClassData.cs`** -- Change `WeaponType WeaponType` to `IReadOnlyList<WeaponType> UsableWeaponTypes`
- **`ClassDataFactory`** -- Update promoted classes per RD:

| Promoted Class | Usable Weapons |
|----------------|---------------|
| Swordmaster | Sword |
| General | Lance, Sword |
| Warrior | Axe, Bow |
| Sage | Fire, Staff |
| Sniper | Bow |
| Bishop | Staff, Fire |

- **`Unit.cs`** -- Validate equipped weapon against `Class.UsableWeaponTypes`
- `CombatResolver`, `AIController` already read `unit.EquippedWeapon` -- no changes needed

**Tests:** `InventoryTests.cs`, `MultiWeaponTests.cs`, `ConsumableItemTests.cs`, `InventorySwapTests.cs`

---

## Phase 3: Third-Tier Classes + Mastery Skills

**Why third**: Depends on Phase 1 (skills) and Phase 2 (multi-weapon). Three-tier class progression is RD's signature feature -- Base -> Advanced -> Master.

**Changes to existing:**
- **`ClassData.cs`** -- Add `int Tier` property (1=Base, 2=Advanced, 3=Master)
- **`ClassDataFactory`** -- Add all third-tier classes:

| Tier 1 (Base) | Tier 2 (Advanced) | Tier 3 (Master) | Mastery Skill |
|---------------|-------------------|-----------------|---------------|
| Myrmidon | Swordmaster | **Trueblade** | Astra |
| Soldier | General | **Marshall** | Sol |
| Fighter | Warrior | **Reaver** | Colossus |
| Mage | Sage | **Archsage** | Flare |
| Archer | Sniper | **Marksman** | Deadeye |
| Cleric | Bishop | **Saint** | Corona |

- **`ClassPromotionService.cs`** -- Extend promotion map for Tier 2 -> Tier 3. Promotion at level 20 for both transitions. Add support for Master Crown item (from Phase 2) as promotion trigger.
- **`Unit.MaxLevel`** -- Remains 20 per tier (effective max: 60 levels of growth across 3 tiers)

**Mastery skill implementations:**

| Skill | Activation | Effect |
|-------|-----------|--------|
| **Astra** | SKL/2 % | 5 consecutive hits at 50% damage each |
| **Sol** | SKL% | Heal HP equal to damage dealt |
| **Luna** | SKL% | Ignore enemy DEF/RES for this attack |
| **Colossus** | STR% | Add STR to damage |
| **Flare** | SKL% | Halve enemy RES |
| **Deadeye** | SKL/2 % | 2x damage + apply Sleep |
| **Corona** | SKL% | Halve enemy RES and DEF |

**Design notes:**
- Mastery skills are learned automatically upon promotion to Tier 3
- Third-tier classes get more weapon types (Marshall: Sword+Lance+Axe, Archsage: Fire+Wind+Thunder+Staff)
- Third-tier stat caps are significantly higher than Tier 2

**Tests:** `ThirdTierPromotionTests.cs`, `MasterySkillTests.cs`, `ThirdTierStatCapTests.cs`

---

## Phase 4: Laguz / Shapeshifters

**Why fourth**: Laguz are RD's most unique gameplay mechanic -- a parallel unit system with transformation. Requires skills (Phase 1) and possibly items (Phase 2 for Laguz Stones/Olivi Grass).

| New File | Purpose |
|----------|---------|
| `Domain/Units/TransformGauge.cs` | Tracks transformation points (0-30). Fills each turn when untransformed (+5/turn), drains when transformed (-2/turn). When full: transform. When empty: revert. |
| `Domain/Units/LaguzClassData.cs` | Extends ClassData with: untransformed stats (halved STR/DEF/etc.), transformed stats (full), gauge fill/drain rates per race |

**Laguz races and classes:**

| Race | Class | Type | Key Traits |
|------|-------|------|------------|
| Cat | Beast | Infantry | Fast, fragile. +8/turn gauge. Strike weapon |
| Tiger | Beast | Infantry | Balanced. +5/turn gauge. Claw weapon |
| Lion | Beast | Infantry | Powerful, slow. +3/turn gauge. Fang weapon |
| Hawk | Bird | Flying (transformed) | High STR. +5/turn gauge. Talon weapon |
| Raven | Bird | Flying (transformed) | Fast, evasive. +7/turn gauge. Beak weapon |
| Red Dragon | Dragon | Infantry | Extremely tanky. +2/turn gauge. Breath weapon |
| White Dragon | Dragon | Infantry | Rare, godlike stats. +1/turn gauge. Breath weapon |
| Heron | Special | Flying (transformed) | No combat. Refreshes 1 unit (untransformed) or 4 units in cross pattern (transformed) |

**Changes to existing:**
- **`IUnit` / `Unit.cs`** -- Add `TransformGauge LaguzGauge` (null for beorc). Add `bool IsTransformed`. Add `void Transform()` / `void Revert()` that swap stat profiles. Use composition (not inheritance) -- check `if (LaguzGauge != null)` for gauge logic
- **`MoveType`** -- Already has `Flying`. Laguz birds use `Flying` when transformed, `Infantry` when not
- **`TurnManager.cs`** -- Tick transform gauges at phase transitions
- **`CombatResolver.cs`** -- Laguz use natural strike weapons (Claw/Fang/Beak/Breath) that are auto-equipped and change stats on transform
- **`AIController.cs`** -- Untransformed Laguz retreat; transformed ones attack aggressively

**Design notes:**
- Laguz Stones (consumable) instantly fill gauge to max
- Olivi Grass (consumable) adds +15 gauge points
- Untransformed Laguz have halved offensive stats -- they're vulnerable
- Herons are already a class; promote them to Laguz-style with gauge + expanded refresh on transform

**Tests:** `TransformGaugeTests.cs`, `LaguzCombatTests.cs`, `LaguzStatSwapTests.cs`, `HeronTransformRefreshTests.cs`

---

## Phase 4 Follow-Up: Heron Cross-Pattern Refresh (small, self-contained)

**Why now**: Heron's expanded refresh is the only missing piece from Phase 4. It's small but important for Heron's unique role.

**Changes to existing:**
- **`TurnManager.cs`** — Add `CanRefreshCross(refresher, targets)` and `RefreshCross(heronId, map)` methods. When Heron is transformed, `RefreshUnit` is upgraded to refresh all allied units in the 4 cardinal adjacent tiles instead of just one
- **`IUnit` / `Unit.cs`** — No changes needed (IsTransformed already exists)

**Implementation details:**
- `RefreshCross(heronId, map)` — gets Heron position, checks each of the 4 cardinal neighbors (x±1 or y±1), collects allied units that have acted, calls `RefreshUnit()` on each. Requires `IGameMap` to read positions.
- Keep existing single-target `RefreshUnit` for untransformed Heron — no behavior change there
- Add `ITurnManager.RefreshCross(int heronId, IGameMap map)` to the interface

**Tests:** Add to `RefreshMechanicTests.cs`:
- `TransformedHeron_CanRefreshCross_UpToFourAdjacentAllies`
- `UntransformedHeron_CannotRefreshCross`
- `RefreshCross_SkipsEnemyUnits`
- `RefreshCross_SkipsUnacedUnits`

---

## Phase 5: Base / Shops + Bonus Experience (BEXP)

**Why next**: Requires inventory (Phase 2 ✅) for shops. BEXP is RD's unique reward/catch-up system that enables strategic unit building. All prerequisites are now done.

**Status**: ✅ DONE (v2.8)

### 5A. Domain Models

| New File | Purpose |
|----------|---------|
| `Domain/Chapter/ArmyGold.cs` | Army-wide gold tracker. Simple value object: `int Gold`, `bool CanAfford(int cost)`, `void Spend(int amount)`, `void Earn(int amount)`. Throws if spending below zero. |
| `Domain/Chapter/ShopService.cs` | Domain service. `Buy(unit, item, gold)` — validates CanAfford, deducts gold, adds item to unit inventory. `Sell(unit, item, gold)` — removes item, earns gold. Sell price = 50% of buy price. |
| `Domain/Chapter/BexpDistributor.cs` | BEXP level-up service (see rules below). Core RD mechanic: BEXP level-ups **always grant exactly +3 stats** (deterministic, not probabilistic). |
| `Domain/Chapter/ChapterData.cs` | Value object defining a chapter: `MapSeed`, `EnemyRoster`, `VictoryCondition`, `BexpReward`, `ShopItems`, `ParTurns`. |
| `Domain/Chapter/BasePhase.cs` | Between-chapter state: `BexpPool`, `ShopInventory`, `ArmyGold`, `AvailableUnits` (full roster), `DeployedUnits` (subset). Methods: `AllocateBexp(unit, amount)`, `OpenShop()`, `DeployUnit(unit)`, `BenchUnit(unit)`. |

### 5B. Changes to Existing Files

- **`Unit.cs` / `IUnit`** — Add `bool GainExperienceBexp(int amount, Random rng = null)` method. BEXP-specific level-up: always grants exactly 3 stat points, chosen from highest growth rates that haven't capped. If fewer than 3 stats can grow (all capped), grants fewer. Different from normal `GainExperience` (probabilistic).

### 5C. BEXP Distribution Rules (RD-accurate)

```
1. Sort stats by growth rate descending: HP, STR, MAG, SKL, SPD, LCK, DEF, RES (MOV excluded)
2. For each level-up triggered by BEXP:
   a. Grant +1 to the 3 stats with highest growth rates that are NOT yet at cap
   b. Skip any stat already at CapStats value
   c. If fewer than 3 uncapped stats exist, grant as many as possible
3. Always costs 50 BEXP per level-up (same as RD)
4. Does NOT use RNG — result is fully deterministic
```

### 5D. BEXP Rewards Per Chapter

```
Completion bonus:  base amount defined in ChapterData (e.g. 200)
Turn bonus:        max(0, (ParTurns - TurnsTaken) * 5) — reward for finishing early
Survival bonus:    (AlliesAlive / TotalAllies) * 50 — reward for no casualties
Objective bonus:   optional objectives add fixed amounts (defined per ChapterData)
```

### 5E. Adapter-Layer (BaseController — out of scope for domain TDD)

> The adapter `BaseController.cs` is not part of Phase 5 domain work. It will be a MonoBehaviour wrapping `BasePhase` for Unity UI. Implement after all domain tests pass.

### 5F. Tests

| File | Tests |
|------|-------|
| `BexpDistributorTests.cs` | Top-3 growth stats chosen, capped stats skipped, fewer than 3 uncapped, XP cost is 50/level, level cap respected, deterministic (no RNG needed), BEXP pool deducted correctly |
| `ShopServiceTests.cs` | Buy succeeds when affordable, buy fails when broke, sell returns 50% price, sell removes item from inventory, buy adds item to inventory, item not in stock cannot be bought |
| `ArmyGoldTests.cs` | Earn adds gold, spend deducts gold, spend below zero throws, CanAfford correct boundary |
| `BasePhaseTests.cs` | AllocateBexp triggers level-up correctly, DeployUnit / BenchUnit roster management, BEXP pool tracks correctly across multiple allocations |

---

## ~~Phase 6: Map Improvements~~ ✅ DONE (v2.9)

> Implemented in v2.9. `MapDefinition` + `MapLoader` for designed maps, 3 hand-crafted chapters (Plains Skirmish, Castle Assault, Desert Holdout). `ReinforcementTrigger` + `ReinforcementService` for turn/tile/death-based spawns, wired into `TurnManager`. `FogOfWar` service with BFS vision (MOV+2 radius), Forest blocking, Torch extension, fog-aware AI with last-known positions. 5 new terrain types: Door, Chest, Throne, Desert, Bridge. `InteractableTile` for mutable Door/Chest state. `TerrainProperties` refactored from `bool isInfantry` to `MoveType` with mage Desert exception.

**Why sixth**: The game needs designed maps, not random generation. Reinforcements and fog of war add tension. This phase makes every chapter feel unique.

### 6A. Fixed Map Designs

| New File | Purpose |
|----------|---------|
| `Domain/Map/MapDefinition.cs` | Data class: 2D terrain array, unit placements, reinforcement triggers, chest/door positions, size |
| `Domain/Map/MapLoader.cs` | Creates `IGameMap` from `MapDefinition` (loaded from JSON or hardcoded) |

- **`GameMap.cs`** -- Add constructor: `GameMap(MapDefinition def)` alongside existing random constructor
- Design at least 5 chapters with distinct terrain layouts and objectives

### 6B. Reinforcements

| New File | Purpose |
|----------|---------|
| `Domain/Turn/ReinforcementTrigger.cs` | Condition-based spawn: turn number, tile stepped on, unit death. Contains spawn position + unit template |
| `Domain/Turn/ReinforcementService.cs` | Evaluates triggers each phase, spawns new units into the unit list |

- **`TurnManager.cs`** -- Check triggers at phase transitions, call `SpawnReinforcement()`

### 6C. Fog of War

| New File | Purpose |
|----------|---------|
| `Domain/Map/FogOfWar.cs` | Tracks visible tiles per team. Vision = MOV + 2 (infantry). Thieves see +3 extra. Forest blocks vision. Torch items extend vision by 5 tiles |

- **`IGameMap`** -- Add `bool IsTileVisible(int x, int y, Team team)`
- **`MapRenderer.cs`** -- Darken non-visible tiles (50% black overlay)
- **`AIController.cs`** -- Enemies in fog move toward last-known player positions

### 6D. New Terrain Types

Add to `TerrainType` enum and `TerrainProperties`:

| Terrain | Move Cost | DEF | AVO | Special |
|---------|-----------|-----|-----|---------|
| Door | Impassable | 0 | 0 | Opened with Key or Thief |
| Chest | 1 | 0 | 0 | Opened with Key or Thief, contains item |
| Throne | 1 | +3 | +30 | Seize target, heals 30% HP |
| Desert | 3 (infantry), 4 (cavalry) | 0 | +5 | Mages unaffected (cost 1) |
| Bridge | 1 | 0 | 0 | Crossable water |

**Tests:** `MapLoaderTests.cs`, `ReinforcementTests.cs`, `FogOfWarTests.cs`, `DesertTerrainTests.cs`

---

## Phase 7: Support / Affinity System + Biorhythm

### 7A. Support System

| New File | Purpose |
|----------|---------|
| `Domain/Support/Affinity.cs` | Enum: Fire, Thunder, Wind, Ice, Earth, Dark, Light, Heaven, Water |
| `Domain/Support/SupportBonus.cs` | Static data: each affinity pair yields specific ATK/DEF/HIT/AVO bonuses per RD tables |
| `Domain/Support/SupportTracker.cs` | Tracks support points between unit pairs. Points accrue when adjacent at end of turn. Ranks: C (35 pts), B (70 pts), A (105 pts) |

**Changes to existing:**
- **`IUnit` / `Unit.cs`** -- Add `Affinity` property
- **`CombatResolver.cs`** -- Apply support bonuses from adjacent allied units in hit/avoid/damage/defense

**RD Affinity bonus table (per rank per matching affinity):**

| Affinity | ATK | DEF | HIT | AVO |
|----------|-----|-----|-----|-----|
| Fire | +0.5 | 0 | +2.5 | +2.5 |
| Thunder | +0.5 | +0.5 | +2.5 | 0 |
| Wind | 0 | +0.5 | +2.5 | +2.5 |
| Ice | +0.5 | +0.5 | 0 | +2.5 |
| Earth | 0 | +0.5 | +2.5 | +2.5 |
| Dark | +0.5 | 0 | +2.5 | +2.5 |
| Light | +0.5 | +0.5 | +2.5 | 0 |
| Heaven | +0.5 | 0 | +2.5 | +2.5 |
| Water | 0 | +0.5 | +2.5 | +2.5 |

Bonuses are summed per support rank (C=1x, B=2x, A=3x) and per adjacent ally.

### 7B. Biorhythm

| New File | Purpose |
|----------|---------|
| `Domain/Units/Biorhythm.cs` | Sine wave per unit. Period varies (5-12 turns). Affects hit/avoid by -10 to +10. Peak = +10 hit/avoid, trough = -10 |

- **`Unit.cs`** -- Add `Biorhythm` property, tick at turn start
- **`CombatResolver.cs`** -- Add biorhythm modifier to hit rate and avoid calculations

**Design notes:**
- Each unit has a random starting phase (0 to 2*PI) and period (5-12 turns)
- Display on unit info panel: "Best", "Good", "Normal", "Bad", "Worst" based on current wave position
- Biorhythm is subtle (+/-10 max) but adds a layer of timing strategy

**Tests:** `SupportBonusTests.cs`, `SupportTrackerTests.cs`, `BiorhythmTests.cs`, `BiorhythmCombatTests.cs`

---

## Phase 8: Shove/Smite, Guard, Stealing, Trading, NPC Units

Smaller mechanics that can be batched together.

### 8A. Shove and Smite

| New File | Purpose |
|----------|---------|
| `Domain/Actions/ShoveService.cs` | Push adjacent unit 1 tile (Shove) or 2 tiles (Smite) in facing direction. Requires STR >= target's Weight (or STR/WT comparison). Target must land on a traversable, unoccupied tile |

- Shove: any unit can do it (costs no action in RD, but unit can't move after)
- Smite: requires Fighter/Warrior/Reaver class (push 2 tiles)
- Cannot shove over ledges, into walls, or off map

### 8B. Guard / Defend Command

- **`Unit.cs`** -- Add `bool IsGuarding` flag. When guarding: +2 DEF, +2 RES until next turn
- **`CombatResolver.cs`** -- Check `IsGuarding` and apply bonus
- Guard replaces the attack action (Move -> Guard/Wait)
- **`TurnManager.cs`** -- Clear all guard flags at start of player phase

### 8C. Stealing

| New File | Purpose |
|----------|---------|
| `Domain/Actions/StealService.cs` | Thief/Rogue action: steal top non-weapon item from adjacent enemy. Requires SPD > target's SPD. Stolen item goes to thief's inventory |

- Add Thief/Rogue to `ClassDataFactory` (Tier 1/2 with high SPD, can steal and pick locks)

### 8D. Trading

| New File | Purpose |
|----------|---------|
| `Domain/Actions/TradeService.cs` | Swap inventory items between adjacent allied units. No action cost (free action before attack in RD) |

### 8E. NPC / Green Units

- **`Team.cs`** -- Add `AllyNPC` value
- **`Phase.cs`** -- Add `AllyPhase`
- **`TurnManager.cs`** -- Three-phase cycle: Player -> Ally -> Enemy
- **`AIController.cs`** -- NPC AI: protect objectives, advance toward enemies, prioritize self-preservation
- **`UnitRenderer.cs`** -- Green color for NPC units
- NPC units can be recruited (converted to PlayerTeam) via Talk action when adjacent with specific units

**Tests:** `ShoveTests.cs`, `GuardTests.cs`, `StealTests.cs`, `TradeTests.cs`, `NPCPhaseTests.cs`, `RecruitmentTests.cs`

---

## Phase 9: Magic Triangle + Weather + Narrative / Dialogue

### 9A. Magic Triangle

- **`WeaponType.cs`** -- Add `WIND` and `THUNDER` weapon types
- **`WeaponTriangle.cs`** -- Extend with magic triangle: Fire > Wind > Thunder > Fire
  - Same bonuses: +1 damage, +10 hit advantage / -1 damage, -10 hit disadvantage
- **`WeaponFactory.cs`** -- Add Wind and Thunder tomes (Iron/Steel/Silver variants)
- **`ClassDataFactory`** -- Sage uses Fire+Wind+Thunder, Archsage uses all three + Staff

### 9B. Weather Effects

| New File | Purpose |
|----------|---------|
| `Domain/Map/Weather.cs` | Enum: Clear, Rain, Snow, Sandstorm |
| `Domain/Map/WeatherEffects.cs` | Static class with weather modifiers |

| Weather | Effect |
|---------|--------|
| Clear | No modifier |
| Rain | -15 hit for bows, +1 move cost for all, fire magic -2 damage |
| Snow | +1 move cost for all, -10 avoid |
| Sandstorm | -20 hit, -2 vision range (fog of war) |

- **`IGameMap`** -- Add `Weather CurrentWeather` property, can change mid-chapter
- **`CombatResolver.cs`** -- Apply weather modifiers

### 9C. Narrative / Dialogue System

| New File | Purpose |
|----------|---------|
| `Domain/Narrative/DialogueLine.cs` | Value object: speaker name, text, optional portrait ID |
| `Domain/Narrative/DialogueScript.cs` | Ordered list of `DialogueLine`. Played before/after chapters or when triggered mid-map |
| `Domain/Narrative/BossConversation.cs` | Triggered when specific unit pairs enter combat. Contains dialogue + optional stat bonuses |

- **`ChapterData.cs`** -- Add `DialogueScript IntroDialogue`, `DialogueScript OutroDialogue`, `List<BossConversation> BossConversations`
- **`GameController.cs`** -- Check for boss conversations before combat resolves
- Adapter-level: new `DialogueRenderer.cs` for text boxes with speaker portraits

**Tests:** `MagicTriangleTests.cs` (extend existing), `WeatherEffectsTests.cs`, `BossConversationTriggerTests.cs`

---

## Phase 10: Weapon Forging + Ledges / Elevation + Final Polish

### 10A. Weapon Forging

| New File | Purpose |
|----------|---------|
| `Domain/Weapons/ForgeService.cs` | Spend gold to boost a weapon's Might (+1 to +5), Hit (+5 to +25), or Crit (+5 to +25). RD limits: 1 forge per weapon, max 5 points distributed across stats |

- Available at Base between chapters
- Cost scales with weapon tier (Silver costs more than Iron)

### 10B. Ledges / Elevation

- **`Tile.cs`** -- Add `int Height` property (0=ground, 1=raised, 2=high)
- **`TerrainProperties.cs`** -- Height affects: +1 damage per height advantage for ranged attacks, +10 avoid for higher ground, +2 vision range per height
- **`PathFinder.cs`** -- Height-aware costs: can move down freely, moving up costs +1 per height difference, cannot climb >1 height without stairs
- Ledges: one-way traversal (down only), creating tactical bottlenecks

### 10C. Balance Pass

- Play-test all 3 tiers of classes with skills, inventory, supports
- Verify skill activation rates feel impactful but not dominant
- Ensure AI handles all new mechanics (skills, laguz, inventory, fog)
- Tune BEXP rewards and level-up distribution
- Adjust weapon stats (might, weight, hit) for satisfying combat math
- Verify all 16+ test suites pass with new mechanics integrated

---

## Summary: Dependency Chain

```
Phase 1: Skills + Weapon Tiers
    |
    v
Phase 2: Inventory + Items + Multi-Weapon ──> Phase 5: Base/Shops + BEXP
    |
    v
Phase 3: Third-Tier Classes + Mastery Skills
    |
    v
Phase 4: Laguz / Shapeshifters

Phase 6: Map Improvements (independent)

Phase 7: Support/Affinity + Biorhythm (independent)

Phase 8: Shove/Guard/Steal/Trade/NPC (depends on Phase 2 for inventory)

Phase 9: Magic Triangle + Weather + Narrative (independent)

Phase 10: Forging + Elevation + Polish (depends on Phase 2, 5, 6)
```

**Phases 6, 7, and 9 can be done in parallel with Phases 3-5** since they're independent systems.

---

## Critical Files (Most Modified)

| File | Phases That Touch It |
|------|---------------------|
| `Domain/Combat/CombatResolver.cs` | 1, 3, 7, 8, 9, 10 |
| `Domain/Units/Unit.cs` | 1, 2, 4, 7, 8 |
| `Domain/Units/ClassData.cs` | 1, 2, 3, 4 |
| `Domain/Turn/TurnManager.cs` | 4, 6, 8 |
| `Domain/AI/AIController.cs` | 4, 6, 8 |
| `Adapters/GameController.cs` | 2, 5, 6, 8, 9 |
| `Adapters/UIManager.cs` | 2, 5, 8, 9 |

---

**Last Updated**: 2026-04-19
**Target**: Fire Emblem: Radiant Dawn parity
**Approach**: TDD, incremental phases, playable at each milestone

## Phase Completion Status

| Phase | Title | Status |
|-------|-------|--------|
| 1 | Skills + Weapon Tiers | ✅ Done (v2.0–v2.3) |
| 2 | Inventory + Items + Multi-Weapon | ✅ Done (v2.5) |
| 3 | Third-Tier Classes + Mastery Skills | ✅ Done (v2.6) |
| 4 | Laguz / Shapeshifters | ✅ Done (v2.7) — 1 gap |
| 4b | Heron Cross-Pattern Refresh | ✅ Done (v2.7) |
| 5 | Base / Shops + BEXP | ✅ Done (v2.8) |
| 6 | Map Improvements | ✅ Done (v2.9) |
| 7 | Support / Affinity + Biorhythm | ❌ Not started |
| 8 | Shove / Guard / Steal / Trade / NPC | ❌ Not started |
| 9 | Magic Triangle + Weather + Narrative | ❌ Not started |
| 10 | Weapon Forging + Elevation + Polish | ❌ Not started |
