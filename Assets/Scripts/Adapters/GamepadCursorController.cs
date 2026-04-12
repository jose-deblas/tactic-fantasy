using System;
using UnityEngine;
using TacticFantasy.Domain.Map;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Controlador del cursor del mando (gamepad).
    /// Permite mover un cursor por el mapa usando el stick analógico o cruceta.
    /// Emite eventos de confirmación, cancelación y fin de turno.
    ///
    /// Arquitectura Hexagonal: Adapter que traduce entrada de mando a eventos de dominio.
    /// </summary>
    public class GamepadCursorController : MonoBehaviour
    {
        /// <summary>Evento disparado cuando el cursor se mueve a una nueva posición.</summary>
        public event Action<(int x, int y)> OnCursorMoved;

        /// <summary>Evento disparado cuando se presiona el botón de confirmación (A).</summary>
        public event Action OnConfirm;

        /// <summary>Evento disparado cuando se presiona el botón de cancelación (B).</summary>
        public event Action OnCancel;

        /// <summary>Evento disparado cuando se presiona el botón de fin de turno (X).</summary>
        public event Action OnEndTurn;

        /// <summary>Evento disparado cuando se presiona el botón para mostrar/ocultar rango de ataque (Y).</summary>
        public event Action OnToggleAttackRange;

        // Constantes nombradas
        private const float MOVEMENT_DELAY = 0.2f;
        private const float STICK_DEADZONE = 0.5f;
        private const int MAP_WIDTH = 16;
        private const int MAP_HEIGHT = 16;

        // Axis names para Input Manager de Unity
        private const string HORIZONTAL_AXIS = "Horizontal";
        private const string VERTICAL_AXIS = "Vertical";
        private const string DPAD_X_AXIS = "DPadX";
        private const string DPAD_Y_AXIS = "DPadY";

        // Botones del mando
        private const string CONFIRM_BUTTON = "Submit"; // A (East)
        private const string CANCEL_BUTTON = "Cancel";  // B (South)
        private const string END_TURN_BUTTON = "Fire1";  // X (North)
        private const string TOGGLE_ATTACK_BUTTON = "Fire2"; // Y (West)

        private (int x, int y) _cursorPosition = (0, 0);
        private float _lastMoveTime = 0f;
        private IGameMap _gameMap;

        /// <summary>
        /// Obtiene la posición actual del cursor.
        /// </summary>
        public (int x, int y) CursorPosition => _cursorPosition;

        /// <summary>
        /// Inicializa el controlador del cursor con la referencia del mapa.
        /// </summary>
        /// <param name="gameMap">Referencia a IGameMap para validación de límites.</param>
        public void Initialize(IGameMap gameMap)
        {
            _gameMap = gameMap ?? throw new ArgumentNullException(nameof(gameMap));
            _cursorPosition = (0, 0);
            _lastMoveTime = 0f;
        }

        /// <summary>
        /// Actualiza el estado del controlador cada frame.
        /// Maneja movimiento del cursor y pulsaciones de botones.
        /// </summary>
        public void Update()
        {
            if (_gameMap == null)
                return;

            HandleCursorMovement();
            HandleButtonInput();
        }

        /// <summary>
        /// Maneja el movimiento del cursor usando stick analógico o cruceta.
        /// Implementa delay entre movimientos para evitar movimientos muy rápidos.
        /// </summary>
        private void HandleCursorMovement()
        {
            float horizontalInput = Input.GetAxis(HORIZONTAL_AXIS);
            float verticalInput = Input.GetAxis(VERTICAL_AXIS);

            // Si no hay entrada en stick, revisar D-pad
            if (Mathf.Abs(horizontalInput) < STICK_DEADZONE && Mathf.Abs(verticalInput) < STICK_DEADZONE)
            {
                horizontalInput = Input.GetAxis(DPAD_X_AXIS);
                verticalInput = Input.GetAxis(DPAD_Y_AXIS);
            }

            // Detectar si hay input de movimiento
            bool hasMovementInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

            if (!hasMovementInput)
            {
                _lastMoveTime = 0f; // Reset el delay cuando no hay input
                return;
            }

            // Aplicar delay entre movimientos
            if (Time.time - _lastMoveTime < MOVEMENT_DELAY)
                return;

            int newX = _cursorPosition.x;
            int newY = _cursorPosition.y;

            // Preferir input discreto sobre analógico continuo
            if (Mathf.Abs(horizontalInput) > STICK_DEADZONE)
            {
                newX += horizontalInput > 0 ? 1 : -1;
            }

            if (Mathf.Abs(verticalInput) > STICK_DEADZONE)
            {
                newY += verticalInput > 0 ? 1 : -1;
            }

            // Clampear a límites del mapa
            newX = Mathf.Clamp(newX, 0, MAP_WIDTH - 1);
            newY = Mathf.Clamp(newY, 0, MAP_HEIGHT - 1);

            // Si la posición cambió, disparar evento
            if (newX != _cursorPosition.x || newY != _cursorPosition.y)
            {
                _cursorPosition = (newX, newY);
                _lastMoveTime = Time.time;
                OnCursorMoved?.Invoke(_cursorPosition);
            }
        }

        /// <summary>
        /// Maneja las pulsaciones de botones del mando.
        /// </summary>
        private void HandleButtonInput()
        {
            if (Input.GetButtonDown(CONFIRM_BUTTON))
            {
                OnConfirm?.Invoke();
            }

            if (Input.GetButtonDown(CANCEL_BUTTON))
            {
                OnCancel?.Invoke();
            }

            if (Input.GetButtonDown(END_TURN_BUTTON))
            {
                OnEndTurn?.Invoke();
            }

            if (Input.GetButtonDown(TOGGLE_ATTACK_BUTTON))
            {
                OnToggleAttackRange?.Invoke();
            }
        }

        /// <summary>
        /// Establece la posición del cursor de forma programática.
        /// Útil para inicializar o teleportar el cursor.
        /// </summary>
        /// <param name="x">Coordenada X (será clampeada a [0, MAP_WIDTH-1])</param>
        /// <param name="y">Coordenada Y (será clampeada a [0, MAP_HEIGHT-1])</param>
        public void SetCursorPosition(int x, int y)
        {
            int clampedX = Mathf.Clamp(x, 0, MAP_WIDTH - 1);
            int clampedY = Mathf.Clamp(y, 0, MAP_HEIGHT - 1);

            if (_cursorPosition.x != clampedX || _cursorPosition.y != clampedY)
            {
                _cursorPosition = (clampedX, clampedY);
                OnCursorMoved?.Invoke(_cursorPosition);
            }
        }

        /// <summary>
        /// Valida si una posición está dentro de los límites del mapa.
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }
    }
}
