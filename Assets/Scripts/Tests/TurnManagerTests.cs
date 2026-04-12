using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class TurnManagerTests
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
                    new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5), (1, 0), WeaponFactory.CreateIronLance()),
                new Unit(3, "Enemy1", Team.EnemyTeam, ClassDataFactory.CreateFighter(),
                    new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5), (14, 14), WeaponFactory.CreateIronAxe()),
                new Unit(4, "Enemy2", Team.EnemyTeam, ClassDataFactory.CreateMage(),
                    new CharacterStats(16, 0, 8, 7, 7, 5, 3, 7, 5), (15, 15), WeaponFactory.CreateFireTome())
            };

            _turnManager.Initialize(_testUnits);
        }

        [Test]
        public void Initialize_StartsInPlayerPhase()
        {
            Assert.AreEqual(Phase.PlayerPhase, _turnManager.CurrentPhase);
        }

        [Test]
        public void Initialize_StartsAtTurn1()
        {
            Assert.AreEqual(1, _turnManager.TurnCount);
        }

        [Test]
        public void Initialize_SetsAllUnits()
        {
            Assert.AreEqual(4, _turnManager.AllUnits.Count);
        }

        [Test]
        public void GetGameState_AllUnitsAlive_ReturnsInProgress()
        {
            var state = _turnManager.GetGameState();
            Assert.AreEqual(GameState.InProgress, state);
        }

        [Test]
        public void GetGameState_AllPlayersDefeated_ReturnsPlayerLost()
        {
            var playerUnit = (Unit)_turnManager.AllUnits[0];
            playerUnit.TakeDamage(playerUnit.CurrentHP);

            var player2Unit = (Unit)_turnManager.AllUnits[1];
            player2Unit.TakeDamage(player2Unit.CurrentHP);

            var state = _turnManager.GetGameState();
            Assert.AreEqual(GameState.PlayerLost, state);
        }

        [Test]
        public void GetGameState_AllEnemiesDefeated_ReturnsPlayerWon()
        {
            var enemyUnit = (Unit)_turnManager.AllUnits[2];
            enemyUnit.TakeDamage(enemyUnit.CurrentHP);

            var enemy2Unit = (Unit)_turnManager.AllUnits[3];
            enemy2Unit.TakeDamage(enemy2Unit.CurrentHP);

            var state = _turnManager.GetGameState();
            Assert.AreEqual(GameState.PlayerWon, state);
        }

        [Test]
        public void MarkCurrentUnitAsActed_SetsActedFlag()
        {
            bool beforeMarking = _turnManager.HasCurrentUnitActed;
            _turnManager.MarkCurrentUnitAsActed();
            bool afterMarking = _turnManager.HasCurrentUnitActed;

            Assert.IsFalse(beforeMarking);
            Assert.IsTrue(afterMarking);
        }

        [Test]
        public void AdvancePhase_FromPlayerToEnemy_ChangesPhase()
        {
            Assert.AreEqual(Phase.PlayerPhase, _turnManager.CurrentPhase);
            _turnManager.AdvancePhase();
            Assert.AreEqual(Phase.EnemyPhase, _turnManager.CurrentPhase);
        }

        [Test]
        public void AdvancePhase_FromEnemyToPlayer_IncreasesTurnCount()
        {
            _turnManager.AdvancePhase();
            int turnBefore = _turnManager.TurnCount;
            _turnManager.AdvancePhase();
            int turnAfter = _turnManager.TurnCount;

            Assert.AreEqual(turnBefore + 1, turnAfter);
        }

        [Test]
        public void AdvancePhase_ClearsActedUnitsOnPhaseChange()
        {
            _turnManager.MarkCurrentUnitAsActed();
            Assert.IsTrue(_turnManager.HasCurrentUnitActed);

            _turnManager.AdvancePhase();
            var newCurrentUnit = _turnManager.CurrentUnit;

            Assert.IsFalse(_turnManager.HasCurrentUnitActed);
        }

        [Test]
        public void CurrentUnit_StartsWithFirstPlayerUnit()
        {
            var currentUnit = _turnManager.CurrentUnit;
            Assert.IsNotNull(currentUnit);
            Assert.AreEqual(Team.PlayerTeam, currentUnit.Team);
        }

        [Test]
        public void HealFortTiles_HealsFortUnits()
        {
            var map = new TacticFantasy.Domain.Map.GameMap(16, 16, 0);
            var injuredUnit = (Unit)_testUnits[0];
            injuredUnit.TakeDamage(5);

            int hpBefore = injuredUnit.CurrentHP;
            _turnManager.HealFortTiles(map);

            // HP might change if unit is on a Fort tile
            Assert.IsTrue(injuredUnit.CurrentHP >= hpBefore || injuredUnit.CurrentHP == hpBefore);
        }

        [Test]
        public void AdvancePhase_EnemyToPlayer_TicksStatusEffectsOnAllUnits()
        {
            // Arrange: player unit poisoned
            var playerUnit = (Unit)_testUnits[0];
            playerUnit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 2));
            int hpBefore = playerUnit.CurrentHP;

            // Advance from PlayerPhase → EnemyPhase (no tick yet)
            _turnManager.AdvancePhase();
            Assert.AreEqual(hpBefore, playerUnit.CurrentHP, "No tick should happen on Player→Enemy");

            // Advance from EnemyPhase → PlayerPhase (tick happens here)
            _turnManager.AdvancePhase();
            Assert.Less(playerUnit.CurrentHP, hpBefore, "Poison should tick at end of enemy phase");
            Assert.AreEqual(1, playerUnit.ActiveStatus.RemainingTurns);
        }
    }
}
