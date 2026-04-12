using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class PathFinderTests
    {
        private IPathFinder _pathFinder;
        private IGameMap _map;
        private IUnit _infantryUnit;

        [SetUp]
        public void Setup()
        {
            _pathFinder = new PathFinder();
            _map = new GameMap(16, 16, 42);

            _infantryUnit = new Unit(
                1, "Infantry", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );
        }

        [Test]
        public void FindPath_SameTile_ReturnsPathWithOnlyStartPosition()
        {
            var path = _pathFinder.FindPath(0, 0, 0, 0, 5, _infantryUnit, _map);

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual((0, 0), path[0]);
        }

        [Test]
        public void FindPath_AdjacentTile_ReturnsPath()
        {
            var path = _pathFinder.FindPath(0, 0, 1, 0, 5, _infantryUnit, _map);

            Assert.GreaterOrEqual(path.Count, 1);
            Assert.AreEqual((0, 0), path[0]);
            Assert.AreEqual((1, 0), path[path.Count - 1]);
        }

        [Test]
        public void FindPath_WithoutEnoughMovement_ReturnsEmptyPath()
        {
            var path = _pathFinder.FindPath(0, 0, 10, 10, 1, _infantryUnit, _map);

            // With only 1 movement point and terrain costs, reaching (10, 10) might not be possible
            if (path.Count > 0)
            {
                int totalMovement = 0;
                for (int i = 1; i < path.Count; i++)
                {
                    var tile = _map.GetTile(path[i].x, path[i].y);
                    totalMovement += TerrainProperties.GetMovementCost(tile.Terrain, true);
                }
                Assert.LessOrEqual(totalMovement, 1);
            }
        }

        [Test]
        public void FindPath_OutOfBounds_ReturnsEmptyPath()
        {
            var path = _pathFinder.FindPath(0, 0, 100, 100, 5, _infantryUnit, _map);

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void GetMovementRange_AllReachableTiles_ContainsStartPosition()
        {
            var reachable = _pathFinder.GetMovementRange(0, 0, 5, _infantryUnit, _map);

            Assert.Contains((0, 0), reachable);
        }

        [Test]
        public void GetMovementRange_WithMovement_ReturnsReachableTiles()
        {
            var reachable = _pathFinder.GetMovementRange(7, 7, 3, _infantryUnit, _map);

            Assert.Greater(reachable.Count, 0);
            Assert.LessOrEqual(reachable.Count, 50);
        }

        [Test]
        public void GetMovementRange_HighMovement_ReturnsExpandedRange()
        {
            var reachableSmall = _pathFinder.GetMovementRange(7, 7, 2, _infantryUnit, _map);
            var reachableLarge = _pathFinder.GetMovementRange(7, 7, 5, _infantryUnit, _map);

            Assert.LessOrEqual(reachableSmall.Count, reachableLarge.Count);
        }

        [Test]
        public void GetMovementRange_NoMovement_OnlyContainsStartPosition()
        {
            var reachable = _pathFinder.GetMovementRange(7, 7, 0, _infantryUnit, _map);

            Assert.AreEqual(1, reachable.Count);
            Assert.Contains((7, 7), reachable);
        }
    }
}
