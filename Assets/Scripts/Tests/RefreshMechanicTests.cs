using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class RefreshMechanicTests
    {
        private ITurnManager _turnManager;
        private IUnit _heron;
        private IUnit _myrmidon;
        private IUnit _enemy;

        [SetUp]
        public void Setup()
        {
            _turnManager = new TurnManager();

            _heron = new Unit(1, "Heron", Team.PlayerTeam,
                ClassDataFactory.CreateHeron(),
                ClassDataFactory.CreateHeron().BaseStats,
                (0, 0), WeaponFactory.CreateRefreshStaff());

            _myrmidon = new Unit(2, "Myrmidon", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (1, 0), WeaponFactory.CreateIronSword());

            _enemy = new Unit(3, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            _turnManager.Initialize(new List<IUnit> { _heron, _myrmidon, _enemy });
        }

        [Test]
        public void CanRefreshTarget_RefresherHasRefreshWeapon_ReturnsTrue()
        {
            // Myrmidon acts first, so it will be marked as acted
            var heroPos = _turnManager.AllUnits[1]; // Get myrmidon from list
            Assert.AreEqual(Team.PlayerTeam, heroPos.Team);

            // Mark myrmidon as acted
            _turnManager.MarkCurrentUnitAsActed();
            Assert.IsTrue(_turnManager.HasUnitActed(_myrmidon.Id));

            // Check if heron (with refresh weapon) can refresh myrmidon
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _myrmidon);
            Assert.IsTrue(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_TargetHasNotActed_ReturnsFalse()
        {
            // Myrmidon has NOT acted yet
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _myrmidon);
            Assert.IsFalse(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_TargetIsEnemy_ReturnsFalse()
        {
            // Mark enemy as "acted" (shouldn't be possible, but testing the logic)
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _enemy);
            Assert.IsFalse(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_RefreshingSelf_ReturnsFalse()
        {
            // Try to refresh the heron itself
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _heron);
            Assert.IsFalse(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_RefresherDoesNotHaveRefreshWeapon_ReturnsFalse()
        {
            // Myrmidon does NOT have a refresh weapon
            bool canRefresh = _turnManager.CanRefreshTarget(_myrmidon, _heron);
            Assert.IsFalse(canRefresh);
        }

        [Test]
        public void RefreshUnit_RemovesActedStatus()
        {
            // Mark myrmidon as acted
            _turnManager.MarkCurrentUnitAsActed();
            Assert.IsTrue(_turnManager.HasUnitActed(_myrmidon.Id));

            // Refresh the myrmidon
            _turnManager.RefreshUnit(_myrmidon.Id);

            // Check that acted status was removed
            Assert.IsFalse(_turnManager.HasUnitActed(_myrmidon.Id));
        }

        [Test]
        public void RefreshUnit_AllowsTargetToActAgain()
        {
            int myrmidonId = _myrmidon.Id;

            // Initial state: not acted
            Assert.IsFalse(_turnManager.HasUnitActed(myrmidonId));

            // Mark as acted
            _turnManager.MarkCurrentUnitAsActed();
            Assert.IsTrue(_turnManager.HasUnitActed(myrmidonId));

            // Refresh removes the acted flag
            _turnManager.RefreshUnit(myrmidonId);
            Assert.IsFalse(_turnManager.HasUnitActed(myrmidonId));

            // Unit can now act again (would be marked as acted once more if it acts)
            _turnManager.MarkCurrentUnitAsActed();
            Assert.IsTrue(_turnManager.HasUnitActed(myrmidonId));
        }

        [Test]
        public void CanRefreshTarget_TargetIsAlive_ReturnsTrue()
        {
            // Ensure myrmidon is alive
            Assert.IsTrue(_myrmidon.IsAlive);

            // Mark myrmidon as acted
            _turnManager.MarkCurrentUnitAsActed();

            // Should be able to refresh
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _myrmidon);
            Assert.IsTrue(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_TargetIsDead_ReturnsFalse()
        {
            // Kill the myrmidon
            ((Unit)_myrmidon).TakeDamage(_myrmidon.MaxHP);
            Assert.IsFalse(_myrmidon.IsAlive);

            // Cannot refresh dead unit
            bool canRefresh = _turnManager.CanRefreshTarget(_heron, _myrmidon);
            Assert.IsFalse(canRefresh);
        }

        [Test]
        public void CanRefreshTarget_MultipleRefreshesInOneTurn_AllowsChainRefresh()
        {
            // Create a second heron
            var heron2 = new Unit(4, "Heron2", Team.PlayerTeam,
                ClassDataFactory.CreateHeron(),
                ClassDataFactory.CreateHeron().BaseStats,
                (2, 0), WeaponFactory.CreateRefreshStaff());

            var turnManager = new TurnManager();
            turnManager.Initialize(new List<IUnit> { _heron, heron2, _myrmidon });

            // Mark all units as acted
            turnManager.MarkCurrentUnitAsActed(); // First unit (heron) acts
            turnManager.MarkCurrentUnitAsActed(); // Second unit (heron2) acts
            turnManager.MarkCurrentUnitAsActed(); // Third unit (myrmidon) acts

            Assert.IsTrue(turnManager.HasUnitActed(_heron.Id));
            Assert.IsTrue(turnManager.HasUnitActed(heron2.Id));
            Assert.IsTrue(turnManager.HasUnitActed(_myrmidon.Id));

            // Myrmidon refreshes heron
            turnManager.RefreshUnit(_heron.Id);
            Assert.IsFalse(turnManager.HasUnitActed(_heron.Id));

            // Heron can now be refreshed again by heron2 (but this is just state removal)
            bool canRefresh = turnManager.CanRefreshTarget(heron2, _heron);
            Assert.IsTrue(canRefresh);
        }

        // ── Cross-pattern refresh (transformed Heron) ──────────────────────

        private Unit CreateLaguzHeron(int id, (int, int) pos, bool transformed)
        {
            var classData = LaguzClassDataFactory.CreateHeron();
            var weapon = LaguzWeaponFactory.CreateForRace(LaguzRace.Heron);
            var heron = new Unit(id, $"LaguzHeron{id}", Team.PlayerTeam,
                classData, classData.UntransformedStats, pos, weapon);
            heron.InitLaguzGauge(classData.GaugeFillRate, classData.GaugeDrainRate);
            if (transformed)
                heron.Transform();
            return heron;
        }

        [Test]
        public void RefreshCross_TransformedHeron_RefreshesUpToFourAdjacentAllies()
        {
            var map = new GameMap(8, 8, 0);
            var heron = CreateLaguzHeron(10, (3, 3), transformed: true);

            // Place 4 allies in cardinal directions
            var north = new Unit(11, "North", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (3, 4), WeaponFactory.CreateIronSword());
            var south = new Unit(12, "South", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (3, 2), WeaponFactory.CreateIronSword());
            var east = new Unit(13, "East", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (4, 3), WeaponFactory.CreateIronSword());
            var west = new Unit(14, "West", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (2, 3), WeaponFactory.CreateIronSword());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { heron, north, south, east, west });

            // Mark all allies as acted
            tm.MarkUnitAsActed(north.Id);
            tm.MarkUnitAsActed(south.Id);
            tm.MarkUnitAsActed(east.Id);
            tm.MarkUnitAsActed(west.Id);

            int count = tm.RefreshCross(heron, map);

            Assert.AreEqual(4, count);
            Assert.IsFalse(tm.HasUnitActed(north.Id));
            Assert.IsFalse(tm.HasUnitActed(south.Id));
            Assert.IsFalse(tm.HasUnitActed(east.Id));
            Assert.IsFalse(tm.HasUnitActed(west.Id));
        }

        [Test]
        public void RefreshCross_UntransformedHeron_ReturnsZero()
        {
            var map = new GameMap(8, 8, 0);
            var heron = CreateLaguzHeron(10, (3, 3), transformed: false);

            var ally = new Unit(11, "Ally", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (3, 4), WeaponFactory.CreateIronSword());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { heron, ally });
            tm.MarkUnitAsActed(ally.Id);

            int count = tm.RefreshCross(heron, map);

            Assert.AreEqual(0, count);
            Assert.IsTrue(tm.HasUnitActed(ally.Id));
        }

        [Test]
        public void RefreshCross_SkipsEnemyUnits()
        {
            var map = new GameMap(8, 8, 0);
            var heron = CreateLaguzHeron(10, (3, 3), transformed: true);

            var enemy = new Unit(11, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(), ClassDataFactory.CreateFighter().BaseStats,
                (3, 4), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { heron, enemy });
            tm.MarkUnitAsActed(enemy.Id);

            int count = tm.RefreshCross(heron, map);

            Assert.AreEqual(0, count);
            Assert.IsTrue(tm.HasUnitActed(enemy.Id));
        }

        [Test]
        public void RefreshCross_SkipsUnactedUnits()
        {
            var map = new GameMap(8, 8, 0);
            var heron = CreateLaguzHeron(10, (3, 3), transformed: true);

            var actedAlly = new Unit(11, "Acted", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (3, 4), WeaponFactory.CreateIronSword());
            var freshAlly = new Unit(12, "Fresh", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (4, 3), WeaponFactory.CreateIronSword());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { heron, actedAlly, freshAlly });
            tm.MarkUnitAsActed(actedAlly.Id);
            // freshAlly has NOT acted

            int count = tm.RefreshCross(heron, map);

            Assert.AreEqual(1, count);
            Assert.IsFalse(tm.HasUnitActed(actedAlly.Id));
            Assert.IsFalse(tm.HasUnitActed(freshAlly.Id));
        }
    }
}
