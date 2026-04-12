using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Adapters
{
    public class GameController : MonoBehaviour
    {
        private IGameMap _gameMap;
        private ITurnManager _turnManager;
        private ICombatResolver _combatResolver;
        private IPathFinder _pathFinder;
        private IAIController _aiController;
        private MapRenderer _mapRenderer;
        private UnitRenderer _unitRenderer;
        private UIManager _uiManager;
        private InputHandler _inputHandler;

        private List<IUnit> _allUnits;
        private IUnit _selectedUnit;
        private HashSet<(int, int)> _currentMovementRange;
        private HashSet<(int, int)> _currentAttackRange;

        private bool _isExecutingEnemyTurn = false;

        public void Awake()
        {
            InitializeDomainLayer();
            InitializeAdapters();
            CreateTeams();
            _turnManager.Initialize(_allUnits);
        }

        public void Update()
        {
            if (_turnManager.CurrentPhase == Phase.EnemyPhase && !_isExecutingEnemyTurn)
            {
                _isExecutingEnemyTurn = true;
                StartCoroutine(ExecuteEnemyPhase());
            }

            if (_turnManager.CurrentPhase == Phase.GameOver)
            {
                HandleGameOver();
            }
        }

        private void InitializeDomainLayer()
        {
            _gameMap = new GameMap(16, 16, System.Environment.TickCount);
            _combatResolver = new CombatResolver();
            _pathFinder = new PathFinder();
            _aiController = new AIController(_combatResolver);
            _turnManager = new TurnManager();
            _allUnits = new List<IUnit>();
            _currentMovementRange = new HashSet<(int, int)>();
            _currentAttackRange = new HashSet<(int, int)>();
        }

        private void InitializeAdapters()
        {
            _mapRenderer = GetComponent<MapRenderer>() ?? gameObject.AddComponent<MapRenderer>();
            _unitRenderer = GetComponent<UnitRenderer>() ?? gameObject.AddComponent<UnitRenderer>();
            _uiManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();
            _inputHandler = GetComponent<InputHandler>() ?? gameObject.AddComponent<InputHandler>();

            _mapRenderer.Initialize(_gameMap);
            _inputHandler.OnTileClicked += HandleTileClick;
            _inputHandler.OnUnitClicked += HandleUnitClick;
        }

        private void CreateTeams()
        {
            UnitFactory.ResetIdCounter();

            var allClasses = ClassDataFactory.GetAllClasses();

            for (int i = 0; i < 4; i++)
            {
                var classData = allClasses[Random.Range(0, allClasses.Length)];
                var weapon = WeaponFactory.GetWeaponForClass(classData.WeaponType);
                var pos = (Random.Range(0, 3), Random.Range(0, 3));

                var unit = new Unit(
                    UnitFactory.GetNextId(),
                    $"Player{i + 1}",
                    Team.PlayerTeam,
                    classData,
                    classData.BaseStats,
                    pos,
                    weapon
                );
                _allUnits.Add(unit);
            }

            for (int i = 0; i < 4; i++)
            {
                var classData = allClasses[Random.Range(0, allClasses.Length)];
                var weapon = WeaponFactory.GetWeaponForClass(classData.WeaponType);
                var pos = (Random.Range(13, 16), Random.Range(13, 16));

                var unit = new Unit(
                    UnitFactory.GetNextId(),
                    $"Enemy{i + 1}",
                    Team.EnemyTeam,
                    classData,
                    classData.BaseStats,
                    pos,
                    weapon
                );
                _allUnits.Add(unit);
            }
        }

        private void HandleUnitClick(int x, int y)
        {
            if (_turnManager.CurrentPhase != Phase.PlayerPhase)
                return;

            var unit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive);

            if (unit == null)
            {
                SelectUnit(null);
                return;
            }

            if (unit.Team != Team.PlayerTeam)
            {
                SelectUnit(null);
                return;
            }

            SelectUnit(unit);
        }

        private void HandleTileClick(int x, int y)
        {
            if (_turnManager.CurrentPhase != Phase.PlayerPhase || _selectedUnit == null)
                return;

            if (_selectedUnit.Team != Team.PlayerTeam)
                return;

            if (_currentMovementRange.Contains((x, y)))
            {
                MoveUnit(_selectedUnit, x, y);
            }
            else if (_currentAttackRange.Contains((x, y)))
            {
                var targetUnit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive);
                if (targetUnit != null && targetUnit.Team == Team.EnemyTeam)
                {
                    AttackUnit(_selectedUnit, targetUnit);
                }
            }
        }

        private void SelectUnit(IUnit unit)
        {
            _selectedUnit = unit;

            _currentMovementRange.Clear();
            _currentAttackRange.Clear();

            if (unit != null && unit.Team == Team.PlayerTeam)
            {
                _currentMovementRange = _pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, _gameMap);

                foreach (var pos in _currentMovementRange)
                {
                    for (int dx = -unit.EquippedWeapon.MaxRange; dx <= unit.EquippedWeapon.MaxRange; dx++)
                    {
                        for (int dy = -unit.EquippedWeapon.MaxRange; dy <= unit.EquippedWeapon.MaxRange; dy++)
                        {
                            int tx = pos.x + dx;
                            int ty = pos.y + dy;
                            if (_gameMap.IsValidPosition(tx, ty) &&
                                _gameMap.GetDistance(pos.x, pos.y, tx, ty) >= unit.EquippedWeapon.MinRange &&
                                _gameMap.GetDistance(pos.x, pos.y, tx, ty) <= unit.EquippedWeapon.MaxRange)
                            {
                                _currentAttackRange.Add((tx, ty));
                            }
                        }
                    }
                }
            }

            _mapRenderer.SetSelectedUnit(_selectedUnit);
            _mapRenderer.SetMovementRange(_currentMovementRange);
            _mapRenderer.SetAttackRange(_currentAttackRange);
            _unitRenderer.UpdateAllUnits(_allUnits);
            _uiManager.UpdateSelectedUnitInfo(_selectedUnit);
        }

        private void MoveUnit(IUnit unit, int targetX, int targetY)
        {
            var path = _pathFinder.FindPath(unit.Position.x, unit.Position.y, targetX, targetY, unit.CurrentStats.MOV, unit, _gameMap);

            if (path.Count > 0)
            {
                unit.SetPosition(path[path.Count - 1].x, path[path.Count - 1].y);
                _mapRenderer.SetSelectedUnit(null);
                _mapRenderer.SetMovementRange(new HashSet<(int, int)>());
                _mapRenderer.SetAttackRange(new HashSet<(int, int)>());
                SelectUnit(unit);
            }
        }

        private void AttackUnit(IUnit attacker, IUnit defender)
        {
            var result = _combatResolver.ResolveCombat(attacker, defender, _gameMap);

            if (result.Hit)
            {
                defender.TakeDamage(result.Damage);
            }

            _unitRenderer.UpdateAllUnits(_allUnits);
            _uiManager.ShowCombatResult(result);

            _turnManager.MarkCurrentUnitAsActed();

            if (_turnManager.GetGameState() != GameState.InProgress)
            {
                _turnManager.AdvancePhase();
            }

            SelectUnit(null);
        }

        public void EndPlayerPhase()
        {
            if (_turnManager.CurrentPhase == Phase.PlayerPhase)
            {
                _turnManager.AdvancePhase();
                SelectUnit(null);
            }
        }

        private System.Collections.IEnumerator ExecuteEnemyPhase()
        {
            yield return new WaitForSeconds(0.5f);

            var enemyUnits = _allUnits.Where(u => u.Team == Team.EnemyTeam && u.IsAlive).ToList();

            foreach (var enemyUnit in enemyUnits)
            {
                _aiController.DecideAction(enemyUnit, _allUnits, _gameMap, _pathFinder,
                    out var moveTarget, out var attackTarget, out var isHealAction);

                if (moveTarget.HasValue)
                {
                    var path = _pathFinder.FindPath(enemyUnit.Position.x, enemyUnit.Position.y,
                        moveTarget.Value.x, moveTarget.Value.y, enemyUnit.CurrentStats.MOV, enemyUnit, _gameMap);

                    if (path.Count > 0)
                    {
                        enemyUnit.SetPosition(path[path.Count - 1].x, path[path.Count - 1].y);
                    }
                }

                if (attackTarget != null)
                {
                    if (isHealAction && enemyUnit.EquippedWeapon.Type == WeaponType.STAFF)
                    {
                        int healAmount = enemyUnit.CurrentStats.MAG + 10;
                        attackTarget.Heal(healAmount);
                    }
                    else
                    {
                        var result = _combatResolver.ResolveCombat(enemyUnit, attackTarget, _gameMap);
                        if (result.Hit)
                        {
                            attackTarget.TakeDamage(result.Damage);
                        }
                    }
                }

                _unitRenderer.UpdateAllUnits(_allUnits);
                yield return new WaitForSeconds(0.3f);
            }

            _isExecutingEnemyTurn = false;
            _turnManager.AdvancePhase();
        }

        private void HandleGameOver()
        {
            var state = _turnManager.GetGameState();
            _uiManager.ShowGameOverScreen(state, _turnManager.TurnCount);
            enabled = false;
        }
    }
}
