using System;
using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class BasePhaseTests
    {
        private List<IUnit> _roster;
        private Unit _unit1;
        private Unit _unit2;
        private Unit _unit3;
        private ArmyGold _gold;
        private ShopService _shop;

        [SetUp]
        public void Setup()
        {
            _unit1 = new Unit(1, "Myrmidon", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());
            _unit2 = new Unit(2, "Soldier", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(), ClassDataFactory.CreateSoldier().BaseStats,
                (1, 0), WeaponFactory.CreateIronLance());
            _unit3 = new Unit(3, "Fighter", Team.PlayerTeam,
                ClassDataFactory.CreateFighter(), ClassDataFactory.CreateFighter().BaseStats,
                (2, 0), WeaponFactory.CreateIronAxe());

            _roster = new List<IUnit> { _unit1, _unit2, _unit3 };

            _gold = new ArmyGold(1000);
            _shop = new ShopService();
            _shop.RegisterItem("Vulnerary", 300, ConsumableFactory.CreateVulnerary);
        }

        [Test]
        public void Constructor_InitializesBexpPool()
        {
            var basePhase = new BasePhase(_roster, 500, _gold, _shop, 4);
            Assert.AreEqual(500, basePhase.BexpPool);
        }

        [Test]
        public void AllocateBexp_LevelsUpUnit()
        {
            var basePhase = new BasePhase(_roster, 200, _gold, _shop, 4);
            int startLevel = _unit1.Level;

            int levels = basePhase.AllocateBexp(_unit1, 100);

            Assert.AreEqual(2, levels);
            Assert.AreEqual(startLevel + 2, _unit1.Level);
        }

        [Test]
        public void AllocateBexp_DeductsFromPool()
        {
            var basePhase = new BasePhase(_roster, 200, _gold, _shop, 4);

            basePhase.AllocateBexp(_unit1, 50);

            Assert.AreEqual(150, basePhase.BexpPool);
        }

        [Test]
        public void AllocateBexp_CannotExceedPool()
        {
            var basePhase = new BasePhase(_roster, 30, _gold, _shop, 4);

            Assert.Throws<InvalidOperationException>(() =>
                basePhase.AllocateBexp(_unit1, 50));
        }

        [Test]
        public void AllocateBexp_MultipleAllocations_TrackPoolCorrectly()
        {
            var basePhase = new BasePhase(_roster, 200, _gold, _shop, 4);

            basePhase.AllocateBexp(_unit1, 50);
            basePhase.AllocateBexp(_unit2, 50);

            Assert.AreEqual(100, basePhase.BexpPool);
        }

        // ── Deploy / Bench ─────────────────────────────────────────────────

        [Test]
        public void DeployUnit_AddsToDeployedList()
        {
            var basePhase = new BasePhase(_roster, 0, _gold, _shop, 4);

            basePhase.DeployUnit(_unit1);
            basePhase.DeployUnit(_unit2);

            Assert.AreEqual(2, basePhase.DeployedUnits.Count);
            Assert.AreEqual(1, basePhase.BenchedUnits.Count);
        }

        [Test]
        public void BenchUnit_RemovesFromDeployed()
        {
            var basePhase = new BasePhase(_roster, 0, _gold, _shop, 4);
            basePhase.DeployUnit(_unit1);

            basePhase.BenchUnit(_unit1);

            Assert.AreEqual(0, basePhase.DeployedUnits.Count);
            Assert.AreEqual(3, basePhase.BenchedUnits.Count);
        }

        [Test]
        public void DeployUnit_ExceedsMaxDeploy_Throws()
        {
            var basePhase = new BasePhase(_roster, 0, _gold, _shop, 2);
            basePhase.DeployUnit(_unit1);
            basePhase.DeployUnit(_unit2);

            Assert.Throws<InvalidOperationException>(() =>
                basePhase.DeployUnit(_unit3));
        }

        [Test]
        public void DeployUnit_NotInRoster_Throws()
        {
            var basePhase = new BasePhase(_roster, 0, _gold, _shop, 4);
            var outsider = new Unit(99, "Outsider", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            Assert.Throws<InvalidOperationException>(() =>
                basePhase.DeployUnit(outsider));
        }

        [Test]
        public void AvailableUnits_ReturnsFullRoster()
        {
            var basePhase = new BasePhase(_roster, 0, _gold, _shop, 4);
            Assert.AreEqual(3, basePhase.AvailableUnits.Count);
        }

        // ── ChapterData BEXP reward ────────────────────────────────────────

        [Test]
        public void ChapterData_CalculateBexpReward_BaseOnly()
        {
            var chapter = new ChapterData("Ch1", 0, 200, 10);

            // Finished exactly on par, all allies alive (4/4)
            int reward = chapter.CalculateBexpReward(10, 4, 4);

            // 200 base + 0 turn bonus + 50 survival = 250
            Assert.AreEqual(250, reward);
        }

        [Test]
        public void ChapterData_CalculateBexpReward_TurnBonus()
        {
            var chapter = new ChapterData("Ch1", 0, 200, 10);

            // Finished 3 turns early
            int reward = chapter.CalculateBexpReward(7, 4, 4);

            // 200 + 15 (3*5) + 50 = 265
            Assert.AreEqual(265, reward);
        }

        [Test]
        public void ChapterData_CalculateBexpReward_SurvivalPenalty()
        {
            var chapter = new ChapterData("Ch1", 0, 200, 10);

            // 2 of 4 allies survived
            int reward = chapter.CalculateBexpReward(10, 2, 4);

            // 200 + 0 + 25 (2/4 * 50) = 225
            Assert.AreEqual(225, reward);
        }

        [Test]
        public void ChapterData_CalculateBexpReward_OverParTurns_NoTurnBonus()
        {
            var chapter = new ChapterData("Ch1", 0, 200, 10);

            // Finished 2 turns late — no turn bonus
            int reward = chapter.CalculateBexpReward(12, 4, 4);

            // 200 + 0 + 50 = 250
            Assert.AreEqual(250, reward);
        }
    }
}
