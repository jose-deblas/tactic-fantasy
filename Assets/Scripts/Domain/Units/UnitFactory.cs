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
            // Iron tier: 40 uses (basic weapon)
            return new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateIronLance()
        {
            // Iron tier: 40 uses
            return new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateIronAxe()
        {
            // Iron tier: 40 uses
            return new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateFireTome()
        {
            // Basic tome: 40 uses
            return new Weapon("Fire", WeaponType.FIRE, DamageType.Magical, 5, 4, 85, 0, 1, 2, uses: 40);
        }

        public static IWeapon CreateWindTome()
        {
            // Basic tome: 40 uses
            return new Weapon("Wind", WeaponType.WIND, DamageType.Magical, 4, 3, 90, 0, 1, 2, uses: 40);
        }

        public static IWeapon CreateThunderTome()
        {
            // Basic tome: 40 uses
            return new Weapon("Thunder", WeaponType.THUNDER, DamageType.Magical, 6, 6, 80, 0, 1, 2, uses: 40);
        }

        public static IWeapon CreateIronBow()
        {
            // Iron tier bow: 40 uses
            return new Weapon("Iron Bow", WeaponType.BOW, DamageType.Physical, 6, 5, 85, 0, 2, 2, uses: 40);
        }

        public static IWeapon CreateHealStaff()
        {
            // Staffs: limited durability (healing staff common): 15 uses
            return new Weapon("Heal Staff", WeaponType.STAFF, DamageType.Magical, 0, 3, 100, 0, 1, 1, uses: 15);
        }

        /// <summary>Inflicts Poison (3 turns) on a successful hit.</summary>
        public static IWeapon CreatePoisonSword()
        {
            // Poison Sword: treat as iron-tier weapon (40 uses)
            return new Weapon("Poison Sword", WeaponType.SWORD, DamageType.Physical, 4, 5, 80, 0, 1, 1,
                onHitStatus: StatusEffectType.Poison, onHitStatusDuration: 3, uses: 40);
        }

        /// <summary>Sleep tome — puts target to sleep (2 turns) on hit but deals 0 damage.</summary>
        public static IWeapon CreateSleepStaff()
        {
            // Sleep Staff: staff durability similar to heal staff
            return new Weapon("Sleep Staff", WeaponType.STAFF, DamageType.Magical, 0, 2, 75, 0, 1, 2,
                onHitStatus: StatusEffectType.Sleep, onHitStatusDuration: 2, uses: 15);
        }

        /// <summary>Refresh staff — allows refreshing an ally who has already acted this turn.</summary>
        public static IWeapon CreateRefreshStaff()
        {
            // Refresh staff: limited uses to prevent abuse
            return new Weapon("Refresh", WeaponType.REFRESH, DamageType.Physical, 0, 0, 100, 0, 1, 1, uses: 15);
        }

        // ── Weapon tiers ─────────────────────────────────────────────────────

        public static IWeapon CreateSteelSword()
        {
            // Steel tier: 30 uses
            return new Weapon("Steel Sword", WeaponType.SWORD, DamageType.Physical, 8, 8, 80, 0, 1, 1, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverSword()
        {
            // Silver tier: 20 uses
            return new Weapon("Silver Sword", WeaponType.SWORD, DamageType.Physical, 11, 7, 75, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveSword()
        {
            // Brave weapons are powerful but still finite: 20 uses
            return new Weapon("Brave Sword", WeaponType.SWORD, DamageType.Physical, 7, 9, 70, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateSteelLance()
        {
            // Steel tier: 30 uses
            return new Weapon("Steel Lance", WeaponType.LANCE, DamageType.Physical, 10, 11, 70, 0, 1, 1, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverLance()
        {
            // Silver tier: 20 uses
            return new Weapon("Silver Lance", WeaponType.LANCE, DamageType.Physical, 13, 10, 70, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveLance()
        {
            // Brave weapons: 20 uses
            return new Weapon("Brave Lance", WeaponType.LANCE, DamageType.Physical, 9, 12, 65, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateSteelAxe()
        {
            // Steel tier: 30 uses
            return new Weapon("Steel Axe", WeaponType.AXE, DamageType.Physical, 11, 13, 65, 0, 1, 1, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverAxe()
        {
            // Silver tier: 20 uses
            return new Weapon("Silver Axe", WeaponType.AXE, DamageType.Physical, 14, 12, 65, 0, 1, 1, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateBraveAxe()
        {
            // Brave weapons: 20 uses
            return new Weapon("Brave Axe", WeaponType.AXE, DamageType.Physical, 9, 14, 60, 0, 1, 1, uses: 20, isBrave: true, requiredRank: WeaponRank.B);
        }

        // ── Wind / Thunder tiers ─────────────────────────────────────────────

        public static IWeapon CreateSteelWindTome()
        {
            // Steel tier tome: 30 uses
            return new Weapon("Elwind", WeaponType.WIND, DamageType.Magical, 7, 5, 85, 0, 1, 2, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverWindTome()
        {
            // Silver tier tome: 20 uses
            return new Weapon("Tornado", WeaponType.WIND, DamageType.Magical, 10, 7, 80, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateSteelThunderTome()
        {
            // Steel tier tome: 30 uses
            return new Weapon("Elthunder", WeaponType.THUNDER, DamageType.Magical, 9, 8, 75, 0, 1, 2, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverThunderTome()
        {
            return new Weapon("Thoron", WeaponType.THUNDER, DamageType.Magical, 12, 10, 70, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateSteelFireTome()
        {
            // Steel tier tome: 30 uses
            return new Weapon("Elfire", WeaponType.FIRE, DamageType.Magical, 8, 6, 80, 0, 1, 2, uses: 30, requiredRank: WeaponRank.D);
        }

        public static IWeapon CreateSilverFireTome()
        {
            // Silver tier tome: 20 uses
            return new Weapon("Arcfire", WeaponType.FIRE, DamageType.Magical, 11, 8, 75, 0, 1, 2, uses: 20, requiredRank: WeaponRank.A);
        }

        // ── Long-range tomes (grimorios de largo alcance) ─────────────────────
        // These are specialized tomes with very large max range (8-10). Uses are assigned
        // based on potency: range 8 = basic (40 uses), range 9 = medium (30 uses), range 10 = powerful (20 uses).

        public static IWeapon CreateLongFireTomeRange8()
        {
            return new Weapon("Long Fire Tome (Range 8)", WeaponType.FIRE, DamageType.Magical,
                might: 8, weight: 6, hit: 70, crit: 0, minRange: 1, maxRange: 8, uses: 40, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateLongFireTomeRange9()
        {
            return new Weapon("Long Fire Tome (Range 9)", WeaponType.FIRE, DamageType.Magical,
                might: 10, weight: 7, hit: 65, crit: 0, minRange: 1, maxRange: 9, uses: 30, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateLongFireTomeRange10()
        {
            return new Weapon("Long Fire Tome (Range 10)", WeaponType.FIRE, DamageType.Magical,
                might: 13, weight: 8, hit: 60, crit: 0, minRange: 1, maxRange: 10, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateLongWindTomeRange8()
        {
            return new Weapon("Long Wind Tome (Range 8)", WeaponType.WIND, DamageType.Magical,
                might: 7, weight: 5, hit: 75, crit: 0, minRange: 1, maxRange: 8, uses: 40, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateLongWindTomeRange9()
        {
            return new Weapon("Long Wind Tome (Range 9)", WeaponType.WIND, DamageType.Magical,
                might: 9, weight: 6, hit: 70, crit: 0, minRange: 1, maxRange: 9, uses: 30, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateLongWindTomeRange10()
        {
            return new Weapon("Long Wind Tome (Range 10)", WeaponType.WIND, DamageType.Magical,
                might: 11, weight: 7, hit: 65, crit: 0, minRange: 1, maxRange: 10, uses: 20, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateLongThunderTomeRange8()
        {
            return new Weapon("Long Thunder Tome (Range 8)", WeaponType.THUNDER, DamageType.Magical,
                might: 9, weight: 6, hit: 70, crit: 0, minRange: 1, maxRange: 8, uses: 40, requiredRank: WeaponRank.B);
        }

        public static IWeapon CreateLongThunderTomeRange9()
        {
            return new Weapon("Long Thunder Tome (Range 9)", WeaponType.THUNDER, DamageType.Magical,
                might: 11, weight: 7, hit: 65, crit: 0, minRange: 1, maxRange: 9, uses: 30, requiredRank: WeaponRank.A);
        }

        public static IWeapon CreateLongThunderTomeRange10()
        {
            return new Weapon("Long Thunder Tome (Range 10)", WeaponType.THUNDER, DamageType.Magical,
                might: 14, weight: 8, hit: 60, crit: 0, minRange: 1, maxRange: 10, uses: 20, requiredRank: WeaponRank.A);
        }

        // ── Durability variants ───────────────────────────────────────────────
        // These factories create weapons with finite uses for richer gameplay.

        public static IWeapon CreateIronSwordWithDurability()
        {
            // Iron tier with explicit durability: 40 uses
            return new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateIronLanceWithDurability()
        {
            // Iron tier with explicit durability: 40 uses
            return new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateIronAxeWithDurability()
        {
            // Iron tier with explicit durability: 40 uses
            return new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1, uses: 40);
        }

        public static IWeapon CreateIronBowWithDurability()
        {
            // Iron tier bow with explicit durability: 40 uses
            return new Weapon("Iron Bow", WeaponType.BOW, DamageType.Physical, 6, 5, 85, 0, 2, 2, uses: 40);
        }

        public static IWeapon CreateFireTomeWithDurability()
        {
            // Basic tome with explicit durability: 40 uses
            return new Weapon("Fire", WeaponType.FIRE, DamageType.Magical, 5, 4, 85, 0, 1, 2, uses: 40);
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
