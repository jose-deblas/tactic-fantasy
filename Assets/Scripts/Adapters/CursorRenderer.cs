using UnityEngine;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Cursor visual del mando en 2D.
    /// Un borde cuadrado pulsante sobre el tile seleccionado. Z=-2 (por delante de unidades).
    /// </summary>
    public class CursorRenderer : MonoBehaviour
    {
        private GameObject _cursor;
        private SpriteRenderer _sr;
        private const float TILE_SIZE   = 1f;
        private const float PULSE_SPEED = 2f;

        public void Initialize()
        {
            _cursor = new GameObject("GamepadCursor");
            _cursor.transform.SetParent(transform);

            _sr = _cursor.AddComponent<SpriteRenderer>();
            _sr.sprite       = CreateBorderSprite();
            _sr.color        = new Color(1f, 1f, 0.2f, 0.9f);
            _sr.sortingOrder = 4;

            _cursor.transform.localScale = Vector3.one * TILE_SIZE;
            _cursor.SetActive(false);
        }

        public void Update()
        {
            if (_cursor == null || !_cursor.activeSelf) return;
            float alpha = Mathf.Lerp(0.4f, 1f, (Mathf.Sin(Time.time * PULSE_SPEED) + 1f) / 2f);
            var c = _sr.color;
            c.a = alpha;
            _sr.color = c;
        }

        public void UpdateCursorPosition(int gridX, int gridY)
        {
            if (_cursor == null) return;
            _cursor.transform.position = new Vector3(gridX * TILE_SIZE, gridY * TILE_SIZE, -2f);
            _cursor.SetActive(true);
        }

        public void SetVisible(bool visible)
        {
            if (_cursor != null) _cursor.SetActive(visible);
        }

        // Sprite de borde cuadrado (hueco en el centro)
        private static Sprite CreateBorderSprite()
        {
            int size   = 32;
            int border = 3;
            var tex    = new Texture2D(size, size);
            var pixels = new Color[size * size];

            for (int i = 0; i < pixels.Length; i++)
            {
                int px = i % size;
                int py = i / size;
                bool onBorder = px < border || px >= size - border ||
                                py < border || py >= size - border;
                pixels[i] = onBorder ? Color.white : Color.clear;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
