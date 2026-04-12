using System;
using UnityEngine;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Gestor centralizado de entrada del usuario.
    /// Maneja tanto input de ratón como de mando (coexistencia simultánea).
    /// Emite eventos que son procesados por GameController.
    ///
    /// Arquitectura Hexagonal: Adapter que traduce entrada de usuario a eventos.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        /// <summary>Evento disparado cuando se hace clic en una casilla (ratón o confirmación del mando).</summary>
        public event Action<int, int> OnTileClicked;

        /// <summary>Evento disparado cuando se hace clic en una unidad (ratón o confirmación del mando).</summary>
        public event Action<int, int> OnUnitClicked;

        private Camera _mainCamera;
        private const float TILE_SIZE = 1f;

        public void Start()
        {
            _mainCamera = Camera.main;
        }

        public void Update()
        {
            // Manejar entrada de ratón (no interfiere con el mando)
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        /// <summary>
        /// Maneja el clic del ratón detectando qué tile o unidad fue clickeado.
        /// </summary>
        private void HandleMouseClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

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
        /// Permite que GamepadCursorController dispare eventos como si fuera un clic.
        /// </summary>
        public void SimulateGamepadClick(int x, int y)
        {
            OnTileClicked?.Invoke(x, y);
            OnUnitClicked?.Invoke(x, y);
        }
    }
}
