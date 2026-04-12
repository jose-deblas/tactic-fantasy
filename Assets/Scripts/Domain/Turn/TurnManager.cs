using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Map;

namespace TacticFantasy.Domain.Turn
{
    public interface ITurnManager
    {
        Phase CurrentPhase { get; }
        int TurnCount { get; }
        IReadOnlyList<IUnit> AllUnits { get; }
        IUnit CurrentUnit { get; }
        bool HasCurrentUnitActed { get; }

        void Initialize(List<IUnit> units);
        void MarkCurrentUnitAsActed();
        void AdvancePhase();
        GameState GetGameState();
        void HealFortTiles(IGameMap map);
    }

    public enum GameState
    {
        InProgress,
        PlayerWon,
        PlayerLost
    }

    public class TurnManager : ITurnManager
    {
        private List<IUnit> _allUnits = new List<IUnit>();
        private int _currentUnitIndex = 0;
        private HashSet<int> _unitsWhoActed = new HashSet<int>();

        public Phase CurrentPhase { get; private set; } = Phase.PlayerPhase;
        public int TurnCount { get; private set; } = 0;
        public IReadOnlyList<IUnit> AllUnits => _allUnits.AsReadOnly();
        public IUnit CurrentUnit => _currentUnitIndex < GetPhaseUnits().Count ? GetPhaseUnits()[_currentUnitIndex] : null;
        public bool HasCurrentUnitActed => CurrentUnit != null && _unitsWhoActed.Contains(CurrentUnit.Id);

        public void Initialize(List<IUnit> units)
        {
            _allUnits = new List<IUnit>(units);
            _currentUnitIndex = 0;
            _unitsWhoActed.Clear();
            CurrentPhase = Phase.PlayerPhase;
            TurnCount = 1;
        }

        public void MarkCurrentUnitAsActed()
        {
            if (CurrentUnit != null)
            {
                _unitsWhoActed.Add(CurrentUnit.Id);
            }
        }

        public void AdvancePhase()
        {
            if (CurrentPhase == Phase.PlayerPhase)
            {
                CurrentPhase = Phase.EnemyPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
            }
            else if (CurrentPhase == Phase.EnemyPhase)
            {
                // Tick status effects at end of enemy phase (= end of full turn)
                foreach (var unit in _allUnits.Where(u => u.IsAlive))
                    unit.TickStatus();

                CurrentPhase = Phase.PlayerPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
                TurnCount++;
            }

            if (GetGameState() != GameState.InProgress)
            {
                CurrentPhase = Phase.GameOver;
            }
        }

        public GameState GetGameState()
        {
            var playerUnits = _allUnits.Where(u => u.Team == Team.PlayerTeam).ToList();
            var enemyUnits = _allUnits.Where(u => u.Team == Team.EnemyTeam).ToList();

            bool allPlayersAlive = playerUnits.All(u => u.IsAlive);
            bool allEnemiesAlive = enemyUnits.All(u => u.IsAlive);

            if (!playerUnits.Any(u => u.IsAlive))
                return GameState.PlayerLost;

            if (!enemyUnits.Any(u => u.IsAlive))
                return GameState.PlayerWon;

            return GameState.InProgress;
        }

        public void HealFortTiles(IGameMap map)
        {
            foreach (var unit in _allUnits.Where(u => u.IsAlive))
            {
                var tile = map.GetTile(unit.Position.x, unit.Position.y);
                int healPercent = Domain.Map.TerrainProperties.GetHealPercent(tile.Terrain);
                if (healPercent > 0)
                {
                    int healAmount = (int)(unit.MaxHP * healPercent / 100f);
                    unit.Heal(healAmount);
                }
            }
        }

        private List<IUnit> GetPhaseUnits()
        {
            return _allUnits.Where(u => u.Team == (CurrentPhase == Phase.PlayerPhase ? Team.PlayerTeam : Team.EnemyTeam) && u.IsAlive)
                .ToList();
        }
    }
}
