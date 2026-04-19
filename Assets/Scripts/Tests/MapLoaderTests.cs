using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class MapLoaderTests
    {
        private MapLoader _loader;

        [SetUp]
        public void Setup()
        {
            _loader = new MapLoader();
        }

        [Test]
        public void CreateMap_DimensionsMatchDefinition()
        {
            var def = CreateSimpleDefinition(8, 6);
            var map = _loader.CreateMap(def);

            Assert.AreEqual(8, map.Width);
            Assert.AreEqual(6, map.Height);
        }

        [Test]
        public void CreateMap_TerrainPlacementMatchesDefinition()
        {
            var terrain = new TerrainType[4, 4];
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    terrain[x, y] = TerrainType.Plain;
            terrain[1, 1] = TerrainType.Forest;
            terrain[2, 2] = TerrainType.Mountain;

            var def = new MapDefinition("Test", 4, 4, terrain,
                new List<UnitPlacement>(), new List<UnitPlacement>());
            var map = _loader.CreateMap(def);

            Assert.AreEqual(TerrainType.Plain, map.GetTile(0, 0).Terrain);
            Assert.AreEqual(TerrainType.Forest, map.GetTile(1, 1).Terrain);
            Assert.AreEqual(TerrainType.Mountain, map.GetTile(2, 2).Terrain);
        }

        [Test]
        public void CreateMap_DoorTile_CreatesInteractableTile()
        {
            var terrain = new TerrainType[3, 3];
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    terrain[x, y] = TerrainType.Plain;
            terrain[1, 1] = TerrainType.Door;

            var def = new MapDefinition("Test", 3, 3, terrain,
                new List<UnitPlacement>(), new List<UnitPlacement>());
            var map = _loader.CreateMap(def);

            var tile = map.GetTile(1, 1);
            Assert.IsInstanceOf<InteractableTile>(tile);
            Assert.AreEqual(TerrainType.Door, tile.Terrain);
            Assert.IsFalse(((InteractableTile)tile).IsOpened);
        }

        [Test]
        public void CreateMap_ChestTile_ContainsItem()
        {
            var terrain = new TerrainType[3, 3];
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    terrain[x, y] = TerrainType.Plain;
            terrain[2, 2] = TerrainType.Chest;

            var item = ConsumableFactory.CreateVulnerary();
            var chests = new List<ChestPlacement>
            {
                new ChestPlacement((2, 2), item)
            };

            var def = new MapDefinition("Test", 3, 3, terrain,
                new List<UnitPlacement>(), new List<UnitPlacement>(),
                chests: chests);
            var map = _loader.CreateMap(def);

            var tile = map.GetTile(2, 2);
            Assert.IsInstanceOf<InteractableTile>(tile);
            var chest = (InteractableTile)tile;
            Assert.AreEqual("Vulnerary", chest.ContainedItem.Name);
        }

        [Test]
        public void CreateUnits_ProducesCorrectTeamAndPosition()
        {
            var terrain = new TerrainType[4, 4];
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    terrain[x, y] = TerrainType.Plain;

            var players = new List<UnitPlacement>
            {
                new UnitPlacement("Hero", "Myrmidon", "Iron Sword", Team.PlayerTeam, (0, 0))
            };
            var enemies = new List<UnitPlacement>
            {
                new UnitPlacement("Foe", "Soldier", "Iron Lance", Team.EnemyTeam, (3, 3))
            };

            var def = new MapDefinition("Test", 4, 4, terrain, players, enemies);
            var units = _loader.CreateUnits(def);

            Assert.AreEqual(2, units.Count);
            Assert.AreEqual("Hero", units[0].Name);
            Assert.AreEqual(Team.PlayerTeam, units[0].Team);
            Assert.AreEqual((0, 0), units[0].Position);
            Assert.AreEqual("Foe", units[1].Name);
            Assert.AreEqual(Team.EnemyTeam, units[1].Team);
            Assert.AreEqual((3, 3), units[1].Position);
        }

        [Test]
        public void CreateUnits_CorrectClassAndWeapon()
        {
            var def = CreateSimpleDefinition(4, 4);
            var units = _loader.CreateUnits(def);

            Assert.AreEqual("Myrmidon", units[0].Class.Name);
            Assert.AreEqual("Iron Sword", units[0].EquippedWeapon.Name);
        }

        [Test]
        public void CreateUnits_LevelAbove1_GainsLevels()
        {
            var terrain = new TerrainType[4, 4];
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    terrain[x, y] = TerrainType.Plain;

            var enemies = new List<UnitPlacement>
            {
                new UnitPlacement("Veteran", "Fighter", "Iron Axe", Team.EnemyTeam, (3, 3), level: 5)
            };

            var def = new MapDefinition("Test", 4, 4, terrain,
                new List<UnitPlacement>(), enemies);
            var units = _loader.CreateUnits(def);

            Assert.AreEqual(5, units[0].Level);
        }

        [Test]
        public void PlainsSkirmish_LoadsSuccessfully()
        {
            var def = MapDefinitions.PlainsSkirmish();
            var map = _loader.CreateMap(def);
            var units = _loader.CreateUnits(def);

            Assert.AreEqual(12, map.Width);
            Assert.AreEqual(10, map.Height);
            Assert.AreEqual(8, units.Count);
        }

        [Test]
        public void CastleAssault_HasThrone()
        {
            var def = MapDefinitions.CastleAssault();
            var map = _loader.CreateMap(def);

            Assert.AreEqual(TerrainType.Throne, map.GetTile(7, 12).Terrain);
        }

        [Test]
        public void CastleAssault_HasDoorsAndChests()
        {
            var def = MapDefinitions.CastleAssault();
            var map = _loader.CreateMap(def);

            Assert.IsInstanceOf<InteractableTile>(map.GetTile(6, 8));
            Assert.AreEqual(TerrainType.Door, map.GetTile(6, 8).Terrain);

            var chest = map.GetTile(4, 12) as InteractableTile;
            Assert.IsNotNull(chest);
            Assert.IsNotNull(chest.ContainedItem);
        }

        [Test]
        public void DesertHoldout_HasDesertAndBridges()
        {
            var def = MapDefinitions.DesertHoldout();
            var map = _loader.CreateMap(def);

            Assert.AreEqual(TerrainType.Desert, map.GetTile(5, 5).Terrain);
            Assert.AreEqual(TerrainType.Bridge, map.GetTile(7, 3).Terrain);
            Assert.AreEqual(TerrainType.Wall, map.GetTile(7, 0).Terrain);
        }

        private MapDefinition CreateSimpleDefinition(int w, int h)
        {
            var terrain = new TerrainType[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    terrain[x, y] = TerrainType.Plain;

            var players = new List<UnitPlacement>
            {
                new UnitPlacement("Hero", "Myrmidon", "Iron Sword", Team.PlayerTeam, (0, 0))
            };

            return new MapDefinition("Test", w, h, terrain, players, new List<UnitPlacement>());
        }
    }
}
