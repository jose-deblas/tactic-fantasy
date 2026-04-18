using System.Collections.Generic;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Units
{
    public enum LaguzRace
    {
        Cat,
        Tiger,
        Lion,
        Hawk,
        Raven,
        RedDragon,
        WhiteDragon,
        Heron
    }

    public class LaguzClassData : IClassData
    {
        public string Name { get; }
        public CharacterStats BaseStats { get; }
        public CharacterStats CapStats { get; }
        public CharacterStats GrowthRates { get; }
        public IReadOnlyList<WeaponType> UsableWeaponTypes { get; }
        public WeaponType WeaponType => UsableWeaponTypes[0];
        public MoveType MoveType { get; }

        /// <summary>Stats used when the Laguz is transformed (full power).</summary>
        public CharacterStats TransformedStats { get; }

        /// <summary>Stats used when the Laguz is untransformed (halved offenses).</summary>
        public CharacterStats UntransformedStats { get; }

        /// <summary>Gauge fill rate per turn when untransformed.</summary>
        public int GaugeFillRate { get; }

        /// <summary>Gauge drain rate per turn when transformed.</summary>
        public int GaugeDrainRate { get; }

        /// <summary>The race of this Laguz unit.</summary>
        public LaguzRace Race { get; }

        /// <summary>MoveType when transformed (birds become Flying).</summary>
        public MoveType TransformedMoveType { get; }

        /// <summary>MoveType when untransformed (always Infantry).</summary>
        public MoveType UntransformedMoveType { get; }

        public int Tier { get; }

        public LaguzClassData(
            string name,
            LaguzRace race,
            CharacterStats transformedStats,
            CharacterStats untransformedStats,
            CharacterStats capStats,
            CharacterStats growthRates,
            MoveType untransformedMoveType,
            MoveType transformedMoveType,
            int gaugeFillRate,
            int gaugeDrainRate,
            int tier = 1)
        {
            Tier = tier;
            Name = name;
            Race = race;
            TransformedStats = transformedStats;
            UntransformedStats = untransformedStats;
            BaseStats = untransformedStats;
            CapStats = capStats;
            GrowthRates = growthRates;
            UsableWeaponTypes = new[] { WeaponType.STRIKE };
            UntransformedMoveType = untransformedMoveType;
            TransformedMoveType = transformedMoveType;
            MoveType = untransformedMoveType;
            GaugeFillRate = gaugeFillRate;
            GaugeDrainRate = gaugeDrainRate;
        }
    }

    public static class LaguzClassDataFactory
    {
        // ── Beast tribe ─────────────────────────────────────────────────────

        public static LaguzClassData CreateCat()
        {
            return new LaguzClassData(
                "Cat",
                LaguzRace.Cat,
                transformedStats:     new CharacterStats(24, 10, 2, 12, 14, 8, 6, 4, 7),
                untransformedStats:   new CharacterStats(24, 5,  1, 12, 14, 8, 3, 2, 6),
                capStats:             new CharacterStats(40, 22, 10, 26, 28, 25, 20, 16, 9),
                growthRates:          new CharacterStats(55, 35, 5, 45, 55, 35, 20, 15, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Infantry,
                gaugeFillRate: 8,
                gaugeDrainRate: 2
            );
        }

        public static LaguzClassData CreateTiger()
        {
            return new LaguzClassData(
                "Tiger",
                LaguzRace.Tiger,
                transformedStats:     new CharacterStats(32, 14, 0, 8, 8, 5, 12, 4, 6),
                untransformedStats:   new CharacterStats(32, 7,  0, 8, 8, 5, 6,  2, 5),
                capStats:             new CharacterStats(50, 30, 5, 22, 22, 20, 28, 14, 9),
                growthRates:          new CharacterStats(70, 50, 0, 30, 30, 20, 35, 10, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Infantry,
                gaugeFillRate: 5,
                gaugeDrainRate: 2
            );
        }

        public static LaguzClassData CreateLion()
        {
            return new LaguzClassData(
                "Lion",
                LaguzRace.Lion,
                transformedStats:     new CharacterStats(38, 18, 2, 10, 10, 6, 14, 6, 6),
                untransformedStats:   new CharacterStats(38, 9,  1, 10, 10, 6, 7,  3, 5),
                capStats:             new CharacterStats(55, 36, 8, 24, 24, 22, 32, 18, 9),
                growthRates:          new CharacterStats(75, 60, 5, 35, 30, 20, 40, 15, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Infantry,
                gaugeFillRate: 3,
                gaugeDrainRate: 2
            );
        }

        // ── Bird tribe ──────────────────────────────────────────────────────

        public static LaguzClassData CreateHawk()
        {
            return new LaguzClassData(
                "Hawk",
                LaguzRace.Hawk,
                transformedStats:     new CharacterStats(28, 14, 2, 10, 10, 6, 10, 4, 7),
                untransformedStats:   new CharacterStats(28, 7,  1, 10, 10, 6, 5,  2, 5),
                capStats:             new CharacterStats(45, 28, 8, 24, 26, 22, 24, 16, 9),
                growthRates:          new CharacterStats(60, 45, 5, 35, 40, 25, 25, 12, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Flying,
                gaugeFillRate: 5,
                gaugeDrainRate: 2
            );
        }

        public static LaguzClassData CreateRaven()
        {
            return new LaguzClassData(
                "Raven",
                LaguzRace.Raven,
                transformedStats:     new CharacterStats(22, 10, 2, 14, 16, 10, 6, 4, 7),
                untransformedStats:   new CharacterStats(22, 5,  1, 14, 16, 10, 3, 2, 5),
                capStats:             new CharacterStats(38, 22, 8, 28, 30, 28, 18, 16, 9),
                growthRates:          new CharacterStats(50, 30, 5, 50, 55, 40, 15, 15, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Flying,
                gaugeFillRate: 7,
                gaugeDrainRate: 2
            );
        }

        // ── Dragon tribe ────────────────────────────────────────────────────

        public static LaguzClassData CreateRedDragon()
        {
            return new LaguzClassData(
                "Red Dragon",
                LaguzRace.RedDragon,
                transformedStats:     new CharacterStats(46, 16, 8, 8, 6, 4, 18, 12, 5),
                untransformedStats:   new CharacterStats(46, 8,  4, 8, 6, 4, 9,  6,  4),
                capStats:             new CharacterStats(60, 34, 20, 20, 18, 18, 36, 28, 7),
                growthRates:          new CharacterStats(80, 45, 25, 25, 20, 15, 50, 30, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Infantry,
                gaugeFillRate: 2,
                gaugeDrainRate: 2
            );
        }

        public static LaguzClassData CreateWhiteDragon()
        {
            return new LaguzClassData(
                "White Dragon",
                LaguzRace.WhiteDragon,
                transformedStats:     new CharacterStats(56, 20, 14, 12, 10, 8, 22, 18, 5),
                untransformedStats:   new CharacterStats(56, 10, 7,  12, 10, 8, 11, 9,  4),
                capStats:             new CharacterStats(70, 40, 30, 26, 24, 22, 40, 36, 7),
                growthRates:          new CharacterStats(85, 55, 35, 30, 25, 20, 55, 40, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Infantry,
                gaugeFillRate: 1,
                gaugeDrainRate: 2
            );
        }

        // ── Special ─────────────────────────────────────────────────────────

        public static LaguzClassData CreateHeron()
        {
            return new LaguzClassData(
                "Laguz Heron",
                LaguzRace.Heron,
                transformedStats:     new CharacterStats(14, 0, 5, 7, 10, 10, 2, 8, 6),
                untransformedStats:   new CharacterStats(14, 0, 5, 7, 10, 10, 2, 8, 5),
                capStats:             new CharacterStats(26, 0, 18, 22, 28, 25, 12, 22, 9),
                growthRates:          new CharacterStats(45, 0, 45, 40, 50, 50, 15, 40, 0),
                untransformedMoveType: MoveType.Infantry,
                transformedMoveType:   MoveType.Flying,
                gaugeFillRate: 4,
                gaugeDrainRate: 2
            );
        }
    }
}
