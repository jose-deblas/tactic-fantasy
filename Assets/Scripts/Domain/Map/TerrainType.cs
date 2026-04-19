using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public enum TerrainType
    {
        Plain,
        Forest,
        Fort,
        Mountain,
        Wall,
        Door,
        Chest,
        Throne,
        Desert,
        Bridge
    }

    public static class TerrainProperties
    {
        public static int GetMovementCost(TerrainType terrain, MoveType moveType, bool isMage = false)
        {
            if (moveType == MoveType.Flying)
            {
                return terrain switch
                {
                    TerrainType.Wall => int.MaxValue,
                    TerrainType.Door => int.MaxValue,
                    _ => 1
                };
            }

            return terrain switch
            {
                TerrainType.Plain => 1,
                TerrainType.Forest => 2,
                TerrainType.Fort => 1,
                TerrainType.Mountain => moveType == MoveType.Infantry || moveType == MoveType.Armored ? 3 : int.MaxValue,
                TerrainType.Wall => int.MaxValue,
                TerrainType.Door => int.MaxValue,
                TerrainType.Chest => 1,
                TerrainType.Throne => 1,
                TerrainType.Desert => isMage ? 1 : (moveType == MoveType.Cavalry ? 4 : 3),
                TerrainType.Bridge => 1,
                _ => int.MaxValue
            };
        }

        public static bool IsPassable(TerrainType terrain, MoveType moveType, bool isMage = false)
        {
            int cost = GetMovementCost(terrain, moveType, isMage);
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
                TerrainType.Throne => 3,
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
                TerrainType.Throne => 30,
                TerrainType.Desert => 5,
                _ => 0
            };
        }

        public static int GetHealPercent(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Fort => 20,
                TerrainType.Throne => 30,
                _ => 0
            };
        }
    }
}
