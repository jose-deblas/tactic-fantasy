# Implementation Summary

## [2026-04-18]
**Category:** AI

### Changes:
- Fixed AI target-scoring: implemented the finisher heuristic so AI strongly prefers targets that can be killed in one attack (single-attack kills). This ensures the "finisher" behavior described in comments is enforced.

### Rationale:
Previously the code documented a finisher heuristic but did not implement it; AI could avoid finishing low-HP enemies due to weapon-triangle or terrain biases. This small, focused fix aligns behavior with tests and game design expectations while keeping change minimal and domain-focused.

**Testing:** Existing AIController tests exercise finisher behavior (`DecideAction_PrefersFinalBlow_OverTriangleAdvantageWhenHpIsVeryLow`).


## [2026-04-18]
**Category:** Extras


### Changes:
- Added `PositionUtils` (domain utility) to centralize grid distance calculation (Chebyshev distance).
- Added `PositionUtilsTests` (TDD-first) verifying correctness and symmetry of the distance function.
- Added `FloatUtils.Clamp` to safely clamp floats with defensive behavior when min > max.
- Added `FloatUtilsTests` (TDD) covering within-range, below-min, above-max and swapped min/max scenarios.

### Rationale:
Small domain utilities to avoid duplicated logic and provide clear, well-tested helpers for future refactors. `FloatUtils.Clamp` is commonly needed across gameplay and UI layers and benefits from centralized, tested behavior.

**Testing:** New tests added under `Tests/DomainTests/FloatUtilsTests.cs` and existing tests continue to cover domain status effects.

## [2026-04-18]
**Category:** Gameplay

### Changes:
- Introduced a domain-level Poison status effect (Assets/Scripts/Domain/StatusEffect.cs) implementing IStatusEffect and a minimal IUnit interface.
- Added TDD-style test `StatusEffectTests` (Tests/DomainTests/StatusEffectTests.cs) to validate poison ticking, damage application, and expiration behavior.

### Rationale:
Add a small, well-contained gameplay mechanic (damage-over-time) in a domain-pure assembly to follow DDD/Hexagonal architecture: status effects live in the domain and are testable outside Unity. This is a minimal step towards a broader status system while respecting small, cohesive changes and TDD.

**Testing:** See `Tests/DomainTests/StatusEffectTests.cs`. To run tests you can use the included .NET test project (if .NET SDK is installed):

  cd Tests/DomainTests && dotnet test

## [2026-04-18]
**Category:** Presentation

### Changes:
- Enhanced terrain info UI: added emoji icons and colored numeric values for move cost, avoid, defense and heal to improve rapid readability.

### Rationale:
Small presentation improvement to make terrain properties scannable during play; keeps domain logic untouched and follows single-responsibility in UI adapter.

**Testing:** Visual change only; no domain logic changed.
