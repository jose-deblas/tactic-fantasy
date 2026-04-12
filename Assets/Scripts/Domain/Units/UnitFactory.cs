using System;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Units
{
    public interface IUnitFactory
    {
        IUnit CreateUnit(int id, string name, Team team, IClassData classData, (int, int) position, IWeapon weapon);
    }

    public class UnitFactory : IUnitFactory
    {
        private static int _nextId = 1;

        public IUnit CreateUnit(int id, string name, Team team, IClassData classData, (int, int) position, IWeapon weapon)
        {
            return new Unit(id, name, team, classData, classData.BaseStats, position, weapon);
        }

        public static int GetNextId()
        {
            return _nextId++;
        }

        public static void ResetIdCounter()
        {
            _nextId = 1;
        }
    }

    public static class WeaponFactory
    {
        public static IWeapon CreateIronSword()
        {
            return new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
        }

        public static IWeapon CreateIronLance()
        {
            return new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1);
        }

        public static IWeapon CreateIronAxe()
        {
            return new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1);
        }

        public static IWeapon CreateFireTome()
        {
            return new Weapon("Fire", WeaponType.FIRE, DamageType.Magical, 5, 4, 85, 0, 1, 2);
        }

        public static IWeapon CreateIronBow()
        {
            return new Weapon("Iron Bow", WeaponType.BOW, DamageType.Physical, 6, 5, 85, 0, 2, 2);
        }

        public static IWeapon CreateHealStaff()
        {
            return new Weapon("Heal Staff", WeaponType.STAFF, DamageType.Magical, 0, 3, 100, 0, 1, 1);
        }

        /// <summary>Inflicts Poison (3 turns) on a successful hit.</summary>
        public static IWeapon CreatePoisonSword()
        {
            return new Weapon("Poison Sword", WeaponType.SWORD, DamageType.Physical, 4, 5, 80, 0, 1, 1,
                onHitStatus: StatusEffectType.Poison, onHitStatusDuration: 3);
        }

        /// <summary>Sleep tome — puts target to sleep (2 turns) on hit but deals 0 damage.</summary>
        public static IWeapon CreateSleepStaff()
        {
            return new Weapon("Sleep Staff", WeaponType.STAFF, DamageType.Magical, 0, 2, 75, 0, 1, 2,
                onHitStatus: StatusEffectType.Sleep, onHitStatusDuration: 2);
        }

        public static IWeapon GetWeaponForClass(WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.SWORD => CreateIronSword(),
                WeaponType.LANCE => CreateIronLance(),
                WeaponType.AXE => CreateIronAxe(),
                WeaponType.FIRE => CreateFireTome(),
                WeaponType.BOW => CreateIronBow(),
                WeaponType.STAFF => CreateHealStaff(),
                _ => throw new ArgumentException($"Unknown weapon type: {weaponType}")
            };
        }
    }
}
