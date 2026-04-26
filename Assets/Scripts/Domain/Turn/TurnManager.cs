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
        int DefaultActionsPerUnit { get; }
        int GetRemainingActions(int unitId);
        bool ConsumeAction(int unitId);
        void GrantActions(int unitId, int amount);
        bool HasUnitAttacked(int unitId);
        void MarkUnitAsAttacked(int unitId);

        // Movement/move-action tracking
        int GetMovementPointsRemaining(int unitId);
        bool TryUseMovement(int unitId, int movementCost);
        bool HasUnitMoved(int unitId);

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
        private HashSet<int> _unitsWhoAttacked = new HashSet<int>();
        private Dictionary<int,int> _actionsRemaining = new Dictionary<int,int>();
        // Movement points remaining for the unit's single movement action this turn.
        private Dictionary<int,int> _movementRemaining = new Dictionary<int,int>();
        private IGameMap _map;
        private IReinforcementService _reinforcementService;
        private List<ReinforcementTrigger> _reinforcementTriggers = new List<ReinforcementTrigger>();

        public int DefaultActionsPerUnit { get; } = 2;

        public Phase CurrentPhase { get; private set; } = Phase.PlayerPhase;
        public int TurnCount { get; private set; } = 0;
        public IReadOnlyList<IUnit> AllUnits => _allUnits.AsReadOnly();
        public IUnit CurrentUnit => _currentUnitIndex < GetPhaseUnits().Count ? GetPhaseUnits()[_currentUnitIndex] : null;
        public bool HasCurrentUnitActed => CurrentUnit != null && HasUnitActed(CurrentUnit.Id);

        /// <inheritdoc/>
        public IVictoryCondition VictoryCondition { get; private set; } = VictoryConditionFactory.Rout();

        public void Initialize(List<IUnit> units)
        {
            Initialize(units, null, null);
        }

        public void Initialize(List<IUnit> units, IVictoryCondition victoryCondition, IGameMap map = null)
        {
            Initialize(units, victoryCondition, map, null, null);
        }

        public void Initialize(List<IUnit> units, IVictoryCondition victoryCondition, IGameMap map,
            IReinforcementService reinforcementService, List<ReinforcementTrigger> triggers)
        {
            _allUnits = new List<IUnit>(units);
            _currentUnitIndex = 0;
            _unitsWhoActed.Clear();
            _unitsWhoAttacked.Clear();
            _actionsRemaining = new Dictionary<int,int>();
            _movementRemaining = new Dictionary<int,int>();
            foreach (var u in _allUnits)
            {
                _actionsRemaining[u.Id] = DefaultActionsPerUnit;
                _movementRemaining[u.Id] = u.CurrentStats.MOV;
            }
            CurrentPhase = Phase.PlayerPhase;
            TurnCount = 1;
            VictoryCondition = victoryCondition ?? VictoryConditionFactory.Rout();
            _map = map;
            _reinforcementService = reinforcementService;
            _reinforcementTriggers = triggers ?? new List<ReinforcementTrigger>();
        }

        public void MarkCurrentUnitAsActed()
        {
            if (CurrentUnit != null)
            {
                _unitsWhoActed.Add(CurrentUnit.Id);
                _actionsRemaining[CurrentUnit.Id] = 0;
                if (_movementRemaining.ContainsKey(CurrentUnit.Id))
                    _movementRemaining[CurrentUnit.Id] = 0;
            }
        }

        public void MarkUnitAsActed(int unitId)
        {
            _unitsWhoActed.Add(unitId);
            _actionsRemaining[unitId] = 0;
            if (_movementRemaining.ContainsKey(unitId))
                _movementRemaining[unitId] = 0;
        }

        public void AdvancePhase()
        {
            if (CurrentPhase == Phase.PlayerPhase)
            {
                // Tick Laguz gauges for player units at end of player phase
                TickLaguzGauges(Team.PlayerTeam);

                // Clear guard flags for ally NPC units (their phase is about to start)
                ClearGuardFlags(Team.AllyNPC);

                CurrentPhase = Phase.AllyPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
                // Reset actions for ally NPC units
                foreach (var unit in _allUnits.Where(u => u.Team == Team.AllyNPC && u.IsAlive && u.CanAct))
                {
                    _actionsRemaining[unit.Id] = DefaultActionsPerUnit;
                    _movementRemaining[unit.Id] = unit.CurrentStats.MOV;
                    _unitsWhoAttacked.Remove(unit.Id);
                }
            }
            else if (CurrentPhase == Phase.AllyPhase)
            {
                // Tick Laguz gauges for ally NPC units at end of ally phase
                TickLaguzGauges(Team.AllyNPC);

                // Clear guard flags for enemy units (their phase is about to start)
                ClearGuardFlags(Team.EnemyTeam);

                CurrentPhase = Phase.EnemyPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
                // Reset actions for enemy units
                foreach (var unit in _allUnits.Where(u => u.Team == Team.EnemyTeam && u.IsAlive && u.CanAct))
                {
                    _actionsRemaining[unit.Id] = DefaultActionsPerUnit;
                    _movementRemaining[unit.Id] = unit.CurrentStats.MOV;
                    _unitsWhoAttacked.Remove(unit.Id);
                }
            }
            else if (CurrentPhase == Phase.EnemyPhase)
            {
                // Tick Laguz gauges for enemy units at end of enemy phase
                TickLaguzGauges(Team.EnemyTeam);

                // Tick status effects at end of enemy phase (= end of full turn)
                foreach (var unit in _allUnits.Where(u => u.IsAlive))
                    unit.TickStatus();

                // Clear guard flags for player units (their phase is about to start)
                ClearGuardFlags(Team.PlayerTeam);

                CurrentPhase = Phase.PlayerPhase;
                _unitsWhoActed.Clear();
                _currentUnitIndex = 0;
                // Reset actions for player units
                foreach (var unit in _allUnits.Where(u => u.Team == Team.PlayerTeam && u.IsAlive && u.CanAct))
                {
                    _actionsRemaining[unit.Id] = DefaultActionsPerUnit;
                    _movementRemaining[unit.Id] = unit.CurrentStats.MOV;
                    _unitsWhoAttacked.Remove(unit.Id);
                }
                TurnCount++;

                // Check reinforcements at the start of each new turn
                CheckReinforcements();
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
            if (_unitsWhoActed.Contains(unitId)) return true;
            if (_actionsRemaining.TryGetValue(unitId, out var rem)) return rem <= 0;
            return false;
        }

        public bool HaveAllPlayerUnitsActed()
        {
            var alivePlayerUnits = _allUnits.Where(u => u.Team == Team.PlayerTeam && u.IsAlive && u.CanAct);
            return alivePlayerUnits.All(u => HasUnitActed(u.Id));
        }

        public bool HaveAllAllyUnitsActed()
        {
            var aliveAllyUnits = _allUnits.Where(u => u.Team == Team.AllyNPC && u.IsAlive && u.CanAct);
            return aliveAllyUnits.All(u => HasUnitActed(u.Id));
        }

        public int GetRemainingActions(int unitId)
        {
            return _actionsRemaining.TryGetValue(unitId, out var rem) ? rem : 0;
        }

        public bool ConsumeAction(int unitId)
        {
            if (!_actionsRemaining.TryGetValue(unitId, out var rem))
                return false;
            if (rem <= 0)
                return false;
            _actionsRemaining[unitId] = rem - 1;
            if (_actionsRemaining[unitId] <= 0)
                _unitsWhoActed.Add(unitId);
            return true;
        }

        public int GetMovementPointsRemaining(int unitId)
        {
            if (!_movementRemaining.TryGetValue(unitId, out var rem))
            {
                var unit = _allUnits.FirstOrDefault(u => u.Id == unitId);
                return unit?.CurrentStats.MOV ?? 0;
            }
            return rem;
        }

        /// <summary>
        /// Attempts to use <paramref name="movementCost"/> movement points from the unit's single movement action.
        /// On the first movement usage the unit will also consume 1 action (ConsumeAction).
        /// Returns true on success; false if not enough movement points or not enough actions.
        /// </summary>
        public bool TryUseMovement(int unitId, int movementCost)
        {
            var unit = _allUnits.FirstOrDefault(u => u.Id == unitId);
            int maxMovement = unit?.CurrentStats.MOV ?? 0;
            if (!_movementRemaining.ContainsKey(unitId))
                _movementRemaining[unitId] = maxMovement;
            int rem = _movementRemaining[unitId];
            if (movementCost > rem)
                return false;
            // First movement consumes an action
            if (rem == maxMovement)
            {
                if (!ConsumeAction(unitId))
                    return false;
            }
            _movementRemaining[unitId] = rem - movementCost;
            return true;
        }

        public bool HasUnitMoved(int unitId)
        {
            var unit = _allUnits.FirstOrDefault(u => u.Id == unitId);
            int maxMovement = unit?.CurrentStats.MOV ?? 0;
            if (!_movementRemaining.TryGetValue(unitId, out var rem))
                return false;
            return rem < maxMovement;
        }

        public void GrantActions(int unitId, int amount)
        {
            if (!_actionsRemaining.ContainsKey(unitId))
                _actionsRemaining[unitId] = 0;
            _actionsRemaining[unitId] += amount;
            if (_actionsRemaining[unitId] > 0)
            {
                _unitsWhoActed.Remove(unitId);
                _unitsWhoAttacked.Remove(unitId);
                var u = _allUnits.FirstOrDefault(x => x.Id == unitId);
                if (u != null)
                    _movementRemaining[unitId] = u.CurrentStats.MOV;
            }
        }

        public bool HasUnitAttacked(int unitId)
        {
            return _unitsWhoAttacked.Contains(unitId);
        }

        public void MarkUnitAsAttacked(int unitId)
        {
            _unitsWhoAttacked.Add(unitId);
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
            if (!HasUnitActed(target.Id))
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
            _unitsWhoAttacked.Remove(targetUnitId);
            _actionsRemaining[targetUnitId] = DefaultActionsPerUnit;
            var target = _allUnits.FirstOrDefault(u => u.Id == targetUnitId);
            if (target != null)
                _movementRemaining[targetUnitId] = target.CurrentStats.MOV;
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
                    && u.Id != heron.Id && HasUnitActed(u.Id));

                if (adjacent != null)
                {
                    RefreshUnit(adjacent.Id);
                    refreshed++;
                }
            }

            return refreshed;
        }

        /// <summary>Clears guard flags for all alive units on the given team.</summary>
        private void ClearGuardFlags(Team team)
        {
            foreach (var unit in _allUnits.Where(u => u.IsAlive && u.Team == team))
            {
                unit.SetGuarding(false);
            }
        }

        /// <summary>Ticks transform gauges for all alive Laguz units on the given team.</summary>
        private void TickLaguzGauges(Team team)
        {
            foreach (var unit in _allUnits.Where(u => u.IsAlive && u.Team == team && u.IsLaguz))
            {
                unit.TickTransformGauge();
            }
        }

        private void CheckReinforcements()
        {
            if (_reinforcementService == null || _reinforcementTriggers.Count == 0)
                return;

            var spawned = _reinforcementService.EvaluateTriggers(_reinforcementTriggers, TurnCount, _allUnits);
            _allUnits.AddRange(spawned);
        }

        private List<IUnit> GetPhaseUnits()
        {
            var team = CurrentPhase switch
            {
                Phase.PlayerPhase => Team.PlayerTeam,
                Phase.AllyPhase => Team.AllyNPC,
                _ => Team.EnemyTeam
            };
            return _allUnits.Where(u => u.Team == team && u.IsAlive)
                .ToList();
        }
    }
}
