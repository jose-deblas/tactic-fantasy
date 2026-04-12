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
            // Destruir objetos existentes que puedan interferir
            var existingCamera = FindFirstObjectByType<Camera>();
            if (existingCamera != null)
                DestroyImmediate(existingCamera.gameObject);

            var existingLight = FindFirstObjectByType<Light>();
            if (existingLight != null)
                DestroyImmediate(existingLight.gameObject);

            // GameController y adaptadores
            GameObject gameControllerGO = new GameObject("GameController");
            gameControllerGO.transform.position = Vector3.zero;
            gameControllerGO.AddComponent<GameController>();
            gameControllerGO.AddComponent<MapRenderer>();
            gameControllerGO.AddComponent<UnitRenderer>();
            gameControllerGO.AddComponent<UIManager>();
            gameControllerGO.AddComponent<InputHandler>();

            // Cámara ortográfica cenital - ve el mapa 16x16 completo siempre
            // Mapa va de (0,0,0) a (15,0,15). Centro = (7.5, 0, 7.5)
            GameObject cameraGO = new GameObject("MainCamera");
            cameraGO.tag = "MainCamera";

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;          // cubre 18 unidades de alto = mapa de 16 + margen
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            // Posición: directamente encima del centro del mapa, mirando hacia abajo
            cameraGO.transform.position = new Vector3(7.5f, 30f, 7.5f);
            cameraGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

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
