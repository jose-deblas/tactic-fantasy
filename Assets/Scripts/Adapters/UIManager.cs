using UnityEngine;
using UnityEngine.UI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Adapters
{
    public class UIManager : MonoBehaviour
    {
        private Canvas _uiCanvas;
        private Text _turnPhaseText;
        private Text _unitInfoText;
        private Text _combatResultText;
        private Text _infoMessageText;
        private GameObject _gameOverPanel;
        private Text _gameOverText;
        private GameController _gameController;
        private GameObject _forecastPanel;
        private Text _forecastText;
        private GameObject _modalMenuPanel;
        private Button _menuEndTurnButton;
        private Button _menuSaveGameButton;
        private Button _menuExitButton;
        private Text _terrainInfoText;
        private GameObject _endTurnPrompt;
        private Button _endTurnPromptButton;
        private GameObject _turnInterstitialPanel;
        private Text _turnInterstitialText;
        private Button _turnInterstitialButton;
        private Text _versionText;

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

            CreateTurnPhaseHeader();
            CreateUnitInfoPanel();
            CreateCombatResultText();
            CreateInfoMessageText();
            CreateTerrainInfoText();
            CreateEndTurnPrompt();
            CreateGameOverPanel();
            CreateForecastPanel();
            CreateModalMenuPanel();
            CreateTurnInterstitialPanel();
            CreateVersionText();
        }

        private void CreateTurnPhaseHeader()
        {
            GameObject textGO = new GameObject("TurnPhaseHeader");
            textGO.transform.SetParent(_uiCanvas.transform);

            _turnPhaseText = textGO.AddComponent<Text>();
            _turnPhaseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _turnPhaseText.text = "Turn: 1, Player Phase";
            _turnPhaseText.fontSize = 26;
            _turnPhaseText.fontStyle = FontStyle.Bold;
            _turnPhaseText.alignment = TextAnchor.UpperCenter;
            _turnPhaseText.color = Color.white;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(-200, -45);
            rt.offsetMax = new Vector2(200, -5);
        }

        private void CreateUnitInfoPanel()
        {
            GameObject panelGO = new GameObject("UnitInfoPanel");
            panelGO.transform.SetParent(_uiCanvas.transform);

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            // NEW: Top-left anchor with compact size
            panelRT.anchorMin = new Vector2(0, 1);
            panelRT.anchorMax = new Vector2(0, 1);
            panelRT.offsetMin = new Vector2(10, -160);    // (left, bottom)
            panelRT.offsetMax = new Vector2(230, -10);    // (right, top) = 220x150 size

            GameObject textGO = new GameObject("UnitInfoText");
            textGO.transform.SetParent(panelGO.transform);

            _unitInfoText = textGO.AddComponent<Text>();
            _unitInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _unitInfoText.text = "Select a unit";
            _unitInfoText.fontSize = 12;  // Slightly smaller for compact panel
            _unitInfoText.alignment = TextAnchor.UpperLeft;
            _unitInfoText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(8, 8);
            textRT.offsetMax = new Vector2(-8, -8);
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

            // Team color for unit name
            string nameColor = unit.Team == Team.PlayerTeam ? "<color=#4488ff>" : "<color=#ff4444>";
            string nameLine = $"{nameColor}<b>{unit.Name}</b></color>";

            // Weapon emoji
            string weaponEmoji = unit.EquippedWeapon.Type switch
            {
                WeaponType.SWORD => "⚔️",
                WeaponType.LANCE => "🗡️",
                WeaponType.AXE => "🪓",
                WeaponType.FIRE => "🔥",
                WeaponType.BOW => "🏹",
                WeaponType.STAFF => "✨",
                WeaponType.REFRESH => "🎵",
                _ => "?"
            };

            // XP bar (current XP / 100 until level up)
            float xpPercent = (unit.Experience % 100) / 100f;
            int xpBars = (int)(xpPercent * 10);
            string xpBar = new string('█', xpBars) + new string('░', 10 - xpBars);

            _unitInfoText.text = $"{nameLine}\n" +
                $"<b>LVL {unit.Level}</b> {unit.Class.Name}\n" +
                $"HP: {unit.CurrentHP}/{unit.MaxHP}\n" +
                $"STR: {unit.CurrentStats.STR} SPD: {unit.CurrentStats.SPD}\n" +
                $"DEF: {unit.CurrentStats.DEF} RES: {unit.CurrentStats.RES}\n" +
                $"{weaponEmoji} {unit.EquippedWeapon.Name}\n" +
                $"XP: {xpBar}" +
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

        private void CreateTerrainInfoText()
        {
            GameObject textGO = new GameObject("TerrainInfoText");
            textGO.transform.SetParent(_uiCanvas.transform);

            _terrainInfoText = textGO.AddComponent<Text>();
            _terrainInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _terrainInfoText.text = "";
            _terrainInfoText.fontSize = 13;
            _terrainInfoText.alignment = TextAnchor.LowerLeft;
            _terrainInfoText.color = new Color(0.7f, 0.9f, 0.7f); // Soft green

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.offsetMin = new Vector2(10, 10);
            rt.offsetMax = new Vector2(300, 80);
        }

        /// <summary>
        /// Shows terrain information for a tile at the bottom-left.
        /// </summary>
        public void ShowTerrainInfo(TerrainType terrain)
        {
            if (_terrainInfoText == null) return;

            string terrainName = $"<color={GetTerrainColor(terrain)}>{terrain.ToString()}</color>";
            int moveCost = TerrainProperties.GetMovementCost(terrain, MoveType.Infantry);
            int defensBonus = TerrainProperties.GetDefenseBonus(terrain);
            int avoidBonus = TerrainProperties.GetAvoidBonus(terrain);
            int healPercent = TerrainProperties.GetHealPercent(terrain);

            string moveCostStr = moveCost == int.MaxValue ? "—" : moveCost.ToString();

            // Friendly icons and colored numeric values for quicker scanning
            string movePart = moveCost == int.MaxValue ? "—" : $"<color=#ffd27f>{moveCostStr}</color>"; // golden for move cost
            string avoidPart = $"<color=#a0e7ff>{avoidBonus}</color>"; // cyan for avoid
            string defPart = defensBonus > 0 ? $"<color=#bdecb6>{defensBonus}</color>" : null; // green for def
            string healPart = healPercent > 0 ? $"<color=#ff9b9b>{healPercent}%</color>" : null; // light red for heal

            // Emojis for quick recognition
            string info = $"{terrainName}\n" +
                          $"🚶 Move: {movePart} | 🌀 Avoid: {avoidPart}";

            if (defPart != null)
                info += $" | 🛡️ Def: {defPart}";

            if (healPart != null)
                info += $" | ❤️ Heal: {healPart}";

            _terrainInfoText.text = info;
            CancelInvoke("ClearTerrainInfo");
            Invoke("ClearTerrainInfo", 3f);
        }

        private string GetTerrainColor(TerrainType terrain)
{
    return terrain switch
    {
        TerrainType.Plain => "#87CEEB", // Soft blue
        TerrainType.Forest => "#228B22", // Forest green
        TerrainType.Mountain => "#A9A9A9", // Dark gray
        TerrainType.Fort => "#DAA520", // Goldenrod
        _ => "#FFFFFF", // Default white
    };
}

private void ClearTerrainInfo()
        {
            _terrainInfoText.text = "";
        }

        public void UpdatePhaseDisplay(Phase phase, int turnCount)
        {
            string phaseName = phase == Phase.PlayerPhase ? "Player Phase" : "Enemy Phase";
            _turnPhaseText.text = $"Turn: {turnCount}, {phaseName}";
            _turnPhaseText.color = phase == Phase.PlayerPhase ? Color.white : new Color(1f, 0.6f, 0.6f);
        }

        private void CreateEndTurnPrompt()
        {
            _endTurnPrompt = new GameObject("EndTurnPrompt");
            _endTurnPrompt.transform.SetParent(_uiCanvas.transform);

            GameObject buttonGO = new GameObject("EndTurnButton");
            buttonGO.transform.SetParent(_endTurnPrompt.transform);

            Image buttonImage = buttonGO.AddComponent<Image>();
            _endTurnPromptButton = buttonGO.AddComponent<Button>();
            _endTurnPromptButton.targetGraphic = buttonImage;

            var colors = _endTurnPromptButton.colors;
            colors.normalColor = new Color(0.15f, 0.35f, 0.65f, 0.9f);
            colors.highlightedColor = new Color(0.25f, 0.5f, 0.85f, 1f);
            colors.pressedColor = new Color(0.1f, 0.25f, 0.5f, 1f);
            _endTurnPromptButton.colors = colors;

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.5f, 0);
            buttonRT.anchorMax = new Vector2(0.5f, 0);
            buttonRT.offsetMin = new Vector2(-110, 20);
            buttonRT.offsetMax = new Vector2(110, 70);

            var outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);

            Text buttonText = textGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = "End Turn";
            buttonText.fontSize = 22;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            _endTurnPromptButton.onClick.AddListener(() => _gameController?.EndPlayerPhase());
            _endTurnPrompt.SetActive(false);
        }

        public void ShowEndTurnPrompt()
        {
            if (_endTurnPrompt != null)
                _endTurnPrompt.SetActive(true);
        }

        public void HideEndTurnPrompt()
        {
            if (_endTurnPrompt != null)
                _endTurnPrompt.SetActive(false);
        }

        private void CreateTurnInterstitialPanel()
        {
            _turnInterstitialPanel = new GameObject("TurnInterstitialPanel");
            _turnInterstitialPanel.transform.SetParent(_uiCanvas.transform);

            Image panelImage = _turnInterstitialPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.85f);

            RectTransform panelRT = _turnInterstitialPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Turn number text
            GameObject textGO = new GameObject("TurnInterstitialText");
            textGO.transform.SetParent(_turnInterstitialPanel.transform);

            _turnInterstitialText = textGO.AddComponent<Text>();
            _turnInterstitialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _turnInterstitialText.text = "Turn 1";
            _turnInterstitialText.fontSize = 60;
            _turnInterstitialText.fontStyle = FontStyle.Bold;
            _turnInterstitialText.alignment = TextAnchor.MiddleCenter;
            _turnInterstitialText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.offsetMin = new Vector2(-200, 10);
            textRT.offsetMax = new Vector2(200, 80);

            // Start button
            GameObject buttonGO = new GameObject("StartButton");
            buttonGO.transform.SetParent(_turnInterstitialPanel.transform);

            Image buttonImage = buttonGO.AddComponent<Image>();
            _turnInterstitialButton = buttonGO.AddComponent<Button>();
            _turnInterstitialButton.targetGraphic = buttonImage;

            var colors = _turnInterstitialButton.colors;
            colors.normalColor = new Color(0.1f, 0.5f, 0.2f, 0.9f);
            colors.highlightedColor = new Color(0.15f, 0.65f, 0.3f, 1f);
            colors.pressedColor = new Color(0.08f, 0.35f, 0.15f, 1f);
            _turnInterstitialButton.colors = colors;

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRT.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRT.offsetMin = new Vector2(-80, -60);
            buttonRT.offsetMax = new Vector2(80, -10);

            var outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(buttonGO.transform);

            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.text = "Start";
            btnText.fontSize = 24;
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;

            _turnInterstitialPanel.SetActive(false);
        }

        private void CreateVersionText()
        {
            GameObject textGO = new GameObject("VersionText");
            textGO.transform.SetParent(_uiCanvas.transform);

            _versionText = textGO.AddComponent<Text>();
            _versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _versionText.text = "v2.5";
            _versionText.fontSize = 12;
            _versionText.alignment = TextAnchor.LowerRight;
            _versionText.color = new Color(1f, 1f, 1f, 0.5f);

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.offsetMin = new Vector2(-80, 5);
            rt.offsetMax = new Vector2(-5, 25);
        }

        public void ShowTurnInterstitial(int turnNumber, System.Action onStart)
        {
            if (_turnInterstitialPanel == null) return;

            _turnInterstitialText.text = $"Turn {turnNumber}";
            _turnInterstitialButton.onClick.RemoveAllListeners();
            _turnInterstitialButton.onClick.AddListener(() =>
            {
                _turnInterstitialPanel.SetActive(false);
                onStart?.Invoke();
            });
            _turnInterstitialPanel.SetActive(true);
        }

        public void HideTurnInterstitial()
        {
            if (_turnInterstitialPanel != null)
                _turnInterstitialPanel.SetActive(false);
        }

        public bool IsTurnInterstitialOpen()
        {
            return _turnInterstitialPanel != null && _turnInterstitialPanel.activeSelf;
        }

        private void CreateForecastPanel()
        {
            _forecastPanel = new GameObject("ForecastPanel");
            _forecastPanel.transform.SetParent(_uiCanvas.transform);

            Image panelImage = _forecastPanel.AddComponent<Image>();
            panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.92f);

            var outline = _forecastPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.7f, 1f, 0.9f);
            outline.effectDistance = new Vector2(2, -2);

            RectTransform panelRT = _forecastPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(1, 0.5f);
            panelRT.anchorMax = new Vector2(1, 0.5f);
            panelRT.offsetMin = new Vector2(-220, -120);
            panelRT.offsetMax = new Vector2(-10, 120);

            GameObject textGO = new GameObject("ForecastText");
            textGO.transform.SetParent(_forecastPanel.transform);

            _forecastText = textGO.AddComponent<Text>();
            _forecastText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _forecastText.fontSize = 13;
            _forecastText.alignment = TextAnchor.UpperLeft;
            _forecastText.color = Color.white;
            _forecastText.text = "";

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 10);
            textRT.offsetMax = new Vector2(-10, -10);

            _forecastPanel.SetActive(false);
        }

        /// <summary>
        /// Shows the battle forecast panel for a potential engagement.
        /// Call before confirming an attack; call HideForecast() to dismiss.
        /// </summary>
        public void ShowForecast(IUnit attacker, IUnit defender, IGameMap map)
        {
            if (_forecastPanel == null) return;

            var service  = new CombatForecastService(new CombatResolver());
            var forecast = service.Calculate(attacker, defender, map);
            _forecastText.text = forecast.FormatFull(attacker.Name, defender.Name);
            _forecastPanel.SetActive(true);
        }

        /// <summary>Hides the battle forecast panel.</summary>
        public void HideForecast()
        {
            _forecastPanel?.SetActive(false);
        }

        public void ShowGameOverScreen(GameState state, int turnCount)
        {
            _gameOverPanel.SetActive(true);

            string message = state == GameState.PlayerWon
                ? $"VICTORY!\nCompleted in {turnCount} turns"
                : "DEFEAT!\nAll units defeated";

            _gameOverText.text = message;
        }

        /// <summary>
        /// Creates the modal menu panel with transparent overlay and 3 menu buttons.
        /// </summary>
        private void CreateModalMenuPanel()
        {
            // Main panel - full screen transparent overlay
            _modalMenuPanel = new GameObject("ModalMenuPanel");
            _modalMenuPanel.transform.SetParent(_uiCanvas.transform);

            Image panelImage = _modalMenuPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.75f);  // 75% transparent black overlay

            RectTransform panelRT = _modalMenuPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Center container for buttons
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(_modalMenuPanel.transform);

            Image containerBg = buttonContainer.AddComponent<Image>();
            containerBg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);  // Dark blue background

            var containerOutline = buttonContainer.AddComponent<Outline>();
            containerOutline.effectColor = new Color(0.4f, 0.7f, 1f, 0.9f);
            containerOutline.effectDistance = new Vector2(3, -3);

            RectTransform containerRT = buttonContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.offsetMin = new Vector2(-150, -120);  // 300px wide
            containerRT.offsetMax = new Vector2(150, 120);    // 240px tall

            // Title text
            GameObject titleGO = new GameObject("MenuTitle");
            titleGO.transform.SetParent(buttonContainer.transform);

            Text titleText = titleGO.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.text = "MENU";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(0, -50);
            titleRT.offsetMax = new Vector2(0, -10);

            // Create three buttons vertically
            _menuEndTurnButton = CreateMenuButton("End Turn", buttonContainer.transform, 0);
            _menuSaveGameButton = CreateMenuButton("Save Game", buttonContainer.transform, 1);
            _menuExitButton = CreateMenuButton("Exit", buttonContainer.transform, 2);

            // Wire up button actions
            _menuEndTurnButton.onClick.AddListener(() => {
                HideModalMenu();
                _gameController?.EndPlayerPhase();
            });

            _menuSaveGameButton.onClick.AddListener(() => {
                // TODO: Implement save game functionality
                ShowInfoMessage("Save Game not yet implemented");
            });

            _menuExitButton.onClick.AddListener(() => {
                // TODO: Implement exit functionality
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });

            // Start hidden
            _modalMenuPanel.SetActive(false);
        }

        /// <summary>
        /// Helper method to create a menu button with consistent styling.
        /// </summary>
        private Button CreateMenuButton(string label, Transform parent, int index)
        {
            GameObject buttonGO = new GameObject($"MenuButton_{label.Replace(" ", "")}");
            buttonGO.transform.SetParent(parent);

            Image buttonImage = buttonGO.AddComponent<Image>();
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button styling
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 0.7f, 0.9f);
            colors.highlightedColor = new Color(0.3f, 0.55f, 0.9f, 1f);
            colors.pressedColor = new Color(0.15f, 0.3f, 0.55f, 1f);
            colors.selectedColor = new Color(0.25f, 0.45f, 0.75f, 0.95f);
            button.colors = colors;

            var outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.7f);
            outline.effectDistance = new Vector2(2, -2);

            // Position buttons vertically, centered
            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRT.anchorMax = new Vector2(0.5f, 0.5f);

            // Vertical spacing: 60px per button
            float yOffset = 20 - (index * 60);  // First at +20, second at -40, third at -100
            buttonRT.offsetMin = new Vector2(-120, yOffset - 25);     // 240px wide, 50px tall
            buttonRT.offsetMax = new Vector2(120, yOffset + 25);

            // Button text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);

            Text buttonText = textGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = label;
            buttonText.fontSize = 18;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return button;
        }

        /// <summary>Shows the modal menu overlay.</summary>
        public void ShowModalMenu()
        {
            if (_modalMenuPanel != null)
            {
                _modalMenuPanel.SetActive(true);
            }
        }

        /// <summary>Hides the modal menu overlay.</summary>
        public void HideModalMenu()
        {
            if (_modalMenuPanel != null)
            {
                _modalMenuPanel.SetActive(false);
            }
        }

        /// <summary>Returns whether the modal menu is currently open.</summary>
        public bool IsModalMenuOpen()
        {
            return _modalMenuPanel != null && _modalMenuPanel.activeSelf;
        }
    }
}
