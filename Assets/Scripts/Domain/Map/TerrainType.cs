namespace TacticFantasy.Domain.Map
{
    public enum TerrainType
    {
        Plain,
        Forest,
        Fort,
        Mountain,
        Wall
    }

    public static class TerrainProperties
    {
        public static int GetMovementCost(TerrainType terrain, bool isInfantry)
        {
            return terrain switch
            {
                TerrainType.Plain => 1,
                TerrainType.Forest => 2,
                TerrainType.Fort => 1,
                TerrainType.Mountain => isInfantry ? 3 : int.MaxValue, // Cavalry can't pass
                TerrainType.Wall => int.MaxValue, // Impassable
                _ => int.MaxValue
            };
        }

        public static bool IsPassable(TerrainType terrain, bool isInfantry)
        {
            int cost = GetMovementCost(terrain, isInfantry);
            return cost != int.MaxValue;
        }

        public static int GetDefenseBonus(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Plain => 0,
                TerrainType.Forest => 1,
                TerrainType.Fort => 2,
                TerrainType.Mountain => 1,
                TerrainType.Wall => 0,
                _ => 0
            };
        }

        public static int GetAvoidBonus(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Plain => 0,
                TerrainType.Forest => 15,
                TerrainType.Fort => 20,
                TerrainType.Mountain => 20,
                TerrainType.Wall => 0,
                _ => 0
            };
        }

        public static int GetHealPercent(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Fort => 20,
                _ => 0
            };
        }
    }
}
