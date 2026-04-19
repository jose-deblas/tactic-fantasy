using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Map
{
    public interface IPathFinder
    {
        List<(int, int)> FindPath(int startX, int startY, int targetX, int targetY, int maxMovement, IUnit unit, IGameMap map, IReadOnlyList<IUnit> allUnits = null);
        HashSet<(int, int)> GetMovementRange(int startX, int startY, int maxMovement, IUnit unit, IGameMap map, IReadOnlyList<IUnit> allUnits = null);
    }

    public class PathFinder : IPathFinder
    {
        private class Node
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int GCost { get; set; }
            public int HCost { get; set; }
            public int FCost => GCost + HCost;
            public Node Parent { get; set; }

            public Node(int x, int y, int gCost, int hCost, Node parent = null)
            {
                X = x;
                Y = y;
                GCost = gCost;
                HCost = hCost;
                Parent = parent;
            }
        }

        public List<(int, int)> FindPath(int startX, int startY, int targetX, int targetY, int maxMovement, IUnit unit, IGameMap map, IReadOnlyList<IUnit> allUnits = null)
        {
            if (!map.IsValidPosition(targetX, targetY))
                return new List<(int, int)>();

            if (allUnits != null && IsOccupiedByOther(targetX, targetY, unit, allUnits))
                return new List<(int, int)>();

            var openSet = new List<Node>();
            var closedSet = new HashSet<(int, int)>();

            var startNode = new Node(startX, startY, 0, Heuristic(startX, startY, targetX, targetY));
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                int current = 0;
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < openSet[current].FCost)
                        current = i;
                }

                Node currentNode = openSet[current];

                if (currentNode.X == targetX && currentNode.Y == targetY)
                {
                    return ReconstructPath(currentNode);
                }

                openSet.RemoveAt(current);
                closedSet.Add((currentNode.X, currentNode.Y));

                foreach (var neighbor in GetNeighbors(currentNode.X, currentNode.Y, map, unit, allUnits))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    int moveCost = GetMovementCost(currentNode.X, currentNode.Y, neighbor.x, neighbor.y, map, unit);
                    int newGCost = currentNode.GCost + moveCost;

                    if (newGCost > maxMovement)
                        continue;

                    var existingNode = openSet.FirstOrDefault(n => n.X == neighbor.x && n.Y == neighbor.y);
                    if (existingNode != null)
                    {
                        if (newGCost < existingNode.GCost)
                        {
                            existingNode.GCost = newGCost;
                            existingNode.Parent = currentNode;
                        }
                    }
                    else
                    {
                        int hCost = Heuristic(neighbor.x, neighbor.y, targetX, targetY);
                        var newNode = new Node(neighbor.x, neighbor.y, newGCost, hCost, currentNode);
                        openSet.Add(newNode);
                    }
                }
            }

            return new List<(int, int)>();
        }

        public HashSet<(int, int)> GetMovementRange(int startX, int startY, int maxMovement, IUnit unit, IGameMap map, IReadOnlyList<IUnit> allUnits = null)
        {
            var reachable = new HashSet<(int, int)>();
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int x, int y, int cost)>();

            queue.Enqueue((startX, startY, 0));

            while (queue.Count > 0)
            {
                var (x, y, cost) = queue.Dequeue();

                if (visited.Contains((x, y)))
                    continue;

                visited.Add((x, y));

                if (cost <= maxMovement)
                {
                    reachable.Add((x, y));

                    foreach (var neighbor in GetNeighbors(x, y, map, unit, allUnits))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            int moveCost = GetMovementCost(x, y, neighbor.x, neighbor.y, map, unit);
                            if (cost + moveCost <= maxMovement)
                            {
                                queue.Enqueue((neighbor.x, neighbor.y, cost + moveCost));
                            }
                        }
                    }
                }
            }

            if (allUnits != null)
            {
                foreach (var other in allUnits)
                {
                    if (other.Id != unit.Id && other.IsAlive)
                        reachable.Remove((other.Position.x, other.Position.y));
                }
            }

            return reachable;
        }

        private List<(int x, int y)> GetNeighbors(int x, int y, IGameMap map, IUnit unit, IReadOnlyList<IUnit> allUnits = null)
        {
            var neighbors = new List<(int, int)>();
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 }, new int[] { 1, 0 },
                new int[] { 0, -1 }, new int[] { -1, 0 },
                new int[] { 1, 1 }, new int[] { 1, -1 },
                new int[] { -1, 1 }, new int[] { -1, -1 }
            };

            bool isMage = IsMagicUser(unit);

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (map.IsValidPosition(nx, ny))
                {
                    var tile = map.GetTile(nx, ny);

                    // Closed doors are impassable
                    if (tile is InteractableTile it && !it.IsOpened && tile.Terrain == TerrainType.Door)
                    {
                        continue;
                    }

                    if (TerrainProperties.IsPassable(tile.Terrain, unit.Class.MoveType, isMage))
                    {
                        // Opened doors use plain movement cost
                        if (allUnits != null && IsOccupiedByEnemy(nx, ny, unit, allUnits))
                            continue;

                        neighbors.Add((nx, ny));
                    }
                }
            }

            return neighbors;
        }

        private static bool IsMagicUser(IUnit unit)
        {
            return unit.Class.UsableWeaponTypes.Contains(WeaponType.FIRE);
        }

        private static bool IsOccupiedByEnemy(int x, int y, IUnit mover, IReadOnlyList<IUnit> allUnits)
        {
            foreach (var other in allUnits)
            {
                if (other.Id != mover.Id && other.IsAlive && other.Position.x == x && other.Position.y == y && other.Team != mover.Team)
                    return true;
            }
            return false;
        }

        private static bool IsOccupiedByOther(int x, int y, IUnit mover, IReadOnlyList<IUnit> allUnits)
        {
            foreach (var other in allUnits)
            {
                if (other.Id != mover.Id && other.IsAlive && other.Position.x == x && other.Position.y == y)
                    return true;
            }
            return false;
        }

        private int GetMovementCost(int fromX, int fromY, int toX, int toY, IGameMap map, IUnit unit)
        {
            var tile = map.GetTile(toX, toY);

            // Opened doors use plain movement cost (1)
            if (tile is InteractableTile it && it.IsOpened &&
                (tile.Terrain == TerrainType.Door || tile.Terrain == TerrainType.Chest))
            {
                return 1;
            }

            return TerrainProperties.GetMovementCost(tile.Terrain, unit.Class.MoveType, IsMagicUser(unit));
        }

        private int Heuristic(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }

        private List<(int, int)> ReconstructPath(Node node)
        {
            var path = new List<(int, int)>();
            while (node != null)
            {
                path.Insert(0, (node.X, node.Y));
                node = node.Parent;
            }
            return path;
        }
    }
}
