using System.IO;
using NUnit.Framework;
using TacticFantasy.Adapters.Persistence;
using TacticFantasy.Domain.Save;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;
using System.Collections.Generic;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for the JSON file-based game repository adapter.
    /// Uses a temp directory so tests are isolated and reproducible.
    /// </summary>
    [TestFixture]
    public class JsonFileGameRepositoryTests
    {
        private string _tempDir;
        private string _filePath;
        private JsonFileGameRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _tempDir  = Path.Combine(Path.GetTempPath(), "tactic-fantasy-tests-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _filePath = Path.Combine(_tempDir, "save.json");
            _repo     = new JsonFileGameRepository(_filePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // --- helpers ----------------------------------------------------------

        private IUnit MakeUnit(int id, string name, Team team, int hp = 30)
        {
            var stats    = new CharacterStats(hp, 10, 0, 8, 7, 5, 5, 0, 5);
            var weapon   = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
            var classData = ClassDataFactory.CreateMyrmidon();
            return new Unit(id, name, team, classData, stats, (0, 0), weapon);
        }

        private GameSnapshot MakeSnapshot(Phase targetPhase = Phase.PlayerPhase)
        {
            var units = new List<IUnit>
            {
                MakeUnit(1, "Hero",  Team.PlayerTeam),
                MakeUnit(2, "Enemy", Team.EnemyTeam)
            };
            var tm = new TurnManager();
            tm.Initialize(units);
            if (targetPhase == Phase.EnemyPhase) tm.AdvancePhase();
            return GameSnapshot.Capture(tm);
        }

        // --- HasSave ----------------------------------------------------------

        [Test]
        public void HasSave_ReturnsFalse_WhenFileDoesNotExist()
        {
            Assert.IsFalse(_repo.HasSave);
        }

        [Test]
        public void HasSave_ReturnsTrue_AfterSave()
        {
            _repo.Save(MakeSnapshot());

            Assert.IsTrue(_repo.HasSave);
        }

        // --- Save / Load round-trip -------------------------------------------

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesPhase()
        {
            var snapshot = MakeSnapshot(Phase.EnemyPhase);

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(Phase.EnemyPhase, loaded.CurrentPhase);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesTurnCount()
        {
            var snapshot = MakeSnapshot();

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(snapshot.TurnCount, loaded.TurnCount);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesUnitCount()
        {
            var snapshot = MakeSnapshot();

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(2, loaded.Units.Count);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesUnitIdentity()
        {
            var snapshot = MakeSnapshot();

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            var hero = loaded.Units[0];
            Assert.AreEqual("Hero",            hero.Name);
            Assert.AreEqual(Team.PlayerTeam,   hero.Team);
            Assert.AreEqual("Myrmidon",        hero.ClassName);
            Assert.AreEqual("Iron Sword",      hero.WeaponName);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesUnitHP()
        {
            var units = new List<IUnit>
            {
                MakeUnit(1, "Hero", Team.PlayerTeam, hp: 30) as Unit
            };
            // cast needed for TakeDamage, since we get IUnit back
            var unit = units[0] as Unit;
            unit.TakeDamage(10);

            var tm = new TurnManager();
            tm.Initialize(units);
            var snapshot = GameSnapshot.Capture(tm);

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(20, loaded.Units[0].CurrentHP);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesUnitPosition()
        {
            var unit = MakeUnit(1, "Hero", Team.PlayerTeam) as Unit;
            unit.SetPosition(4, 7);

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { unit, MakeUnit(2, "Enemy", Team.EnemyTeam) });
            var snapshot = GameSnapshot.Capture(tm);

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(4, loaded.Units[0].PositionX);
            Assert.AreEqual(7, loaded.Units[0].PositionY);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesStatusEffect()
        {
            var unit = MakeUnit(1, "Hero", Team.PlayerTeam) as Unit;
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { unit, MakeUnit(2, "Enemy", Team.EnemyTeam) });
            var snapshot = GameSnapshot.Capture(tm);

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(StatusEffectType.Poison, loaded.Units[0].StatusType);
            Assert.AreEqual(3,                       loaded.Units[0].StatusRemainingTurns);
        }

        [Test]
        public void SaveAndLoad_RoundTrip_NoStatusEffect_StaysNone()
        {
            var snapshot = MakeSnapshot();

            _repo.Save(snapshot);
            var loaded = _repo.Load();

            Assert.AreEqual(StatusEffectType.None, loaded.Units[0].StatusType);
            Assert.AreEqual(0,                     loaded.Units[0].StatusRemainingTurns);
        }

        // --- File creation ---------------------------------------------------

        [Test]
        public void Save_CreatesFile_AtExpectedPath()
        {
            _repo.Save(MakeSnapshot());

            Assert.IsTrue(File.Exists(_filePath));
        }

        [Test]
        public void Save_CreatesParentDirectory_IfMissing()
        {
            var nestedPath = Path.Combine(_tempDir, "subdir", "nested", "save.json");
            var repo = new JsonFileGameRepository(nestedPath);

            repo.Save(MakeSnapshot());

            Assert.IsTrue(File.Exists(nestedPath));
        }

        // --- Overwrite -------------------------------------------------------

        [Test]
        public void Save_Overwrites_PreviousSave()
        {
            // First save: 2 units
            var snapshot1 = MakeSnapshot();
            _repo.Save(snapshot1);

            // Second save: 1 unit only
            var units2 = new List<IUnit> { MakeUnit(1, "Hero", Team.PlayerTeam) };
            var tm2 = new TurnManager();
            tm2.Initialize(units2);
            _repo.Save(GameSnapshot.Capture(tm2));

            var loaded = _repo.Load();
            Assert.AreEqual(1, loaded.Units.Count);
        }
    }
}
