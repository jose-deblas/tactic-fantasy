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
        MoveType MoveType { get; }
    }

    public class ClassData : IClassData
    {
        public string Name { get; }
        public CharacterStats BaseStats { get; }
        public CharacterStats CapStats { get; }
        public CharacterStats GrowthRates { get; }
        public WeaponType WeaponType { get; }
        public MoveType MoveType { get; }

        public ClassData(
            string name,
            CharacterStats baseStats,
            CharacterStats capStats,
            CharacterStats growthRates,
            WeaponType weaponType,
            MoveType moveType)
        {
            Name = name;
            BaseStats = baseStats;
            CapStats = capStats;
            GrowthRates = growthRates;
            WeaponType = weaponType;
            MoveType = moveType;
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

        public static IClassData[] GetAllClasses()
        {
            return new IClassData[]
            {
                CreateMyrmidon(),
                CreateSoldier(),
                CreateFighter(),
                CreateMage(),
                CreateArcher(),
                CreateCleric()
            };
        }
    }
}
