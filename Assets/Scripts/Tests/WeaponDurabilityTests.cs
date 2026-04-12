using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for weapon durability (uses/charges) system.
    /// Weapons have limited uses; combat consumes them; broken weapons cannot be used.
    /// </summary>
    [TestFixture]
    public class WeaponDurabilityTests
    {
        // ── Weapon value object ─────────────────────────────────────────────

        [Test]
        public void Weapon_WithUses_HasCorrectInitialDurability()
        {
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 90, crit: 0, minRange: 1, maxRange: 1, uses: 30);

            Assert.AreEqual(30, weapon.CurrentUses);
            Assert.AreEqual(30, weapon.MaxUses);
            Assert.IsFalse(weapon.IsBroken);
        }

        [Test]
        public void Weapon_ConsumeUse_DecrementsCurrentUses()
        {
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 90, crit: 0, minRange: 1, maxRange: 1, uses: 5);

            weapon.ConsumeUse();

            Assert.AreEqual(4, weapon.CurrentUses);
            Assert.IsFalse(weapon.IsBroken);
        }

        [Test]
        public void Weapon_ConsumeAllUses_BecomesIsBroken()
        {
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 90, crit: 0, minRange: 1, maxRange: 1, uses: 1);

            weapon.ConsumeUse();

            Assert.AreEqual(0, weapon.CurrentUses);
            Assert.IsTrue(weapon.IsBroken);
        }

        [Test]
        public void Weapon_ConsumeUseWhenBroken_DoesNotGoBelowZero()
        {
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 90, crit: 0, minRange: 1, maxRange: 1, uses: 0);

            weapon.ConsumeUse();

            Assert.AreEqual(0, weapon.CurrentUses);
            Assert.IsTrue(weapon.IsBroken);
        }

        [Test]
        public void Weapon_WithUnlimitedUses_NeverBreaks()
        {
            // uses = -1 (or 0 in legacy factory) means unlimited
            var weapon = WeaponFactory.CreateIronSword(); // legacy: no uses param → unlimited

            weapon.ConsumeUse();
            weapon.ConsumeUse();

            Assert.IsFalse(weapon.IsBroken);
            Assert.AreEqual(-1, weapon.MaxUses); // sentinel value for unlimited
        }

        // ── IUnit.CanUseWeapon ──────────────────────────────────────────────

        [Test]
        public void Unit_WithBrokenWeapon_CannotAct()
        {
            var brokenSword = new Weapon("Broken Sword", WeaponType.SWORD, DamageType.Physical,
                might: 0, weight: 0, hit: 0, crit: 0, minRange: 1, maxRange: 1, uses: 0);

            var unit = new Unit(
                1, "Warrior", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 7, 0, 10, 11, 5, 5, 0, 5),
                (0, 0),
                brokenSword
            );

            Assert.IsTrue(unit.HasBrokenWeapon);
        }

        [Test]
        public void Unit_WithHealthyWeapon_HasBrokenWeaponFalse()
        {
            var unit = new Unit(
                1, "Warrior", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 7, 0, 10, 11, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            Assert.IsFalse(unit.HasBrokenWeapon);
        }

        // ── CombatResolver consumes uses ────────────────────────────────────

        [Test]
        public void CombatResolver_AfterCombat_AttackerWeaponUsesConsumed()
        {
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 100, crit: 0, minRange: 1, maxRange: 1, uses: 10);

            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 7, 0, 10, 11, 5, 5, 0, 5),
                (0, 0),
                sword
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 5, 0, 8, 7, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            var map = new GameMap(16, 16, 0);
            var resolver = new CombatResolver();

            // Force hit with seeded resolver
            resolver.ResolveCombat(attacker, defender, map);

            // Attacker weapon should have consumed at least 1 use
            Assert.Less(sword.CurrentUses, 10);
        }

        [Test]
        public void CombatResolver_WhenAttackerMisses_StillConsumesUse()
        {
            // Weapon with 0% hit → always miss, but use is still consumed (the attempt was made)
            var missingSword = new Weapon("Miss Sword", WeaponType.SWORD, DamageType.Physical,
                might: 5, weight: 5, hit: 0, crit: 0, minRange: 1, maxRange: 1, uses: 10);

            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 0, 0, 0, 0, 0, 0, 0, 5),
                (0, 0),
                missingSword
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 5, 0, 8, 7, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            var map = new GameMap(16, 16, 0);
            var resolver = new CombatResolver();

            resolver.ResolveCombat(attacker, defender, map);

            // Attacker spent an action → use consumed
            Assert.AreEqual(9, missingSword.CurrentUses);
        }

        // ── WeaponFactory uses ──────────────────────────────────────────────

        [Test]
        public void WeaponFactory_AllWeapons_HaveExpectedUses()
        {
            Assert.AreEqual(30, WeaponFactory.CreateIronSwordWithDurability().MaxUses);
            Assert.AreEqual(30, WeaponFactory.CreateIronLanceWithDurability().MaxUses);
            Assert.AreEqual(30, WeaponFactory.CreateIronAxeWithDurability().MaxUses);
            Assert.AreEqual(30, WeaponFactory.CreateIronBowWithDurability().MaxUses);
            Assert.AreEqual(30, WeaponFactory.CreateFireTomeWithDurability().MaxUses);
            Assert.AreEqual(15, WeaponFactory.CreateHealStaffWithDurability().MaxUses);
        }
    }
}
