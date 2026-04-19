using TacticFantasy.Domain.Items;

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

    public class InteractableTile : ITile
    {
        public int X { get; }
        public int Y { get; }
        public TerrainType Terrain { get; }
        public bool IsOpened { get; private set; }
        public IItem ContainedItem { get; }

        public InteractableTile(int x, int y, TerrainType terrain, IItem containedItem = null)
        {
            X = x;
            Y = y;
            Terrain = terrain;
            IsOpened = false;
            ContainedItem = containedItem;
        }

        public void Open()
        {
            IsOpened = true;
        }

        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is InteractableTile tile && tile.X == X && tile.Y == Y;
        }

        public override string ToString()
        {
            return $"InteractableTile({X}, {Y}, {Terrain}, Opened={IsOpened})";
        }
    }
}
