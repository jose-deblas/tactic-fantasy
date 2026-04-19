# Tactic Fantasy v1 - Implementation Summary

## Project Status: ✅ COMPLETE + Active Development

## Changelog

### v2.8.2 - Presentation: Health formatter (2026-04-19)
- **HealthFormatter** (`Presentation/`) — Pure utility to format health for HUD and world-space HP bars. `Format(current, max)` clamps values and returns a string like `HP: 30/50 (60%)`. Added `HealthFormatterTests.cs` with cases for normal values, clamping below zero, clamping above max, invalid max (throws), and rounding behavior for percent calculation.

### v2.8.1 - Gameplay: Regeneration status effect (2026-04-19)
- **RegenerationEffect** (`Domain/`) — New status effect that heals a target over time (`duration`, `healPerSecond`). Implemented as domain class with full TDD coverage (RegenerationHealsAndExpires test)
- **ShopService.cs** (`Domain/Chapter/`) — Domain service with item catalog (`RegisterItem`), `Buy(unit, itemName, gold)` (validates stock, affordability, inventory space), `Sell(unit, item, gold)` (50% of buy price). Uses factory functions for fresh item instances
- **BexpDistributor.cs** (`Domain/Chapter/`) — BEXP allocation service: 50 BEXP per level-up, deducts from a shared pool, stops at max level. Delegates actual level-up to `Unit.GainLevelBexp()`
- **ChapterData.cs** (`Domain/Chapter/`) — Value object defining a chapter: name, map seed, base BEXP reward, par turns, shop items, victory condition. `CalculateBexpReward(turnsTaken, alliesAlive, totalAllies)` computes total with turn bonus (+5 per under-par turn) and survival bonus (proportional to allies alive)
- **BasePhase.cs** (`Domain/Chapter/`) — Between-chapter state: BEXP pool, army gold, shop, available/deployed/benched unit roster. `AllocateBexp(unit, amount)` deducts from pool and levels unit. `DeployUnit`/`BenchUnit` manage roster with max deploy cap
- **Unit.cs / IUnit** — New `GainLevelBexp()` method: deterministic BEXP level-up that always grants exactly 3 stat points, chosen from the highest growth rates not yet at cap (MOV excluded). Skips capped stats; grants fewer than 3 if fewer are uncapped. No RNG involved
- 4 new test files: `ArmyGoldTests.cs` (11 tests), `ShopServiceTests.cs` (11 tests), `BexpDistributorTests.cs` (13 tests), `BasePhaseTests.cs` (13 tests)

### v2.8 - Base / Shops + Bonus Experience (2026-04-19)
- **ArmyGold.cs** (`Domain/Chapter/`) — Army-wide gold tracker: `Earn(amount)`, `Spend(amount)`, `CanAfford(cost)`. Throws on negative values or overspend

### v2.7 - Laguz / Shapeshifters (2026-04-19)
- **TransformGauge.cs** (`Domain/Units/`) — MaxGauge=30. `Tick(isTransformed)` fills by fillRate when untransformed, drains by drainRate when transformed; auto-returns true when state-change threshold is crossed. `FillToMax()` (Laguz Stone) and `AddPoints(n)` (Olivi Grass) for consumable interactions
- **LaguzClassData.cs** (`Domain/Units/`) — Extends `IClassData`. Stores separate `TransformedStats` / `UntransformedStats`, `GaugeFillRate`, `GaugeDrainRate`, `Race` (enum `LaguzRace`), and dual MoveType (untransformed vs transformed)
- **LaguzClassDataFactory** — 8 Laguz races with RD-accurate stats: Cat (+8/turn, infantry), Tiger (+5/turn, infantry), Lion (+3/turn, infantry), Hawk (+5/turn, infantry→flying on transform), Raven (+7/turn, infantry→flying), Red Dragon (+2/turn, infantry), White Dragon (+1/turn, infantry), Heron (+4/turn, infantry→flying). All drain 2/turn while transformed
- **LaguzWeaponFactory.cs** (`Domain/Weapons/`) — Race-specific natural weapons (Strike, Claw, Fang, Talon, Beak, Breath) all as `WeaponType.STRIKE`. Breath is Magical; the rest are Physical. `CreateForRace(race)` picks the correct weapon automatically
- **LaguzItemFactory.cs** (`Domain/Items/`) — Laguz Stone (1 use, fills gauge to 30), Olivi Grass (3 uses, +15 gauge points)
- **Unit.cs** — Added `InitLaguzGauge(fillRate, drainRate, initial)`, `LaguzGauge`, `IsLaguz`, `IsTransformed`, `Transform()` (swaps to transformed stats and MoveType), `Revert()` (swaps back), `TickTransformGauge()` (ticks gauge and auto-transforms/reverts when full/empty)
- **TurnManager.cs** — `TickLaguzGauges(team)` called at each phase transition; `CanRefreshTarget(refresher, target)` + `RefreshUnit(unitId)` for Heron single-target refresh; `RefreshCross(heron, map)` for transformed Heron cross-pattern refresh (refreshes up to 4 allied units in cardinal directions, returns count refreshed; skips enemies, unacted units, and self)
- **AIController.cs** — Untransformed Laguz retreat toward safety (halved stats = vulnerability); transformed Laguz attack normally through existing heuristics
- 4 new test files: `TransformGaugeTests.cs`, `LaguzCombatTests.cs`, `LaguzStatSwapTests.cs`, `RefreshMechanicTests.cs` (13 tests: 9 single-target refresh + 4 cross-pattern refresh)

- **Tests:** added GameSaveService test to assert active status is captured in GameSnapshot (technical TDD enhancement)

### v2.6 - Third-Tier Classes + Mastery Skills (2026-04-19)
- **ClassData.cs / IClassData** — Added `int Tier` property (1=Base, 2=Advanced, 3=Master) to interface and all implementations including `LaguzClassData`
- **ClassDataFactory** — 6 new Tier-3 (Master) classes with RD-accurate stats and caps:
  - **Trueblade** (Sword, Infantry) — Highest SPD/SKL, tier 3 Myrmidon line; learns Astra
  - **Marshall** (Sword+Lance+Axe, Armored) — Triple weapon type; tier 3 Soldier line; learns Sol
  - **Reaver** (Axe+Bow, Infantry) — Heavy hitter; tier 3 Fighter line; learns Colossus
  - **Archsage** (Fire+Staff, Infantry) — Magical powerhouse; tier 3 Mage line; learns Flare (note: WIND/THUNDER weapons are Phase 9)
  - **Marksman** (Bow, Infantry) — Highest SKL in game; tier 3 Archer line; learns Deadeye
  - **Saint** (Staff+Fire, Infantry) — Healer + caster; tier 3 Cleric line; learns Corona
- **ClassPromotionService.cs** — Extended with Tier 2→3 promotion paths (Swordmaster→Trueblade, General→Marshall, Warrior→Reaver, Sage→Archsage, Sniper→Marksman, Bishop→Saint) and `_masterySkillMap`: mastery skill auto-learned on Tier 3 promotion via `unit.LearnSkill()`
- **SkillDatabase** — 5 new mastery skill implementations:
  - **Astra** (OnAttack, SKL/2% chance): flags `AstraActive`; CombatResolver executes 5 consecutive hits at 50% damage each
  - **Colossus** (OnAttack, STR% chance): flags `ColossusActive`; adds attacker STR to damage for the strike
  - **Flare** (OnAttack, SKL% chance): flags `FlareActive`; halves enemy RES for the strike
  - **Deadeye** (OnAttack, SKL/2% chance): flags `DeadeyeActive`; 2× damage + applies Sleep status on hit
  - **Corona** (OnAttack, SKL% chance): flags `CoronaActive`; halves enemy RES **and** DEF for the strike
- 3 new test files: `ThirdTierPromotionTests.cs`, `MasterySkillTests.cs`, `ThirdTierStatCapTests.cs`

### v2.5 - Inventory System, Items & Multi-Weapon Classes (2026-04-18)
- **IItem interface** (`Domain/Items/IItem.cs`) — Base interface for all items (weapons, consumables, key items). `IWeapon` now extends `IItem`
- **Inventory** (`Domain/Items/Inventory.cs`) — 7-slot item container with Add, Remove, Swap, GetWeapons, GetFirstUsableWeapon
- **ConsumableItem** (`Domain/Items/ConsumableItem.cs`) — Consumable items with use-count tracking: Vulnerary (heal 10 HP, 3 uses), Elixir (full heal, 3 uses), Antitoxin (cure Poison), Pure Water (+7 RES)
- **StatBooster** (`Domain/Items/StatBooster.cs`) — Single-use permanent stat items: Energy Drop (+2 STR), Spirit Dust (+2 MAG), Speedwing (+2 SPD), Seraph Robe (+7 HP), Boots (+2 MOV), and more
- **Unit.cs** — `EquippedWeapon` is now a computed property from inventory (first usable weapon). New `Inventory` property, `CanEquip(IWeapon)` validation, `ApplyStatBoost()` for stat boosters. Backward-compatible constructor preserved
- **ClassData.cs** — `WeaponType` replaced by `UsableWeaponTypes` (IReadOnlyList). `WeaponType` kept as computed property (primary weapon) for backward compat. Promoted classes updated: General (Lance+Sword), Warrior (Axe+Bow), Sage (Fire+Staff), Bishop (Staff+Fire)
- **UnitSnapshot.cs** — Now captures full inventory (`InventoryItemNames`) instead of single weapon. Backward-compatible `Rebuild` overload for old save format
- **JsonFileGameRepository.cs** — DTO updated with `InventoryItems` list, backward compat with old `WeaponName` field on load
- **UIManager.cs** — Version label ("v2.5") displayed in bottom-right corner, semi-transparent
- 4 new test files: `InventoryTests.cs`, `ConsumableItemTests.cs`, `UnitInventoryTests.cs`, `MultiWeaponClassTests.cs`

### v2.4 - AI Self-Preservation: Retreat to Fort (2026-04-18)
- **AIController.cs** - New `TryRetreatToFort` method: when a unit's HP ≤ 30% of MaxHP AND a Fort tile is reachable within its MOV, the unit retreats to the nearest Fort instead of attacking
- Retreat gives the unit the Fort's 20% HP heal per turn (processed by TurnManager at end of phase)
- No retreat if no Fort is within movement range — unit fights normally as fallback
- **Constant** `LowHpThresholdPercent = 30` (tunable)
- **AIControllerTests.cs** - 3 new TDD tests: retreat fires when Fort reachable, no retreat when no Fort exists, threshold boundary (>30% HP = normal attack)

### v2.3 - Sol and Luna Skills (2026-04-17)
- **SolSkill** (OnDamageDealt, SKL/2% activation): heals the attacker for the exact damage dealt on the triggering strike; heals accumulate across multi-hit combos
- **LunaSkill** (OnAttack, SKL/2% activation): halves defender DEF/RES for the triggering strike only (per-strike roll, resets between strikes)
- **CombatContext** - new fields: `LunaActive`, `SolHealAmount`, `LastStrikeDamage`
- **CombatResult** - new field: `AttackerHealedHP` (healed HP reported to callers)
- **CombatResolver** - Luna rolls independently before each attacker strike; Sol fires in a new `OnDamageDealt` hook after each hit that deals damage; Sol heal applied to attacker HP and capped at MaxHP
- **SkillDatabase** - `CreateSol()` + `CreateLuna()` factory methods
- Nihil correctly negates both Sol and Luna on the opponent
- **SolLunaTests.cs** - 14 new TDD tests covering: activation phase, SKL-gated CanActivate, no-heal on miss, damage comparison (Luna vs normal), ActivatedSkills reporting, Nihil negation, Sol+Wrath combo

### v2.2 - Class Promotion System (2026-04-13)
- **ClassPromotionService.cs** - Pure domain service: `CanPromote(unit)` + `Promote(unit)` with full promotion map
- **ClassDataFactory** - 6 promoted classes added: Swordmaster, General, Warrior, Sage, Sniper, Bishop with Fire Emblem-accurate stats/growths/caps
- **MoveType** - `Armored` variant added (General uses it)
- **Unit.ChangeClass(newClass)** - Applies promotion stat bumps (max of current vs new base), resets Level to 1, XP to 0
- Promotion paths: Myrmidon→Swordmaster, Soldier→General, Fighter→Warrior, Mage→Sage, Archer→Sniper, Cleric→Bishop
- Promoted classes have no further promotion (CanPromote returns false)
- **ClassPromotionTests.cs** - 14 TDD tests covering CanPromote guards, stat bumps, class name changes, level/XP reset, all promotion paths

### v2.1 - Broken Weapon Guard in AI (2026-04-12)
- **AIController.cs** - `DecideAction` now checks `unit.HasBrokenWeapon` before any attack/heal logic
- Units with a broken weapon call the new `AdvanceTowardNearestEnemy` helper: they still move toward opponents but never attempt to attack or heal
- Healers with a broken staff simply do nothing (no heal target, no movement toward injured ally)
- **AIControllerTests.cs** - 2 new TDD tests: `DecideAction_BrokenWeaponUnit_DoesNotAttack_ButStillMoves` and `DecideAction_BrokenStaffHealer_DoesNothing`

### v1.9 - Weapon Durability (2026-04-12)
- `IWeapon` extended with `MaxUses`, `CurrentUses`, `IsBroken`, `ConsumeUse()`
- Sentinel value `-1` = unlimited uses (legacy factory methods unchanged)
- `Unit.HasBrokenWeapon` computed property for UI/AI checks
- `CombatResolver` consumes attacker use each engagement; defender use on counter
- `WeaponFactory` gains `*WithDurability()` variants: 30 uses for weapons, 15 for staves
- 10 new TDD tests in `WeaponDurabilityTests.cs`

### v1.8 - JSON File Persistence Adapter (2026-04-12)
- **JsonFileGameRepository.cs** - Hexagonal adapter in `Adapters/Persistence`: persists `GameSnapshot` to a JSON file on disk
  - Pure C# / `System.Text.Json` — zero Unity dependencies, fully testable outside the engine
  - Private DTO layer (`GameSnapshotDto`, `UnitSnapshotDto`) keeps serialisation details out of the domain
  - Auto-creates parent directories; idempotent overwrites on re-save
- **GameSnapshot.Rebuild()** / **UnitSnapshot.Rebuild()** — static factory methods added to domain objects so adapters can reconstruct snapshots without exposing public constructors (open/closed)
- **JsonFileGameRepositoryTests.cs** - 13 new TDD tests: `HasSave` guards, full round-trip for phase, turn, unit count, identity, HP, position, status effects, file creation, nested directory creation, and overwrite behaviour
### v1.7 - Combat Forecast (2026-04-12)
- **CombatForecast.cs** - Immutable value object: deterministic battle stats (damage, hit%, crit%, doubles flag, counter info) computed before dice are rolled
- **CombatForecastService** - Pure domain service; mirrors CombatResolver formulas (SKL×2, AS×2, weapon triangle, terrain avoid) but deterministic
- `FormatSummary()` / `FormatFull()` — one-line and two-sided panel display text
- **UIManager.cs** - `ShowForecast(attacker, defender, map)` / `HideForecast()`: right-side overlay panel with blue border, classic Fire Emblem style
- **CombatForecastTests.cs** - 12 new TDD tests covering structure, clamping, doubles, counter range, weapon-triangle advantage, formatting

### v1.6 - Status-Aware AI Target Scoring (2026-04-12)
- **AIController.cs** - `ScoreAttackOption` now factors in target's active status effect
- `NoCounterBias` (-20): AI prefers attacking sleeping or stunned targets (they cannot counter-attack)
- `RedundantStatusPenalty` (+20): AI deprioritizes re-applying a status the target already has (e.g. poison on already-poisoned)
- Constants are tuned to interact sensibly with existing triangle and terrain biases
- **AIControllerTests.cs** - 3 new TDD tests: prefer sleeping target, prefer stunned target, avoid re-poisoning

### v1.4 - Terrain-Aware AI Positioning (2026-04-12)
- **AIController.cs** - `FindBestAttackPosition` now factors in terrain defense bonus when scoring attack positions
- `ScoreTerrainBonus()`: each point of tile defense reduces score by `TerrainDefenseBiasPerPoint` (8)
- AI prefers Fort > Forest > Plain when multiple tiles can attack the same target
- Tuned so terrain preference coexists with weapon triangle and finisher heuristics without overriding them
- **AIControllerTests.cs** - 2 new TDD tests: Forest preference over Plain, Fort preference over Forest

### v1.3 - Experience & Level-Up System (2026-04-12)
- **Unit.cs** - Extended with `Level`, `Experience`, `GainExperience(amount, rng)` method
- ... (file continues unchanged)


## Contact & Support

For questions about architecture, code structure, or how specific systems work, refer to:
- README.md (user guide and troubleshooting)
- Inline code comments (complex algorithms)
- Test files (usage examples)

---

**Project Status**: ✅ Feature Complete & Ready to Play
**Version**: 2.0.0
**Build Date**: 2026-04-12
**Architecture**: Domain-Driven Design + Hexagonal Architecture
**Code Quality**: SOLID Principles, Clean Code standards
- feat(IA): AI prefers higher-defense terrain when choosing attack position. Added test to verify AIController selects Fort tiles when scores tie.
- feat(IA): refine attack-position tie-breakers to prefer higher terrain defense and, when equal, the closer tile to the attacker. Added test DecideAction_PrefersCloserTile_WhenDefenseAndScoreEqual.
- test(technical): added GameSaveService load-null test to assert Load() returns null when repository has no save
- dev(extras): added run-tests.sh convenience script to run DomainTests from the terminal
- chore(extras): improved run-tests.sh to detect missing build artifacts, run dotnet build automatically, provide a helpful error message when the dotnet CLI is not present, and accept an optional filter argument to run a subset of tests
