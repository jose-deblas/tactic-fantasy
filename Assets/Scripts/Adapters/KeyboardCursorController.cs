using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TacticFantasy.Domain.Map;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Keyboard-driven cursor controller. Mirrors GamepadCursorController API so
    /// `GameController` can subscribe to the same events and reuse renderers/logic.
    /// </summary>
    public class KeyboardCursorController : MonoBehaviour
    {
        public event Action<(int x, int y)> OnCursorMoved;
        public event Action OnConfirm;
        public event Action OnCancel;
        public event Action OnToggleAttackRange;

        private const float MOVEMENT_DELAY = 0.2f;
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
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            int dx = 0;
            int dy = 0;

            // Horizontal movement: immediate on key down or repeat while held
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                dx = -1;
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                dx = 1;
            else if ((keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) || (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed))
            {
                if (Time.time - _lastMoveTime >= MOVEMENT_DELAY)
                {
                    if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) dx = -1;
                    else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) dx = 1;
                }
            }

            // Vertical movement
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                dy = 1;
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                dy = -1;
            else if ((keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) || (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed))
            {
                if (Time.time - _lastMoveTime >= MOVEMENT_DELAY)
                {
                    if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) dy = 1;
                    else if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) dy = -1;
                }
            }

            if (dx != 0 || dy != 0)
            {
                int newX = Mathf.Clamp(_cursorPosition.x + dx, 0, MAP_WIDTH - 1);
                int newY = Mathf.Clamp(_cursorPosition.y + dy, 0, MAP_HEIGHT - 1);

                if (newX != _cursorPosition.x || newY != _cursorPosition.y)
                {
                    _cursorPosition = (newX, newY);
                    _lastMoveTime = Time.time;
                    OnCursorMoved?.Invoke(_cursorPosition);
                }
            }
        }

        private void HandleButtonInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame)
                OnConfirm?.Invoke();

            if (keyboard.xKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
                OnCancel?.Invoke();

            if (keyboard.rKey.wasPressedThisFrame)
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
