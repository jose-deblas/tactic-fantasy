using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Gestiona la entrada del ratón en 2D.
    /// Usa Physics2D.Raycast con la cámara ortográfica para detectar tiles.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public event Action<int, int> OnTileClicked;
        public event Action<int, int> OnUnitClicked;
        public event Action OnEndTurnPressed;  // NEW: Keyboard shortcut for end turn

        private Camera _mainCamera;
        private const float TILE_SIZE = 1f;

        public void Start()
        {
            _mainCamera = Camera.main;
        }

        public void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                HandleMouseClick();

            // NEW: Space or Enter to end turn
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    OnEndTurnPressed?.Invoke();
                }
            }
        }

        private void HandleMouseClick()
        {
            if (_mainCamera == null) return;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 worldPos    = _mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreen.x, mouseScreen.y, 0f));

            // Redondear a coordenadas de grid
            int x = Mathf.RoundToInt(worldPos.x / TILE_SIZE);
            int y = Mathf.RoundToInt(worldPos.y / TILE_SIZE);

            OnTileClicked?.Invoke(x, y);
            OnUnitClicked?.Invoke(x, y);
        }

        public void SimulateGamepadClick(int x, int y)
        {
            OnTileClicked?.Invoke(x, y);
            OnUnitClicked?.Invoke(x, y);
        }
    }
}
