using System.Collections.Generic;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Units
{
    public interface IClassData
    {
        string Name { get; }
        CharacterStats BaseStats { get; }
        CharacterStats CapStats { get; }
        CharacterStats GrowthRates { get; }
        WeaponType WeaponType { get; }
        IReadOnlyList<WeaponType> UsableWeaponTypes { get; }
        MoveType MoveType { get; }
        /// <summary>Class tier: 1=Base, 2=Advanced, 3=Master.</summary>
        int Tier { get; }
    }

    public class ClassData : IClassData
    {
        public string Name { get; }
        public CharacterStats BaseStats { get; }
        public CharacterStats CapStats { get; }
        public CharacterStats GrowthRates { get; }
        public IReadOnlyList<WeaponType> UsableWeaponTypes { get; }
        public WeaponType WeaponType => UsableWeaponTypes[0];
        public MoveType MoveType { get; }
        public int Tier { get; }

        public ClassData(
            string name,
            CharacterStats baseStats,
            CharacterStats capStats,
            CharacterStats growthRates,
            WeaponType weaponType,
            MoveType moveType,
            int tier = 1)
            : this(name, baseStats, capStats, growthRates, new[] { weaponType }, moveType, tier)
        {
        }

        public ClassData(
            string name,
            CharacterStats baseStats,
            CharacterStats capStats,
            CharacterStats growthRates,
            IReadOnlyList<WeaponType> usableWeaponTypes,
            MoveType moveType,
            int tier = 1)
        {
            Name = name;
            BaseStats = baseStats;
            CapStats = capStats;
            GrowthRates = growthRates;
            UsableWeaponTypes = usableWeaponTypes;
            MoveType = moveType;
            Tier = tier;
        }
    }

    public static class ClassDataFactory
    {
        public static IClassData CreateMyrmidon()
        {
            return new ClassData(
                "Myrmidon",
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                new CharacterStats(30, 20, 5, 25, 25, 20, 20, 10, 9),
                new CharacterStats(55, 35, 5, 50, 55, 30, 20, 10, 0),
                WeaponType.SWORD,
                MoveType.Infantry
            );
        }

        public static IClassData CreateSoldier()
        {
            return new ClassData(
                "Soldier",
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                new CharacterStats(32, 24, 5, 22, 22, 18, 25, 15, 9),
                new CharacterStats(60, 40, 5, 35, 35, 20, 25, 15, 0),
                WeaponType.LANCE,
                MoveType.Infantry
            );
        }

        public static IClassData CreateFighter()
        {
            return new ClassData(
                "Fighter",
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                new CharacterStats(35, 28, 5, 20, 23, 18, 23, 8, 9),
                new CharacterStats(70, 55, 0, 30, 35, 25, 20, 5, 0),
                WeaponType.AXE,
                MoveType.Infantry
            );
        }

        public static IClassData CreateMage()
        {
            return new ClassData(
                "Mage",
                new CharacterStats(16, 0, 8, 7, 7, 5, 3, 7, 5),
                new CharacterStats(28, 5, 25, 22, 22, 20, 13, 25, 9),
                new CharacterStats(50, 5, 55, 35, 40, 30, 10, 35, 0),
                WeaponType.FIRE,
                MoveType.Infantry
            );
        }

        public static IClassData CreateArcher()
        {
            return new ClassData(
                "Archer",
                new CharacterStats(18, 6, 0, 10, 7, 5, 5, 2, 5),
                new CharacterStats(30, 20, 5, 25, 23, 20, 20, 12, 9),
                new CharacterStats(55, 35, 0, 50, 40, 30, 20, 10, 0),
                WeaponType.BOW,
                MoveType.Infantry
            );
        }

        public static IClassData CreateCleric()
        {
            return new ClassData(
                "Cleric",
                new CharacterStats(16, 0, 7, 5, 5, 7, 2, 8, 5),
                new CharacterStats(28, 0, 24, 18, 18, 25, 12, 25, 9),
                new CharacterStats(45, 0, 50, 30, 30, 40, 10, 40, 0),
                WeaponType.STAFF,
                MoveType.Infantry
            );
        }

        public static IClassData CreateHeron()
        {
            return new ClassData(
                "Heron",
                new CharacterStats(14, 0, 5, 7, 10, 10, 2, 8, 5),
                new CharacterStats(26, 0, 18, 22, 28, 25, 12, 22, 9),
                new CharacterStats(45, 0, 45, 40, 50, 50, 15, 40, 0),
                WeaponType.REFRESH,
                MoveType.Infantry
            );
        }

        public static IClassData CreateDancer()
        {
            return new ClassData(
                "Dancer",
                new CharacterStats(16, 0, 4, 6, 12, 8, 3, 6, 5),
                new CharacterStats(28, 0, 16, 20, 30, 23, 14, 20, 9),
                new CharacterStats(50, 0, 40, 35, 55, 45, 18, 35, 0),
                WeaponType.REFRESH,
                MoveType.Infantry
            );
        }

        // ── Promoted classes ──────────────────────────────────────────────────

        public static IClassData CreateSwordmaster()
        {
            return new ClassData(
                "Swordmaster",
                new CharacterStats(24, 10, 0, 17, 18, 7, 8, 2, 6),
                new CharacterStats(40, 26, 8, 30, 30, 25, 25, 14, 9),
                new CharacterStats(65, 45, 5, 60, 65, 35, 25, 12, 0),
                WeaponType.SWORD,
                MoveType.Infantry,
                tier: 2
            );
        }

        public static IClassData CreateGeneral()
        {
            return new ClassData(
                "General",
                new CharacterStats(26, 12, 0, 10, 8, 4, 15, 8, 4),
                new CharacterStats(42, 28, 5, 24, 22, 20, 30, 20, 7),
                new CharacterStats(70, 50, 5, 40, 35, 25, 35, 20, 0),
                new[] { WeaponType.LANCE, WeaponType.SWORD },
                MoveType.Armored,
                tier: 2
            );
        }

        public static IClassData CreateWarrior()
        {
            return new ClassData(
                "Warrior",
                new CharacterStats(30, 14, 0, 8, 9, 5, 10, 2, 6),
                new CharacterStats(46, 34, 5, 24, 26, 22, 28, 10, 9),
                new CharacterStats(75, 65, 0, 35, 40, 28, 25, 8, 0),
                new[] { WeaponType.AXE, WeaponType.BOW },
                MoveType.Infantry,
                tier: 2
            );
        }

        public static IClassData CreateSage()
        {
            return new ClassData(
                "Sage",
                new CharacterStats(22, 0, 13, 10, 10, 7, 6, 12, 6),
                new CharacterStats(36, 8, 30, 26, 26, 24, 18, 30, 9),
                new CharacterStats(55, 5, 65, 40, 45, 35, 15, 40, 0),
                new[] { WeaponType.FIRE, WeaponType.WIND, WeaponType.THUNDER, WeaponType.STAFF },
                MoveType.Infantry,
                tier: 2
            );
        }

        public static IClassData CreateSniper()
        {
            return new ClassData(
                "Sniper",
                new CharacterStats(24, 10, 0, 16, 10, 7, 8, 4, 6),
                new CharacterStats(38, 26, 5, 30, 27, 24, 24, 16, 9),
                new CharacterStats(60, 45, 0, 60, 45, 35, 25, 14, 0),
                WeaponType.BOW,
                MoveType.Infantry,
                tier: 2
            );
        }

        public static IClassData CreateBishop()
        {
            return new ClassData(
                "Bishop",
                new CharacterStats(20, 0, 12, 8, 8, 9, 5, 13, 6),
                new CharacterStats(34, 0, 28, 22, 22, 28, 16, 28, 9),
                new CharacterStats(50, 0, 60, 35, 35, 45, 14, 45, 0),
                new[] { WeaponType.STAFF, WeaponType.FIRE },
                MoveType.Infantry,
                tier: 2
            );
        }

        // ── Third-tier (Master) classes ──────────────────────────────────────

        public static IClassData CreateTrueblade()
        {
            return new ClassData(
                "Trueblade",
                new CharacterStats(32, 16, 2, 22, 24, 10, 12, 5, 7),
                new CharacterStats(50, 34, 12, 38, 38, 30, 30, 18, 9),
                new CharacterStats(70, 55, 10, 70, 70, 40, 30, 15, 0),
                WeaponType.SWORD,
                MoveType.Infantry,
                tier: 3
            );
        }

        public static IClassData CreateMarshall()
        {
            return new ClassData(
                "Marshall",
                new CharacterStats(34, 18, 2, 14, 12, 6, 20, 12, 5),
                new CharacterStats(52, 36, 10, 30, 28, 24, 38, 26, 7),
                new CharacterStats(75, 60, 10, 45, 40, 30, 40, 25, 0),
                new[] { WeaponType.SWORD, WeaponType.LANCE, WeaponType.AXE },
                MoveType.Armored,
                tier: 3
            );
        }

        public static IClassData CreateReaver()
        {
            return new ClassData(
                "Reaver",
                new CharacterStats(38, 20, 2, 12, 13, 7, 14, 4, 7),
                new CharacterStats(56, 40, 8, 28, 30, 26, 32, 14, 9),
                new CharacterStats(80, 70, 5, 40, 45, 32, 30, 12, 0),
                new[] { WeaponType.AXE, WeaponType.BOW },
                MoveType.Infantry,
                tier: 3
            );
        }

        public static IClassData CreateArchsage()
        {
            return new ClassData(
                "Archsage",
                new CharacterStats(28, 2, 18, 14, 14, 9, 8, 16, 7),
                new CharacterStats(44, 12, 38, 32, 32, 28, 22, 36, 9),
                new CharacterStats(60, 10, 75, 50, 50, 40, 20, 50, 0),
                new[] { WeaponType.FIRE, WeaponType.WIND, WeaponType.THUNDER, WeaponType.STAFF },
                MoveType.Infantry,
                tier: 3
            );
        }

        public static IClassData CreateMarksman()
        {
            return new ClassData(
                "Marksman",
                new CharacterStats(30, 14, 2, 22, 14, 9, 12, 6, 7),
                new CharacterStats(46, 32, 8, 36, 32, 28, 28, 20, 9),
                new CharacterStats(65, 55, 5, 70, 50, 40, 30, 18, 0),
                WeaponType.BOW,
                MoveType.Infantry,
                tier: 3
            );
        }

        public static IClassData CreateSaint()
        {
            return new ClassData(
                "Saint",
                new CharacterStats(26, 2, 16, 10, 10, 12, 7, 18, 7),
                new CharacterStats(42, 5, 34, 26, 26, 32, 20, 34, 9),
                new CharacterStats(55, 5, 70, 40, 40, 50, 18, 55, 0),
                new[] { WeaponType.STAFF, WeaponType.FIRE },
                MoveType.Infantry,
                tier: 3
            );
        }

        public static IClassData CreateThief()
        {
            return new ClassData(
                "Thief",
                new CharacterStats(17, 5, 0, 9, 13, 6, 4, 1, 7),
                new CharacterStats(28, 16, 5, 26, 28, 22, 14, 10, 9),
                new CharacterStats(40, 25, 0, 50, 60, 35, 15, 10, 0),
                WeaponType.SWORD,
                MoveType.Infantry
            );
        }

        public static IClassData CreateRogue()
        {
            return new ClassData(
                "Rogue",
                new CharacterStats(20, 8, 0, 14, 17, 8, 6, 3, 7),
                new CharacterStats(34, 22, 5, 30, 34, 28, 18, 14, 9),
                new CharacterStats(45, 30, 0, 55, 65, 40, 18, 15, 0),
                WeaponType.SWORD,
                MoveType.Infantry,
                tier: 2
            );
        }

        public static IClassData[] GetAllClasses()
        {
            return new IClassData[]
            {
                CreateMyrmidon(),
                CreateSoldier(),
                CreateFighter(),
                CreateMage(),
                CreateArcher(),
                CreateCleric(),
                CreateHeron(),
                CreateDancer(),
                CreateThief(),
                CreateSwordmaster(),
                CreateGeneral(),
                CreateWarrior(),
                CreateSage(),
                CreateSniper(),
                CreateBishop(),
                CreateRogue(),
                CreateTrueblade(),
                CreateMarshall(),
                CreateReaver(),
                CreateArchsage(),
                CreateMarksman(),
                CreateSaint()
            };
        }
    }
}
