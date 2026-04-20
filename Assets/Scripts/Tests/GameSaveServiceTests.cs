using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Save;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for the save/load domain service.
    /// </summary>
    [TestFixture]
    public class GameSaveServiceTests
    {
        // --- helpers ----------------------------------------------------------

        private IUnit MakeUnit(int id, string name, Team team, int hp = 30)
        {
            var stats = new CharacterStats(hp, 10, 0, 8, 7, 5, 5, 0, 5);
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
            var classData = ClassDataFactory.CreateMyrmidon();
            return new Unit(id, name, team, classData, stats, (0, 0), weapon);
        }

        private TurnManager MakeTurnManager(List<IUnit> units)
        {
            var tm = new TurnManager();
            tm.Initialize(units);
            return tm;
        }

        // --- GameSnapshot tests -----------------------------------------------

        [Test]
        public void GameSnapshot_CapturesPhaseAndTurn()
        {
            var units = new List<IUnit> { MakeUnit(1, "Hero", Team.PlayerTeam) };
            var tm = MakeTurnManager(units);

            var snapshot = GameSnapshot.Capture(tm);

            Assert.AreEqual(Phase.PlayerPhase, snapshot.CurrentPhase);
            Assert.AreEqual(1, snapshot.TurnCount);
        }

        [Test]
        public void GameSnapshot_CapturesAllUnits()
        {
            var units = new List<IUnit>
            {
                MakeUnit(1, "Hero",  Team.PlayerTeam),
                MakeUnit(2, "Enemy", Team.EnemyTeam)
            };
            var tm = MakeTurnManager(units);

            var snapshot = GameSnapshot.Capture(tm);

            Assert.AreEqual(2, snapshot.Units.Count);
        }

        [Test]
        public void UnitSnapshot_CapturesHpAndPosition()
        {
            var unit = MakeUnit(1, "Hero", Team.PlayerTeam, hp: 30) as Unit;
            unit.TakeDamage(8);
            unit.SetPosition(3, 5);

            var snap = UnitSnapshot.Capture(unit);

            Assert.AreEqual(22, snap.CurrentHP);
            Assert.AreEqual(3,  snap.PositionX);
            Assert.AreEqual(5,  snap.PositionY);
        }

        [Test]
        public void UnitSnapshot_CapturesStatusEffect()
        {
            var unit = MakeUnit(1, "Hero", Team.PlayerTeam) as Unit;
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));

            var snap = UnitSnapshot.Capture(unit);

            Assert.AreEqual(StatusEffectType.Poison, snap.StatusType);
            Assert.AreEqual(3, snap.StatusRemainingTurns);
        }

        [Test]
        public void UnitSnapshot_NullStatusEffect_StoredAsNone()
        {
            var unit = MakeUnit(1, "Hero", Team.PlayerTeam);

            var snap = UnitSnapshot.Capture(unit);

            Assert.AreEqual(StatusEffectType.None, snap.StatusType);
            Assert.AreEqual(0, snap.StatusRemainingTurns);
        }

        // --- GameSaveService tests -------------------------------------------

        [Test]
        public void GameSaveService_Save_CallsRepository()
        {
            var repo = new InMemoryGameRepository();
            var svc  = new GameSaveService(repo);
            var units = new List<IUnit> { MakeUnit(1, "Hero", Team.PlayerTeam) };
            var tm   = MakeTurnManager(units);

            svc.Save(tm);

            Assert.IsTrue(repo.HasSave);
        }

        [Test]
        public void GameSaveService_Load_ReturnsSavedSnapshot()
        {
            var repo  = new InMemoryGameRepository();
            var svc   = new GameSaveService(repo);
            var units = new List<IUnit>
            {
                MakeUnit(1, "Hero",  Team.PlayerTeam),
                MakeUnit(2, "Enemy", Team.EnemyTeam)
            };
            var tm = MakeTurnManager(units);
            tm.AdvancePhase(); // Player → Ally
            tm.AdvancePhase(); // Ally → Enemy (now EnemyPhase)

            svc.Save(tm);
            var loaded = svc.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(Phase.EnemyPhase, loaded.CurrentPhase);
            Assert.AreEqual(2, loaded.Units.Count);
        }

        [Test]
        public void GameSaveService_Load_WithNoSave_ReturnsNull()
        {
            var repo = new InMemoryGameRepository();
            var svc  = new GameSaveService(repo);

            var loaded = svc.Load();

            Assert.IsNull(loaded);
        }
    }
}
