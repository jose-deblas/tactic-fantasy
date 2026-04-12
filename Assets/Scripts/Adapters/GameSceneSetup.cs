using UnityEngine;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Inicializa la escena 2D en runtime.
    /// Crea el GameController y todos sus adapters.
    /// La cámara ya está configurada en la escena (orthographic, centrada en 7.5,7.5).
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        public void Start()
        {
            if (FindFirstObjectByType<GameController>() == null)
                SetupGame();
        }

        private void SetupGame()
        {
            var go = new GameObject("GameController");
            go.AddComponent<GameController>();
            go.AddComponent<MapRenderer>();
            go.AddComponent<UnitRenderer>();
            go.AddComponent<UIManager>();
            go.AddComponent<InputHandler>();
            go.AddComponent<GamepadCursorController>();
            go.AddComponent<CursorRenderer>();
        }
    }
}
