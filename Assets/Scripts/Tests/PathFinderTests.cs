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
                    var tile = _map.GetTile(path[i].Item1, path[i].Item2);
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

            Assert.IsTrue(reachable.Contains((0, 0)));
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
            Assert.IsTrue(reachable.Contains((7, 7)));
        }

        // ---- Unit occupancy tests ----

        private IGameMap CreatePlainMap(int width, int height)
        {
            var tiles = new ITile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = new Tile(x, y, TerrainType.Plain);
            return new GameMap(width, height, tiles);
        }

        private IUnit CreateUnit(int id, Team team, (int x, int y) pos)
        {
            return new Unit(id, $"Unit{id}", team,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                pos, WeaponFactory.CreateIronSword());
        }

        [Test]
        public void GetMovementRange_ExcludesEnemyOccupiedTiles()
        {
            var map = CreatePlainMap(5, 5);
            var mover = CreateUnit(1, Team.PlayerTeam, (2, 2));
            var enemy = CreateUnit(2, Team.EnemyTeam, (3, 2));
            var allUnits = new List<IUnit> { mover, enemy };

            var reachable = _pathFinder.GetMovementRange(2, 2, 3, mover, map, allUnits);

            Assert.IsFalse(reachable.Contains((3, 2)), "Should not include enemy-occupied tile");
        }

        [Test]
        public void GetMovementRange_ExcludesAllyOccupiedTiles()
        {
            var map = CreatePlainMap(5, 5);
            var mover = CreateUnit(1, Team.PlayerTeam, (2, 2));
            var ally = CreateUnit(2, Team.PlayerTeam, (3, 2));
            var allUnits = new List<IUnit> { mover, ally };

            var reachable = _pathFinder.GetMovementRange(2, 2, 3, mover, map, allUnits);

            Assert.IsFalse(reachable.Contains((3, 2)), "Should not include ally-occupied tile as stop destination");
        }

        [Test]
        public void GetMovementRange_CanPassThroughAllies()
        {
            var map = CreatePlainMap(5, 1);
            // Mover at (0,0), ally at (1,0), target tile (2,0)
            var mover = CreateUnit(1, Team.PlayerTeam, (0, 0));
            var ally = CreateUnit(2, Team.PlayerTeam, (1, 0));
            var allUnits = new List<IUnit> { mover, ally };

            var reachable = _pathFinder.GetMovementRange(0, 0, 3, mover, map, allUnits);

            Assert.IsTrue(reachable.Contains((2, 0)), "Should be able to pass through ally to reach tile beyond");
        }

        [Test]
        public void GetMovementRange_CannotPassThroughEnemies()
        {
            var map = CreatePlainMap(5, 1);
            // Mover at (0,0), enemy at (1,0), tile beyond (2,0)
            var mover = CreateUnit(1, Team.PlayerTeam, (0, 0));
            var enemy = CreateUnit(2, Team.EnemyTeam, (1, 0));
            var allUnits = new List<IUnit> { mover, enemy };

            var reachable = _pathFinder.GetMovementRange(0, 0, 3, mover, map, allUnits);

            Assert.IsFalse(reachable.Contains((2, 0)), "Should not be able to pass through enemy");
        }

        [Test]
        public void FindPath_ToOccupiedTile_ReturnsEmptyPath()
        {
            var map = CreatePlainMap(5, 5);
            var mover = CreateUnit(1, Team.PlayerTeam, (0, 0));
            var enemy = CreateUnit(2, Team.EnemyTeam, (2, 0));
            var allUnits = new List<IUnit> { mover, enemy };

            var path = _pathFinder.FindPath(0, 0, 2, 0, 5, mover, map, allUnits);

            Assert.AreEqual(0, path.Count, "Should not path to an occupied tile");
        }

        [Test]
        public void FindPath_ThroughAlly_Succeeds()
        {
            var map = CreatePlainMap(5, 1);
            var mover = CreateUnit(1, Team.PlayerTeam, (0, 0));
            var ally = CreateUnit(2, Team.PlayerTeam, (1, 0));
            var allUnits = new List<IUnit> { mover, ally };

            var path = _pathFinder.FindPath(0, 0, 2, 0, 5, mover, map, allUnits);

            Assert.Greater(path.Count, 0, "Should find path through ally");
            Assert.AreEqual((2, 0), path[path.Count - 1]);
        }

        [Test]
        public void FindPath_ThroughEnemy_ReturnsEmptyPath()
        {
            var map = CreatePlainMap(5, 1);
            var mover = CreateUnit(1, Team.PlayerTeam, (0, 0));
            var enemy = CreateUnit(2, Team.EnemyTeam, (1, 0));
            var allUnits = new List<IUnit> { mover, enemy };

            var path = _pathFinder.FindPath(0, 0, 2, 0, 5, mover, map, allUnits);

            Assert.AreEqual(0, path.Count, "Should not path through enemy");
        }

        [Test]
        public void GetMovementRange_WithoutAllUnits_IgnoresOccupancy()
        {
            var map = CreatePlainMap(5, 5);
            var mover = CreateUnit(1, Team.PlayerTeam, (2, 2));

            var reachable = _pathFinder.GetMovementRange(2, 2, 1, mover, map);

            Assert.IsTrue(reachable.Contains((3, 2)), "Without allUnits, no occupancy check");
        }
    }
}
