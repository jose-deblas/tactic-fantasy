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
            if (FindObjectOfType<GameController>() == null)
            {
                SetupGameScene();
            }
        }

        private void SetupGameScene()
        {
            GameObject gameControllerGO = new GameObject("GameController");
            gameControllerGO.transform.position = Vector3.zero;

            GameController gameController = gameControllerGO.AddComponent<GameController>();
            gameControllerGO.AddComponent<MapRenderer>();
            gameControllerGO.AddComponent<UnitRenderer>();
            gameControllerGO.AddComponent<UIManager>();
            gameControllerGO.AddComponent<InputHandler>();

            GameObject cameraGO = new GameObject("MainCamera");
            cameraGO.transform.position = new Vector3(8, 15, 8);
            cameraGO.transform.rotation = Quaternion.Euler(45, -45, 0);

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            camera.farClipPlane = 100;

            cameraGO.tag = "MainCamera";

            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(45, -45, 0);
        }
    }
}
