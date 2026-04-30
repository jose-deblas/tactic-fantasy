using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Map
{
    public interface IReinforcementService
    {
        List<IUnit> EvaluateTriggers(
            IReadOnlyList<ReinforcementTrigger> triggers,
            int currentTurn,
            IReadOnlyList<IUnit> allUnits,
            IGameMap map = null);
    }

    public class ReinforcementService : IReinforcementService
    {
        private readonly IMapLoader _mapLoader;
        private int _nextId;

        public ReinforcementService(IMapLoader mapLoader, int startingId = 100)
        {
            _mapLoader = mapLoader;
            _nextId = startingId;
        }

        public List<IUnit> EvaluateTriggers(
            IReadOnlyList<ReinforcementTrigger> triggers,
            int currentTurn,
            IReadOnlyList<IUnit> allUnits,
            IGameMap map = null)
        {
            var spawned = new List<IUnit>();
            var occupied = new HashSet<(int, int)>(allUnits.Where(u => u.IsAlive).Select(u => u.Position));

            foreach (var trigger in triggers)
            {
                if (trigger.HasFired)
                    continue;

                if (ShouldFire(trigger, currentTurn, allUnits))
                {
                    trigger.HasFired = true;
                    foreach (var placement in trigger.UnitsToSpawn)
                    {
                        var classData = ResolveClass(placement.ClassName);
                        var weapon = WeaponFactory.GetWeaponForClass(classData.WeaponType);
                        var spawnPos = FindNearestFreePosition(placement.Position, occupied, classData, map);
                        var unit = new Unit(_nextId++, placement.Name, placement.Team, classData, classData.BaseStats, spawnPos, weapon);
                        spawned.Add(unit);
                        occupied.Add(spawnPos);
                    }
                }
            }

            return spawned;
        }

        private (int, int) FindNearestFreePosition((int x, int y) desired, HashSet<(int, int)> occupied, IClassData classData, IGameMap map)
        {
            int width = map != null ? map.Width : 16;
            int height = map != null ? map.Height : 16;

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

            bool isMage = classData.UsableWeaponTypes.Contains(WeaponType.FIRE);

            foreach (var c in candidates)
            {
                var (cx, cy) = c.pos;
                if (occupied.Contains((cx, cy))) continue;
                if (map != null)
                {
                    if (!map.IsValidPosition(cx, cy)) continue;
                    var tile = map.GetTile(cx, cy);
                    if (!TerrainProperties.IsPassable(tile.Terrain, classData.MoveType, isMage)) continue;
                }
                else
                {
                    // no map available - assume tile is valid/passable
                }
                return (cx, cy);
            }

            // fallback: return desired
            return desired;
        }

        private bool ShouldFire(ReinforcementTrigger trigger, int currentTurn, IReadOnlyList<IUnit> allUnits)
        {
            switch (trigger.Condition)
            {
                case TriggerCondition.OnTurn:
                    return currentTurn == trigger.TurnNumber;

                case TriggerCondition.OnTileSteppedOn:
                    if (!trigger.TriggerTile.HasValue)
                        return false;
                    var (tx, ty) = trigger.TriggerTile.Value;
                    return allUnits.Any(u => u.IsAlive && u.Position == (tx, ty));

                case TriggerCondition.OnUnitDeath:
                    if (!trigger.TriggerUnitId.HasValue)
                        return false;
                    var target = allUnits.FirstOrDefault(u => u.Id == trigger.TriggerUnitId.Value);
                    return target != null && !target.IsAlive;

                default:
                    return false;
            }
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
                "swordmaster" => ClassDataFactory.CreateSwordmaster(),
                "general" => ClassDataFactory.CreateGeneral(),
                "warrior" => ClassDataFactory.CreateWarrior(),
                "sage" => ClassDataFactory.CreateSage(),
                "sniper" => ClassDataFactory.CreateSniper(),
                "bishop" => ClassDataFactory.CreateBishop(),
                _ => ClassDataFactory.CreateSoldier()
            };
        }
    }
}
