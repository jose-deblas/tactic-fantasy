# Implementation Summary

## [2026-04-18]
**Category:** Extras

### Changes:
- Added `PositionUtils` (domain utility) to centralize grid distance calculation (Chebyshev distance).
- Added `PositionUtilsTests` (TDD-first) verifying correctness and symmetry of the distance function.

### Rationale:
Small domain utility to avoid duplicated distance logic and provide a clear, well-tested helper for future refactors (e.g., replacing inline Math.Max distance calculations).

**Testing:** New tests added under `Assets/Scripts/Tests/PositionUtilsTests.cs`.

## [2026-04-18]
**Category:** Gameplay

### Changes:
- Introduced a domain-level Poison status effect (Assets/Scripts/Domain/StatusEffect.cs) implementing IStatusEffect and a minimal IUnit interface.
- Added TDD-style test `StatusEffectTests` (Tests/DomainTests/StatusEffectTests.cs) to validate poison ticking, damage application, and expiration behavior.

### Rationale:
Add a small, well-contained gameplay mechanic (damage-over-time) in a domain-pure assembly to follow DDD/Hexagonal architecture: status effects live in the domain and are testable outside Unity. This is a minimal step towards a broader status system while respecting small, cohesive changes and TDD.

**Testing:** See `Tests/DomainTests/StatusEffectTests.cs`. To run tests you can use the included .NET test project (if .NET SDK is installed):

  cd Tests/DomainTests && dotnet test
