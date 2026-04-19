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

        /// <summary>Active victory condition for the current chapter (defaults to Rout).</summary>
        IVictoryCondition VictoryCondition { get; }

        void Initialize(List<IUnit> units);

        /// <summary>
        /// Initialise with a specific victory condition. If null, defaults to Rout.
        /// </summary>
        void Initialize(List<IUnit> units, IVictoryCondition victoryCondition, IGameMap map = null);

        void MarkCurrentUnitAsActed();
        void MarkUnitAsActed(int unitId);
        void AdvancePhase();
        GameState GetGameState();
        void HealFortTiles(IGameMap map);
        bool HasUnitActed(int unitId);
        bool HaveAllPlayerUnitsActed();
        bool CanRefreshTarget(IUnit refresher, IUnit target);
        void RefreshUnit(int targetUnitId);

        /// <summary>
        /// Refreshes all allied units adjacent (4 cardinal directions) to a transformed Heron.
        /// Returns the number of units refreshed. Returns 0 if the Heron is not transformed.
        /// </summary>
        int RefreshCross(IUnit heron, IGameMap map);
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
        private IGameMap _map;

        public Phase CurrentPhase { get; private set; } = Phase.PlayerPhase;
        public int TurnCount { get; private set; } = 0;
        public IReadOnlyList<IUnit> AllUnits => _allUnits.AsReadOnly();
        public IUnit CurrentUnit => _currentUnitIndex < GetPhaseUnits().Count ? GetPhaseUnits()[_currentUnitIndex] : null;
        public bool HasCurrentUnitActed => CurrentUnit != null && _unitsWhoActed.Contains(CurrentUnit.Id);

        /// <inheritdoc/>
        public IVictoryCondition VictoryCondition { get; private set; } = VictoryConditionFactory.Rout();

        public void Initialize(List<IUnit> units)
        {
            Initialize(units, null, null);
        }

        public void Initialize(List<IUnit> units, IVictoryCondition victoryCondition, IGameMap map = null)
        {
            _allUnits = new List<IUnit>(units);
            _currentUnitIndex = 0;
            _unitsWhoActed.Clear();
            CurrentPhase = Phase.PlayerPhase;
            TurnCount = 1;
            VictoryCondition = victoryCondition ?? VictoryConditionFactory.Rout();
            _map = map;
        }

        public void MarkCurrentUnitAsActed()
        {
            if (CurrentUnit != null)
            {
                _unitsWhoActed.Add(CurrentUnit.Id);
            }
        }

        public void MarkUnitAsActed(int unitId)
        {
            _unitsWhoActed.Add(unitId);
        }

        public void AdvancePhase()
        {
            if (CurrentPhase == Phase.PlayerPhase)
            {
                // Tick Laguz gauges for player units at end of player phase
                TickLaguzGauges(Team.PlayerTeam);

                CurrentPhase = Phase.EnemyPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
            }
            else if (CurrentPhase == Phase.EnemyPhase)
            {
                // Tick Laguz gauges for enemy units at end of enemy phase
                TickLaguzGauges(Team.EnemyTeam);

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
            var enemyUnits  = _allUnits.Where(u => u.Team == Team.EnemyTeam).ToList();

            var state = VictoryCondition.Evaluate(playerUnits, enemyUnits, TurnCount, _map);

            return state switch
            {
                VictoryState.PlayerWon  => GameState.PlayerWon,
                VictoryState.PlayerLost => GameState.PlayerLost,
                _                       => GameState.InProgress
            };
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

        public bool HasUnitActed(int unitId)
        {
            return _unitsWhoActed.Contains(unitId);
        }

        public bool HaveAllPlayerUnitsActed()
        {
            var alivePlayerUnits = _allUnits.Where(u => u.Team == Team.PlayerTeam && u.IsAlive && u.CanAct);
            return alivePlayerUnits.All(u => _unitsWhoActed.Contains(u.Id));
        }

        public bool CanRefreshTarget(IUnit refresher, IUnit target)
        {
            // Validation rules:
            // 1. Refresher must have REFRESH weapon type
            if (refresher.EquippedWeapon.Type != Weapons.WeaponType.REFRESH)
                return false;

            // 2. Target must be an ally
            if (target.Team != refresher.Team)
                return false;

            // 3. Target must have already acted
            if (!_unitsWhoActed.Contains(target.Id))
                return false;

            // 4. Target must be alive and able to act
            if (!target.IsAlive || !target.CanAct)
                return false;

            // 5. Cannot refresh self
            if (refresher.Id == target.Id)
                return false;

            return true;
        }

        public void RefreshUnit(int targetUnitId)
        {
            _unitsWhoActed.Remove(targetUnitId);
        }

        public int RefreshCross(IUnit heron, IGameMap map)
        {
            if (!heron.IsTransformed)
                return 0;

            var (hx, hy) = heron.Position;
            var cardinalOffsets = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
            int refreshed = 0;

            foreach (var (dx, dy) in cardinalOffsets)
            {
                int nx = hx + dx;
                int ny = hy + dy;
                if (nx < 0 || ny < 0 || nx >= map.Width || ny >= map.Height)
                    continue;

                var adjacent = _allUnits.FirstOrDefault(u =>
                    u.IsAlive && u.Position == (nx, ny) && u.Team == heron.Team
                    && u.Id != heron.Id && _unitsWhoActed.Contains(u.Id));

                if (adjacent != null)
                {
                    RefreshUnit(adjacent.Id);
                    refreshed++;
                }
            }

            return refreshed;
        }

        /// <summary>Ticks transform gauges for all alive Laguz units on the given team.</summary>
        private void TickLaguzGauges(Team team)
        {
            foreach (var unit in _allUnits.Where(u => u.IsAlive && u.Team == team && u.IsLaguz))
            {
                unit.TickTransformGauge();
            }
        }

        private List<IUnit> GetPhaseUnits()
        {
            return _allUnits.Where(u => u.Team == (CurrentPhase == Phase.PlayerPhase ? Team.PlayerTeam : Team.EnemyTeam) && u.IsAlive)
                .ToList();
        }
    }
}
