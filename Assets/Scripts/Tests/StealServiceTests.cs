using System;
using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class StealServiceTests
    {
        private StealService _service;

        [SetUp]
        public void Setup()
        {
            _service = new StealService();
        }

        private Unit MakeThief((int x, int y) pos, int spd = 13)
        {
            var stats = new CharacterStats(17, 5, 0, 9, spd, 6, 4, 1, 7);
            return new Unit(1, "Thief", Team.PlayerTeam,
                ClassDataFactory.CreateThief(), stats, pos, WeaponFactory.CreateIronSword());
        }

        private Unit MakeEnemy((int x, int y) pos, int spd = 8)
        {
            var stats = new CharacterStats(20, 7, 0, 8, spd, 3, 7, 2, 5);
            return new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(), stats, pos, WeaponFactory.CreateIronLance());
        }

        [Test]
        public void CanSteal_ThiefFasterThanTarget_WithConsumable_ReturnsTrue()
        {
            var thief = MakeThief((3, 3), spd: 13);
            var enemy = MakeEnemy((4, 3), spd: 8);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsTrue(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_SpdEqual_ReturnsFalse()
        {
            var thief = MakeThief((3, 3), spd: 10);
            var enemy = MakeEnemy((4, 3), spd: 10);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_SpdLower_ReturnsFalse()
        {
            var thief = MakeThief((3, 3), spd: 7);
            var enemy = MakeEnemy((4, 3), spd: 10);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_NotThiefClass_ReturnsFalse()
        {
            // Make a non-thief unit with high SPD
            var unit = new Unit(1, "Myrm", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 15, 5, 5, 0, 5),
                (3, 3), WeaponFactory.CreateIronSword());
            var enemy = MakeEnemy((4, 3), spd: 8);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(unit, enemy));
        }

        [Test]
        public void CanSteal_OnlyWeaponsInInventory_ReturnsFalse()
        {
            var thief = MakeThief((3, 3), spd: 13);
            var enemy = MakeEnemy((4, 3), spd: 8);
            // Enemy has only its weapon (iron lance), no consumables

            Assert.IsFalse(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_ThiefInventoryFull_ReturnsFalse()
        {
            var thief = MakeThief((3, 3), spd: 13);
            // Fill thief inventory (has 1 sword, add 6 more items)
            for (int i = 0; i < 6; i++)
                thief.Inventory.Add(ConsumableFactory.CreateVulnerary());

            var enemy = MakeEnemy((4, 3), spd: 8);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_NotAdjacent_ReturnsFalse()
        {
            var thief = MakeThief((0, 0), spd: 13);
            var enemy = MakeEnemy((5, 5), spd: 8);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(thief, enemy));
        }

        [Test]
        public void CanSteal_SameTeam_ReturnsFalse()
        {
            var thief = MakeThief((3, 3), spd: 13);
            var ally = new Unit(2, "Ally", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (4, 3), WeaponFactory.CreateIronLance());
            ally.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsFalse(_service.CanSteal(thief, ally));
        }

        [Test]
        public void Steal_RemovesFromTarget_AddsToThief()
        {
            var thief = MakeThief((3, 3), spd: 13);
            var enemy = MakeEnemy((4, 3), spd: 8);
            var vulnerary = ConsumableFactory.CreateVulnerary();
            enemy.Inventory.Add(vulnerary);

            var stolen = _service.Steal(thief, enemy);

            Assert.AreEqual(vulnerary, stolen);
            Assert.IsTrue(thief.Inventory.Items.Contains(vulnerary));
            Assert.IsFalse(enemy.Inventory.Items.Contains(vulnerary));
        }

        [Test]
        public void Steal_TakesFirstNonWeaponItem()
        {
            var thief = MakeThief((3, 3), spd: 13);
            var enemy = MakeEnemy((4, 3), spd: 8);
            var vuln1 = ConsumableFactory.CreateVulnerary();
            var vuln2 = ConsumableFactory.CreateVulnerary();
            enemy.Inventory.Add(vuln1);
            enemy.Inventory.Add(vuln2);

            var stolen = _service.Steal(thief, enemy);

            // Should steal the first non-weapon item (vuln1)
            Assert.AreEqual(vuln1, stolen);
        }

        [Test]
        public void CanSteal_RogueClass_ReturnsTrue()
        {
            var rogue = new Unit(1, "Rogue", Team.PlayerTeam,
                ClassDataFactory.CreateRogue(),
                new CharacterStats(20, 8, 0, 14, 17, 8, 6, 3, 7),
                (3, 3), WeaponFactory.CreateIronSword());
            var enemy = MakeEnemy((4, 3), spd: 8);
            enemy.Inventory.Add(ConsumableFactory.CreateVulnerary());

            Assert.IsTrue(_service.CanSteal(rogue, enemy));
        }
    }
}
