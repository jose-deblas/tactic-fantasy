using System.Collections.Generic;
using UnityEngine;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters
{
    /// <summary>
    /// Renderiza las unidades en 2D como círculos coloreados con barra de HP.
    /// Plano Z=-1 para que queden por delante de los tiles (Z=0).
    /// </summary>
    public class UnitRenderer : MonoBehaviour
    {
        private Dictionary<int, GameObject> _unitVisuals = new Dictionary<int, GameObject>();

        private const float UNIT_RADIUS  = 0.35f;
        private const float TILE_SIZE    = 1f;
        private const float HP_BAR_WIDTH = 0.8f;
        private const float HP_BAR_HEIGHT= 0.1f;

        public void UpdateAllUnits(List<IUnit> units)
        {
            var seen = new HashSet<int>();

            foreach (var unit in units)
            {
                seen.Add(unit.Id);
                if (unit.IsAlive)
                    UpdateUnit(unit);
                else
                    RemoveUnit(unit.Id);
            }

            // Limpiar unidades que ya no existen
            var toRemove = new List<int>();
            foreach (var id in _unitVisuals.Keys)
                if (!seen.Contains(id)) toRemove.Add(id);
            foreach (var id in toRemove) RemoveUnit(id);
        }

        private void UpdateUnit(IUnit unit)
        {
            if (!_unitVisuals.TryGetValue(unit.Id, out var go))
                go = CreateUnitVisual(unit);

            // Posición: mismo XY que el tile, Z=-1 (por delante)
            go.transform.position = new Vector3(
                unit.Position.x * TILE_SIZE,
                unit.Position.y * TILE_SIZE,
                -1f);

            // Color según equipo + tinte de status
            var sr = go.GetComponent<SpriteRenderer>();
            sr.color = GetUnitColor(unit);

            // Actualizar HP bar
            UpdateHPBar(go, unit);
        }

        private GameObject CreateUnitVisual(IUnit unit)
        {
            var go = new GameObject($"Unit_{unit.Id}_{unit.Name}");
            go.transform.SetParent(transform);

            // Círculo usando un sprite de disco
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = CreateCircleSprite(32);
            sr.color        = GetUnitColor(unit);
            sr.sortingOrder = 1;
            go.transform.localScale = Vector3.one * UNIT_RADIUS * 2f;

            // HP bar como hijo
            CreateHPBar(go, unit);

            _unitVisuals[unit.Id] = go;
            return go;
        }

        private void CreateHPBar(GameObject parent, IUnit unit)
        {
            var bar = new GameObject("HPBar");
            bar.transform.SetParent(parent.transform);
            bar.transform.localPosition = new Vector3(0f, 0.65f, 0f); // encima del círculo
            bar.transform.localScale    = Vector3.one;

            // Fondo (gris)
            var bg = new GameObject("BG");
            bg.transform.SetParent(bar.transform);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale    = new Vector3(HP_BAR_WIDTH, HP_BAR_HEIGHT, 1f);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite       = CreateSquareSprite();
            bgSr.color        = new Color(0.2f, 0.2f, 0.2f);
            bgSr.sortingOrder = 2;

            // Relleno (verde)
            var fill = new GameObject("Fill");
            fill.transform.SetParent(bar.transform);
            fill.transform.localPosition = new Vector3(-HP_BAR_WIDTH / 2f + HP_BAR_WIDTH / 2f, 0f, 0f);
            fill.transform.localScale    = new Vector3(HP_BAR_WIDTH, HP_BAR_HEIGHT, 1f);
            var fillSr = fill.AddComponent<SpriteRenderer>();
            fillSr.sprite       = CreateSquareSprite();
            fillSr.color        = Color.green;
            fillSr.sortingOrder = 3;
        }

        private void UpdateHPBar(GameObject go, IUnit unit)
        {
            var bar  = go.transform.Find("HPBar");
            if (bar == null) return;
            var fill = bar.Find("Fill");
            if (fill == null) return;

            float ratio = (float)unit.CurrentHP / unit.MaxHP;
            ratio = Mathf.Clamp01(ratio);

            // Escalar horizontalmente desde la izquierda
            var s = fill.localScale;
            fill.localScale = new Vector3(HP_BAR_WIDTH * ratio, s.y, s.z);
            fill.localPosition = new Vector3(
                -HP_BAR_WIDTH / 2f + (HP_BAR_WIDTH * ratio) / 2f,
                0f, 0f);

            var fillSr = fill.GetComponent<SpriteRenderer>();
            fillSr.color = ratio > 0.5f ? Color.green :
                           ratio > 0.25f ? Color.yellow : Color.red;
        }

        private void RemoveUnit(int id)
        {
            if (_unitVisuals.TryGetValue(id, out var go))
            {
                Destroy(go);
                _unitVisuals.Remove(id);
            }
        }

        private Color GetUnitColor(IUnit unit)
        {
            // Tinte base por equipo
            Color baseColor = unit.Team == Team.PlayerTeam
                ? new Color(0.2f, 0.4f, 1f)   // azul
                : new Color(1f,   0.2f, 0.2f); // rojo

            // Tinte de status
            if (unit.ActiveStatus != null)
            {
                baseColor = unit.ActiveStatus.Type switch
                {
                    StatusEffectType.Poison => Color.Lerp(baseColor, new Color(0.6f, 0.1f, 0.6f), 0.5f),
                    StatusEffectType.Sleep  => Color.Lerp(baseColor, new Color(0.5f, 0.5f, 1f),   0.5f),
                    StatusEffectType.Stun   => Color.Lerp(baseColor, Color.yellow,                  0.4f),
                    _                       => baseColor
                };
            }

            return baseColor;
        }

        // --- Sprites generados en runtime ---

        private static Sprite _squareSprite;
        private static Sprite CreateSquareSprite()
        {
            if (_squareSprite != null) return _squareSprite;
            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return _squareSprite;
        }

        private static Sprite CreateCircleSprite(int resolution)
        {
            var tex = new Texture2D(resolution, resolution);
            var pixels = new Color[resolution * resolution];
            float center = resolution / 2f;
            float radius = resolution / 2f - 1f;

            for (int i = 0; i < pixels.Length; i++)
            {
                int px = i % resolution;
                int py = i / resolution;
                float dx = px - center;
                float dy = py - center;
                pixels[i] = (dx * dx + dy * dy <= radius * radius) ? Color.white : Color.clear;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f),
                resolution);
        }
    }
}
