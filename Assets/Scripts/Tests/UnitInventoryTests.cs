using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class UnitInventoryTests
    {
        [Test]
        public void Unit_ConstructedWithWeapon_HasInventoryWithOneWeapon()
        {
            var sword = WeaponFactory.CreateIronSword();
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), sword);

            Assert.AreEqual(1, unit.Inventory.Count);
            Assert.AreEqual(sword, unit.Inventory.Items[0]);
        }

        [Test]
        public void Unit_EquippedWeapon_ReturnsFirstWeapon()
        {
            var sword = WeaponFactory.CreateIronSword();
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), sword);

            Assert.AreEqual(sword, unit.EquippedWeapon);
        }

        [Test]
        public void Unit_EquipWeapon_AddsToInventoryAndBecomesEquipped()
        {
            var sword = WeaponFactory.CreateIronSword();
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), sword);

            var steelSword = WeaponFactory.CreateSteelSword();
            unit.EquipWeapon(steelSword);

            Assert.AreEqual(2, unit.Inventory.Count);
            Assert.AreEqual(steelSword, unit.EquippedWeapon);
        }

        [Test]
        public void Unit_EquippedWeapon_SkipsBrokenWeapons()
        {
            var brokenSword = new Weapon("Broken Sword", WeaponType.SWORD, DamageType.Physical,
                5, 5, 90, 0, 1, 1, uses: 1);
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), brokenSword);
            brokenSword.ConsumeUse();

            var lance = WeaponFactory.CreateIronLance();
            unit.Inventory.Add(lance);

            Assert.AreEqual(lance, unit.EquippedWeapon);
        }

        [Test]
        public void Unit_EquippedWeapon_ReturnsFirstWeaponWhenAllBroken()
        {
            var brokenSword = new Weapon("Broken Sword", WeaponType.SWORD, DamageType.Physical,
                5, 5, 90, 0, 1, 1, uses: 1);
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), brokenSword);
            brokenSword.ConsumeUse();

            // Should still return the broken weapon as fallback
            Assert.AreEqual(brokenSword, unit.EquippedWeapon);
        }

        [Test]
        public void Unit_EquipWeapon_AlreadyInInventory_MovesToFront()
        {
            var sword = WeaponFactory.CreateIronSword();
            var lance = WeaponFactory.CreateIronLance();
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), sword);
            unit.Inventory.Add(lance);

            unit.EquipWeapon(lance);

            Assert.AreEqual(lance, unit.Inventory.Items[0]);
            Assert.AreEqual(sword, unit.Inventory.Items[1]);
        }

        [Test]
        public void Unit_ConstructedWithInventory_PreservesItems()
        {
            var sword = WeaponFactory.CreateIronSword();
            var vulnerary = ConsumableFactory.CreateVulnerary();
            var inventory = new Inventory(new IItem[] { sword, vulnerary });

            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), inventory);

            Assert.AreEqual(2, unit.Inventory.Count);
            Assert.AreEqual(sword, unit.EquippedWeapon);
        }

        [Test]
        public void Unit_CanEquip_ValidatesClassWeaponType()
        {
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0),
                WeaponFactory.CreateIronSword());

            Assert.IsTrue(unit.CanEquip(WeaponFactory.CreateIronSword()));
            Assert.IsFalse(unit.CanEquip(WeaponFactory.CreateIronLance()));
        }

        [Test]
        public void Unit_UseConsumable_ReducesUses()
        {
            var sword = WeaponFactory.CreateIronSword();
            var unit = new Unit(1, "Ike", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 2, 5), (0, 0), sword);
            unit.TakeDamage(10);

            var vulnerary = ConsumableFactory.CreateVulnerary();
            unit.Inventory.Add(vulnerary);
            vulnerary.Use(unit);

            Assert.AreEqual(2, vulnerary.CurrentUses);
            Assert.AreEqual(20, unit.CurrentHP);
        }
    }
}
