using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public interface IPathFinder
    {
        List<(int, int)> FindPath(int startX, int startY, int targetX, int targetY, int maxMovement, IUnit unit, IGameMap map);
        HashSet<(int, int)> GetMovementRange(int startX, int startY, int maxMovement, IUnit unit, IGameMap map);
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

        public List<(int, int)> FindPath(int startX, int startY, int targetX, int targetY, int maxMovement, IUnit unit, IGameMap map)
        {
            if (!map.IsValidPosition(targetX, targetY))
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

                foreach (var neighbor in GetNeighbors(currentNode.X, currentNode.Y, map, unit))
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

        public HashSet<(int, int)> GetMovementRange(int startX, int startY, int maxMovement, IUnit unit, IGameMap map)
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

                    foreach (var neighbor in GetNeighbors(x, y, map, unit))
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

            return reachable;
        }

        private List<(int x, int y)> GetNeighbors(int x, int y, IGameMap map, IUnit unit)
        {
            var neighbors = new List<(int, int)>();
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 }, new int[] { 1, 0 },
                new int[] { 0, -1 }, new int[] { -1, 0 },
                new int[] { 1, 1 }, new int[] { 1, -1 },
                new int[] { -1, 1 }, new int[] { -1, -1 }
            };

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (map.IsValidPosition(nx, ny))
                {
                    var tile = map.GetTile(nx, ny);
                    bool isInfantry = unit.Class.MoveType == Units.MoveType.Infantry;
                    if (TerrainProperties.IsPassable(tile.Terrain, isInfantry))
                    {
                        neighbors.Add((nx, ny));
                    }
                }
            }

            return neighbors;
        }

        private int GetMovementCost(int fromX, int fromY, int toX, int toY, IGameMap map, IUnit unit)
        {
            var tile = map.GetTile(toX, toY);
            bool isInfantry = unit.Class.MoveType == Units.MoveType.Infantry;
            return TerrainProperties.GetMovementCost(tile.Terrain, isInfantry);
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
