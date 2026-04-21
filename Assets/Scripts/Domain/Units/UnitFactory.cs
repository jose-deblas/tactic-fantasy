using System;
using TacticFantasy.Domain.Weapons;
using System.Linq;

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

        public static IWeapon CreateWindTome()
        {
            return new Weapon("Wind", WeaponType.WIND, DamageType.Magical, 4, 3, 90, 0, 1, 2);
        }

        public static IWeapon CreateThunderTome()
        {
            return new Weapon("Thunder", WeaponType.THUNDER, DamageType.Magical, 6, 6, 80, 0, 1, 2);
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

        /// <summary>Refresh staff — allows refreshing an ally who has already acted this turn.</summary>
        public static IWeapon CreateRefreshStaff()
        {
            return new Weapon("Refresh", WeaponType.REFRESH, DamageType.Physical, 0, 0, 100, 0, 1, 1);
        }

        // ── Weapon tiers ─────────────────────────────────────────────────────

        public static IWeapon CreateSteelSword()
        {
            return new Weapon("Steel Sword", WeaponType.SWORD, DamageType.Physical, 8, 8, 80, 0, 1, 1, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverSword()
        {
            return new Weapon("Silver Sword", WeaponType.SWORD, DamageType.Physical, 11, 7, 75, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveSword()
        {
            return new Weapon("Brave Sword", WeaponType.SWORD, DamageType.Physical, 7, 9, 70, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateSteelLance()
        {
            return new Weapon("Steel Lance", WeaponType.LANCE, DamageType.Physical, 10, 11, 70, 0, 1, 1, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverLance()
        {
            return new Weapon("Silver Lance", WeaponType.LANCE, DamageType.Physical, 13, 10, 70, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveLance()
        {
            return new Weapon("Brave Lance", WeaponType.LANCE, DamageType.Physical, 9, 12, 65, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateSteelAxe()
        {
            return new Weapon("Steel Axe", WeaponType.AXE, DamageType.Physical, 11, 13, 65, 0, 1, 1, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverAxe()
        {
            return new Weapon("Silver Axe", WeaponType.AXE, DamageType.Physical, 14, 12, 65, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveAxe()
        {
            return new Weapon("Brave Axe", WeaponType.AXE, DamageType.Physical, 9, 14, 60, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        // ── Wind / Thunder tiers ─────────────────────────────────────────────

        public static IWeapon CreateSteelWindTome()
        {
            return new Weapon("Elwind", WeaponType.WIND, DamageType.Magical, 7, 5, 85, 0, 1, 2, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverWindTome()
        {
            return new Weapon("Tornado", WeaponType.WIND, DamageType.Magical, 10, 7, 80, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateSteelThunderTome()
        {
            return new Weapon("Elthunder", WeaponType.THUNDER, DamageType.Magical, 9, 8, 75, 0, 1, 2, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverThunderTome()
        {
            return new Weapon("Thoron", WeaponType.THUNDER, DamageType.Magical, 12, 10, 70, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateSteelFireTome()
        {
            return new Weapon("Elfire", WeaponType.FIRE, DamageType.Magical, 8, 6, 80, 0, 1, 2, uses: 25, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverFireTome()
        {
            return new Weapon("Arcfire", WeaponType.FIRE, DamageType.Magical, 11, 8, 75, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        // ── Durability variants ───────────────────────────────────────────────
        // These factories create weapons with finite uses for richer gameplay.

        public static IWeapon CreateIronSwordWithDurability()
        {
            return new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1, uses: 30);
        }

        public static IWeapon CreateIronLanceWithDurability()
        {
            return new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1, uses: 30);
        }

        public static IWeapon CreateIronAxeWithDurability()
        {
            return new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1, uses: 30);
        }

        public static IWeapon CreateIronBowWithDurability()
        {
            return new Weapon("Iron Bow", WeaponType.BOW, DamageType.Physical, 6, 5, 85, 0, 2, 2, uses: 30);
        }

        public static IWeapon CreateFireTomeWithDurability()
        {
            return new Weapon("Fire", WeaponType.FIRE, DamageType.Magical, 5, 4, 85, 0, 1, 2, uses: 30);
        }

        public static IWeapon CreateHealStaffWithDurability()
        {
            return new Weapon("Heal Staff", WeaponType.STAFF, DamageType.Magical, 0, 3, 100, 0, 1, 1, uses: 15);
        }

        public static IWeapon GetWeaponForClass(WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.SWORD => CreateIronSword(),
                WeaponType.LANCE => CreateIronLance(),
                WeaponType.AXE => CreateIronAxe(),
                WeaponType.FIRE => CreateFireTome(),
                WeaponType.WIND => CreateWindTome(),
                WeaponType.THUNDER => CreateThunderTome(),
                WeaponType.BOW => CreateIronBow(),
                WeaponType.STAFF => CreateHealStaff(),
                WeaponType.REFRESH => CreateRefreshStaff(),
                WeaponType.STRIKE => LaguzWeaponFactory.CreateStrike(),
                _ => throw new ArgumentException($"Unknown weapon type: {weaponType}")
            };
        }
    }
}
