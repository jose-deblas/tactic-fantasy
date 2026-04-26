using System;
using UnityEngine;
using UnityEngine.UI;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Adapters
{
    public class InventoryActionResult
    {
        public enum ActionType { None, Use, Equip, Give }
        public ActionType Action { get; }
        public IItem Item { get; }

        public InventoryActionResult(ActionType action, IItem item)
        {
            Action = action;
            Item = item;
        }
    }

    public static class InventoryWindow
    {
        public static void Show(Canvas parentCanvas, IUnit unit, Action<InventoryActionResult> onAction, Action<InventoryActionResult> onClose)
        {
            if (parentCanvas == null || unit == null) return;

            GameObject panel = new GameObject("InventoryWindow");
            panel.transform.SetParent(parentCanvas.transform, false);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.04f, 0.04f, 0.12f, 0.95f);

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.offsetMin = new Vector2(-320, -200);
            prt.offsetMax = new Vector2(320, 200);

            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panel.transform, false);
            Text title = titleGO.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.text = unit.Name + " - Bolsa";
            title.alignment = TextAnchor.UpperCenter;
            title.fontSize = 22;
            title.color = Color.white;

            RectTransform trt = titleGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(10, -42);
            trt.offsetMax = new Vector2(-10, -6);

            // Quick summary line under title
            GameObject summaryGO = new GameObject("Summary");
            summaryGO.transform.SetParent(panel.transform, false);
            Text summary = summaryGO.AddComponent<Text>();
            summary.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            summary.text = $"Equipo: {unit.Class.Name}  |  Nivel: {unit.Level}  |  AP: {unit.CurrentHP}/{unit.MaxHP}";
            summary.alignment = TextAnchor.UpperCenter;
            summary.fontSize = 12;
            summary.color = new Color(0.8f, 0.8f, 0.9f);

            RectTransform srt = summaryGO.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 1);
            srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(10, -64);
            srt.offsetMax = new Vector2(-10, -46);

            // Items list
            var items = unit.Inventory.GetAll();
            if (items.Count == 0)
            {
                GameObject noItems = new GameObject("NoItems");
                noItems.transform.SetParent(panel.transform, false);
                Text t = noItems.AddComponent<Text>();
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.text = "(No items)";
                t.alignment = TextAnchor.MiddleCenter;
                t.fontSize = 16;
                t.color = Color.gray;

                RectTransform nrt = noItems.GetComponent<RectTransform>();
                nrt.anchorMin = new Vector2(0, 0);
                nrt.anchorMax = new Vector2(1, 1);
                nrt.offsetMin = new Vector2(10, 10);
                nrt.offsetMax = new Vector2(-10, -50);
            }
            else
            {
                float startY = 80f;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    GameObject row = new GameObject($"ItemRow_{i}");
                    row.transform.SetParent(panel.transform, false);

                    RectTransform rrt = row.AddComponent<RectTransform>();
                    rrt.anchorMin = new Vector2(0, 0.5f);
                    rrt.anchorMax = new Vector2(1, 0.5f);
                    rrt.offsetMin = new Vector2(12, startY - (i * 40) - 20);
                    rrt.offsetMax = new Vector2(-12, startY - (i * 40) + 16);

                    Text itemText = row.AddComponent<Text>();
                    itemText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    // Show equipped marker
                    var equippedMarker = "";
                    if (item is IWeapon w && unit.EquippedWeapon != null && ReferenceEquals(unit.EquippedWeapon, w))
                        equippedMarker = " (Equipped)";

                    // Display uses as remaining/total. Use infinity symbol for unlimited (-1).
                    string remainingStr = item.CurrentUses == -1 ? "∞" : item.CurrentUses.ToString();
                    string totalStr = item.MaxUses == -1 ? "∞" : item.MaxUses.ToString();
                    itemText.text = $"{item.Name}{equippedMarker}  —  {remainingStr}/{totalStr}";
                    itemText.alignment = TextAnchor.MiddleLeft;
                    itemText.fontSize = 16;
                    itemText.color = Color.white;

                    // Right-side buttons container
                    GameObject btnContainer = new GameObject("BtnContainer");
                    btnContainer.transform.SetParent(row.transform, false);
                    RectTransform bcrt = btnContainer.AddComponent<RectTransform>();
                    bcrt.anchorMin = new Vector2(1, 0.5f);
                    bcrt.anchorMax = new Vector2(1, 0.5f);
                    bcrt.offsetMin = new Vector2(-220, -14);
                    bcrt.offsetMax = new Vector2(-6, 14);

                    // Use button
                    if (item.IsUsable)
                    {
                        var btnGO = new GameObject("UseButton");
                        btnGO.transform.SetParent(btnContainer.transform, false);
                        Button btn = btnGO.AddComponent<Button>();
                        Image img = btnGO.AddComponent<Image>();
                        img.color = new Color(0.15f, 0.45f, 0.15f, 1f);
                        RectTransform brt = btnGO.GetComponent<RectTransform>();
                        brt.anchorMin = new Vector2(1, 0.5f);
                        brt.anchorMax = new Vector2(1, 0.5f);
                        brt.offsetMin = new Vector2(-200 + (i*0), -12);
                        brt.offsetMax = new Vector2(-140 + (i*0), 12);

                        Text btxt = new GameObject("Text").AddComponent<Text>();
                        btxt.transform.SetParent(btnGO.transform, false);
                        btxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        btxt.text = "Usar";
                        btxt.alignment = TextAnchor.MiddleCenter;
                        btxt.color = Color.white;

                        btn.onClick.AddListener(() => { onClose?.Invoke(new InventoryActionResult(InventoryActionResult.ActionType.Use, item)); GameObject.Destroy(panel); });
                    }

                    // Equip button for weapons (does not close window)
                    if (item is IWeapon)
                    {
                        var btnGO = new GameObject("EquipButton");
                        btnGO.transform.SetParent(btnContainer.transform, false);
                        Button btn = btnGO.AddComponent<Button>();
                        Image img = btnGO.AddComponent<Image>();
                        img.color = new Color(0.12f, 0.2f, 0.45f, 1f);
                        RectTransform brt = btnGO.GetComponent<RectTransform>();
                        brt.anchorMin = new Vector2(1, 0.5f);
                        brt.anchorMax = new Vector2(1, 0.5f);
                        brt.offsetMin = new Vector2(-130 + (i*0), -12);
                        brt.offsetMax = new Vector2(-70 + (i*0), 12);

                        Text btxt = new GameObject("Text").AddComponent<Text>();
                        btxt.transform.SetParent(btnGO.transform, false);
                        btxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        btxt.text = "Equipar";
                        btxt.alignment = TextAnchor.MiddleCenter;
                        btxt.color = Color.white;

                        btn.onClick.AddListener(() =>
                        {
                            // Invoke action callback so caller can equip immediately without closing
                            onAction?.Invoke(new InventoryActionResult(InventoryActionResult.ActionType.Equip, item));

                            // Update visual marker to show equipped
                            if (item is IWeapon iw)
                            {
                                // Display uses as remaining/total. Use infinity symbol for unlimited (-1).
                                string remainingStrEq = item.CurrentUses == -1 ? "∞" : item.CurrentUses.ToString();
                                string totalStrEq = item.MaxUses == -1 ? "∞" : item.MaxUses.ToString();
                                itemText.text = $"{item.Name} (Equipped)  —  {remainingStrEq}/{totalStrEq}";
                            }
                        });
                    }

                    // Small 'Give' placeholder (disabled if no adjacent allies)
                    var giveBtn = new GameObject("GiveButton");
                    giveBtn.transform.SetParent(btnContainer.transform, false);
                    var giveImage = giveBtn.AddComponent<Image>();
                    giveImage.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    var giveRect = giveBtn.GetComponent<RectTransform>();
                    giveRect.anchorMin = new Vector2(1, 0.5f);
                    giveRect.anchorMax = new Vector2(1, 0.5f);
                    giveRect.offsetMin = new Vector2(-60, -12);
                    giveRect.offsetMax = new Vector2(-10, 12);

                    var giveText = new GameObject("Text").AddComponent<Text>();
                    giveText.transform.SetParent(giveBtn.transform, false);
                    giveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    giveText.text = "Dar";
                    giveText.alignment = TextAnchor.MiddleCenter;
                    giveText.color = Color.white;

                    // Note: Give not implemented fully here; call onAction if needed
                    var giveButtonComp = giveBtn.AddComponent<Button>();
                    giveButtonComp.onClick.AddListener(() => { onAction?.Invoke(new InventoryActionResult(InventoryActionResult.ActionType.Give, item)); });
                }
            }

            // Close button
            GameObject closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(panel.transform, false);
            Button closeBtn = closeGO.AddComponent<Button>();
            Image closeImg = closeGO.AddComponent<Image>();
            closeImg.color = new Color(0.4f, 0.1f, 0.1f, 1f);
            RectTransform crt = closeGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0);
            crt.anchorMax = new Vector2(0.5f, 0);
            crt.offsetMin = new Vector2(-60, 10);
            crt.offsetMax = new Vector2(60, 40);

            Text ctext = new GameObject("Text").AddComponent<Text>();
            ctext.transform.SetParent(closeGO.transform, false);
            ctext.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ctext.text = "Cerrar";
            ctext.alignment = TextAnchor.MiddleCenter;
            ctext.color = Color.white;

            closeBtn.onClick.AddListener(() => { onClose?.Invoke(new InventoryActionResult(InventoryActionResult.ActionType.None, null)); GameObject.Destroy(panel); });
        }
    }
}
