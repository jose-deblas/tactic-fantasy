using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class FogOfWarTests
    {
        private FogOfWar _fog;

        [SetUp]
        public void Setup()
        {
            _fog = new FogOfWar();
        }

        [Test]
        public void VisionRadius_IsMovPlusTwo()
        {
            var map = CreatePlainMap(16, 16);
            // MOV=5, so vision = 5+2 = 7
            var unit = CreateUnit(1, Team.PlayerTeam, (8, 8), 5);
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            // Tile within radius 7
            Assert.IsTrue(_fog.IsTileVisible(8, 8, Team.PlayerTeam), "Origin should be visible");
            Assert.IsTrue(_fog.IsTileVisible(8, 15, Team.PlayerTeam), "7 tiles away should be visible");

            // Tile outside radius
            Assert.IsFalse(_fog.IsTileVisible(0, 0, Team.PlayerTeam), "Far away tile should not be visible");
        }

        [Test]
        public void TilesOutsideRadius_NotVisible()
        {
            var map = CreatePlainMap(20, 20);
            // MOV=3, vision = 3+2 = 5
            var unit = CreateUnit(1, Team.PlayerTeam, (10, 10), 3);
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            Assert.IsFalse(_fog.IsTileVisible(10, 16, Team.PlayerTeam), "6 tiles away should not be visible");
            Assert.IsFalse(_fog.IsTileVisible(0, 0, Team.PlayerTeam));
        }

        [Test]
        public void Forest_BlocksVision_ForInfantry()
        {
            // Create a map with a forest wall blocking vision
            var tiles = new ITile[10, 1];
            for (int x = 0; x < 10; x++)
                tiles[x, 0] = new Tile(x, 0, TerrainType.Plain);
            // Forest at x=3 should block vision beyond it
            tiles[3, 0] = new Tile(3, 0, TerrainType.Forest);
            var map = new GameMap(10, 1, tiles);

            var unit = CreateUnit(1, Team.PlayerTeam, (0, 0), 8);
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            Assert.IsTrue(_fog.IsTileVisible(3, 0, Team.PlayerTeam), "Forest tile itself should be visible");
            Assert.IsFalse(_fog.IsTileVisible(4, 0, Team.PlayerTeam), "Tile behind forest should not be visible");
        }

        [Test]
        public void Forest_DoesNotBlockVision_ForFlying()
        {
            var tiles = new ITile[10, 1];
            for (int x = 0; x < 10; x++)
                tiles[x, 0] = new Tile(x, 0, TerrainType.Plain);
            tiles[3, 0] = new Tile(3, 0, TerrainType.Forest);
            var map = new GameMap(10, 1, tiles);

            // Create a flying unit
            var flyingClass = new ClassData(
                "Hawk", new CharacterStats(20, 8, 0, 10, 10, 5, 6, 2, 7),
                new CharacterStats(40, 25, 5, 25, 25, 20, 20, 10, 9),
                new CharacterStats(55, 40, 0, 50, 50, 30, 25, 10, 0),
                WeaponType.STRIKE, MoveType.Flying);
            var unit = new Unit(1, "Hawk", Team.PlayerTeam, flyingClass,
                new CharacterStats(20, 8, 0, 10, 10, 5, 6, 2, 7),
                (0, 0), WeaponFactory.CreateIronSword());
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            Assert.IsTrue(_fog.IsTileVisible(4, 0, Team.PlayerTeam), "Flying unit should see past forest");
        }

        [Test]
        public void Torch_ExtendsVisionBy5()
        {
            var map = CreatePlainMap(20, 1);
            // MOV=3, base vision = 5. Without torch, can't see (10,0)
            var unit = CreateUnit(1, Team.PlayerTeam, (0, 0), 3);

            _fog.RecalculateVision(new List<IUnit> { unit }, map);
            Assert.IsFalse(_fog.IsTileVisible(10, 0, Team.PlayerTeam), "Without torch, tile 10 is not visible");

            // Add torch to inventory
            unit.Inventory.Add(ConsumableFactory.CreateTorch());
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            // Vision is now 3+2+5 = 10
            Assert.IsTrue(_fog.IsTileVisible(10, 0, Team.PlayerTeam), "With torch, vision extends by 5");
        }

        [Test]
        public void MultipleUnits_VisionIsUnion()
        {
            var map = CreatePlainMap(20, 1);
            var unit1 = CreateUnit(1, Team.PlayerTeam, (0, 0), 3);   // vision = 5
            var unit2 = CreateUnit(2, Team.PlayerTeam, (15, 0), 3);  // vision = 5

            _fog.RecalculateVision(new List<IUnit> { unit1, unit2 }, map);

            Assert.IsTrue(_fog.IsTileVisible(0, 0, Team.PlayerTeam));
            Assert.IsTrue(_fog.IsTileVisible(15, 0, Team.PlayerTeam));
            // Gap in between might not be visible
            Assert.IsFalse(_fog.IsTileVisible(8, 0, Team.PlayerTeam), "Gap between units not covered");
        }

        [Test]
        public void SeparateVisibility_PerTeam()
        {
            var map = CreatePlainMap(20, 1);
            var player = CreateUnit(1, Team.PlayerTeam, (0, 0), 3);
            var enemy = CreateUnit(2, Team.EnemyTeam, (19, 0), 3);

            _fog.RecalculateVision(new List<IUnit> { player, enemy }, map);

            Assert.IsTrue(_fog.IsTileVisible(0, 0, Team.PlayerTeam));
            Assert.IsFalse(_fog.IsTileVisible(0, 0, Team.EnemyTeam), "Enemy shouldn't see player's area");
            Assert.IsTrue(_fog.IsTileVisible(19, 0, Team.EnemyTeam));
            Assert.IsFalse(_fog.IsTileVisible(19, 0, Team.PlayerTeam), "Player shouldn't see enemy's area");
        }

        [Test]
        public void GetVisibleTiles_ReturnsCorrectSet()
        {
            var map = CreatePlainMap(5, 1);
            var unit = CreateUnit(1, Team.PlayerTeam, (2, 0), 2); // vision = 4
            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            var visible = _fog.GetVisibleTiles(Team.PlayerTeam);
            Assert.IsTrue(visible.Contains((2, 0)));
            Assert.AreEqual(5, visible.Count, "All 5 tiles should be visible with radius 4");
        }

        [Test]
        public void DeadUnit_DoesNotContributeVision()
        {
            var map = CreatePlainMap(10, 1);
            var unit = CreateUnit(1, Team.PlayerTeam, (0, 0), 3);
            unit.TakeDamage(100);

            _fog.RecalculateVision(new List<IUnit> { unit }, map);

            Assert.IsFalse(_fog.IsTileVisible(0, 0, Team.PlayerTeam), "Dead unit should not provide vision");
        }

        [Test]
        public void NoUnitsForTeam_NoVisibility()
        {
            var map = CreatePlainMap(5, 5);
            _fog.RecalculateVision(new List<IUnit>(), map);

            Assert.IsFalse(_fog.IsTileVisible(0, 0, Team.PlayerTeam));
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private IGameMap CreatePlainMap(int width, int height)
        {
            var tiles = new ITile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = new Tile(x, y, TerrainType.Plain);
            return new GameMap(width, height, tiles);
        }

        private Unit CreateUnit(int id, Team team, (int, int) pos, int mov)
        {
            return new Unit(id, $"Unit{id}", team,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, mov),
                pos, WeaponFactory.CreateIronSword());
        }
    }
}
