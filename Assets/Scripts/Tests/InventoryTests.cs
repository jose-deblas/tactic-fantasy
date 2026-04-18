using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        [Test]
        public void Inventory_StartsEmpty()
        {
            var inv = new Inventory();
            Assert.AreEqual(0, inv.Count);
            Assert.IsFalse(inv.IsFull);
        }

        [Test]
        public void Inventory_Add_SingleItem_Succeeds()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            Assert.IsTrue(inv.Add(sword));
            Assert.AreEqual(1, inv.Count);
        }

        [Test]
        public void Inventory_Add_SevenItems_AllPresent()
        {
            var inv = new Inventory();
            for (int i = 0; i < 7; i++)
                Assert.IsTrue(inv.Add(WeaponFactory.CreateIronSword()));
            Assert.AreEqual(7, inv.Count);
            Assert.IsTrue(inv.IsFull);
        }

        [Test]
        public void Inventory_Add_EighthItem_ReturnsFalse()
        {
            var inv = new Inventory();
            for (int i = 0; i < 7; i++)
                inv.Add(WeaponFactory.CreateIronSword());
            Assert.IsFalse(inv.Add(WeaponFactory.CreateIronLance()));
            Assert.AreEqual(7, inv.Count);
        }

        [Test]
        public void Inventory_Remove_ExistingItem_Succeeds()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            inv.Add(sword);
            Assert.IsTrue(inv.Remove(sword));
            Assert.AreEqual(0, inv.Count);
        }

        [Test]
        public void Inventory_Remove_NonexistentItem_ReturnsFalse()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            Assert.IsFalse(inv.Remove(sword));
        }

        [Test]
        public void Inventory_Swap_TwoPositions()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            var lance = WeaponFactory.CreateIronLance();
            inv.Add(sword);
            inv.Add(lance);

            inv.Swap(0, 1);

            Assert.AreEqual(lance, inv.Items[0]);
            Assert.AreEqual(sword, inv.Items[1]);
        }

        [Test]
        public void Inventory_Swap_InvalidIndex_Throws()
        {
            var inv = new Inventory();
            inv.Add(WeaponFactory.CreateIronSword());
            Assert.Throws<System.ArgumentOutOfRangeException>(() => inv.Swap(0, 5));
        }

        [Test]
        public void Inventory_GetWeapons_ReturnsOnlyWeapons()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            var vulnerary = ConsumableFactory.CreateVulnerary();
            inv.Add(sword);
            inv.Add(vulnerary);

            var weapons = inv.GetWeapons();
            Assert.AreEqual(1, weapons.Count);
            Assert.AreEqual(sword, weapons[0]);
        }

        [Test]
        public void Inventory_GetFirstUsableWeapon_SkipsBroken()
        {
            var inv = new Inventory();
            var brokenSword = new Weapon("Broken Sword", WeaponType.SWORD, DamageType.Physical,
                5, 5, 90, 0, 1, 1, uses: 1);
            brokenSword.ConsumeUse(); // now broken
            var lance = WeaponFactory.CreateIronLance();
            inv.Add(brokenSword);
            inv.Add(lance);

            Assert.AreEqual(lance, inv.GetFirstUsableWeapon());
        }

        [Test]
        public void Inventory_GetFirstUsableWeapon_ReturnsNull_WhenAllBroken()
        {
            var inv = new Inventory();
            var brokenSword = new Weapon("Broken Sword", WeaponType.SWORD, DamageType.Physical,
                5, 5, 90, 0, 1, 1, uses: 1);
            brokenSword.ConsumeUse();
            inv.Add(brokenSword);

            Assert.IsNull(inv.GetFirstUsableWeapon());
        }

        [Test]
        public void Inventory_ConstructorWithWeapon_HasOneItem()
        {
            var sword = WeaponFactory.CreateIronSword();
            var inv = new Inventory(sword);
            Assert.AreEqual(1, inv.Count);
            Assert.AreEqual(sword, inv.Items[0]);
        }

        [Test]
        public void Inventory_GetAll_ReturnsAllItems()
        {
            var inv = new Inventory();
            var sword = WeaponFactory.CreateIronSword();
            var lance = WeaponFactory.CreateIronLance();
            inv.Add(sword);
            inv.Add(lance);

            var all = inv.GetAll();
            Assert.AreEqual(2, all.Count);
        }

        [Test]
        public void Inventory_ConstructorWithItems_AddsAll()
        {
            var items = new IItem[]
            {
                WeaponFactory.CreateIronSword(),
                WeaponFactory.CreateIronLance(),
                WeaponFactory.CreateIronAxe()
            };
            var inv = new Inventory(items);
            Assert.AreEqual(3, inv.Count);
        }
    }
}
