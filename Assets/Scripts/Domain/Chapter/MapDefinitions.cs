using System.Collections.Generic;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Chapter
{
    public static class MapDefinitions
    {
        /// <summary>
        /// Plains Skirmish (12x10): open field with forests and forts. Rout condition.
        /// </summary>
        public static MapDefinition PlainsSkirmish()
        {
            int w = 12, h = 10;
            var terrain = new TerrainType[w, h];

            // Fill with plains
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    terrain[x, y] = TerrainType.Plain;

            // Forest clusters
            terrain[3, 3] = TerrainType.Forest;
            terrain[3, 4] = TerrainType.Forest;
            terrain[4, 3] = TerrainType.Forest;
            terrain[7, 5] = TerrainType.Forest;
            terrain[7, 6] = TerrainType.Forest;
            terrain[8, 6] = TerrainType.Forest;
            terrain[5, 7] = TerrainType.Forest;
            terrain[6, 7] = TerrainType.Forest;

            // Forts
            terrain[2, 1] = TerrainType.Fort;
            terrain[9, 8] = TerrainType.Fort;

            // Small mountain ridge
            terrain[5, 4] = TerrainType.Mountain;
            terrain[6, 4] = TerrainType.Mountain;
            terrain[6, 5] = TerrainType.Mountain;

            var players = new List<UnitPlacement>
            {
                new UnitPlacement("Ike", "Myrmidon", "Iron Sword", Team.PlayerTeam, (1, 1)),
                new UnitPlacement("Oscar", "Soldier", "Iron Lance", Team.PlayerTeam, (1, 2)),
                new UnitPlacement("Boyd", "Fighter", "Iron Axe", Team.PlayerTeam, (0, 2)),
                new UnitPlacement("Rhys", "Cleric", "Heal Staff", Team.PlayerTeam, (0, 1)),
            };

            var enemies = new List<UnitPlacement>
            {
                new UnitPlacement("Bandit A", "Fighter", "Iron Axe", Team.EnemyTeam, (10, 8), level: 2),
                new UnitPlacement("Bandit B", "Fighter", "Iron Axe", Team.EnemyTeam, (9, 7), level: 2),
                new UnitPlacement("Soldier A", "Soldier", "Iron Lance", Team.EnemyTeam, (10, 7)),
                new UnitPlacement("Archer A", "Archer", "Iron Bow", Team.EnemyTeam, (11, 8)),
            };

            return new MapDefinition("Plains Skirmish", w, h, terrain, players, enemies);
        }

        /// <summary>
        /// Castle Assault (14x14): walls, doors, throne, chests. Seize condition.
        /// </summary>
        public static MapDefinition CastleAssault()
        {
            int w = 14, h = 14;
            var terrain = new TerrainType[w, h];

            // Fill with plains
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    terrain[x, y] = TerrainType.Plain;

            // Castle outer walls (top section, y=8..13)
            for (int x = 3; x <= 10; x++)
            {
                terrain[x, 8] = TerrainType.Wall;
                terrain[x, 13] = TerrainType.Wall;
            }
            for (int y = 8; y <= 13; y++)
            {
                terrain[3, y] = TerrainType.Wall;
                terrain[10, y] = TerrainType.Wall;
            }

            // Castle entrance (door)
            terrain[6, 8] = TerrainType.Door;
            terrain[7, 8] = TerrainType.Door;

            // Inner castle floor
            for (int x = 4; x <= 9; x++)
                for (int y = 9; y <= 12; y++)
                    terrain[x, y] = TerrainType.Plain;

            // Throne at the back
            terrain[7, 12] = TerrainType.Throne;

            // Chests inside castle
            terrain[4, 12] = TerrainType.Chest;
            terrain[9, 12] = TerrainType.Chest;

            // Approach terrain: forests and forts
            terrain[4, 5] = TerrainType.Forest;
            terrain[5, 5] = TerrainType.Forest;
            terrain[8, 5] = TerrainType.Forest;
            terrain[9, 5] = TerrainType.Forest;
            terrain[6, 3] = TerrainType.Fort;
            terrain[7, 3] = TerrainType.Fort;

            var chests = new List<ChestPlacement>
            {
                new ChestPlacement((4, 12), ConsumableFactory.CreateElixir()),
                new ChestPlacement((9, 12), StatBoosterFactory.CreateEnergyDrop()),
            };

            var players = new List<UnitPlacement>
            {
                new UnitPlacement("Ike", "Myrmidon", "Iron Sword", Team.PlayerTeam, (6, 0)),
                new UnitPlacement("Oscar", "Soldier", "Iron Lance", Team.PlayerTeam, (7, 0)),
                new UnitPlacement("Soren", "Mage", "Fire", Team.PlayerTeam, (5, 0)),
                new UnitPlacement("Mist", "Cleric", "Heal Staff", Team.PlayerTeam, (8, 0)),
            };

            var enemies = new List<UnitPlacement>
            {
                new UnitPlacement("Guard A", "Soldier", "Iron Lance", Team.EnemyTeam, (5, 7), level: 3),
                new UnitPlacement("Guard B", "Soldier", "Iron Lance", Team.EnemyTeam, (8, 7), level: 3),
                new UnitPlacement("Archer A", "Archer", "Iron Bow", Team.EnemyTeam, (6, 10)),
                new UnitPlacement("Archer B", "Archer", "Iron Bow", Team.EnemyTeam, (7, 10)),
                new UnitPlacement("Boss", "Swordmaster", "Steel Sword", Team.EnemyTeam, (7, 11), level: 5),
            };

            return new MapDefinition("Castle Assault", w, h, terrain, players, enemies,
                VictoryConditionFactory.Seize(7, 12), chests);
        }

        /// <summary>
        /// Desert Holdout (16x12): desert terrain, bridges. Survive 8 turns.
        /// </summary>
        public static MapDefinition DesertHoldout()
        {
            int w = 16, h = 12;
            var terrain = new TerrainType[w, h];

            // Fill with desert
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    terrain[x, y] = TerrainType.Desert;

            // Player starting area: plains (oasis)
            for (int x = 0; x <= 3; x++)
                for (int y = 0; y <= 3; y++)
                    terrain[x, y] = TerrainType.Plain;

            // Fort in the oasis
            terrain[1, 1] = TerrainType.Fort;
            terrain[2, 2] = TerrainType.Fort;

            // A gap (impassable wall representing cliffs) with bridges
            for (int y = 0; y < h; y++)
                terrain[7, y] = TerrainType.Wall;
            terrain[7, 3] = TerrainType.Bridge;
            terrain[7, 8] = TerrainType.Bridge;

            // Some forest patches
            terrain[5, 2] = TerrainType.Forest;
            terrain[5, 3] = TerrainType.Forest;
            terrain[10, 5] = TerrainType.Forest;
            terrain[10, 6] = TerrainType.Forest;

            var players = new List<UnitPlacement>
            {
                new UnitPlacement("Micaiah", "Mage", "Fire", Team.PlayerTeam, (1, 0)),
                new UnitPlacement("Nolan", "Fighter", "Iron Axe", Team.PlayerTeam, (2, 0)),
                new UnitPlacement("Edward", "Myrmidon", "Iron Sword", Team.PlayerTeam, (0, 1)),
                new UnitPlacement("Laura", "Cleric", "Heal Staff", Team.PlayerTeam, (1, 2)),
            };

            var enemies = new List<UnitPlacement>
            {
                new UnitPlacement("Raider A", "Fighter", "Iron Axe", Team.EnemyTeam, (14, 10), level: 2),
                new UnitPlacement("Raider B", "Fighter", "Iron Axe", Team.EnemyTeam, (15, 10), level: 2),
                new UnitPlacement("Raider C", "Archer", "Iron Bow", Team.EnemyTeam, (14, 9)),
                new UnitPlacement("Raider D", "Soldier", "Iron Lance", Team.EnemyTeam, (12, 8), level: 3),
            };

            return new MapDefinition("Desert Holdout", w, h, terrain, players, enemies,
                VictoryConditionFactory.Survive(8));
        }
    }
}
