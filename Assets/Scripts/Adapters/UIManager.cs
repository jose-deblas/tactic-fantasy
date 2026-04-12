using UnityEngine;
using UnityEngine.UI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters
{
    public class UIManager : MonoBehaviour
    {
        private Canvas _uiCanvas;
        private Text _turnCounterText;
        private Text _phaseIndicatorText;
        private Text _unitInfoText;
        private Text _combatResultText;
        private Text _infoMessageText;
        private GameObject _gameOverPanel;
        private Text _gameOverText;
        private Button _endTurnButton;
        private GameController _gameController;

        public void Awake()
        {
            _gameController = GetComponent<GameController>();
            CreateUICanvas();
        }

        private void CreateUICanvas()
        {
            GameObject canvasGO = new GameObject("UICanvas");
            canvasGO.transform.SetParent(null);

            _uiCanvas = canvasGO.AddComponent<Canvas>();
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;

            CreateTurnCounter();
            CreatePhaseIndicator();
            CreateUnitInfoPanel();
            CreateCombatResultText();
            CreateInfoMessageText();
            CreateEndTurnButton();
            CreateGameOverPanel();
        }

        private void CreateTurnCounter()
        {
            GameObject textGO = new GameObject("TurnCounter");
            textGO.transform.SetParent(_uiCanvas.transform);

            _turnCounterText = textGO.AddComponent<Text>();
            _turnCounterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _turnCounterText.text = "Turn: 1";
            _turnCounterText.fontSize = 30;
            _turnCounterText.fontStyle = FontStyle.Bold;
            _turnCounterText.alignment = TextAnchor.UpperCenter;
            _turnCounterText.color = Color.white;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(-100, -50);
            rt.offsetMax = new Vector2(100, 0);
        }

        private void CreatePhaseIndicator()
        {
            GameObject textGO = new GameObject("PhaseIndicator");
            textGO.transform.SetParent(_uiCanvas.transform);

            _phaseIndicatorText = textGO.AddComponent<Text>();
            _phaseIndicatorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _phaseIndicatorText.text = "PLAYER PHASE";
            _phaseIndicatorText.fontSize = 24;
            _phaseIndicatorText.alignment = TextAnchor.UpperCenter;
            _phaseIndicatorText.color = Color.cyan;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(-100, -100);
            rt.offsetMax = new Vector2(100, -50);
        }

        private void CreateUnitInfoPanel()
        {
            GameObject panelGO = new GameObject("UnitInfoPanel");
            panelGO.transform.SetParent(_uiCanvas.transform);

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0, 0);
            panelRT.anchorMax = new Vector2(0, 0);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = new Vector2(300, 200);

            GameObject textGO = new GameObject("UnitInfoText");
            textGO.transform.SetParent(panelGO.transform);

            _unitInfoText = textGO.AddComponent<Text>();
            _unitInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _unitInfoText.text = "Select a unit";
            _unitInfoText.fontSize = 14;
            _unitInfoText.alignment = TextAnchor.UpperLeft;
            _unitInfoText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 10);
            textRT.offsetMax = new Vector2(-10, -10);
        }

        private void CreateCombatResultText()
        {
            GameObject textGO = new GameObject("CombatResultText");
            textGO.transform.SetParent(_uiCanvas.transform);

            _combatResultText = textGO.AddComponent<Text>();
            _combatResultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _combatResultText.text = "";
            _combatResultText.fontSize = 18;
            _combatResultText.alignment = TextAnchor.MiddleCenter;
            _combatResultText.color = Color.yellow;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(-200, -50);
            rt.offsetMax = new Vector2(200, 50);
        }

        private void CreateEndTurnButton()
        {
            GameObject buttonGO = new GameObject("EndTurnButton");
            buttonGO.transform.SetParent(_uiCanvas.transform);

            Image buttonImage = buttonGO.AddComponent<Image>();
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // NEW: Better color scheme with hover effects
            var colors = button.colors;
            colors.normalColor = new Color(0.15f, 0.35f, 0.65f, 0.9f);      // Nice blue
            colors.highlightedColor = new Color(0.25f, 0.5f, 0.85f, 1f);    // Bright on hover
            colors.pressedColor = new Color(0.1f, 0.25f, 0.5f, 1f);         // Dark on press
            colors.selectedColor = new Color(0.2f, 0.4f, 0.7f, 0.95f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            button.colors = colors;

            // NEW: Larger size (200x50 vs 150x40)
            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.5f, 0);
            buttonRT.anchorMax = new Vector2(0.5f, 0);
            buttonRT.offsetMin = new Vector2(-100, 20);  // 200px wide
            buttonRT.offsetMax = new Vector2(100, 70);   // 50px tall

            // NEW: Add white border using Outline component
            var outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // Text (larger font)
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);

            Text buttonText = textGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = "End Turn";
            buttonText.fontSize = 20;  // Larger (was 16)
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            _endTurnButton = button;
            _endTurnButton.onClick.AddListener(() => _gameController?.EndPlayerPhase());
        }

        private void CreateGameOverPanel()
        {
            _gameOverPanel = new GameObject("GameOverPanel");
            _gameOverPanel.transform.SetParent(_uiCanvas.transform);

            Image panelImage = _gameOverPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.9f);

            RectTransform panelRT = _gameOverPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            GameObject textGO = new GameObject("GameOverText");
            textGO.transform.SetParent(_gameOverPanel.transform);

            _gameOverText = textGO.AddComponent<Text>();
            _gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _gameOverText.text = "Game Over";
            _gameOverText.fontSize = 50;
            _gameOverText.fontStyle = FontStyle.Bold;
            _gameOverText.alignment = TextAnchor.MiddleCenter;
            _gameOverText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            _gameOverPanel.SetActive(false);
        }

        public void UpdateSelectedUnitInfo(IUnit unit)
        {
            if (unit == null)
            {
                _unitInfoText.text = "Select a unit";
                return;
            }

            string statusLine = UnitDisplayFormatter.FormatStatus(unit);
            _unitInfoText.text = $"<b>{unit.Name}</b>\n" +
                $"Class: {unit.Class.Name}\n" +
                $"HP: {unit.CurrentHP}/{unit.MaxHP}\n" +
                $"STR: {unit.CurrentStats.STR} SPD: {unit.CurrentStats.SPD}\n" +
                $"DEF: {unit.CurrentStats.DEF} RES: {unit.CurrentStats.RES}\n" +
                $"Weapon: {unit.EquippedWeapon.Name}" +
                (statusLine.Length > 0 ? $"\n{statusLine}" : "");
        }

        private string BuildStatusLine(IUnit unit)
        {
            if (unit.ActiveStatus == null)
                return "";

            return unit.ActiveStatus.Type switch
            {
                StatusEffectType.Poison => $"☠ Poisoned ({unit.ActiveStatus.RemainingTurns} turns)",
                StatusEffectType.Sleep  => $"💤 Sleep ({unit.ActiveStatus.RemainingTurns} turns)",
                StatusEffectType.Stun   => $"⚡ Stun ({unit.ActiveStatus.RemainingTurns} turn)",
                _                       => ""
            };
        }

        public void ShowCombatResult(CombatResult result)
        {
            string resultText = result.Hit ? $"Hit! Damage: {result.Damage}" : "Miss!";
            if (result.IsCritical)
                resultText += " CRITICAL!";

            _combatResultText.text = resultText;
            CancelInvoke("ClearCombatResultText");
            Invoke("ClearCombatResultText", 2f);
        }

        private void ClearCombatResultText()
        {
            _combatResultText.text = "";
        }

        private void CreateInfoMessageText()
        {
            GameObject textGO = new GameObject("InfoMessageText");
            textGO.transform.SetParent(_uiCanvas.transform);

            _infoMessageText = textGO.AddComponent<Text>();
            _infoMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _infoMessageText.text = "";
            _infoMessageText.fontSize = 16;
            _infoMessageText.alignment = TextAnchor.MiddleCenter;
            _infoMessageText.color = new Color(0.2f, 1f, 1f); // Cyan claro

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(-200, 50);
            rt.offsetMax = new Vector2(200, 100);
        }

        public void ShowInfoMessage(string message)
        {
            if (_infoMessageText != null)
            {
                _infoMessageText.text = message;
                CancelInvoke("ClearInfoMessage");
                Invoke("ClearInfoMessage", 2f);
            }
        }

        private void ClearInfoMessage()
        {
            _infoMessageText.text = "";
        }

        public void UpdatePhaseDisplay(Phase phase, int turnCount)
        {
            _turnCounterText.text = $"Turn: {turnCount}";
            _phaseIndicatorText.text = phase == Phase.PlayerPhase ? "PLAYER PHASE" : "ENEMY PHASE";
            _phaseIndicatorText.color = phase == Phase.PlayerPhase ? Color.cyan : Color.red;
        }

        public void ShowGameOverScreen(GameState state, int turnCount)
        {
            _gameOverPanel.SetActive(true);

            string message = state == GameState.PlayerWon
                ? $"VICTORY!\nCompleted in {turnCount} turns"
                : "DEFEAT!\nAll units defeated";

            _gameOverText.text = message;
        }
    }
}
