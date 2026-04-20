using System;
using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class TradeServiceTests
    {
        private TradeService _service;

        [SetUp]
        public void Setup()
        {
            _service = new TradeService();
        }

        private Unit MakeUnit(int id, Team team, (int x, int y) pos)
        {
            return new Unit(id, $"Unit{id}", team,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                pos, WeaponFactory.CreateIronLance());
        }

        [Test]
        public void CanTrade_AdjacentAllies_ReturnsTrue()
        {
            var a = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var b = MakeUnit(2, Team.PlayerTeam, (3, 4));

            Assert.IsTrue(_service.CanTrade(a, b));
        }

        [Test]
        public void CanTrade_NotAdjacent_ReturnsFalse()
        {
            var a = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var b = MakeUnit(2, Team.PlayerTeam, (3, 3));

            Assert.IsFalse(_service.CanTrade(a, b));
        }

        [Test]
        public void CanTrade_DifferentTeams_ReturnsFalse()
        {
            var a = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var b = MakeUnit(2, Team.EnemyTeam, (3, 4));

            Assert.IsFalse(_service.CanTrade(a, b));
        }

        [Test]
        public void CanTrade_DiagonalAdjacent_ReturnsFalse()
        {
            var a = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var b = MakeUnit(2, Team.PlayerTeam, (4, 4));

            Assert.IsFalse(_service.CanTrade(a, b));
        }

        [Test]
        public void TradeItem_ValidAdjacentAllies_MovesItem()
        {
            var giver = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var receiver = MakeUnit(2, Team.PlayerTeam, (3, 4));

            var vulnerary = ConsumableFactory.CreateVulnerary();
            giver.Inventory.Add(vulnerary);

            _service.TradeItem(giver, receiver, vulnerary);

            Assert.IsFalse(giver.Inventory.Items.Contains(vulnerary));
            Assert.IsTrue(receiver.Inventory.Items.Contains(vulnerary));
        }

        [Test]
        public void TradeItem_ReceiverFull_Throws()
        {
            var giver = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var receiver = MakeUnit(2, Team.PlayerTeam, (3, 4));

            // Fill receiver inventory (already has 1 lance, add 6 more)
            for (int i = 0; i < 6; i++)
                receiver.Inventory.Add(ConsumableFactory.CreateVulnerary());

            var item = ConsumableFactory.CreateVulnerary();
            giver.Inventory.Add(item);

            Assert.Throws<InvalidOperationException>(() =>
                _service.TradeItem(giver, receiver, item));
        }

        [Test]
        public void TradeItem_NotAdjacent_Throws()
        {
            var giver = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var receiver = MakeUnit(2, Team.PlayerTeam, (5, 5));

            var item = ConsumableFactory.CreateVulnerary();
            giver.Inventory.Add(item);

            Assert.Throws<InvalidOperationException>(() =>
                _service.TradeItem(giver, receiver, item));
        }

        [Test]
        public void TradeItem_ItemNotInInventory_Throws()
        {
            var giver = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var receiver = MakeUnit(2, Team.PlayerTeam, (3, 4));

            var item = ConsumableFactory.CreateVulnerary();
            // item is NOT added to giver

            Assert.Throws<InvalidOperationException>(() =>
                _service.TradeItem(giver, receiver, item));
        }

        [Test]
        public void SwapItems_ValidSlots_SwapsCorrectly()
        {
            var unitA = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var unitB = MakeUnit(2, Team.PlayerTeam, (3, 4));

            var vulnA = ConsumableFactory.CreateVulnerary();
            var vulnB = ConsumableFactory.CreateVulnerary();
            unitA.Inventory.Add(vulnA);
            unitB.Inventory.Add(vulnB);

            // Slot 1 in both (slot 0 is their lance)
            _service.SwapItems(unitA, 1, unitB, 1);

            Assert.IsTrue(unitA.Inventory.Items.Contains(vulnB));
            Assert.IsTrue(unitB.Inventory.Items.Contains(vulnA));
        }

        [Test]
        public void SwapItems_InvalidSlot_Throws()
        {
            var unitA = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var unitB = MakeUnit(2, Team.PlayerTeam, (3, 4));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.SwapItems(unitA, 5, unitB, 0));
        }
    }
}
