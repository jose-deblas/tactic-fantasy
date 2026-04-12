namespace TacticFantasy.Domain.Map
{
    public interface ITile
    {
        int X { get; }
        int Y { get; }
        TerrainType Terrain { get; }
    }

    public class Tile : ITile
    {
        public int X { get; }
        public int Y { get; }
        public TerrainType Terrain { get; }

        public Tile(int x, int y, TerrainType terrain)
        {
            X = x;
            Y = y;
            Terrain = terrain;
        }

        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Tile tile && tile.X == X && tile.Y == Y;
        }

        public override string ToString()
        {
            return $"Tile({X}, {Y}, {Terrain})";
        }
    }
}
