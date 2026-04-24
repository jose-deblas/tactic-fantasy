using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;
using TacticFantasy.Domain.Items;

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
        private GamepadCursorController _gamepadCursorController;
        private KeyboardCursorController _keyboardCursorController;
        private CursorRenderer _cursorRenderer;
        private bool _unitHasMoved = false;

        private List<IUnit> _allUnits;
        private IUnit _selectedUnit;
        private HashSet<(int, int)> _currentMovementRange;
        private HashSet<(int, int)> _currentAttackRange;

        private bool _isExecutingAllyTurn = false;
        private bool _isExecutingEnemyTurn = false;
        private bool _isShowingAttackRange = false;
        // Awaiting player to pick an enemy target after choosing Attack from the action menu
        private bool _awaitingAttackTarget = false;
        private int _awaitingAttackerId = -1;

        private IFogOfWar _fogOfWar;
        private IReinforcementService _reinforcementService;
        private ChapterData _chapterData;
        private IStealService _stealService;
        private ITradeService _tradeService;

        public void Awake()
        {
            InitializeDomainLayer();
            InitializeAdapters();
            CreateTeams();
            _turnManager.Initialize(_allUnits);
            // CRITICAL: Render all units immediately after initialization so they're visible on load
            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
            _uiManager.UpdatePhaseDisplay(_turnManager.CurrentPhase, _turnManager.TurnCount);
        }

        public void Update()
        {
            if (_uiManager.IsTurnInterstitialOpen())
                return;

            if (_turnManager.CurrentPhase == Phase.AllyPhase && !_isExecutingAllyTurn)
            {
                _isExecutingAllyTurn = true;
                StartCoroutine(ExecuteAllyPhase());
            }

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
            _stealService = new StealService();
            _tradeService = new TradeService();
        }

        private void InitializeAdapters()
        {
            _mapRenderer = GetComponent<MapRenderer>() ?? gameObject.AddComponent<MapRenderer>();
            _unitRenderer = GetComponent<UnitRenderer>() ?? gameObject.AddComponent<UnitRenderer>();
            _uiManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();
            _inputHandler = GetComponent<InputHandler>() ?? gameObject.AddComponent<InputHandler>();
            _gamepadCursorController = GetComponent<GamepadCursorController>() ?? gameObject.AddComponent<GamepadCursorController>();
            _keyboardCursorController = GetComponent<KeyboardCursorController>() ?? gameObject.AddComponent<KeyboardCursorController>();
            _cursorRenderer = GetComponent<CursorRenderer>() ?? gameObject.AddComponent<CursorRenderer>();

            _mapRenderer.Initialize(_gameMap);
            _gamepadCursorController.Initialize(_gameMap);
            _keyboardCursorController.Initialize(_gameMap);
            _cursorRenderer.Initialize();

            // Suscribirse a eventos del ratón
            _inputHandler.OnTileClicked += HandleTileClick;
            _inputHandler.OnUnitClicked += HandleUnitClick;
            _inputHandler.OnUnitRightClicked += HandleUnitRightClick;
            _inputHandler.OnEndTurnPressed += HandleEndTurnPressed;  // NEW: Keyboard shortcut
            _inputHandler.OnMenuTogglePressed += HandleMenuTogglePressed;  // NEW: ESC key for menu

            // Suscribirse a eventos del mando
            _gamepadCursorController.OnCursorMoved += HandleGamepadCursorMoved;
            _gamepadCursorController.OnConfirm += HandleGamepadConfirm;
            _gamepadCursorController.OnCancel += HandleGamepadCancel;
            _gamepadCursorController.OnEndTurn += HandleGamepadEndTurn;
            _gamepadCursorController.OnToggleAttackRange += HandleGamepadToggleAttackRange;
            _gamepadCursorController.OnMenuToggle += HandleMenuTogglePressed;  // NEW: Start button for menu

            // Suscribirse a eventos del teclado (KeyboardCursorController)
            _keyboardCursorController.OnCursorMoved += HandleGamepadCursorMoved;
            _keyboardCursorController.OnConfirm += () => PerformConfirmAt(_keyboardCursorController.CursorPosition.x, _keyboardCursorController.CursorPosition.y);
            _keyboardCursorController.OnCancel += HandleGamepadCancel;
            _keyboardCursorController.OnToggleAttackRange += HandleGamepadToggleAttackRange;
            // Note: menu toggle and end-turn are handled by InputHandler to avoid duplicate events
            // Subscribe to UI action menu events
            _uiManager.OnActionMenuSelected += HandleActionMenuSelected;
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
            if (_uiManager.IsModalMenuOpen() || _uiManager.IsTurnInterstitialOpen())
                return;

            if (_turnManager.CurrentPhase != Phase.PlayerPhase)
                return;

            var unit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive);

            if (unit == null)
            {
                // If awaiting an attack target and clicked empty tile, cancel awaiting
                if (_awaitingAttackTarget)
                {
                    _awaitingAttackTarget = false;
                    _awaitingAttackerId = -1;
                    _currentAttackRange.Clear();
                    _mapRenderer.SetAttackRange(_currentAttackRange);
                    _uiManager.ShowInfoMessage("Attack cancelled");
                }
                else
                {
                    SelectUnit(null);
                }
                return;
            }

            // If awaiting an attack target and clicked an enemy in range, attack
            if (_awaitingAttackTarget && unit.Team == Team.EnemyTeam && _currentAttackRange.Contains((unit.Position.x, unit.Position.y)))
            {
                var attacker = _allUnits.FirstOrDefault(u => u.Id == _awaitingAttackerId);
                if (attacker != null)
                {
                    _awaitingAttackTarget = false;
                    _awaitingAttackerId = -1;
                    AttackUnit(attacker, unit);
                    return;
                }
            }

            // NEW: Toggle behavior - clicking same unit deselects
            if (_selectedUnit != null && _selectedUnit.Id == unit.Id)
            {
                SelectUnit(null);
                return;
            }

            // NEW: Allow selection of any unit (player or enemy) for inspection
            SelectUnit(unit);
        }

        private void HandleTileClick(int x, int y)
        {
            if (_uiManager.IsModalMenuOpen() || _uiManager.IsTurnInterstitialOpen())
                return;

            // Sync mouse click with keyboard/gamepad cursor and renderer
            _keyboardCursorController?.SetCursorPosition(x, y);
            _gamepadCursorController?.SetCursorPosition(x, y);
            _cursorRenderer?.UpdateCursorPosition(x, y);

            // NEW: Always show terrain info on tile click (even without unit selected)
            var tile = _gameMap.GetTile(x, y);
            _uiManager.ShowTerrainInfo(tile.Terrain);

            if (_turnManager.CurrentPhase != Phase.PlayerPhase || _selectedUnit == null)
                return;

            if (_selectedUnit.Team != Team.PlayerTeam)
                return;

            // If awaiting an attack target (player clicked Attack), handle tile clicks as potential attack targets
            if (_awaitingAttackTarget)
            {
                var targetUnit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive && u.Team == Team.EnemyTeam);
                if (targetUnit != null && _currentAttackRange.Contains((x, y)))
                {
                    var attacker = _allUnits.FirstOrDefault(u => u.Id == _awaitingAttackerId);
                    _awaitingAttackTarget = false;
                    _awaitingAttackerId = -1;
                    if (attacker != null) AttackUnit(attacker, targetUnit);
                    return;
                }

                // Clicked not a valid target -> cancel awaiting state
                _awaitingAttackTarget = false;
                _awaitingAttackerId = -1;
                _uiManager.ShowInfoMessage("Attack cancelled");
                _currentAttackRange.Clear();
                _mapRenderer.SetAttackRange(_currentAttackRange);
                return;
            }

            if (_currentMovementRange.Contains((x, y)))
            {
                MoveUnit(_selectedUnit, x, y);
            }
            else if (_currentAttackRange.Contains((x, y)))
            {
                var targetUnit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive);
                if (targetUnit != null)
                {
                    // NEW: Check if this is a refresh action (unit has REFRESH weapon)
                    if (_selectedUnit.EquippedWeapon.Type == WeaponType.REFRESH)
                    {
                        if (_turnManager.CanRefreshTarget(_selectedUnit, targetUnit))
                        {
                            RefreshUnit(_selectedUnit, targetUnit);
                        }
                        else
                        {
                            _uiManager.ShowInfoMessage("Cannot refresh this unit!");
                        }
                    }
                    else if (targetUnit.Team == Team.EnemyTeam)
                    {
                        AttackUnit(_selectedUnit, targetUnit);
                    }
                }
            }
        }

        private void HandleUnitRightClick(int x, int y)
        {
            if (_uiManager.IsModalMenuOpen() || _uiManager.IsTurnInterstitialOpen())
                return;

            if (_turnManager.CurrentPhase != Phase.PlayerPhase)
                return;

            var unit = _allUnits.FirstOrDefault(u => u.Position.x == x && u.Position.y == y && u.IsAlive);
            if (unit == null) return;
            if (unit.Team != Team.PlayerTeam) return;

            // Select the unit and show context menu with available actions
            SelectUnit(unit);

            // Determine if Attack should be enabled: check from current position and all reachable movement tiles
            bool canAttack = false;
            var remaining = _turnManager.GetRemainingActions(unit.Id);
            // include current pos even if no movement
            var potentialPositions = new HashSet<(int, int)> { (unit.Position.x, unit.Position.y) };
            if (remaining > 0)
            {
                var movement = _pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, _gameMap, _allUnits);
                foreach (var p in movement) potentialPositions.Add(p);
            }
            foreach (var pos in potentialPositions)
            {
                if (HasAttackableEnemyAtPosition(unit, pos.Item1, pos.Item2)) { canAttack = true; break; }
            }

            bool canSteal = _allUnits.Any(u => u.IsAlive && u.Team != unit.Team && _stealService.CanSteal(unit, u));

            bool canTrade = _allUnits.Any(u => u.IsAlive && TeamRelations.AreAllied(u.Team, unit.Team) && _tradeService.CanTrade(unit, u));

            _uiManager.ShowActionMenu(unit, (x, y), canAttack, canSteal, canTrade);
        }

        private bool HasAttackableEnemyAtPosition(IUnit unit, int posX, int posY)
        {
            var maxRange = unit.EquippedWeapon.MaxRange;
            var minRange = unit.EquippedWeapon.MinRange;

            for (int dx = -maxRange; dx <= maxRange; dx++)
            {
                for (int dy = -maxRange; dy <= maxRange; dy++)
                {
                    int tx = posX + dx;
                    int ty = posY + dy;
                    if (!_gameMap.IsValidPosition(tx, ty)) continue;
                    int distance = _gameMap.GetDistance(posX, posY, tx, ty);
                    if (distance < minRange || distance > maxRange) continue;

                    var targetUnit = _allUnits.FirstOrDefault(u => u.Position.x == tx && u.Position.y == ty && u.IsAlive && u.Team == Team.EnemyTeam);
                    if (targetUnit != null) return true;
                }
            }

            return false;
        }

        private void HandleActionMenuSelected(ActionMenuChoice choice, IUnit unit)
        {
            if (unit == null) return;

            switch (choice)
            {
                case ActionMenuChoice.Attack:
                    // Prepare attack targets: if unit has remaining actions, consider movement+attack; otherwise only current position
                    _currentMovementRange.Clear();
                    _currentAttackRange.Clear();
                    var rem = _turnManager.GetRemainingActions(unit.Id);
                    if (rem > 0)
                    {
                        _currentMovementRange = _pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, _gameMap, _allUnits);
                        CalculateAttackRangeFromMovement(unit);
                    }
                    else
                    {
                        CalculateAttackRangeFromPosition(unit);
                    }

                    // Highlight attackable tiles and instruct the player
                    _mapRenderer.SetAttackRange(_currentAttackRange);
                    SelectUnit(unit);

                    // If there's exactly one enemy in range, auto-attack; otherwise enter awaiting state
                    var enemiesInRange = _allUnits.Where(u => u.IsAlive && u.Team == Team.EnemyTeam && _currentAttackRange.Contains((u.Position.x, u.Position.y))).ToList();
                    if (enemiesInRange.Count == 1)
                    {
                        // Close menu (already destroyed by UI) and perform attack
                        AttackUnit(unit, enemiesInRange[0]);
                    }
                    else
                    {
                        _awaitingAttackTarget = true;
                        _awaitingAttackerId = unit.Id;
                        _uiManager.ShowInfoMessage("Select an enemy to attack (B to cancel)");
                    }
                    break;

                case ActionMenuChoice.Bag:
                    // Open inventory UI
                    _uiManager.ShowInventory(unit, result =>
                    {
                        if (result != null && result.Action == InventoryActionResult.ActionType.Use && result.Item != null)
                        {
                            // Use consumable and consume action
                            result.Item.Use(unit);
                            _turnManager.ConsumeAction(unit.Id);
                            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
                            _uiManager.ShowInfoMessage($"{unit.Name} used {result.Item.Name}");
                        }
                        else if (result != null && result.Action == InventoryActionResult.ActionType.Equip && result.Item != null)
                        {
                            if (result.Item is TacticFantasy.Domain.Weapons.IWeapon w && unit.CanEquip(w))
                            {
                                unit.EquipWeapon(w);
                                _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
                                _uiManager.ShowInfoMessage($"{unit.Name} equipped {w.Name}");
                            }
                        }
                    });
                    break;

                case ActionMenuChoice.Steal:
                    {
                        var target = _allUnits.FirstOrDefault(u => u.IsAlive && u.Team != unit.Team && _stealService.CanSteal(unit, u));
                        if (target != null)
                        {
                            var item = _stealService.Steal(unit, target);
                            _turnManager.ConsumeAction(unit.Id);
                            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
                            _uiManager.ShowInfoMessage($"{unit.Name} stole {item.Name} from {target.Name}");
                        }
                        else
                        {
                            _uiManager.ShowInfoMessage("No valid steal targets adjacent.");
                        }
                    }
                    break;

                case ActionMenuChoice.Trade:
                    _uiManager.ShowInfoMessage("Trade not yet implemented");
                    break;

                case ActionMenuChoice.Wait:
                case ActionMenuChoice.Cancel:
                default:
                    // Do nothing
                    break;
            }
        }

        private void SelectUnit(IUnit unit)
        {
            // No implicit "wait" on deselect - actions are tracked by TurnManager (AP)

            _selectedUnit = unit;

            _currentMovementRange.Clear();
            _currentAttackRange.Clear();

            if (unit != null)
            {
                if (unit.Team == Team.PlayerTeam)
                {
                    if (_turnManager.HasUnitActed(unit.Id))
                    {
                        // Already acted: show info only, no ranges
                    }
                    else
                    {
                        // Has remaining actions: show movement + attack options calculated from movement
                        var remaining = _turnManager.GetRemainingActions(unit.Id);
                        if (remaining > 0)
                        {
                            _currentMovementRange = _pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, _gameMap, _allUnits);
                            CalculateAttackRangeFromMovement(unit);
                        }
                    }
                }
                else if (unit.Team == Team.EnemyTeam)
                {
                    // Enemy units: Show movement range (blue) + attack range from all reachable positions (red)
                    _currentMovementRange = _pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, _gameMap, _allUnits);
                    CalculateAttackRangeFromMovement(unit);
                }
            }

            _mapRenderer.SetSelectedUnit(_selectedUnit);
            _mapRenderer.SetMovementRange(_currentMovementRange);
            _mapRenderer.SetAttackRange(_currentAttackRange);
            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
            _uiManager.UpdateSelectedUnitInfo(_selectedUnit);
        }

        private void CalculateAttackRangeFromMovement(IUnit unit)
        {
            foreach (var pos in _currentMovementRange)
            {
                for (int dx = -unit.EquippedWeapon.MaxRange; dx <= unit.EquippedWeapon.MaxRange; dx++)
                {
                    for (int dy = -unit.EquippedWeapon.MaxRange; dy <= unit.EquippedWeapon.MaxRange; dy++)
                    {
                        int tx = pos.Item1 + dx;
                        int ty = pos.Item2 + dy;
                        if (_gameMap.IsValidPosition(tx, ty) &&
                            _gameMap.GetDistance(pos.Item1, pos.Item2, tx, ty) >= unit.EquippedWeapon.MinRange &&
                            _gameMap.GetDistance(pos.Item1, pos.Item2, tx, ty) <= unit.EquippedWeapon.MaxRange)
                        {
                            _currentAttackRange.Add((tx, ty));
                        }
                    }
                }
            }
        }

        private void CalculateAttackRangeFromPosition(IUnit unit)
        {
            int maxRange = unit.EquippedWeapon.MaxRange;
            int minRange = unit.EquippedWeapon.MinRange;

            for (int dx = -maxRange; dx <= maxRange; dx++)
            {
                for (int dy = -maxRange; dy <= maxRange; dy++)
                {
                    int tx = unit.Position.x + dx;
                    int ty = unit.Position.y + dy;
                    int distance = _gameMap.GetDistance(unit.Position.x, unit.Position.y, tx, ty);

                    if (_gameMap.IsValidPosition(tx, ty) && distance >= minRange && distance <= maxRange)
                    {
                        _currentAttackRange.Add((tx, ty));
                    }
                }
            }
        }

        private void MoveUnit(IUnit unit, int targetX, int targetY)
        {
            var path = _pathFinder.FindPath(unit.Position.x, unit.Position.y, targetX, targetY, unit.CurrentStats.MOV, unit, _gameMap, _allUnits);

            if (path.Count > 0)
            {
                unit.SetPosition(path[path.Count - 1].Item1, path[path.Count - 1].Item2);
                // Consume one action for movement
                _turnManager.ConsumeAction(unit.Id);
                _unitHasMoved = true;
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

            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
            _uiManager.ShowCombatResult(result);

            // Consume one action for attacking
            _turnManager.ConsumeAction(attacker.Id);

            if (_turnManager.GetGameState() != GameState.InProgress)
            {
                _turnManager.AdvancePhase();
            }

            SelectUnit(null);
            CheckAllPlayerUnitsActed();
        }

        private void RefreshUnit(IUnit refresher, IUnit target)
        {
            _turnManager.RefreshUnit(target.Id);
            // Consumes an action from the refresher
            _turnManager.ConsumeAction(refresher.Id);

            _uiManager.ShowInfoMessage($"{refresher.Name} refreshed {target.Name}!");
            _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);

            SelectUnit(null);
            CheckAllPlayerUnitsActed();
        }

        public void EndPlayerPhase()
        {
            if (_turnManager.CurrentPhase == Phase.PlayerPhase)
            {
                _unitHasMoved = false;
                _uiManager.HideEndTurnPrompt();
                _turnManager.AdvancePhase();
                _uiManager.UpdatePhaseDisplay(_turnManager.CurrentPhase, _turnManager.TurnCount);
                SelectUnit(null);
            }
        }

        private void CheckAllPlayerUnitsActed()
        {
            if (_turnManager.CurrentPhase == Phase.PlayerPhase && _turnManager.HaveAllPlayerUnitsActed())
            {
                _uiManager.ShowEndTurnPrompt();
            }
        }

        private void HandleEndTurnPressed()
        {
            if (_uiManager.IsTurnInterstitialOpen())
                return;

            if (_turnManager.CurrentPhase == Phase.PlayerPhase)
            {
                EndPlayerPhase();
            }
        }

        /// <summary>
        /// Maneja el movimiento del cursor del mando.
        /// Actualiza la posición visual del cursor en el mapa.
        /// </summary>
        private void HandleGamepadCursorMoved((int x, int y) position)
        {
            _cursorRenderer.UpdateCursorPosition(position.x, position.y);
        }

        /// <summary>
        /// Maneja la confirmación del mando (botón A).
        /// Simula un clic en la posición actual del cursor del mando.
        /// </summary>
        private void PerformConfirmAt(int x, int y)
        {
            if (_uiManager.IsModalMenuOpen() || _uiManager.IsTurnInterstitialOpen())
                return;

            _inputHandler.SimulateGamepadClick(x, y);
        }

        private void HandleGamepadConfirm()
        {
            // If an interstitial is open, prefer activating its Start button
            if (_uiManager.IsTurnInterstitialOpen())
            {
                _uiManager.PressTurnStartButton();
                return;
            }

            // Otherwise perform normal confirm at cursor position
            PerformConfirmAt(_gamepadCursorController.CursorPosition.x, _gamepadCursorController.CursorPosition.y);
        }

        /// <summary>
        /// Maneja la cancelación del mando (botón B).
        /// Deselecciona la unidad actualmente seleccionada.
        /// </summary>
        private void HandleGamepadCancel()
        {
            if (_turnManager.CurrentPhase == Phase.PlayerPhase)
            {
                SelectUnit(null);
            }
        }

        /// <summary>
        /// Maneja el fin de turno del mando (botón X).
        /// Avanza la fase del juego si es el turno del jugador.
        /// </summary>
        private void HandleGamepadEndTurn()
        {
            if (_uiManager.IsTurnInterstitialOpen())
                return;

            EndPlayerPhase();
        }

        /// <summary>
        /// Maneja el toggle del rango de ataque del mando (botón Y).
        /// Muestra u oculta alternativamente los rangos de ataque de las unidades enemigas.
        /// </summary>
        private void HandleGamepadToggleAttackRange()
        {
            _isShowingAttackRange = !_isShowingAttackRange;

            if (_isShowingAttackRange)
            {
                // Mostrar rangos de ataque de todas las unidades enemigas vivas
                var allEnemyAttackRanges = new HashSet<(int, int)>();

                foreach (var enemy in _allUnits)
                {
                    if (enemy.Team == Team.EnemyTeam && enemy.IsAlive)
                    {
                        int maxRange = enemy.EquippedWeapon.MaxRange;
                        int minRange = enemy.EquippedWeapon.MinRange;

                        // Calcular rango de ataque desde la posición del enemigo
                        for (int dx = -maxRange; dx <= maxRange; dx++)
                        {
                            for (int dy = -maxRange; dy <= maxRange; dy++)
                            {
                                int tx = enemy.Position.x + dx;
                                int ty = enemy.Position.y + dy;
                                int distance = _gameMap.GetDistance(enemy.Position.x, enemy.Position.y, tx, ty);

                                if (_gameMap.IsValidPosition(tx, ty) && distance >= minRange && distance <= maxRange)
                                {
                                    allEnemyAttackRanges.Add((tx, ty));
                                }
                            }
                        }
                    }
                }

                // Mostrar los rangos
                _mapRenderer.SetAttackRange(allEnemyAttackRanges);
                _uiManager.ShowInfoMessage("Enemy attack ranges shown");
            }
            else
            {
                // Ocultar los rangos (volver al estado anterior)
                _mapRenderer.SetAttackRange(new HashSet<(int, int)>());
                _uiManager.ShowInfoMessage("Enemy attack ranges hidden");
            }
        }

        /// <summary>
        /// Maneja el toggle del menú modal (ESC o Start button).
        /// Abre o cierra el menú dependiendo del estado actual.
        /// </summary>
        private void HandleMenuTogglePressed()
        {
            if (_uiManager.IsModalMenuOpen())
            {
                _uiManager.HideModalMenu();
            }
            else
            {
                _uiManager.ShowModalMenu();
            }
        }

        private System.Collections.IEnumerator ExecuteAllyPhase()
        {
            yield return new WaitForSeconds(0.3f);

            var allyUnits = _allUnits.Where(u => u.Team == Team.AllyNPC && u.IsAlive).ToList();

            foreach (var allyUnit in allyUnits)
            {
                _aiController.DecideAction(allyUnit, _allUnits, _gameMap, _pathFinder,
                    out var moveTarget, out var attackTarget, out var isHealAction, null, _turnManager);

                if (moveTarget.HasValue)
                {
                    var path = _pathFinder.FindPath(allyUnit.Position.x, allyUnit.Position.y,
                        moveTarget.Value.Item1, moveTarget.Value.Item2, allyUnit.CurrentStats.MOV, allyUnit, _gameMap, _allUnits);

                    if (path.Count > 0)
                    {
                        allyUnit.SetPosition(path[path.Count - 1].Item1, path[path.Count - 1].Item2);
                        // Consume one action for movement (AI should respect AP)
                        _turnManager.ConsumeAction(allyUnit.Id);
                    }
                }

                // Only attempt attack/heal if AI still has actions remaining
                if (_turnManager.GetRemainingActions(allyUnit.Id) > 0 && attackTarget != null)
                {
                    if (isHealAction && allyUnit.EquippedWeapon.Type == WeaponType.STAFF)
                    {
                        int healAmount = allyUnit.CurrentStats.MAG + 10;
                        attackTarget.Heal(healAmount);
                    }
                    else
                    {
                        var result = _combatResolver.ResolveCombat(allyUnit, attackTarget, _gameMap);
                        if (result.Hit)
                        {
                            attackTarget.TakeDamage(result.Damage);
                        }
                        _uiManager.ShowCombatResult(result);
                    }

                    // Consume one action for attacking/healing
                    _turnManager.ConsumeAction(allyUnit.Id);
                }

                _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
                yield return new WaitForSeconds(0.3f);
            }

            _isExecutingAllyTurn = false;
            _turnManager.AdvancePhase();
            _uiManager.UpdatePhaseDisplay(_turnManager.CurrentPhase, _turnManager.TurnCount);
        }

        private System.Collections.IEnumerator ExecuteEnemyPhase()
        {
            yield return new WaitForSeconds(0.5f);

            var enemyUnits = _allUnits.Where(u => u.Team == Team.EnemyTeam && u.IsAlive).ToList();

            foreach (var enemyUnit in enemyUnits)
            {
                _aiController.DecideAction(enemyUnit, _allUnits, _gameMap, _pathFinder,
                    out var moveTarget, out var attackTarget, out var isHealAction, null, _turnManager);

                if (moveTarget.HasValue)
                {
                    var path = _pathFinder.FindPath(enemyUnit.Position.x, enemyUnit.Position.y,
                        moveTarget.Value.Item1, moveTarget.Value.Item2, enemyUnit.CurrentStats.MOV, enemyUnit, _gameMap, _allUnits);

                    if (path.Count > 0)
                    {
                        enemyUnit.SetPosition(path[path.Count - 1].Item1, path[path.Count - 1].Item2);
                        // Consume one action for movement (AI should respect AP)
                        _turnManager.ConsumeAction(enemyUnit.Id);
                    }
                }

                // Only attempt attack/heal if enemy still has actions remaining
                if (_turnManager.GetRemainingActions(enemyUnit.Id) > 0 && attackTarget != null)
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
                        _uiManager.ShowCombatResult(result);
                    }

                    // Consume one action for attacking/healing
                    _turnManager.ConsumeAction(enemyUnit.Id);
                }

                _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
                yield return new WaitForSeconds(0.3f);
            }

            _isExecutingEnemyTurn = false;
            _turnManager.AdvancePhase();
            _uiManager.UpdatePhaseDisplay(_turnManager.CurrentPhase, _turnManager.TurnCount);

            _uiManager.ShowTurnInterstitial(_turnManager.TurnCount, () =>
            {
                _unitRenderer.UpdateAllUnits(_allUnits, _turnManager);
            });
        }

        private void HandleGameOver()
        {
            var state = _turnManager.GetGameState();
            _uiManager.ShowGameOverScreen(state, _turnManager.TurnCount);
            enabled = false;
        }
    }
}
