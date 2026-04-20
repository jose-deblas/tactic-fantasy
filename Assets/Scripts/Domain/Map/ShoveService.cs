using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public interface IShoveService
    {
        bool CanShove(IUnit shover, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits);
        bool CanSmite(IUnit smiter, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits);
        bool Shove(IUnit shover, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits);
        bool Smite(IUnit smiter, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits);
    }

    public class ShoveService : IShoveService
    {
        private static readonly HashSet<string> SmiteClasses = new HashSet<string>
        {
            "Fighter", "Warrior", "Reaver"
        };

        public bool CanShove(IUnit shover, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits)
        {
            if (!IsValidShoveSetup(shover, target, map)) return false;
            if (shover.CurrentStats.STR < target.CurrentStats.STR) return false;

            var (dx, dy) = GetDirection(shover, target);
            var dest = (target.Position.x + dx, target.Position.y + dy);
            return IsTileAvailable(dest.Item1, dest.Item2, map, target, allUnits);
        }

        public bool CanSmite(IUnit smiter, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits)
        {
            if (!IsValidShoveSetup(smiter, target, map)) return false;
            if (!SmiteClasses.Contains(smiter.Class.Name)) return false;
            if (smiter.CurrentStats.STR < target.CurrentStats.STR) return false;

            var (dx, dy) = GetDirection(smiter, target);
            int midX = target.Position.x + dx;
            int midY = target.Position.y + dy;
            int destX = target.Position.x + dx * 2;
            int destY = target.Position.y + dy * 2;

            if (!IsTileAvailable(midX, midY, map, target, allUnits)) return false;
            return IsTileAvailable(destX, destY, map, target, allUnits);
        }

        public bool Shove(IUnit shover, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits)
        {
            if (!CanShove(shover, target, map, allUnits)) return false;

            var (dx, dy) = GetDirection(shover, target);
            target.SetPosition(target.Position.x + dx, target.Position.y + dy);
            return true;
        }

        public bool Smite(IUnit smiter, IUnit target, IGameMap map, IReadOnlyList<IUnit> allUnits)
        {
            if (!CanSmite(smiter, target, map, allUnits)) return false;

            var (dx, dy) = GetDirection(smiter, target);
            target.SetPosition(target.Position.x + dx * 2, target.Position.y + dy * 2);
            return true;
        }

        private bool IsValidShoveSetup(IUnit shover, IUnit target, IGameMap map)
        {
            if (shover == null || target == null) return false;
            if (!shover.IsAlive || !target.IsAlive) return false;
            if (shover.Id == target.Id) return false;

            int distance = Math.Abs(shover.Position.x - target.Position.x)
                         + Math.Abs(shover.Position.y - target.Position.y);
            return distance == 1;
        }

        private (int dx, int dy) GetDirection(IUnit from, IUnit to)
        {
            int dx = Math.Sign(to.Position.x - from.Position.x);
            int dy = Math.Sign(to.Position.y - from.Position.y);
            return (dx, dy);
        }

        private bool IsTileAvailable(int x, int y, IGameMap map, IUnit target, IReadOnlyList<IUnit> allUnits)
        {
            if (!map.IsValidPosition(x, y)) return false;

            var tile = map.GetTile(x, y);
            if (!TerrainProperties.IsPassable(tile.Terrain, target.Class.MoveType)) return false;

            bool occupied = allUnits.Any(u => u.IsAlive && u.Id != target.Id && u.Position.x == x && u.Position.y == y);
            return !occupied;
        }
    }
}
