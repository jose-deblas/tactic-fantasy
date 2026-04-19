using System.Collections.Generic;
using UnityEngine;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Renderiza el mapa en 2D usando sprites coloreados en el plano XY.
    /// Cada tile es un quad con SpriteRenderer. Plano Z=0.
    /// </summary>
    public class MapRenderer : MonoBehaviour
    {
        private IGameMap _map;
        private GameObject[,] _tileObjects;
        private IUnit _selectedUnit;
        private HashSet<(int, int)> _movementRange = new HashSet<(int, int)>();
        private HashSet<(int, int)> _attackRange  = new HashSet<(int, int)>();
        private IFogOfWar _fogOfWar;
        private Team _viewingTeam = Team.PlayerTeam;

        private const float TILE_SIZE = 1f;

        public void Initialize(IGameMap map)
        {
            _map = map;
            _tileObjects = new GameObject[map.Width, map.Height];
            RenderMap();
        }

        private void RenderMap()
        {
            for (int x = 0; x < _map.Width; x++)
                for (int y = 0; y < _map.Height; y++)
                    CreateTile(x, y);
        }

        private void CreateTile(int x, int y)
        {
            var tile = _map.GetTile(x, y);

            var go = new GameObject($"Tile_{x}_{y}");
            go.transform.SetParent(transform);
            // En 2D usamos XY. Y invertida para que (0,0) quede arriba-izquierda visualmente
            go.transform.position = new Vector3(x * TILE_SIZE, y * TILE_SIZE, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite   = CreateSquareSprite();
            sr.color    = GetTerrainColor(tile.Terrain);
            sr.sortingOrder = 0;

            // Añadir borde sutil para mejorar la legibilidad del mapa
            var borderGo = new GameObject($"TileBorder_{x}_{y}");
            borderGo.transform.SetParent(go.transform);
            borderGo.transform.localPosition = Vector3.zero;
            var borderSr = borderGo.AddComponent<SpriteRenderer>();
            borderSr.sprite = CreateBorderSprite();
            borderSr.color = new Color(0f, 0f, 0f, 0.15f); // negro muy tenue
            borderSr.sortingOrder = 1; // encima del tile base

            // Collider 2D para raycast del ratón
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * 0.98f; // ligero gap entre tiles

            _tileObjects[x, y] = go;
        }

        public void SetSelectedUnit(IUnit unit)
        {
            _selectedUnit = unit;
            UpdateHighlights();
        }

        public void SetMovementRange(HashSet<(int, int)> range)
        {
            _movementRange = new HashSet<(int, int)>(range);
            UpdateHighlights();
        }

        public void SetAttackRange(HashSet<(int, int)> range)
        {
            _attackRange = new HashSet<(int, int)>(range);
            UpdateHighlights();
        }

        public void SetFogOfWar(IFogOfWar fog, Team viewingTeam)
        {
            _fogOfWar = fog;
            _viewingTeam = viewingTeam;
            UpdateHighlights();
        }

        private void UpdateHighlights()
        {
            if (_tileObjects == null) return;

            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    var sr = _tileObjects[x, y].GetComponent<SpriteRenderer>();
                    var tile = _map.GetTile(x, y);

                    if (_selectedUnit != null &&
                        x == _selectedUnit.Position.x && y == _selectedUnit.Position.y)
                        sr.color = Color.yellow;
                    else if (_movementRange.Contains((x, y)))
                        sr.color = new Color(0.3f, 0.5f, 1f, 0.85f);
                    else if (_attackRange.Contains((x, y)))
                        sr.color = new Color(1f, 0.3f, 0.3f, 0.85f);
                    else
                    {
                        var baseColor = GetTerrainColor(tile.Terrain);
                        if (_fogOfWar != null && !_fogOfWar.IsTileVisible(x, y, _viewingTeam))
                            baseColor *= 0.3f;
                        sr.color = baseColor;
                    }
                }
            }
        }

        private Color GetTerrainColor(TerrainType terrain) => terrain switch
        {
            TerrainType.Plain    => new Color(0.25f, 0.75f, 0.25f),
            TerrainType.Forest   => new Color(0.1f,  0.45f, 0.1f),
            TerrainType.Fort     => new Color(0.85f, 0.75f, 0.2f),
            TerrainType.Mountain => new Color(0.55f, 0.55f, 0.55f),
            TerrainType.Wall     => new Color(0.2f,  0.2f,  0.2f),
            TerrainType.Door     => new Color(0.45f, 0.25f, 0.1f),
            TerrainType.Chest    => new Color(0.9f,  0.75f, 0.1f),
            TerrainType.Throne   => new Color(0.55f, 0.2f,  0.6f),
            TerrainType.Desert   => new Color(0.85f, 0.75f, 0.5f),
            TerrainType.Bridge   => new Color(0.6f,  0.45f, 0.25f),
            _                    => Color.white
        };

        // Sprite cuadrado blanco 1x1 generado en runtime
        private static Sprite _cachedSquare;
        private static Sprite CreateSquareSprite()
        {
            if (_cachedSquare != null) return _cachedSquare;

            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _cachedSquare = Sprite.Create(tex,
                new Rect(0, 0, 2, 2),
                new Vector2(0.5f, 0.5f),
                2f); // pixelsPerUnit = 2 → sprite de 1 unidad Unity
            return _cachedSquare;
        }

        // Sprite para borde tenue: centro transparente, 1px de borde negro (en runtime)
        private static Sprite _cachedBorder;
        private static Sprite CreateBorderSprite()
        {
            if (_cachedBorder != null) return _cachedBorder;

            // Textura 4x4 para permitir un borde claro alrededor del centro
            var size = 4;
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];

            for (int iy = 0; iy < size; iy++)
            for (int ix = 0; ix < size; ix++)
            {
                // Si estamos en el borde, poner negro; si no, transparente
                bool isBorder = ix == 0 || iy == 0 || ix == size - 1 || iy == size - 1;
                pixels[iy * size + ix] = isBorder ? Color.black : new Color(0,0,0,0);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedBorder = Sprite.Create(tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size); // pixelsPerUnit = size → sprite de 1 unidad
            return _cachedBorder;
        }
    }
}
