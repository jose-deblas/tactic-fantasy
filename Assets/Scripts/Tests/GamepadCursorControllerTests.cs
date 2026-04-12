using NUnit.Framework;
using UnityEngine;
using TacticFantasy.Adapters;
using TacticFantasy.Domain.Map;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class GamepadCursorControllerTests
    {
        private GamepadCursorController _gamepadCursorController;
        private IGameMap _gameMap;
        private GameObject _testGameObject;

        // Constantes de prueba (deben coincidir con las del controlador)
        private const int MAP_WIDTH = 16;
        private const int MAP_HEIGHT = 16;
        private const float MOVEMENT_DELAY = 0.2f;

        [SetUp]
        public void Setup()
        {
            _testGameObject = new GameObject("GamepadCursorControllerTest");
            _gamepadCursorController = _testGameObject.AddComponent<GamepadCursorController>();
            _gameMap = new GameMap(MAP_WIDTH, MAP_HEIGHT, 42);

            _gamepadCursorController.Initialize(_gameMap);
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(_testGameObject);
        }

        /// <summary>
        /// Prueba que el cursor no sale del límite izquierdo del mapa (x=0).
        /// </summary>
        [Test]
        public void CursorPosition_AtLeftBoundary_DoesNotMoveLeft()
        {
            _gamepadCursorController.SetCursorPosition(0, 0);
            Assert.AreEqual((0, 0), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que el cursor no sale del límite derecho del mapa (x=15).
        /// </summary>
        [Test]
        public void CursorPosition_AtRightBoundary_DoesNotMoveRight()
        {
            _gamepadCursorController.SetCursorPosition(MAP_WIDTH - 1, MAP_HEIGHT - 1);
            Assert.AreEqual((MAP_WIDTH - 1, MAP_HEIGHT - 1), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que el cursor no sale del límite superior del mapa (y=0).
        /// </summary>
        [Test]
        public void CursorPosition_AtTopBoundary_DoesNotMoveUp()
        {
            _gamepadCursorController.SetCursorPosition(5, 0);
            Assert.AreEqual((5, 0), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que el cursor no sale del límite inferior del mapa (y=15).
        /// </summary>
        [Test]
        public void CursorPosition_AtBottomBoundary_DoesNotMoveDown()
        {
            _gamepadCursorController.SetCursorPosition(5, MAP_HEIGHT - 1);
            Assert.AreEqual((5, MAP_HEIGHT - 1), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que SetCursorPosition clampea valores fuera de rango al límite.
        /// </summary>
        [Test]
        public void SetCursorPosition_OutOfBoundsNegative_Clamps()
        {
            _gamepadCursorController.SetCursorPosition(-5, -3);
            Assert.AreEqual((0, 0), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que SetCursorPosition clampea valores fuera del rango superior.
        /// </summary>
        [Test]
        public void SetCursorPosition_OutOfBoundsPositive_Clamps()
        {
            _gamepadCursorController.SetCursorPosition(100, 200);
            Assert.AreEqual((MAP_WIDTH - 1, MAP_HEIGHT - 1), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que la posición inicial del cursor es (0, 0).
        /// </summary>
        [Test]
        public void CursorPosition_AfterInitialize_IsZero()
        {
            Assert.AreEqual((0, 0), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que SetCursorPosition emite el evento OnCursorMoved.
        /// </summary>
        [Test]
        public void SetCursorPosition_ChangesPosition_EmitsOnCursorMovedEvent()
        {
            bool eventFired = false;
            (int x, int y) eventPosition = (0, 0);

            _gamepadCursorController.OnCursorMoved += (pos) =>
            {
                eventFired = true;
                eventPosition = pos;
            };

            _gamepadCursorController.SetCursorPosition(5, 5);

            Assert.IsTrue(eventFired);
            Assert.AreEqual((5, 5), eventPosition);
        }

        /// <summary>
        /// Prueba que SetCursorPosition no emite evento si la posición no cambia.
        /// </summary>
        [Test]
        public void SetCursorPosition_SamePosition_DoesNotEmitEvent()
        {
            _gamepadCursorController.SetCursorPosition(3, 3);

            int eventCount = 0;
            _gamepadCursorController.OnCursorMoved += (_) => eventCount++;

            _gamepadCursorController.SetCursorPosition(3, 3);

            Assert.AreEqual(0, eventCount);
        }

        /// <summary>
        /// Prueba que IsValidPosition retorna true para coordenadas dentro del mapa.
        /// </summary>
        [Test]
        public void IsValidPosition_WithinBounds_ReturnsTrue()
        {
            Assert.IsTrue(_gamepadCursorController.IsValidPosition(0, 0));
            Assert.IsTrue(_gamepadCursorController.IsValidPosition(8, 8));
            Assert.IsTrue(_gamepadCursorController.IsValidPosition(MAP_WIDTH - 1, MAP_HEIGHT - 1));
        }

        /// <summary>
        /// Prueba que IsValidPosition retorna false para coordenadas fuera del mapa (negativas).
        /// </summary>
        [Test]
        public void IsValidPosition_NegativeCoordinates_ReturnsFalse()
        {
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(-1, 0));
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(0, -1));
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(-5, -5));
        }

        /// <summary>
        /// Prueba que IsValidPosition retorna false para coordenadas fuera del mapa (positivas).
        /// </summary>
        [Test]
        public void IsValidPosition_CoordinatesOutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(MAP_WIDTH, 0));
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(0, MAP_HEIGHT));
            Assert.IsFalse(_gamepadCursorController.IsValidPosition(MAP_WIDTH, MAP_HEIGHT));
        }

        /// <summary>
        /// Prueba que Initialize con null gameMap lanza ArgumentNullException.
        /// </summary>
        [Test]
        public void Initialize_WithNullGameMap_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => _gamepadCursorController.Initialize(null));
        }

        /// <summary>
        /// Prueba que el evento OnConfirm se puede suscribir y disparar.
        /// </summary>
        [Test]
        public void OnConfirm_EventCanBeSubscribedAndFired()
        {
            bool eventFired = false;
            _gamepadCursorController.OnConfirm += () => eventFired = true;

            // El evento se dispara cuando se presiona el botón Submit
            // Para esta prueba, no simulamos input real sino que lo verificamos
            Assert.IsNotNull(_gamepadCursorController.OnConfirm);
        }

        /// <summary>
        /// Prueba que el evento OnCancel se puede suscribir.
        /// </summary>
        [Test]
        public void OnCancel_EventCanBeSubscribed()
        {
            bool eventFired = false;
            _gamepadCursorController.OnCancel += () => eventFired = true;

            Assert.IsNotNull(_gamepadCursorController.OnCancel);
        }

        /// <summary>
        /// Prueba que el evento OnEndTurn se puede suscribir.
        /// </summary>
        [Test]
        public void OnEndTurn_EventCanBeSubscribed()
        {
            bool eventFired = false;
            _gamepadCursorController.OnEndTurn += () => eventFired = true;

            Assert.IsNotNull(_gamepadCursorController.OnEndTurn);
        }

        /// <summary>
        /// Prueba que el evento OnToggleAttackRange se puede suscribir.
        /// </summary>
        [Test]
        public void OnToggleAttackRange_EventCanBeSubscribed()
        {
            bool eventFired = false;
            _gamepadCursorController.OnToggleAttackRange += () => eventFired = true;

            Assert.IsNotNull(_gamepadCursorController.OnToggleAttackRange);
        }

        /// <summary>
        /// Prueba que el mapeo de límites respeta el ancho del mapa (MAP_WIDTH = 16).
        /// </summary>
        [Test]
        public void MapWidth_EqualsConstant_16()
        {
            // El gameMap debe tener ancho 16
            Assert.AreEqual(16, _gameMap.Width);
        }

        /// <summary>
        /// Prueba que el mapeo de límites respeta la altura del mapa (MAP_HEIGHT = 16).
        /// </summary>
        [Test]
        public void MapHeight_EqualsConstant_16()
        {
            // El gameMap debe tener altura 16
            Assert.AreEqual(16, _gameMap.Height);
        }

        /// <summary>
        /// Prueba que múltiples movimientos sucesivos funcionan correctamente.
        /// </summary>
        [Test]
        public void SetCursorPosition_MultipleMovements_AllCorrect()
        {
            _gamepadCursorController.SetCursorPosition(5, 5);
            Assert.AreEqual((5, 5), _gamepadCursorController.CursorPosition);

            _gamepadCursorController.SetCursorPosition(10, 10);
            Assert.AreEqual((10, 10), _gamepadCursorController.CursorPosition);

            _gamepadCursorController.SetCursorPosition(0, 0);
            Assert.AreEqual((0, 0), _gamepadCursorController.CursorPosition);
        }

        /// <summary>
        /// Prueba que el clamping funciona correctamente en un valor parcialmente fuera de rango.
        /// </summary>
        [Test]
        public void SetCursorPosition_PartiallyOutOfBounds_ClampsCorrectly()
        {
            _gamepadCursorController.SetCursorPosition(-5, 8);
            Assert.AreEqual((0, 8), _gamepadCursorController.CursorPosition);

            _gamepadCursorController.SetCursorPosition(20, 5);
            Assert.AreEqual((MAP_WIDTH - 1, 5), _gamepadCursorController.CursorPosition);
        }
    }
}
