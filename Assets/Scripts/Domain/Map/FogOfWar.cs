using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public interface IFogOfWar
    {
        bool IsTileVisible(int x, int y, Team team);
        void RecalculateVision(IReadOnlyList<IUnit> units, IGameMap map);
        HashSet<(int, int)> GetVisibleTiles(Team team);
    }

    public class FogOfWar : IFogOfWar
    {
        private readonly Dictionary<Team, HashSet<(int, int)>> _visibleTiles = new Dictionary<Team, HashSet<(int, int)>>();

        private const int BASE_VISION_BONUS = 2;
        private const int TORCH_VISION_BONUS = 5;
        private const string TORCH_ITEM_NAME = "Torch";

        public bool IsTileVisible(int x, int y, Team team)
        {
            if (!_visibleTiles.ContainsKey(team))
                return false;
            return _visibleTiles[team].Contains((x, y));
        }

        public void RecalculateVision(IReadOnlyList<IUnit> units, IGameMap map)
        {
            _visibleTiles.Clear();

            foreach (var unit in units.Where(u => u.IsAlive))
            {
                if (!_visibleTiles.ContainsKey(unit.Team))
                    _visibleTiles[unit.Team] = new HashSet<(int, int)>();

                int visionRadius = unit.CurrentStats.MOV + BASE_VISION_BONUS;

                if (HasTorch(unit))
                    visionRadius += TORCH_VISION_BONUS;

                bool isFlying = unit.Class.MoveType == MoveType.Flying;
                var unitVision = CalculateVision(unit.Position.x, unit.Position.y, visionRadius, map, isFlying);

                foreach (var tile in unitVision)
                    _visibleTiles[unit.Team].Add(tile);
            }
        }

        public HashSet<(int, int)> GetVisibleTiles(Team team)
        {
            if (!_visibleTiles.ContainsKey(team))
                return new HashSet<(int, int)>();
            return new HashSet<(int, int)>(_visibleTiles[team]);
        }

        private HashSet<(int, int)> CalculateVision(int originX, int originY, int radius, IGameMap map, bool isFlying)
        {
            var visible = new HashSet<(int, int)>();
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int x, int y, int cost)>();

            queue.Enqueue((originX, originY, 0));

            while (queue.Count > 0)
            {
                var (x, y, cost) = queue.Dequeue();

                if (visited.Contains((x, y)))
                    continue;

                visited.Add((x, y));

                if (cost > radius)
                    continue;

                visible.Add((x, y));

                // Forest blocks vision propagation for non-flying units
                bool blocksVision = !isFlying && map.GetTile(x, y).Terrain == TerrainType.Forest && (x, y) != (originX, originY);

                if (blocksVision)
                    continue;

                // Expand to 8-directional neighbors
                int[][] directions = new int[][]
                {
                    new[] { 0, 1 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { -1, 0 },
                    new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 }
                };

                foreach (var dir in directions)
                {
                    int nx = x + dir[0];
                    int ny = y + dir[1];
                    if (map.IsValidPosition(nx, ny) && !visited.Contains((nx, ny)))
                    {
                        queue.Enqueue((nx, ny, cost + 1));
                    }
                }
            }

            return visible;
        }

        private static bool HasTorch(IUnit unit)
        {
            if (unit.Inventory == null)
                return false;
            return unit.Inventory.GetAll().Any(item => item.Name == TORCH_ITEM_NAME);
        }
    }
}
