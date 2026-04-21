using System;
using System.Collections.Generic;

namespace TacticFantasy.Domain.Map
{
    public interface IGameMap
    {
        int Width { get; }
        int Height { get; }
        ITile GetTile(int x, int y);
        bool IsValidPosition(int x, int y);
        int GetDistance(int x1, int y1, int x2, int y2);
        Weather CurrentWeather { get; }
        void SetWeather(Weather weather);
    }

    public class GameMap : IGameMap
    {
        private readonly ITile[,] _tiles;
        private readonly Random _random;

        public int Width { get; }
        public int Height { get; }
        public Weather CurrentWeather { get; private set; }

        public void SetWeather(Weather weather)
        {
            CurrentWeather = weather;
        }

        public GameMap(int width, int height, int seed = 0)
        {
            Width = width;
            Height = height;
            _tiles = new ITile[width, height];
            _random = new Random(seed);
            GenerateRandomTerrain();
        }

        public GameMap(int width, int height, ITile[,] tiles)
        {
            Width = width;
            Height = height;
            _tiles = tiles;
            _random = new Random();
        }

        public ITile GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y))
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds.");
            return _tiles[x, y];
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public int GetDistance(int x1, int y1, int x2, int y2)
        {
            // Chebyshev distance (max of absolute differences)
            return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }

        private void GenerateRandomTerrain()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TerrainType terrain = GenerateTileAtPosition(x, y);
                    _tiles[x, y] = new Tile(x, y, terrain);
                }
            }
        }

        private TerrainType GenerateTileAtPosition(int x, int y)
        {
            int roll = _random.Next(100);

            if (roll < 60) return TerrainType.Plain;
            if (roll < 75) return TerrainType.Forest;
            if (roll < 85) return TerrainType.Mountain;
            if (roll < 95) return TerrainType.Fort;
            return TerrainType.Wall;
        }
    }
}
