using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class TurnManagerActionsTests
    {
        private ITurnManager _turnManager;
        private List<IUnit> _testUnits;

        [SetUp]
        public void Setup()
        {
            _turnManager = new TurnManager();
            _testUnits = new List<IUnit>
            {
                new Unit(1, "Player1", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(),
                    new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5), (0, 0), WeaponFactory.CreateIronSword()),
                new Unit(2, "Player2", Team.PlayerTeam, ClassDataFactory.CreateSoldier(),
                    new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5), (1, 0), WeaponFactory.CreateIronLance())
            };
            _turnManager.Initialize(_testUnits);
        }

        [Test]
        public void DefaultActionsPerUnit_IsTwo()
        {
            Assert.AreEqual(2, _turnManager.DefaultActionsPerUnit);
        }

        [Test]
        public void ConsumeAction_DecrementsAndMarksUnitActedWhenZero()
        {
            int id = _testUnits[0].Id;
            Assert.AreEqual(2, _turnManager.GetRemainingActions(id));
            Assert.IsTrue(_turnManager.ConsumeAction(id));
            Assert.AreEqual(1, _turnManager.GetRemainingActions(id));
            Assert.IsFalse(_turnManager.HasUnitActed(id));
            Assert.IsTrue(_turnManager.ConsumeAction(id));
            Assert.AreEqual(0, _turnManager.GetRemainingActions(id));
            Assert.IsTrue(_turnManager.HasUnitActed(id));
            Assert.IsFalse(_turnManager.ConsumeAction(id));
        }

        [Test]
        public void GrantActions_IncreasesRemainingAndUnmarksActed()
        {
            int id = _testUnits[0].Id;
            // reduce to 0
            _turnManager.MarkUnitAsActed(id);
            Assert.IsTrue(_turnManager.HasUnitActed(id));
            Assert.AreEqual(0, _turnManager.GetRemainingActions(id));
            _turnManager.GrantActions(id, 1);
            Assert.AreEqual(1, _turnManager.GetRemainingActions(id));
            Assert.IsFalse(_turnManager.HasUnitActed(id));
        }

        [Test]
        public void AdvancePhase_ResetsActionsForPlayerUnitsOnNewTurn()
        {
            int id = _testUnits[0].Id;
            _turnManager.ConsumeAction(id);
            _turnManager.ConsumeAction(id);
            Assert.IsTrue(_turnManager.HasUnitActed(id));
            _turnManager.AdvancePhase(); // Player->Ally
            _turnManager.AdvancePhase(); // Ally->Enemy
            _turnManager.AdvancePhase(); // Enemy->Player
            Assert.AreEqual(_turnManager.DefaultActionsPerUnit, _turnManager.GetRemainingActions(id));
            Assert.IsFalse(_turnManager.HasUnitActed(id));
        }
    }
}
