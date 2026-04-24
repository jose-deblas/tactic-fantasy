**Plan: Sistema de 2 acciones por unidad**

TL;DR: Añadir soporte de 2 acciones por unidad (AP=2), menú contextual de acciones (Atacar, Bolsa, Robar, Intercambiar), y UI de inventario en 4 fases. Implementación incremental: primero dominio (AP en TurnManager), luego adaptadores (GameController consume AP), después UI (menú y ventana Bolsa) y por último integración/IA y tests.

**Steps**
1. Fase 1 — Dominio: API de acciones por unidad
- Objetivo: introducir un contador de acciones por unidad sin tocar estructura persistente de `Unit`.
- Pasos:
  - Añadir a la interfaz `ITurnManager` y a `TurnManager` las nuevas APIs: `int DefaultActionsPerUnit { get; }`, `int GetRemainingActions(int unitId)`, `bool ConsumeAction(int unitId)`, `void GrantActions(int unitId, int amount)` y mantener `MarkUnitAsActed(int)` como compatibilidad (que pondrá acciones a 0).
  - Implementar internamente `Dictionary<int,int> _actionsRemaining` y inicializar en `Initialize(...)` con `DefaultActionsPerUnit` por unidad.
  - Modificar `HasUnitActed(int)` y `HaveAllPlayerUnitsActed()` para basarse en `_actionsRemaining[unitId] <= 0` (mantener comportamiento actual en tests que usen `MarkUnitAsActed`).
  - Asegurar que `AdvancePhase()` resetea acciones para el que comienza la fase (o para todas las unidades según el flujo actual).
- Archivos a tocar:
  - [Assets/Scripts/Domain/Turn/TurnManager.cs](Assets/Scripts/Domain/Turn/TurnManager.cs)
- Tests/validación:
  - Actualizar [Assets/Scripts/Tests/TurnManagerTests.cs](Assets/Scripts/Tests/TurnManagerTests.cs): comprobar `DefaultActionsPerUnit == 2`, `ConsumeAction` decrementa, `HasUnitActed` solo true cuando acciones == 0, `AdvancePhase` resetea.

2. Fase 2 — Adaptadores: consumo de acciones y comportamiento de juego
- Objetivo: que movimiento, ataque, robar, intercambiar y uso de pociones consuman AP según reglas.
- Pasos:
  - Reemplazar tracking local `_unitHasMoved` por consultas a `TurnManager.GetRemainingActions(unit.Id)` y llamadas a `TurnManager.ConsumeAction(unit.Id)`:
    - `MoveUnit(...)` → tras mover con éxito: `_turnManager.ConsumeAction(unit.Id)` (consume 1 acción).
    - `AttackUnit(...)` → tras resolver combate: `_turnManager.ConsumeAction(attacker.Id)`.
    - `ExecuteSteal(...)` → usar `StealService.Steal(...)` y `_turnManager.ConsumeAction(thief.Id)`.
    - `ExecuteTrade(...)` → usar `TradeService` y `_turnManager.ConsumeAction(initiator.Id)`.
    - `UseConsumable(...)` → `ConsumableItem.Use(unit)` + `_turnManager.ConsumeAction(unit.Id)`.
  - Ajustar `SelectUnit`/rango para que la lógica de mostrar movimiento/ataque se base en acciones restantes y posición actual. Permitir atacar después de moverse si quedan acciones.
  - Añadir métodos en `GameController` para operaciones de menú: `OpenInventoryFor(IUnit)`, `ExecuteSteal(IUnit,IUnit)`, `ExecuteTrade(IUnit,IUnit,ItemSpec)`.
- Archivos a tocar:
  - [Assets/Scripts/Adapters/GameController.cs](Assets/Scripts/Adapters/GameController.cs)
  - [Assets/Scripts/Adapters/UnitRenderer.cs](Assets/Scripts/Adapters/UnitRenderer.cs) (pintar gris si `GetRemainingActions(unit.Id) == 0`)
- Tests/validación:
  - Añadir tests de integración en `Assets/Scripts/Tests/ActionConsumptionTests.cs`: mover consume 1, atacar consume 1, mover+atacar consume 2, usar poción consume 1, robar/intercambiar consumen 1.

3. Fase 3 — UI: menú contextual y ventana Bolsa
- Objetivo: ofrecer interfaz para `Atacar`, `Bolsa`, `Robar`, `Intercambiar` y permitir equipar/usar objetos.
- Pasos:
  - Extender `UIManager` con: `ShowActionMenu(IUnit unit, (int x,int y) tilePos)`, `HideActionMenu()`, y evento `OnActionSelected(ActionType action, object payload)`.
  - Crear `InventoryWindow` (nuevo adaptador) con API `ShowInventory(IUnit unit, Action<InventoryResult> onClose)` que lista `unit.Inventory.GetAll()`, permite `Equip(item)`, `Use(item)` y `Give(item,targetUnit)`.
  - Implementar menu condicional: habilitar `Atacar` si hay enemigos en rango (`GameController.CalculateAttackRangeFromPosition` / `CalculateAttackRangeFromMovement`), `Bolsa` siempre, `Robar` solo para clases `Thief`/`Rogue` y si hay enemigo adyacente con objeto robable, `Intercambiar` solo si hay aliado adyacente.
  - Regla UI: `Use Consumable` consume 1 acción; `Equip` no consume acción; cerrar `Bolsa` permite seguir con otras acciones si quedan AP.
- Archivos a tocar/añadir:
  - [Assets/Scripts/Adapters/UIManager.cs](Assets/Scripts/Adapters/UIManager.cs)
  - [Assets/Scripts/Adapters/InventoryWindow.cs] (nuevo)
  - [Assets/Scripts/Adapters/GameController.cs] (enganchar callbacks UI)
- Tests/validación:
  - `UIManagerActionMenuTests` (mostrar/ocultar y callbacks), `InventoryWindowTests` (equipar, usar, cerrar y su consumo de AP). Probar flujo completo: seleccionar unidad → menú Bolsa → equipar arma (sin consumir AP) → atacar si quedan acciones.

4. Fase 4 — Integración, IA y pulido
- Objetivo: actualizar AI y asegurarse de que todo el juego respete AP; cerrar edge-cases.
- Pasos:
  - Actualizar `AIController.DecideAction(...)` para consultar `turnManager.GetRemainingActions(unit.Id)` y planificar acciones en función de AP disponibles (p.ej. priorizar atacar si hay 1 AP o mover si necesita abrirse paso).
  - Unificar semántica de "adyacente": migrar `StealService` de Manhattan a Chebyshev (o documentar explícitamente la excepción). Recomiendo usar Chebyshev para consistencia con `GetDistance` usado en combate.
  - Ejecutar y arreglar tests fallidos; pulir UI/UX (indicadores visuales de AP, tooltip en `Bolsa` que muestre si usar consumible consumirá acción).
- Archivos a tocar:
  - [Assets/Scripts/Domain/AI/AIController.cs](Assets/Scripts/Domain/AI/AIController.cs)
  - [Assets/Scripts/Domain/Items/StealService.cs](Assets/Scripts/Domain/Items/StealService.cs)
  - Documentación: `README.md`, `CLAUDE.md` (actualizar reglas de AP y menú)
- Tests/validación:
  - Ejecutar suite completa: `TurnManagerTests`, `ActionConsumptionTests`, `CombatResolverTests`, `StealServiceTests`, `AIControllerTests`.

**Relevant files**
- **Turn manager**: [Assets/Scripts/Domain/Turn/TurnManager.cs](Assets/Scripts/Domain/Turn/TurnManager.cs) — añadir `GetRemainingActions`/`ConsumeAction`.
- **Game flow / adaptador**: [Assets/Scripts/Adapters/GameController.cs](Assets/Scripts/Adapters/GameController.cs) — consumir AP, abrir menú, nuevos `ExecuteSteal/ExecuteTrade/OpenInventoryFor`.
- **UI manager**: [Assets/Scripts/Adapters/UIManager.cs](Assets/Scripts/Adapters/UIManager.cs) — añadir `ShowActionMenu` y `ShowInventory`.
- **Inventory (dominio)**: [Assets/Scripts/Domain/Items/Inventory.cs](Assets/Scripts/Domain/Items/Inventory.cs) — ya existe; comprobar APIs `Add/Remove/Swap/GetAll`.
- **Steal/Trade services**: [Assets/Scripts/Domain/Items/StealService.cs](Assets/Scripts/Domain/Items/StealService.cs), [Assets/Scripts/Domain/Items/TradeService.cs](Assets/Scripts/Domain/Items/TradeService.cs) — usar desde `GameController`.
- **Weapons/rango**: [Assets/Scripts/Domain/Weapons/Weapon.cs](Assets/Scripts/Domain/Weapons/Weapon.cs) y [Assets/Scripts/Domain/Units/UnitFactory.cs](Assets/Scripts/Domain/Units/UnitFactory.cs) — verificar `MinRange/MaxRange` y que las armas especiales respeten 2x/3x/5-10x del ratio si se necesita regla global.
- **Combat**: [Assets/Scripts/Domain/Combat/CombatResolver.cs](Assets/Scripts/Domain/Combat/CombatResolver.cs) — usa `GetDistance` (Chebyshev) y `IWeapon.MinRange/MaxRange`.
- **Render**: [Assets/Scripts/Adapters/UnitRenderer.cs](Assets/Scripts/Adapters/UnitRenderer.cs) — pintar unidad gris cuando `GetRemainingActions(unit.Id)==0`.
- **Tests**: [Assets/Scripts/Tests/TurnManagerTests.cs](Assets/Scripts/Tests/TurnManagerTests.cs), añadir [Assets/Scripts/Tests/ActionConsumptionTests.cs] y actualizar [Assets/Scripts/Tests/StealServiceTests.cs] y UI tests.

**Verificación**
- Ejecutar tests EditMode (dominio) tras cada fase. Comando local:

```bash
./run-tests.sh
```

- Tests mínimos a pasar por fase:
  - Fase 1: `TurnManagerTests` actualizado.
  - Fase 2: `ActionConsumptionTests`, `PathFinderTests` (asegurar movimiento no rota coste), integraciones básicas GameController.
  - Fase 3: tests UI/Inventory (EditMode si están disponibles), pruebas manual en Editor (abrir `InventoryWindow`, equipar, usar, atacar luego de equipar).
  - Fase 4: suite completa.

**Decisiones y supuestos (propuestos, ya siguen tus reglas especificadas)**
- Movimiento consume 1 acción.
- Atacar consume 1 acción.
- Usar poción/antídoto consume 1 acción.
- Cambiar de arma NO consume acción; tras cerrar `Bolsa` se puede usar acción de atacar si quedan AP.
- Robar consume 1 acción y está permitido solo para clases `Thief`/`Rogue` (nombres exactos en `ClassData`).
- Intercambiar consume 1 acción del personaje que inicia el intercambio.
- Default AP por unidad = 2.
- Mantenemos `MarkUnitAsActed` como alias de compatibilidad (pone acciones a 0). Recomendado: migrar llamadas existentes a `ConsumeAction` gradualmente.
- Unificaremos "adyacente" a Chebyshev (distancia `GetDistance(...) == 1`) para coherencia con combate; actualizar `StealService` (actualmente usa Manhattan).

**Further considerations / riesgos**
1. Estado fragmentado (moved vs acted): actualmente `GameController._unitHasMoved` y `TurnManager` realizan tracking distinto. El plan elimina esa inconsistencia centralizando en `TurnManager`.
2. Tests: la migración afectará muchos tests; priorizar Fase 1 y actualizarlos antes de grandes cambios en adaptadores/UI.
3. UI/UX: diseñar cómo mostrar "AP restante" (icono sobre la unidad / HUD). Añadir indicador visual en `UnitRenderer`.

**Siguiente paso**
- He guardado este plan en `/memories/session/plan.md`. ¿Quieres que copie este plan al archivo `improvements.md` del repositorio ahora, o prefieres que primero haga la Fase 1 (cambios en `TurnManager`) y te muestre el diff antes de modificar el repo?

- Opciones recomendadas:
  - "Copiar ahora en `improvements.md` y preparar commit" — yo aplico el cambio en el repo (requiere permiso para editar archivos). 
  - "Solo guardar en memoria y proceder a Fase 1" — inicio la implementación de dominio y actualizaré tests (requiere permiso para editar archivos). 
  - "Solo guardar el plan (por ahora)" — no modifico el repo hasta instrucciones.

