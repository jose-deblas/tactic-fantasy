using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Gestor centralizado de entrada del usuario (ratón).
    /// Usa el nuevo Input System de Unity 6.
    /// Emite eventos que son procesados por GameController.
    ///
    /// Arquitectura Hexagonal: Adapter que traduce entrada de usuario a eventos de dominio.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        /// <summary>Evento disparado cuando se hace clic en una casilla.</summary>
        public event Action<int, int> OnTileClicked;

        /// <summary>Evento disparado cuando se hace clic en una unidad.</summary>
        public event Action<int, int> OnUnitClicked;

        private Camera _mainCamera;
        private const float TILE_SIZE = 1f;

        public void Start()
        {
            _mainCamera = Camera.main;
        }

        public void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPoint = hit.point;
                int x = Mathf.RoundToInt(hitPoint.x / TILE_SIZE);
                int y = Mathf.RoundToInt(hitPoint.z / TILE_SIZE);

                OnTileClicked?.Invoke(x, y);
                OnUnitClicked?.Invoke(x, y);
            }
        }

        /// <summary>
        /// Emite un evento de tile clickeado para ser usado por el mando.
        /// </summary>
        public void SimulateGamepadClick(int x, int y)
        {
            OnTileClicked?.Invoke(x, y);
            OnUnitClicked?.Invoke(x, y);
        }
    }
}
