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

            // Track occupied positions to avoid placing two units on the same tile
            var occupied = new HashSet<(int, int)>();

            // Helper: find nearest passable, unoccupied tile to a desired position
            (int, int) FindNearestFree((int x, int y) desired, IClassData cls)
            {
                int width = definition.Width;
                int height = definition.Height;
                var candidates = new List<((int x, int y) pos, int dist)>();
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int d = System.Math.Abs(x - desired.x) + System.Math.Abs(y - desired.y);
                        candidates.Add(((x, y), d));
                    }
                }

                candidates.Sort((a, b) =>
                {
                    int cmp = a.dist.CompareTo(b.dist);
                    if (cmp != 0) return cmp;
                    if (a.pos.x != b.pos.x) return a.pos.x.CompareTo(b.pos.x);
                    return a.pos.y.CompareTo(b.pos.y);
                });

                bool isMage = cls.UsableWeaponTypes.Contains(Weapons.WeaponType.FIRE);

                foreach (var c in candidates)
                {
                    var (cx, cy) = c.pos;
                    if (occupied.Contains((cx, cy))) continue;
                    var terrain = definition.Terrain[cx, cy];
                    if (TerrainProperties.IsPassable(terrain, cls.MoveType, isMage))
                        return (cx, cy);
                }

                // Fallback: return desired if nothing else found
                return desired;
            }

            foreach (var placement in definition.PlayerPlacements)
            {
                var classData = ResolveClass(placement.ClassName);
                var weapon = ResolveWeapon(placement.WeaponName, classData);
                var pos = FindNearestFree(placement.Position, classData);
                var unit = new Unit(nextId++, placement.Name, placement.Team, classData, classData.BaseStats, pos, weapon);

                // Apply levels above 1
                if (placement.Level > 1)
                {
                    var rng = new System.Random(unit.Id * 31 + placement.Level);
                    for (int i = 1; i < placement.Level; i++)
                    {
                        unit.GainExperience(100, rng);
                    }
                }

                units.Add(unit);
                occupied.Add(pos);
            }

            foreach (var placement in definition.EnemyPlacements)
            {
                var classData = ResolveClass(placement.ClassName);
                var weapon = ResolveWeapon(placement.WeaponName, classData);
                var pos = FindNearestFree(placement.Position, classData);
                var unit = new Unit(nextId++, placement.Name, placement.Team, classData, classData.BaseStats, pos, weapon);

                if (placement.Level > 1)
                {
                    var rng = new System.Random(unit.Id * 31 + placement.Level);
                    for (int i = 1; i < placement.Level; i++)
                    {
                        unit.GainExperience(100, rng);
                    }
                }

                units.Add(unit);
                occupied.Add(pos);
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
