using System.Collections.Generic;
using UnityEngine;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters
{
    public class MapRenderer : MonoBehaviour
    {
        private IGameMap _map;
        private GameObject[,] _tileGameObjects;
        private IUnit _selectedUnit;
        private HashSet<(int, int)> _movementRange = new HashSet<(int, int)>();
        private HashSet<(int, int)> _attackRange = new HashSet<(int, int)>();

        private const float TILE_SIZE = 1f;
        private const float TILE_HEIGHT = 0.1f;

        public void Initialize(IGameMap map)
        {
            _map = map;
            _tileGameObjects = new GameObject[map.Width, map.Height];
            RenderMap();
        }

        private void RenderMap()
        {
            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    var tile = _map.GetTile(x, y);
                    CreateTileVisual(tile, x, y);
                }
            }
        }

        private void CreateTileVisual(ITile tile, int x, int y)
        {
            GameObject tileGO = new GameObject($"Tile_{x}_{y}");
            tileGO.transform.SetParent(transform);
            tileGO.transform.position = new Vector3(x * TILE_SIZE, 0, y * TILE_SIZE);

            Mesh quadMesh = new Mesh();
            quadMesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(TILE_SIZE, 0, 0),
                new Vector3(TILE_SIZE, 0, TILE_SIZE),
                new Vector3(0, 0, TILE_SIZE)
            };
            quadMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            quadMesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            quadMesh.RecalculateNormals();

            MeshFilter meshFilter = tileGO.AddComponent<MeshFilter>();
            meshFilter.mesh = quadMesh;

            MeshRenderer meshRenderer = tileGO.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = GetTerrainColor(tile.Terrain);

            MeshCollider meshCollider = tileGO.AddComponent<MeshCollider>();
            meshCollider.convex = false;

            _tileGameObjects[x, y] = tileGO;
        }

        public void SetSelectedUnit(IUnit unit)
        {
            _selectedUnit = unit;
            UpdateTileHighlights();
        }

        public void SetMovementRange(HashSet<(int, int)> range)
        {
            _movementRange = new HashSet<(int, int)>(range);
            UpdateTileHighlights();
        }

        public void SetAttackRange(HashSet<(int, int)> range)
        {
            _attackRange = new HashSet<(int, int)>(range);
            UpdateTileHighlights();
        }

        private void UpdateTileHighlights()
        {
            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    var tile = _map.GetTile(x, y);
                    var tileGO = _tileGameObjects[x, y];
                    var meshRenderer = tileGO.GetComponent<MeshRenderer>();

                    if (_selectedUnit != null && x == _selectedUnit.Position.x && y == _selectedUnit.Position.y)
                    {
                        meshRenderer.material.color = Color.yellow;
                    }
                    else if (_movementRange.Contains((x, y)))
                    {
                        meshRenderer.material.color = Color.blue;
                    }
                    else if (_attackRange.Contains((x, y)))
                    {
                        meshRenderer.material.color = Color.red;
                    }
                    else
                    {
                        meshRenderer.material.color = GetTerrainColor(tile.Terrain);
                    }
                }
            }
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Plain => new Color(0.2f, 0.8f, 0.2f),
                TerrainType.Forest => new Color(0.1f, 0.5f, 0.1f),
                TerrainType.Fort => new Color(0.9f, 0.8f, 0.2f),
                TerrainType.Mountain => new Color(0.5f, 0.5f, 0.5f),
                TerrainType.Wall => new Color(0.2f, 0.2f, 0.2f),
                _ => Color.white
            };
        }
    }
}
