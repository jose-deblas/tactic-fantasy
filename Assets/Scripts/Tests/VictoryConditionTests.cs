using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for the VictoryCondition domain service.
    /// Covers: Rout, Seize, and Survive conditions.
    /// </summary>
    [TestFixture]
    public class VictoryConditionTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────

        private static IUnit MakeUnit(int id, Team team, bool alive = true, (int x, int y) pos = default)
        {
            var weapon = new Weapon("Sword", WeaponType.SWORD, DamageType.Physical, 5, 75, 0, 1, 1, 1, null);
            var stats  = new CharacterStats(20, 5, 0, 5, 5, 3, 4, 2, 5);
            var classData = ClassDataFactory.CreateFighter();
            var unit = new Unit(id, "Test", team, classData, stats, pos, weapon);
            if (!alive) unit.TakeDamage(9999);
            return unit;
        }

        // ── Rout Condition ───────────────────────────────────────────────────

        [Test]
        public void Rout_NotMet_WhenEnemiesAlive()
        {
            var cond    = VictoryConditionFactory.Rout();
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.InProgress, result);
        }

        [Test]
        public void Rout_Met_WhenAllEnemiesDead()
        {
            var cond    = VictoryConditionFactory.Rout();
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam, alive: false) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.PlayerWon, result);
        }

        [Test]
        public void Rout_PlayerLoses_WhenAllPlayersDead()
        {
            var cond    = VictoryConditionFactory.Rout();
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam, alive: false) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.PlayerLost, result);
        }

        // ── Seize Condition ──────────────────────────────────────────────────

        [Test]
        public void Seize_NotMet_WhenNoPlayerOnSeizeTile()
        {
            var seizeTile = (x: 5, y: 5);
            var cond      = VictoryConditionFactory.Seize(seizeTile.x, seizeTile.y);
            var players   = new List<IUnit> { MakeUnit(1, Team.PlayerTeam, pos: (3, 3)) };
            var enemies   = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.InProgress, result);
        }

        [Test]
        public void Seize_Met_WhenPlayerOnSeizeTile()
        {
            var seizeTile = (x: 5, y: 5);
            var cond      = VictoryConditionFactory.Seize(seizeTile.x, seizeTile.y);
            var players   = new List<IUnit> { MakeUnit(1, Team.PlayerTeam, pos: (5, 5)) };
            var enemies   = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.PlayerWon, result);
        }

        [Test]
        public void Seize_NotMet_WhenDeadPlayerOnSeizeTile()
        {
            // A dead unit on the seize tile doesn't count
            var seizeTile = (x: 5, y: 5);
            var cond      = VictoryConditionFactory.Seize(seizeTile.x, seizeTile.y);
            var players   = new List<IUnit> { MakeUnit(1, Team.PlayerTeam, alive: false, pos: (5, 5)) };
            var enemies   = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 1, map: null);

            Assert.AreEqual(VictoryState.PlayerLost, result);
        }

        // ── Survive Condition ────────────────────────────────────────────────

        [Test]
        public void Survive_NotMet_WhenTurnCountBelowTarget()
        {
            var cond    = VictoryConditionFactory.Survive(turnsToSurvive: 5);
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 3, map: null);

            Assert.AreEqual(VictoryState.InProgress, result);
        }

        [Test]
        public void Survive_Met_WhenTurnCountReachesTarget()
        {
            var cond    = VictoryConditionFactory.Survive(turnsToSurvive: 5);
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 5, map: null);

            Assert.AreEqual(VictoryState.PlayerWon, result);
        }

        [Test]
        public void Survive_PlayerLoses_WhenAllPlayersDieBeforeTarget()
        {
            var cond    = VictoryConditionFactory.Survive(turnsToSurvive: 10);
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam, alive: false) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam) };

            var result = cond.Evaluate(players, enemies, turnCount: 3, map: null);

            Assert.AreEqual(VictoryState.PlayerLost, result);
        }

        [Test]
        public void Survive_PlayerWins_EvenIfEnemiesAllDead()
        {
            // In survive mode, wiping enemies also wins
            var cond    = VictoryConditionFactory.Survive(turnsToSurvive: 10);
            var players = new List<IUnit> { MakeUnit(1, Team.PlayerTeam) };
            var enemies = new List<IUnit> { MakeUnit(2, Team.EnemyTeam, alive: false) };

            var result = cond.Evaluate(players, enemies, turnCount: 2, map: null);

            Assert.AreEqual(VictoryState.PlayerWon, result);
        }

        // ── Description ──────────────────────────────────────────────────────

        [Test]
        public void Rout_HasCorrectDescription()
        {
            var cond = VictoryConditionFactory.Rout();
            Assert.That(cond.Description, Does.Contain("Rout").Or.Contain("rout").Or.Contain("enemy").Or.Contain("Enemy"));
        }

        [Test]
        public void Seize_DescriptionContainsCoordinates()
        {
            var cond = VictoryConditionFactory.Seize(7, 3);
            Assert.That(cond.Description, Does.Contain("7").And.Contain("3"));
        }

        [Test]
        public void Survive_DescriptionContainsTurnCount()
        {
            var cond = VictoryConditionFactory.Survive(8);
            Assert.That(cond.Description, Does.Contain("8"));
        }
    }
}
