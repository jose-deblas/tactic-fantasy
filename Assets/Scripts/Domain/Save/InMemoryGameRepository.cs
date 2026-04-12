using TacticFantasy.Domain.Save;

namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// In-memory implementation of IGameRepository.
    /// Used in tests and as a reference for production adapters.
    /// </summary>
    public class InMemoryGameRepository : IGameRepository
    {
        private GameSnapshot _saved;

        public bool HasSave => _saved != null;

        public void Save(GameSnapshot snapshot)
        {
            _saved = snapshot;
        }

        public GameSnapshot Load()
        {
            return _saved;
        }
    }
}
