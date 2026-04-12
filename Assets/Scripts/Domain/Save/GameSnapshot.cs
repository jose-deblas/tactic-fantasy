using System.Collections.Generic;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// Immutable snapshot of the full game state at a point in time.
    /// Pure domain object — no Unity dependencies.
    /// </summary>
    public class GameSnapshot
    {
        public Phase CurrentPhase { get; }
        public int TurnCount { get; }
        public IReadOnlyList<UnitSnapshot> Units { get; }

        private GameSnapshot(Phase phase, int turnCount, List<UnitSnapshot> units)
        {
            CurrentPhase = phase;
            TurnCount    = turnCount;
            Units        = units.AsReadOnly();
        }

        /// <summary>Creates a snapshot from the current ITurnManager state.</summary>
        public static GameSnapshot Capture(ITurnManager tm)
        {
            var unitSnaps = new List<UnitSnapshot>();
            foreach (var unit in tm.AllUnits)
                unitSnaps.Add(UnitSnapshot.Capture(unit));

            return new GameSnapshot(tm.CurrentPhase, tm.TurnCount, unitSnaps);
        }
    }
}
