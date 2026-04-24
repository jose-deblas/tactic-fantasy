using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TacticFantasy.Adapters;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class UIManagerInterstitialTests
    {
        private GameObject _testGO;
        private UIManager _uiManager;

        [SetUp]
        public void Setup()
        {
            _testGO = new GameObject("UIManagerTestHost");
            _uiManager = _testGO.AddComponent<UIManager>();
        }

        [TearDown]
        public void Teardown()
        {
            // Destroy UI objects created by UIManager
            var canvas = GameObject.Find("UICanvas");
            if (canvas != null)
                Object.DestroyImmediate(canvas);

            var interstitial = GameObject.Find("TurnInterstitialPanel");
            if (interstitial != null)
                Object.DestroyImmediate(interstitial);

            Object.DestroyImmediate(_testGO);
        }

        [Test]
        public void CreateUICanvas_AddsGraphicRaycaster()
        {
            var canvas = GameObject.Find("UICanvas");
            Assert.IsNotNull(canvas, "UICanvas should be created by UIManager Awake");

            var gr = canvas.GetComponent<GraphicRaycaster>();
            Assert.IsNotNull(gr, "UICanvas must have a GraphicRaycaster to receive UI clicks");
        }

        [Test]
        public void ShowTurnInterstitial_StartButtonInvokesCallbackAndHidesPanel()
        {
            bool started = false;

            _uiManager.ShowTurnInterstitial(2, () => { started = true; });

            var panel = GameObject.Find("TurnInterstitialPanel");
            Assert.IsNotNull(panel);
            Assert.IsTrue(panel.activeSelf, "Interstitial panel should be active after ShowTurnInterstitial");

            var startBtn = GameObject.Find("StartButton");
            Assert.IsNotNull(startBtn, "Start button should exist in the interstitial");

            var btn = startBtn.GetComponent<Button>();
            Assert.IsNotNull(btn, "Start button must have a Button component");

            // Simulate click
            btn.onClick.Invoke();

            Assert.IsTrue(started, "onStart callback should be invoked when pressing Start");
            Assert.IsFalse(panel.activeSelf, "Interstitial panel should be hidden after pressing Start");
        }
    }
}
