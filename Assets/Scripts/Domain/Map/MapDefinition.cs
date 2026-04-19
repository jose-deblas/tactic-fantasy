using System.Collections.Generic;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public class UnitPlacement
    {
        public string Name { get; }
        public string ClassName { get; }
        public string WeaponName { get; }
        public Team Team { get; }
        public (int x, int y) Position { get; }
        public int Level { get; }

        public UnitPlacement(string name, string className, string weaponName, Team team, (int, int) position, int level = 1)
        {
            Name = name;
            ClassName = className;
            WeaponName = weaponName;
            Team = team;
            Position = position;
            Level = level;
        }
    }

    public class ChestPlacement
    {
        public (int x, int y) Position { get; }
        public IItem Item { get; }

        public ChestPlacement((int, int) position, IItem item)
        {
            Position = position;
            Item = item;
        }
    }

    public class MapDefinition
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public TerrainType[,] Terrain { get; }
        public List<UnitPlacement> PlayerPlacements { get; }
        public List<UnitPlacement> EnemyPlacements { get; }
        public List<ChestPlacement> Chests { get; }
        public IVictoryCondition VictoryCondition { get; }

        public MapDefinition(
            string name,
            int width,
            int height,
            TerrainType[,] terrain,
            List<UnitPlacement> playerPlacements,
            List<UnitPlacement> enemyPlacements,
            IVictoryCondition victoryCondition = null,
            List<ChestPlacement> chests = null)
        {
            Name = name;
            Width = width;
            Height = height;
            Terrain = terrain;
            PlayerPlacements = playerPlacements;
            EnemyPlacements = enemyPlacements;
            VictoryCondition = victoryCondition ?? VictoryConditionFactory.Rout();
            Chests = chests ?? new List<ChestPlacement>();
        }
    }
}
