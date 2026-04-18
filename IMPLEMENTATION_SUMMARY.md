# Implementation Summary

## [2026-04-18]
**Category:** Extras

### Changes:
- Added `PositionUtils` (domain utility) to centralize grid distance calculation (Chebyshev distance).
- Added `PositionUtilsTests` (TDD-first) verifying correctness and symmetry of the distance function.

### Rationale:
Small domain utility to avoid duplicated distance logic and provide a clear, well-tested helper for future refactors (e.g., replacing inline Math.Max distance calculations).

**Testing:** New tests added under `Assets/Scripts/Tests/PositionUtilsTests.cs`.
