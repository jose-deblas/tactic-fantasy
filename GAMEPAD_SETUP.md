# Configuración de Mando (Gamepad) - Tactic Fantasy

## Introducción

Este documento describe la implementación completa de soporte para mando (gamepad) en Tactic Fantasy, con soporte específico para Nintendo Switch Pro Controller y Joy-Cons conectados vía Bluetooth al Mac.

## Características Implementadas

✅ **Control del cursor** - Mover por el mapa tile a tile usando stick analógico o cruceta
✅ **Confirmación** - Botón A para seleccionar unidad/confirmar acción
✅ **Cancelación** - Botón B para cancelar/deseleccionar
✅ **Fin de turno** - Botón X para terminar el turno del jugador
✅ **Visualización de rango** - Botón Y para mostrar/ocultar rango de ataque enemigo
✅ **Cursor visual** - Quad pulsante en amarillo que se mueve suavemente
✅ **Coexistencia** - Mando y ratón funcionan simultáneamente sin conflictos

## Mapeo de Controles

| Acción | Control | Botón |
|--------|---------|--------|
| **Mover cursor** | Stick analógico izquierdo o Cruceta | - |
| **Seleccionar unidad** | Botón A | East |
| **Confirmar acción** | Botón A | East |
| **Cancelar selección** | Botón B | South |
| **Fin de turno** | Botón X | North |
| **Toggle rango ataque** | Botón Y | West |

## Mapeo de Input Manager

En Unity, los siguientes controles han sido configurados:

### Axes (Analógicos)
- **Horizontal** - Stick izquierdo X (eje 0)
- **Vertical** - Stick izquierdo Y (eje 1)
- **DPadX** - Cruceta horizontal (eje 6)
- **DPadY** - Cruceta vertical (eje 7)

### Buttons (Digitales)
- **Submit** - Retorno/Enter (teclado) + Botón 0 (mando) = Botón A
- **Cancel** - Escape (teclado) + Botón 1 (mando) = Botón B
- **Fire1** - Ctrl izquierdo (teclado) + Botón 2 (mando) = Botón X
- **Fire2** - Alt izquierdo (teclado) + Botón 1 (mando) = Botón Y

## Componentes Implementados

### 1. GamepadCursorController.cs
**Ubicación**: `Assets/Scripts/Adapters/GamepadCursorController.cs`

Controlador de cursor del mando con las siguientes características:
- Detección de entrada del stick analógico y cruceta
- Delay configurable entre movimientos (0.2s por defecto)
- Validación de límites del mapa (16x16)
- Eventos: `OnCursorMoved`, `OnConfirm`, `OnCancel`, `OnEndTurn`, `OnToggleAttackRange`

**Constantes principales**:
- `MOVEMENT_DELAY = 0.2f` - Delay entre movimientos
- `STICK_DEADZONE = 0.5f` - Zona muerta del stick analógico
- `MAP_WIDTH = 16` - Ancho del mapa
- `MAP_HEIGHT = 16` - Alto del mapa

**Métodos públicos**:
```csharp
void Initialize(IGameMap gameMap)
void Update()
public (int, int) CursorPosition { get; }
void SetCursorPosition(int x, int y)
bool IsValidPosition(int x, int y)
```

### 2. CursorRenderer.cs
**Ubicación**: `Assets/Scripts/Adapters/CursorRenderer.cs`

Renderizador visual del cursor con las siguientes características:
- Quad 3D con color amarillo semitransparente
- Animación pulsante (oscila alpha de 0.4 a 1.0)
- Interpolación suave de movimiento
- Elevación sobre el terreno para visibilidad

**Constantes principales**:
- `CURSOR_HEIGHT = 0.2f` - Altura sobre el terreno
- `MOVEMENT_SPEED = 5f` - Velocidad de interpolación
- `PULSE_SPEED = 2f` - Velocidad de parpadeo
- `CURSOR_SIZE = 0.9f` - Tamaño del quad

**Métodos públicos**:
```csharp
void Initialize()
void Update()
void UpdateCursorPosition(int gridX, int gridY)
void SetVisible(bool isVisible)
```

### 3. Modificaciones a GameController.cs
**Ubicación**: `Assets/Scripts/Adapters/GameController.cs`

Cambios realizados:
- Inicialización de `GamepadCursorController` y `CursorRenderer`
- Suscripción a eventos del mando
- Manejadores de eventos: `HandleGamepadCursorMoved`, `HandleGamepadConfirm`, `HandleGamepadCancel`, `HandleGamepadEndTurn`, `HandleGamepadToggleAttackRange`

**Nuevos manejadores**:
- `HandleGamepadCursorMoved()` - Actualiza posición visual del cursor
- `HandleGamepadConfirm()` - Simula clic en la posición actual del cursor
- `HandleGamepadCancel()` - Deselecciona la unidad actual
- `HandleGamepadEndTurn()` - Avanza fase si es turno del jugador
- `HandleGamepadToggleAttackRange()` - Alterna visualización de rangos enemigos

### 4. Modificaciones a InputHandler.cs
**Ubicación**: `Assets/Scripts/Adapters/InputHandler.cs`

Cambios realizados:
- Nuevo método `SimulateGamepadClick(int x, int y)` para disparar eventos de click desde el mando
- Mantenimiento de la entrada por ratón sin cambios

**Método público**:
```csharp
void SimulateGamepadClick(int x, int y)
```

### 5. Modificaciones a UIManager.cs
**Ubicación**: `Assets/Scripts/Adapters/UIManager.cs`

Cambios realizados:
- Nuevo campo `_infoMessageText` para mensajes informativos
- Nuevo método `ShowInfoMessage(string message)` para mostrar información al usuario
- Nuevo método `ClearInfoMessage()` privado para limpiar mensajes

**Métodos públicos**:
```csharp
void ShowInfoMessage(string message)
```

### 6. Tests Unitarios: GamepadCursorControllerTests.cs
**Ubicación**: `Assets/Scripts/Tests/GamepadCursorControllerTests.cs`

Suite de pruebas completa con 20+ tests unitarios:

**Tests de límites**:
- ✅ Cursor no sale del límite izquierdo (x=0)
- ✅ Cursor no sale del límite derecho (x=15)
- ✅ Cursor no sale del límite superior (y=0)
- ✅ Cursor no sale del límite inferior (y=15)

**Tests de validación de posición**:
- ✅ Posiciones dentro de rango son válidas
- ✅ Posiciones negativas son inválidas
- ✅ Posiciones fuera de rango son inválidas

**Tests de eventos**:
- ✅ OnCursorMoved se dispara al cambiar posición
- ✅ OnCursorMoved no se dispara si no hay cambio
- ✅ Todos los eventos pueden ser suscritos

**Tests de clamping**:
- ✅ SetCursorPosition clampea valores negativos a 0
- ✅ SetCursorPosition clampea valores altos a límite
- ✅ Clamping parcial funciona correctamente

## Flujo de Entrada del Mando

```
Update() en GamepadCursorController
├─ HandleCursorMovement()
│  ├─ Leer Input.GetAxis("Horizontal") / Input.GetAxis("Vertical")
│  ├─ Si deadzone < 0.5, leer D-Pad (DPadX / DPadY)
│  ├─ Clampear posición a límites [0..15]
│  └─ Disparar OnCursorMoved si cambió
│
└─ HandleButtonInput()
   ├─ Input.GetButtonDown("Submit") → OnConfirm
   ├─ Input.GetButtonDown("Cancel") → OnCancel
   ├─ Input.GetButtonDown("Fire1") → OnEndTurn
   └─ Input.GetButtonDown("Fire2") → OnToggleAttackRange
```

## Compatibilidad con Nintendo Switch Pro Controller

El código utiliza los ejes de entrada estándar de Unity, que son compatible con:
- ✅ Nintendo Switch Pro Controller (Bluetooth)
- ✅ Joy-Cons (Bluetooth)
- ✅ Cualquier controlador compatible con GameInput de Unity

**Macbook**: Asegúrese de que el mando esté emparejado via Bluetooth.

## Arquitectura Hexagonal

La implementación sigue estrictamente el patrón hexagonal:

### Domain Layer (Pura)
- `IGameMap` - Interface para validación de límites
- Lógica de juego sin dependencias de Unity

### Adapter Layer (MonoBehaviours)
- `GamepadCursorController` - Traduce input a eventos
- `CursorRenderer` - Renderiza cursor en Unity
- `GameController` - Orquesta dominio y adaptadores
- `InputHandler` - Centraliza entrada de usuario

### Separación de Responsabilidades
- Movimiento y validación en `GamepadCursorController`
- Rendering en `CursorRenderer`
- Orquestación en `GameController`
- Input user-facing en `InputHandler`

## SOLID Principles

### S - Single Responsibility
- `GamepadCursorController`: Solo maneja entrada y eventos
- `CursorRenderer`: Solo renderiza
- `GameController`: Solo orquesta

### O - Open/Closed
- Nuevos tipos de entrada pueden ser agregados sin modificar existentes
- Eventos son extensibles sin cambiar código base

### L - Liskov Substitution
- `IGameMap` es reemplazable en tests

### I - Interface Segregation
- `GamepadCursorController` solo expone lo que necesita
- Eventos son específicos y bien definidos

### D - Dependency Inversion
- Depende de `IGameMap`, no de implementación concreta
- Inyección de dependencias en `Initialize()`

## Constantes sin Magic Numbers

Todas las constantes están nombradas y documentadas:

```csharp
private const float MOVEMENT_DELAY = 0.2f;
private const float STICK_DEADZONE = 0.5f;
private const float CURSOR_HEIGHT = 0.2f;
private const float MOVEMENT_SPEED = 5f;
private const float PULSE_SPEED = 2f;
private const int MAP_WIDTH = 16;
private const int MAP_HEIGHT = 16;
// ... más constantes
```

## Instrucciones de Uso

### Para Jugadores

1. **Conectar Mando**: Emparejar Nintendo Switch Pro Controller vía Bluetooth
2. **Iniciar Juego**: Ejecutar la escena de juego
3. **Usar Controles**:
   - Mover cursor: Stick analógico izquierdo o Cruceta
   - Seleccionar: Botón A
   - Cancelar: Botón B
   - Fin de turno: Botón X
   - Ver rango enemigo: Botón Y

### Para Desarrolladores

1. **Ejecutar Tests**:
   ```bash
   # En Unity Test Runner
   Window > Testing > Test Runner
   # Ejecutar GamepadCursorControllerTests
   ```

2. **Debuggear**:
   - Usar `SetCursorPosition()` para teleportar cursor
   - Suscribirse a eventos para observar cambios
   - Revisar logs en Console

3. **Extender**:
   - Agregar nuevos eventos en `GamepadCursorController`
   - Crear nuevos manejadores en `GameController`
   - Mantener arquitectura hexagonal

## Cambios Realizados Resumen

| Archivo | Tipo | Cambios |
|---------|------|---------|
| **GamepadCursorController.cs** | ✨ Nuevo | Controlador del cursor del mando |
| **CursorRenderer.cs** | ✨ Nuevo | Renderizador visual del cursor |
| **GamepadCursorControllerTests.cs** | ✨ Nuevo | Suite de tests (20+ casos) |
| **GameController.cs** | ✏️ Modificado | Integración de mando |
| **InputHandler.cs** | ✏️ Modificado | Método SimulateGamepadClick |
| **UIManager.cs** | ✏️ Modificado | ShowInfoMessage |
| **InputManager.asset** | ✏️ Modificado | Ejes DPadX y DPadY |

## Validación Completa

✅ Cero magic numbers (todas constantes nombradas)
✅ Arquitectura hexagonal mantenida
✅ SOLID principles seguidos
✅ Tests unitarios completos
✅ Sin dependencias de Unity en dominio
✅ Ratón y mando coexisten sin conflictos
✅ Compatible con Nintendo Switch Pro Controller
✅ Clean Code

## Troubleshooting

### El cursor no se mueve
- ✓ Verificar que el mando está emparejado en Bluetooth
- ✓ Revisar que InputManager tiene ejes correctos
- ✓ Comprobar deadzone en Input Manager (< 0.5)

### El mando no responde
- ✓ Desconectar y reconectar el mando
- ✓ Revisar que Submit/Cancel están mapeados a botones correctos
- ✓ Verificar en Console si hay errores

### Cursor parpadea
- ✓ Es comportamiento normal (animación pulsante)
- ✓ Si es muy rápido, reducir PULSE_SPEED
- ✓ Si es muy lento, aumentar PULSE_SPEED

## Referencias

- [Unity Input System Documentation](https://docs.unity3d.com/ScriptReference/Input.html)
- [Nintendo Switch Pro Controller Compatibility](https://support.apple.com/en-us/HT210414)
- [Hexagonal Architecture Primer](https://alistair.cockburn.us/hexagonal-architecture/)
