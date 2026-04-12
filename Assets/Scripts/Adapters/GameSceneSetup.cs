using UnityEngine;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// This script sets up the GameScene with all necessary components.
    /// Place this on a GameObject in the scene to initialize the game.
    /// </summary>
    [ExecuteInEditMode]
    public class GameSceneSetup : MonoBehaviour
    {
        public void Start()
        {
            if (FindFirstObjectByType<GameController>() == null)
            {
                SetupGameScene();
            }
        }

        private void SetupGameScene()
        {
            // Destruir la cámara existente en la escena para evitar duplicados
            var existingCamera = FindFirstObjectByType<Camera>();
            if (existingCamera != null)
                DestroyImmediate(existingCamera.gameObject);

            // GameController y adaptadores
            GameObject gameControllerGO = new GameObject("GameController");
            gameControllerGO.transform.position = Vector3.zero;

            gameControllerGO.AddComponent<GameController>();
            gameControllerGO.AddComponent<MapRenderer>();
            gameControllerGO.AddComponent<UnitRenderer>();
            gameControllerGO.AddComponent<UIManager>();
            gameControllerGO.AddComponent<InputHandler>();

            // Cámara: posicionada para ver el mapa 16x16 completo desde arriba-isométrico
            // Centro del mapa: (7.5, 0, 7.5). Cámara elevada y ligeramente inclinada.
            GameObject cameraGO = new GameObject("MainCamera");
            cameraGO.tag = "MainCamera";
            cameraGO.transform.position = new Vector3(7.5f, 22f, -2f);
            cameraGO.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f); // casi negro-azulado
            camera.farClipPlane = 200f;
            camera.fieldOfView = 50f;

            // Luz direccional
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = Color.white;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}
