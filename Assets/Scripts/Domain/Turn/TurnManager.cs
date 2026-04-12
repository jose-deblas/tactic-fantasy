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

        private List<IUnit> GetPhaseUnits()
        {
            return _allUnits.Where(u => u.Team == (CurrentPhase == Phase.PlayerPhase ? Team.PlayerTeam : Team.EnemyTeam) && u.IsAlive)
                .ToList();
        }
    }
}
