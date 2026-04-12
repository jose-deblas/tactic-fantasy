using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Turn
{
    /// <summary>
    /// The possible outcomes that a VictoryCondition can return after evaluation.
    /// </summary>
    public enum VictoryState
    {
        InProgress,
        PlayerWon,
        PlayerLost
    }

    /// <summary>
    /// Pure domain interface representing one evaluable win/lose condition for a chapter.
    /// No Unity or infrastructure dependencies.
    /// </summary>
    public interface IVictoryCondition
    {
        /// <summary>Human-readable description shown to the player, e.g. "Rout all enemies".</summary>
        string Description { get; }

        /// <summary>
        /// Evaluates the current state of the battle.
        /// Called at the end of each phase after mutations (deaths, moves) have been applied.
        /// </summary>
        VictoryState Evaluate(
            IReadOnlyList<IUnit> playerUnits,
            IReadOnlyList<IUnit> enemyUnits,
            int turnCount,
            IGameMap map);
    }

    /// <summary>
    /// Factory methods for the built-in victory condition types.
    /// Keeps instantiation centralised and the concrete types private/internal.
    /// </summary>
    public static class VictoryConditionFactory
    {
        /// <summary>Win by defeating every enemy unit.</summary>
        public static IVictoryCondition Rout() => new RoutCondition();

        /// <summary>Win by having at least one alive player unit on the specified tile.</summary>
        public static IVictoryCondition Seize(int x, int y) => new SeizeCondition(x, y);

        /// <summary>Win by surviving the specified number of turns (or by routing the enemy).</summary>
        public static IVictoryCondition Survive(int turnsToSurvive) => new SurviveCondition(turnsToSurvive);
    }

    // ── Concrete conditions ──────────────────────────────────────────────────

    /// <summary>Classic "Rout" condition: defeat all enemies.</summary>
    internal sealed class RoutCondition : IVictoryCondition
    {
        public string Description => "Rout all Enemy units.";

        public VictoryState Evaluate(
            IReadOnlyList<IUnit> playerUnits,
            IReadOnlyList<IUnit> enemyUnits,
            int turnCount,
            IGameMap map)
        {
            if (!playerUnits.Any(u => u.IsAlive))
                return VictoryState.PlayerLost;

            if (!enemyUnits.Any(u => u.IsAlive))
                return VictoryState.PlayerWon;

            return VictoryState.InProgress;
        }
    }

    /// <summary>
    /// "Seize" condition: an alive player unit must stand on the designated castle/throne tile.
    /// The player also loses if all their units die.
    /// </summary>
    internal sealed class SeizeCondition : IVictoryCondition
    {
        private readonly int _targetX;
        private readonly int _targetY;

        public SeizeCondition(int x, int y)
        {
            _targetX = x;
            _targetY = y;
        }

        public string Description => $"Seize the objective at ({_targetX}, {_targetY}).";

        public VictoryState Evaluate(
            IReadOnlyList<IUnit> playerUnits,
            IReadOnlyList<IUnit> enemyUnits,
            int turnCount,
            IGameMap map)
        {
            if (!playerUnits.Any(u => u.IsAlive))
                return VictoryState.PlayerLost;

            bool seized = playerUnits.Any(u =>
                u.IsAlive &&
                u.Position.x == _targetX &&
                u.Position.y == _targetY);

            return seized ? VictoryState.PlayerWon : VictoryState.InProgress;
        }
    }

    /// <summary>
    /// "Survive" condition: keep at least one unit alive until the specified turn.
    /// Routing the enemy also counts as a win.
    /// </summary>
    internal sealed class SurviveCondition : IVictoryCondition
    {
        private readonly int _turnsToSurvive;

        public SurviveCondition(int turnsToSurvive)
        {
            _turnsToSurvive = turnsToSurvive;
        }

        public string Description => $"Survive for {_turnsToSurvive} turns.";

        public VictoryState Evaluate(
            IReadOnlyList<IUnit> playerUnits,
            IReadOnlyList<IUnit> enemyUnits,
            int turnCount,
            IGameMap map)
        {
            if (!playerUnits.Any(u => u.IsAlive))
                return VictoryState.PlayerLost;

            // Routing enemies in a survive map is always a valid win
            if (!enemyUnits.Any(u => u.IsAlive))
                return VictoryState.PlayerWon;

            if (turnCount >= _turnsToSurvive)
                return VictoryState.PlayerWon;

            return VictoryState.InProgress;
        }
    }
}
