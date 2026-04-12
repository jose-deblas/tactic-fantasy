using TacticFantasy.Domain.Turn;

namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// Application-layer service that orchestrates save and load operations.
    /// Depends on the IGameRepository port; fully testable without Unity.
    /// </summary>
    public class GameSaveService
    {
        private readonly IGameRepository _repository;

        public GameSaveService(IGameRepository repository)
        {
            _repository = repository;
        }

        /// <summary>Captures the current game state and persists it.</summary>
        public void Save(ITurnManager turnManager)
        {
            var snapshot = GameSnapshot.Capture(turnManager);
            _repository.Save(snapshot);
        }

        /// <summary>
        /// Loads the most recent snapshot.
        /// Returns null if no save data exists.
        /// </summary>
        public GameSnapshot Load()
        {
            return _repository.HasSave ? _repository.Load() : null;
        }
    }
}
