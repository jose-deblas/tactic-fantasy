using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TacticFantasy.Domain.Map;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Controlador del cursor del mando (gamepad) usando el nuevo Input System.
    /// Permite mover un cursor por el mapa usando el stick analógico o cruceta.
    ///
    /// Arquitectura Hexagonal: Adapter que traduce entrada de mando a eventos de dominio.
    /// </summary>
    public class GamepadCursorController : MonoBehaviour
    {
        /// <summary>Evento disparado cuando el cursor se mueve a una nueva posición.</summary>
        public event Action<(int x, int y)> OnCursorMoved;

        /// <summary>Evento disparado cuando se presiona el botón de confirmación (A/South).</summary>
        public event Action OnConfirm;

        /// <summary>Evento disparado cuando se presiona el botón de cancelación (B/East).</summary>
        public event Action OnCancel;

        /// <summary>Evento disparado cuando se presiona el botón de fin de turno (X/West).</summary>
        public event Action OnEndTurn;

        /// <summary>Evento disparado cuando se presiona el botón para mostrar/ocultar rango de ataque (Y/North).</summary>
        public event Action OnToggleAttackRange;

        private const float MOVEMENT_DELAY = 0.2f;
        private const float STICK_DEADZONE = 0.5f;
        private const int MAP_WIDTH = 16;
        private const int MAP_HEIGHT = 16;

        private (int x, int y) _cursorPosition = (0, 0);
        private float _lastMoveTime = 0f;
        private IGameMap _gameMap;

        public (int x, int y) CursorPosition => _cursorPosition;

        public void Initialize(IGameMap gameMap)
        {
            _gameMap = gameMap ?? throw new ArgumentNullException(nameof(gameMap));
            _cursorPosition = (0, 0);
            _lastMoveTime = 0f;
        }

        public void Update()
        {
            if (_gameMap == null)
                return;

            HandleCursorMovement();
            HandleButtonInput();
        }

        private void HandleCursorMovement()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return;

            // Stick izquierdo o D-pad
            Vector2 stickInput = gamepad.leftStick.ReadValue();
            Vector2 dpadInput  = gamepad.dpad.ReadValue();

            float horizontalInput = Mathf.Abs(stickInput.x) > STICK_DEADZONE ? stickInput.x : dpadInput.x;
            float verticalInput   = Mathf.Abs(stickInput.y) > STICK_DEADZONE ? stickInput.y : dpadInput.y;

            bool hasMovementInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

            if (!hasMovementInput)
            {
                _lastMoveTime = 0f;
                return;
            }

            if (Time.time - _lastMoveTime < MOVEMENT_DELAY)
                return;

            int newX = _cursorPosition.x;
            int newY = _cursorPosition.y;

            if (Mathf.Abs(horizontalInput) > STICK_DEADZONE)
                newX += horizontalInput > 0 ? 1 : -1;

            if (Mathf.Abs(verticalInput) > STICK_DEADZONE)
                newY += verticalInput > 0 ? 1 : -1;

            newX = Mathf.Clamp(newX, 0, MAP_WIDTH - 1);
            newY = Mathf.Clamp(newY, 0, MAP_HEIGHT - 1);

            if (newX != _cursorPosition.x || newY != _cursorPosition.y)
            {
                _cursorPosition = (newX, newY);
                _lastMoveTime = Time.time;
                OnCursorMoved?.Invoke(_cursorPosition);
            }
        }

        private void HandleButtonInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return;

            if (gamepad.buttonSouth.wasPressedThisFrame)
                OnConfirm?.Invoke();

            if (gamepad.buttonEast.wasPressedThisFrame)
                OnCancel?.Invoke();

            if (gamepad.buttonWest.wasPressedThisFrame)
                OnEndTurn?.Invoke();

            if (gamepad.buttonNorth.wasPressedThisFrame)
                OnToggleAttackRange?.Invoke();
        }

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

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }
    }
}
