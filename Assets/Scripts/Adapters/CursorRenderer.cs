using UnityEngine;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Renderiza el cursor visual del mando en el mapa.
    /// Muestra un quad ligeramente elevado sobre el mapa con animación pulsante.
    /// Se mueve suavemente interpolando entre posiciones.
    ///
    /// Arquitectura Hexagonal: Adapter que renderiza el cursor del dominio.
    /// </summary>
    public class CursorRenderer : MonoBehaviour
    {
        // Constantes nombradas
        private const float TILE_SIZE = 1f;
        private const float CURSOR_HEIGHT = 0.2f; // Elevación del cursor sobre el terreno
        private const float MOVEMENT_SPEED = 5f; // Velocidad de interpolación suave
        private const float PULSE_SPEED = 2f; // Velocidad de animación pulsante
        private const float PULSE_MIN_ALPHA = 0.4f;
        private const float PULSE_MAX_ALPHA = 1f;
        private const float CURSOR_SIZE = 0.9f; // Tamaño del quad del cursor

        private GameObject _cursorGameObject;
        private Material _cursorMaterial;
        private Vector3 _targetPosition;
        private Vector3 _currentPosition;
        private bool _isInitialized = false;

        /// <summary>
        /// Inicializa el renderer del cursor.
        /// Crea el GameObject del cursor y lo configura visualmente.
        /// </summary>
        public void Initialize()
        {
            CreateCursorVisual();
            _isInitialized = true;
        }

        /// <summary>
        /// Actualiza el renderer del cursor cada frame.
        /// Maneja la interpolación suave y la animación pulsante.
        /// </summary>
        public void Update()
        {
            if (!_isInitialized || _cursorGameObject == null)
                return;

            // Interpolación suave de la posición
            _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, Time.deltaTime * MOVEMENT_SPEED);
            _cursorGameObject.transform.position = _currentPosition;

            // Animación pulsante de alpha
            float pulse = Mathf.Lerp(PULSE_MIN_ALPHA, PULSE_MAX_ALPHA,
                (Mathf.Sin(Time.time * PULSE_SPEED) + 1f) / 2f);

            Color color = _cursorMaterial.color;
            color.a = pulse;
            _cursorMaterial.color = color;
        }

        /// <summary>
        /// Actualiza la posición del cursor basada en coordenadas del grid.
        /// </summary>
        /// <param name="gridX">Coordenada X del grid (0..15)</param>
        /// <param name="gridY">Coordenada Y del grid (0..15)</param>
        public void UpdateCursorPosition(int gridX, int gridY)
        {
            _targetPosition = new Vector3(
                gridX * TILE_SIZE + TILE_SIZE / 2f,
                CURSOR_HEIGHT,
                gridY * TILE_SIZE + TILE_SIZE / 2f
            );
        }

        /// <summary>
        /// Muestra o oculta el cursor.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            if (_cursorGameObject != null)
            {
                _cursorGameObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Crea el GameObject visual del cursor.
        /// Consiste en un quad ligeramente elevado con color blanco semitransparente.
        /// </summary>
        private void CreateCursorVisual()
        {
            _cursorGameObject = new GameObject("GamepadCursor");
            _cursorGameObject.transform.SetParent(transform);
            _targetPosition = Vector3.zero;
            _currentPosition = Vector3.zero;

            // Crear el mesh del cursor (quad)
            Mesh quadMesh = new Mesh();
            float halfSize = CURSOR_SIZE / 2f;

            quadMesh.vertices = new Vector3[]
            {
                new Vector3(-halfSize, 0, -halfSize),
                new Vector3(halfSize, 0, -halfSize),
                new Vector3(halfSize, 0, halfSize),
                new Vector3(-halfSize, 0, halfSize)
            };

            quadMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            quadMesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            quadMesh.RecalculateNormals();

            // Agregar mesh filter y renderer
            MeshFilter meshFilter = _cursorGameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = quadMesh;

            MeshRenderer meshRenderer = _cursorGameObject.AddComponent<MeshRenderer>();
            _cursorMaterial = new Material(Shader.Find("Standard"));

            // Color blanco amarillento ligeramente semitransparente
            _cursorMaterial.color = new Color(1f, 1f, 0.3f, PULSE_MAX_ALPHA);

            // Hacer el material transparente
            _cursorMaterial.SetFloat("_Mode", 3);
            _cursorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _cursorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _cursorMaterial.SetInt("_ZWrite", 0);
            _cursorMaterial.DisableKeyword("_ALPHATEST_ON");
            _cursorMaterial.EnableKeyword("_ALPHABLEND_ON");
            _cursorMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _cursorMaterial.renderQueue = 3000;

            meshRenderer.material = _cursorMaterial;

            // Agregar collider para que no interfiera con clicks
            // (usar un pequeño collider)
            SphereCollider collider = _cursorGameObject.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            collider.isTrigger = true;
        }

        /// <summary>
        /// Limpia los recursos cuando se destruye el componente.
        /// </summary>
        private void OnDestroy()
        {
            if (_cursorGameObject != null)
            {
                Destroy(_cursorGameObject);
            }
            if (_cursorMaterial != null)
            {
                Destroy(_cursorMaterial);
            }
        }
    }
}
