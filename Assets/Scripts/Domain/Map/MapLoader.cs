using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public interface IMapLoader
    {
        IGameMap CreateMap(MapDefinition definition);
        List<IUnit> CreateUnits(MapDefinition definition);
    }

    public class MapLoader : IMapLoader
    {
        public IGameMap CreateMap(MapDefinition definition)
        {
            var tiles = new ITile[definition.Width, definition.Height];
            var chestPositions = new HashSet<(int, int)>();
            var chestItems = new Dictionary<(int, int), Items.IItem>();

            foreach (var chest in definition.Chests)
            {
                chestPositions.Add(chest.Position);
                chestItems[chest.Position] = chest.Item;
            }

            for (int x = 0; x < definition.Width; x++)
            {
                for (int y = 0; y < definition.Height; y++)
                {
                    var terrain = definition.Terrain[x, y];

                    if (terrain == TerrainType.Door)
                    {
                        tiles[x, y] = new InteractableTile(x, y, TerrainType.Door);
                    }
                    else if (terrain == TerrainType.Chest || chestPositions.Contains((x, y)))
                    {
                        var item = chestItems.ContainsKey((x, y)) ? chestItems[(x, y)] : null;
                        tiles[x, y] = new InteractableTile(x, y, TerrainType.Chest, item);
                    }
                    else
                    {
                        tiles[x, y] = new Tile(x, y, terrain);
                    }
                }
            }

            var map = new GameMap(definition.Width, definition.Height, tiles);
            map.SetWeather(definition.Weather);
            return map;
        }

        public List<IUnit> CreateUnits(MapDefinition definition)
        {
            var units = new List<IUnit>();
            int nextId = 1;

            foreach (var placement in definition.PlayerPlacements)
            {
                units.Add(CreateUnitFromPlacement(nextId++, placement));
            }

            foreach (var placement in definition.EnemyPlacements)
            {
                units.Add(CreateUnitFromPlacement(nextId++, placement));
            }

            return units;
        }

        private IUnit CreateUnitFromPlacement(int id, UnitPlacement placement)
        {
            var classData = ResolveClass(placement.ClassName);
            var weapon = ResolveWeapon(placement.WeaponName, classData);
            var unit = new Unit(id, placement.Name, placement.Team, classData, classData.BaseStats, placement.Position, weapon);

            // Apply levels above 1
            if (placement.Level > 1)
            {
                var rng = new Random(id * 31 + placement.Level);
                for (int i = 1; i < placement.Level; i++)
                {
                    unit.GainExperience(100, rng);
                }
            }

            return unit;
        }

        private static IClassData ResolveClass(string className)
        {
            return className.ToLower() switch
            {
                "myrmidon" => ClassDataFactory.CreateMyrmidon(),
                "soldier" => ClassDataFactory.CreateSoldier(),
                "fighter" => ClassDataFactory.CreateFighter(),
                "mage" => ClassDataFactory.CreateMage(),
                "archer" => ClassDataFactory.CreateArcher(),
                "cleric" => ClassDataFactory.CreateCleric(),
                "heron" => ClassDataFactory.CreateHeron(),
                "swordmaster" => ClassDataFactory.CreateSwordmaster(),
                "general" => ClassDataFactory.CreateGeneral(),
                "warrior" => ClassDataFactory.CreateWarrior(),
                "sage" => ClassDataFactory.CreateSage(),
                "sniper" => ClassDataFactory.CreateSniper(),
                "bishop" => ClassDataFactory.CreateBishop(),
                "trueblade" => ClassDataFactory.CreateTrueblade(),
                "marshall" => ClassDataFactory.CreateMarshall(),
                "reaver" => ClassDataFactory.CreateReaver(),
                "archsage" => ClassDataFactory.CreateArchsage(),
                "marksman" => ClassDataFactory.CreateMarksman(),
                "saint" => ClassDataFactory.CreateSaint(),
                _ => throw new ArgumentException($"Unknown class name: {className}")
            };
        }

        private static Weapons.IWeapon ResolveWeapon(string weaponName, IClassData classData)
        {
            if (string.IsNullOrEmpty(weaponName))
            {
                return WeaponFactory.GetWeaponForClass(classData.WeaponType);
            }

            return weaponName.ToLower() switch
            {
                "iron sword" => WeaponFactory.CreateIronSword(),
                "iron lance" => WeaponFactory.CreateIronLance(),
                "iron axe" => WeaponFactory.CreateIronAxe(),
                "iron bow" => WeaponFactory.CreateIronBow(),
                "fire" => WeaponFactory.CreateFireTome(),
                "wind" => WeaponFactory.CreateWindTome(),
                "thunder" => WeaponFactory.CreateThunderTome(),
                "elfire" => WeaponFactory.CreateSteelFireTome(),
                "elwind" => WeaponFactory.CreateSteelWindTome(),
                "elthunder" => WeaponFactory.CreateSteelThunderTome(),
                "arcfire" => WeaponFactory.CreateSilverFireTome(),
                "tornado" => WeaponFactory.CreateSilverWindTome(),
                "thoron" => WeaponFactory.CreateSilverThunderTome(),
                "heal staff" => WeaponFactory.CreateHealStaff(),
                "steel sword" => WeaponFactory.CreateSteelSword(),
                "steel lance" => WeaponFactory.CreateSteelLance(),
                "steel axe" => WeaponFactory.CreateSteelAxe(),
                "silver sword" => WeaponFactory.CreateSilverSword(),
                "silver lance" => WeaponFactory.CreateSilverLance(),
                "silver axe" => WeaponFactory.CreateSilverAxe(),
                _ => WeaponFactory.GetWeaponForClass(classData.WeaponType)
            };
        }
    }
}
