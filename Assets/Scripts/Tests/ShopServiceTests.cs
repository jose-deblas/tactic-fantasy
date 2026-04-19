using System;
using NUnit.Framework;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ShopServiceTests
    {
        private ShopService _shop;
        private ArmyGold _gold;
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            _shop = new ShopService();
            _shop.RegisterItem("Vulnerary", 300, ConsumableFactory.CreateVulnerary);
            _shop.RegisterItem("Elixir", 3000, ConsumableFactory.CreateElixir);

            _gold = new ArmyGold(1000);

            _unit = new Unit(1, "TestUnit", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());
        }

        [Test]
        public void Buy_Succeeds_WhenAffordable()
        {
            _shop.Buy(_unit, "Vulnerary", _gold);

            Assert.AreEqual(700, _gold.Gold);
            Assert.AreEqual(2, _unit.Inventory.Count); // sword + vulnerary
        }

        [Test]
        public void Buy_AddsItemToInventory()
        {
            _shop.Buy(_unit, "Vulnerary", _gold);

            var items = _unit.Inventory.GetAll();
            Assert.AreEqual("Vulnerary", items[1].Name);
        }

        [Test]
        public void Buy_Fails_WhenCannotAfford()
        {
            var poorGold = new ArmyGold(100);

            Assert.Throws<InvalidOperationException>(() =>
                _shop.Buy(_unit, "Vulnerary", poorGold));
        }

        [Test]
        public void Buy_Fails_WhenNotInStock()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _shop.Buy(_unit, "MythicalItem", _gold));
        }

        [Test]
        public void Buy_Fails_WhenInventoryFull()
        {
            // Fill the inventory (already has 1 weapon, add 6 more)
            for (int i = 0; i < 6; i++)
                _unit.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsTrue(_unit.Inventory.IsFull);

            _gold = new ArmyGold(10000);
            Assert.Throws<InvalidOperationException>(() =>
                _shop.Buy(_unit, "Vulnerary", _gold));
        }

        [Test]
        public void Sell_ReturnsHalfPrice()
        {
            // Buy first, then sell
            _shop.Buy(_unit, "Vulnerary", _gold);
            Assert.AreEqual(700, _gold.Gold);

            var vulnerary = _unit.Inventory.GetAll()[1]; // second item
            _shop.Sell(_unit, vulnerary, _gold);

            // 700 + 150 (50% of 300) = 850
            Assert.AreEqual(850, _gold.Gold);
        }

        [Test]
        public void Sell_RemovesItemFromInventory()
        {
            _shop.Buy(_unit, "Vulnerary", _gold);
            var vulnerary = _unit.Inventory.GetAll()[1];

            _shop.Sell(_unit, vulnerary, _gold);

            Assert.AreEqual(1, _unit.Inventory.Count); // only sword remains
        }

        [Test]
        public void Sell_Fails_WhenUnitDoesNotHaveItem()
        {
            var looseItem = ConsumableFactory.CreateVulnerary();

            Assert.Throws<InvalidOperationException>(() =>
                _shop.Sell(_unit, looseItem, _gold));
        }

        [Test]
        public void GetSellPrice_IsHalfOfBuyPrice()
        {
            Assert.AreEqual(150, _shop.GetSellPrice("Vulnerary"));
            Assert.AreEqual(1500, _shop.GetSellPrice("Elixir"));
        }

        [Test]
        public void IsInStock_ReturnsTrueForRegisteredItem()
        {
            Assert.IsTrue(_shop.IsInStock("Vulnerary"));
        }

        [Test]
        public void IsInStock_ReturnsFalseForUnknownItem()
        {
            Assert.IsFalse(_shop.IsInStock("PhoenixDown"));
        }
    }
}
