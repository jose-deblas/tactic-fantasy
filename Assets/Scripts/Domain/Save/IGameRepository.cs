namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// Port (domain interface) for persisting and loading game snapshots.
    /// Concrete adapters (PlayerPrefs, file, cloud) implement this interface.
    /// </summary>
    public interface IGameRepository
    {
        void Save(GameSnapshot snapshot);
        GameSnapshot Load();   // null if no save exists
        bool HasSave { get; }
    }
}
