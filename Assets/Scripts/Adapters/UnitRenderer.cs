using System.Collections.Generic;
using UnityEngine;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters
{
    public class UnitRenderer : MonoBehaviour
    {
        private Dictionary<int, GameObject> _unitVisuals = new Dictionary<int, GameObject>();
        private const float TILE_SIZE = 1f;
        private const float UNIT_RADIUS = 0.3f;

        public void UpdateAllUnits(List<IUnit> units)
        {
            foreach (var unit in units)
            {
                UpdateUnitVisual(unit);
            }

            var unitsToRemove = new List<int>();
            foreach (var unitId in _unitVisuals.Keys)
            {
                if (!units.Exists(u => u.Id == unitId))
                {
                    unitsToRemove.Add(unitId);
                }
            }

            foreach (var unitId in unitsToRemove)
            {
                if (_unitVisuals.TryGetValue(unitId, out var go))
                {
                    Destroy(go);
                    _unitVisuals.Remove(unitId);
                }
            }
        }

        private void UpdateUnitVisual(IUnit unit)
        {
            if (!_unitVisuals.TryGetValue(unit.Id, out var unitGO))
            {
                unitGO = CreateUnitVisual(unit);
                _unitVisuals[unit.Id] = unitGO;
            }

            unitGO.transform.position = new Vector3(unit.Position.x * TILE_SIZE + TILE_SIZE * 0.5f, 0.5f, unit.Position.y * TILE_SIZE + TILE_SIZE * 0.5f);
            unitGO.name = unit.Name;

            var hpBar = unitGO.GetComponentInChildren<Canvas>();
            if (hpBar != null)
            {
                UpdateHPBar(hpBar, unit);
            }

            var meshRenderer = unitGO.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material.color = GetUnitColor(unit);
            }
        }

        private Color GetUnitColor(IUnit unit)
        {
            if (!unit.IsAlive)
                return Color.gray;

            // Status tints override team color
            if (unit.ActiveStatus != null)
            {
                return unit.ActiveStatus.Type switch
                {
                    StatusEffectType.Poison => new Color(0.6f, 0.2f, 0.8f),   // purple-ish
                    StatusEffectType.Sleep  => new Color(0.4f, 0.8f, 1.0f),   // pale blue
                    StatusEffectType.Stun   => new Color(1.0f, 0.85f, 0.1f),  // yellow
                    _                       => unit.Team == Team.PlayerTeam ? Color.blue : Color.red
                };
            }

            return unit.Team == Team.PlayerTeam ? Color.blue : Color.red;
        }

        private GameObject CreateUnitVisual(IUnit unit)
        {
            GameObject unitGO = new GameObject($"Unit_{unit.Name}");
            unitGO.transform.SetParent(transform);

            Mesh sphereMesh = new Mesh();
            int meridians = 16;
            int parallels = 8;
            Vector3[] vertices = new Vector3[(meridians + 1) * (parallels + 1)];
            int[] triangles = new int[meridians * parallels * 6];

            for (int p = 0; p <= parallels; p++)
            {
                float phi = Mathf.PI * p / parallels;
                for (int m = 0; m <= meridians; m++)
                {
                    float theta = 2 * Mathf.PI * m / meridians;
                    int idx = p * (meridians + 1) + m;
                    vertices[idx] = new Vector3(
                        UNIT_RADIUS * Mathf.Sin(phi) * Mathf.Cos(theta),
                        UNIT_RADIUS * Mathf.Cos(phi),
                        UNIT_RADIUS * Mathf.Sin(phi) * Mathf.Sin(theta)
                    );
                }
            }

            int triIdx = 0;
            for (int p = 0; p < parallels; p++)
            {
                for (int m = 0; m < meridians; m++)
                {
                    int a = p * (meridians + 1) + m;
                    int b = a + 1;
                    int c = (p + 1) * (meridians + 1) + m;
                    int d = c + 1;

                    triangles[triIdx++] = a;
                    triangles[triIdx++] = c;
                    triangles[triIdx++] = b;

                    triangles[triIdx++] = b;
                    triangles[triIdx++] = c;
                    triangles[triIdx++] = d;
                }
            }

            sphereMesh.vertices = vertices;
            sphereMesh.triangles = triangles;
            sphereMesh.RecalculateNormals();

            MeshFilter meshFilter = unitGO.AddComponent<MeshFilter>();
            meshFilter.mesh = sphereMesh;

            MeshRenderer meshRenderer = unitGO.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = unit.Team == Team.PlayerTeam ? Color.blue : Color.red;

            unitGO.AddComponent<SphereCollider>();

            CreateHPBar(unitGO, unit);

            return unitGO;
        }

        private void CreateHPBar(GameObject unitGO, IUnit unit)
        {
            GameObject canvasGO = new GameObject("HPCanvas");
            canvasGO.transform.SetParent(unitGO.transform);
            canvasGO.transform.localPosition = Vector3.up * 0.8f;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(0.8f, 0.1f);
            canvasRT.localScale = Vector3.one * 0.005f;

            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.black;
            RectTransform bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(canvasGO.transform);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            UnitHPBar hpBar = canvasGO.AddComponent<UnitHPBar>();
            hpBar.SetFillImage(fillImage);
            hpBar.SetUnit(unit);
        }

        private void UpdateHPBar(Canvas canvas, IUnit unit)
        {
            var hpBar = canvas.GetComponent<UnitHPBar>();
            if (hpBar != null)
            {
                hpBar.UpdateHP();
            }
        }
    }

    public class UnitHPBar : MonoBehaviour
    {
        private Image _fillImage;
        private IUnit _unit;

        public void SetFillImage(Image fillImage)
        {
            _fillImage = fillImage;
        }

        public void SetUnit(IUnit unit)
        {
            _unit = unit;
        }

        public void UpdateHP()
        {
            if (_unit != null && _fillImage != null)
            {
                float fillAmount = (float)_unit.CurrentHP / _unit.MaxHP;
                _fillImage.fillAmount = fillAmount;
            }
        }
    }
}
