using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ShoveServiceTests
    {
        private ShoveService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ShoveService();
        }

        private IGameMap CreatePlainMap(int width, int height)
        {
            var tiles = new ITile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = new Tile(x, y, TerrainType.Plain);
            return new GameMap(width, height, tiles);
        }

        private IGameMap CreateMapWithWall(int width, int height, int wallX, int wallY)
        {
            var tiles = new ITile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = new Tile(x, y, TerrainType.Plain);
            tiles[wallX, wallY] = new Tile(wallX, wallY, TerrainType.Wall);
            return new GameMap(width, height, tiles);
        }

        private Unit MakeUnit(int id, Team team, (int x, int y) pos, int str, string className = "Soldier")
        {
            IClassData classData = className switch
            {
                "Fighter" => ClassDataFactory.CreateFighter(),
                "Warrior" => ClassDataFactory.CreateWarrior(),
                "Reaver" => ClassDataFactory.CreateReaver(),
                _ => ClassDataFactory.CreateSoldier()
            };
            var stats = new CharacterStats(20, str, 0, 8, 8, 3, 7, 2, 5);
            return new Unit(id, $"Unit{id}", team, classData, stats, pos, WeaponFactory.CreateIronLance());
        }

        [Test]
        public void CanShove_Adjacent_StrongerShover_ReturnsTrue()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsTrue(_service.CanShove(shover, target, map, allUnits));
        }

        [Test]
        public void CanShove_EqualStr_ReturnsTrue()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 8);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 8);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsTrue(_service.CanShove(shover, target, map, allUnits));
        }

        [Test]
        public void CanShove_WeakerShover_ReturnsFalse()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 5);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 10);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsFalse(_service.CanShove(shover, target, map, allUnits));
        }

        [Test]
        public void CanShove_NotAdjacent_ReturnsFalse()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (0, 0), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (3, 3), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsFalse(_service.CanShove(shover, target, map, allUnits));
        }

        [Test]
        public void Shove_PushesTarget1Tile()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            bool result = _service.Shove(shover, target, map, allUnits);

            Assert.IsTrue(result);
            Assert.AreEqual((5, 3), target.Position);
        }

        [Test]
        public void Shove_DestinationWall_ReturnsFalse()
        {
            var map = CreateMapWithWall(8, 8, 5, 3);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsFalse(_service.Shove(shover, target, map, allUnits));
            Assert.AreEqual((4, 3), target.Position); // unchanged
        }

        [Test]
        public void Shove_DestinationOccupied_ReturnsFalse()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (4, 3), str: 7);
            var blocker = MakeUnit(3, Team.PlayerTeam, (5, 3), str: 5);
            var allUnits = new List<IUnit> { shover, target, blocker };

            Assert.IsFalse(_service.Shove(shover, target, map, allUnits));
        }

        [Test]
        public void Shove_OffMapEdge_ReturnsFalse()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (6, 0), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (7, 0), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            // Pushing right from x=7 goes to x=8, which is off the 8-wide map
            Assert.IsFalse(_service.CanShove(shover, target, map, allUnits));
        }

        [Test]
        public void Shove_VerticalDirection_Works()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.PlayerTeam, (3, 4), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            bool result = _service.Shove(shover, target, map, allUnits);

            Assert.IsTrue(result);
            Assert.AreEqual((3, 5), target.Position);
        }

        [Test]
        public void CanSmite_FighterClass_ReturnsTrue()
        {
            var map = CreatePlainMap(8, 8);
            var smiter = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10, className: "Fighter");
            var target = MakeUnit(2, Team.EnemyTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            Assert.IsTrue(_service.CanSmite(smiter, target, map, allUnits));
        }

        [Test]
        public void CanSmite_WarriorClass_ReturnsTrue()
        {
            var map = CreatePlainMap(8, 8);
            var smiter = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10, className: "Warrior");
            var target = MakeUnit(2, Team.EnemyTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            Assert.IsTrue(_service.CanSmite(smiter, target, map, allUnits));
        }

        [Test]
        public void CanSmite_NonFighterClass_ReturnsFalse()
        {
            var map = CreatePlainMap(8, 8);
            var smiter = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10, className: "Soldier");
            var target = MakeUnit(2, Team.EnemyTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            Assert.IsFalse(_service.CanSmite(smiter, target, map, allUnits));
        }

        [Test]
        public void Smite_PushesTarget2Tiles()
        {
            var map = CreatePlainMap(8, 8);
            var smiter = MakeUnit(1, Team.PlayerTeam, (2, 3), str: 10, className: "Fighter");
            var target = MakeUnit(2, Team.EnemyTeam, (3, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            bool result = _service.Smite(smiter, target, map, allUnits);

            Assert.IsTrue(result);
            Assert.AreEqual((5, 3), target.Position);
        }

        [Test]
        public void Smite_IntermediateBlocked_ReturnsFalse()
        {
            var map = CreateMapWithWall(8, 8, 4, 3);
            var smiter = MakeUnit(1, Team.PlayerTeam, (2, 3), str: 10, className: "Fighter");
            var target = MakeUnit(2, Team.EnemyTeam, (3, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            Assert.IsFalse(_service.Smite(smiter, target, map, allUnits));
        }

        [Test]
        public void Smite_FinalTileBlocked_ReturnsFalse()
        {
            var map = CreateMapWithWall(8, 8, 5, 3);
            var smiter = MakeUnit(1, Team.PlayerTeam, (2, 3), str: 10, className: "Fighter");
            var target = MakeUnit(2, Team.EnemyTeam, (3, 3), str: 7);
            var allUnits = new List<IUnit> { smiter, target };

            Assert.IsFalse(_service.Smite(smiter, target, map, allUnits));
        }

        [Test]
        public void Shove_CanShoveEnemyUnits()
        {
            var map = CreatePlainMap(8, 8);
            var shover = MakeUnit(1, Team.PlayerTeam, (3, 3), str: 10);
            var target = MakeUnit(2, Team.EnemyTeam, (4, 3), str: 7);
            var allUnits = new List<IUnit> { shover, target };

            Assert.IsTrue(_service.CanShove(shover, target, map, allUnits));
        }
    }
}
