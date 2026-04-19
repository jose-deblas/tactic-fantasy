using NUnit.Framework;
using System.Collections.Generic;
using TacticFantasy.Domain.Save;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain;

namespace DomainTests
{
    // Minimal fake implementations to exercise GameSnapshot capture without Unity
    class FakeUnitForSave : IUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Team Team { get; set; }
        public IClassData Class => null;
        public CharacterStats CurrentStats => null;
        public int CurrentHP { get; set; }
        public int MaxHP => 10;
        public (int x, int y) Position { get; set; }
        public IWeapon EquippedWeapon => null;
        public Inventory Inventory { get; set; } = new Inventory();
        public bool CanEquip(IWeapon weapon) => false;
        public bool IsAlive => CurrentHP > 0;
        public StatusEffect ActiveStatus { get; set; }
        public bool CanAct => true;
        public bool HasBrokenWeapon => false;
        public int Level { get; set; }
        public int Experience { get; set; }
        public IReadOnlyList<ISkill> EquippedSkills => new List<ISkill>().AsReadOnly();

        public void TakeDamage(int damage) { CurrentHP = System.Math.Max(0, CurrentHP - damage); }
        public void Heal(int amount) { CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount); }
        public void SetPosition(int x, int y) { Position = (x, y); }
        public void EquipWeapon(IWeapon weapon) { }
        public void ApplyStatus(StatusEffect effect) { ActiveStatus = effect; }
        public void ClearStatus() { ActiveStatus = null; }
        public void TickStatus() { }
        public bool GainExperience(int amount, System.Random rng = null) => false;
        public void ChangeClass(IClassData newClass) { }
        public void LearnSkill(ISkill skill) { }
        public void EquipSkill(ISkill skill) { }
        public void UnequipSkill(ISkill skill) { }
        public void ApplyStatBoost(int hp, int str, int mag, int skl, int spd, int lck, int def, int res, int mov) { }
    }

    class FakeTurnManager : ITurnManager
    {
        public Phase CurrentPhase { get; private set; } = Phase.PlayerPhase;
        public int TurnCount { get; private set; } = 7;
        public IReadOnlyList<IUnit> AllUnits { get; private set; }
        public IUnit CurrentUnit => AllUnits.Count > 0 ? AllUnits[0] : null;
        public bool HasCurrentUnitActed => false;
        public IVictoryCondition VictoryCondition => null;
        public FakeTurnManager(List<IUnit> units)
        {
            AllUnits = units.AsReadOnly();
        }

        public void Initialize(List<IUnit> units) { }
        public void Initialize(List<IUnit> units, IVictoryCondition victoryCondition, TacticFantasy.Domain.Map.IGameMap map = null) { }
        public void MarkCurrentUnitAsActed() { }
        public void MarkUnitAsActed(int unitId) { }
        public void AdvancePhase() { }
        public TacticFantasy.Domain.Turn.GameState GetGameState() => GameState.InProgress;
        public void HealFortTiles(TacticFantasy.Domain.Map.IGameMap map) { }
        public bool HasUnitActed(int unitId) => false;
        public bool HaveAllPlayerUnitsActed() => false;
        public bool CanRefreshTarget(IUnit refresher, IUnit target) => false;
        public void RefreshUnit(int targetUnitId) { }
    }

    public class GameSaveServiceTests
    {
        [Test]
        public void SaveAndLoadRoundtripProducesSnapshotWithUnits()
        {
            var repo = new InMemoryGameRepository();
            var svc = new GameSaveService(repo);

            var u1 = new FakeUnitForSave { Id = 1, Name = "Alice", Team = Team.PlayerTeam, CurrentHP = 8, Position = (2,3), Level = 3, Experience = 42 };
            var u2 = new FakeUnitForSave { Id = 2, Name = "Bob",   Team = Team.EnemyTeam,  CurrentHP = 5, Position = (5,6), Level = 1, Experience = 0 };

            var tm = new FakeTurnManager(new List<IUnit> { u1, u2 });

            svc.Save(tm);

            Assert.IsTrue(repo.HasSave);

            var loaded = svc.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(tm.CurrentPhase, loaded.CurrentPhase);
            Assert.AreEqual(tm.TurnCount, loaded.TurnCount);
            Assert.AreEqual(2, loaded.Units.Count);

            var snap1 = loaded.Units[0];
            Assert.AreEqual(1, snap1.Id);
            Assert.AreEqual("Alice", snap1.Name);
            Assert.AreEqual(8, snap1.CurrentHP);
            Assert.AreEqual(2, snap1.PositionX);
            Assert.AreEqual(3, snap1.PositionY);
            Assert.AreEqual(3, snap1.Level);
            Assert.AreEqual(42, snap1.Experience);
        }

        [Test]
        public void SaveIncludesActiveStatusOnUnit()
        {
            var repo = new InMemoryGameRepository();
            var svc = new GameSaveService(repo);

            var u = new FakeUnitForSave { Id = 7, Name = "StunGuy", Team = Team.EnemyTeam, CurrentHP = 6, Position = (1,1), Level = 1, Experience = 0 };

            // Use a domain status effect implementation to exercise capture logic
            var stun = new TacticFantasy.Domain.StunEffect(duration: 2f);
            u.ActiveStatus = stun;

            var tm = new FakeTurnManager(new List<IUnit> { u });
            svc.Save(tm);

            var loaded = svc.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.Units.Count);

            var snap = loaded.Units[0];
            // Snapshot should record a non-none status and a positive remaining turns value
            Assert.AreNotEqual(TacticFantasy.Domain.StatusEffectType.None, snap.StatusType);
            Assert.Greater(snap.StatusRemainingTurns, 0);
        }

        [Test]
        public void LoadReturnsNullWhenNoSaveExists()
        {
            var repo = new InMemoryGameRepository();
            var svc = new GameSaveService(repo);

            // No save has been performed on the repository
            var loaded = svc.Load();

            Assert.IsNull(loaded, "Load should return null when repository has no save data");
        }

        [Test]
        public void SaveOverwritesPreviousSave()
        {
            var repo = new InMemoryGameRepository();
            var svc = new GameSaveService(repo);

            var u1 = new FakeUnitForSave { Id = 1, Name = "First", Team = Team.PlayerTeam, CurrentHP = 10, Position = (0,0), Level = 1, Experience = 0 };
            var tm1 = new FakeTurnManager(new System.Collections.Generic.List<IUnit> { u1 });
            // First save
            svc.Save(tm1);

            var u2 = new FakeUnitForSave { Id = 2, Name = "Second", Team = Team.PlayerTeam, CurrentHP = 5, Position = (1,1), Level = 2, Experience = 10 };
            var tm2 = new FakeTurnManager(new System.Collections.Generic.List<IUnit> { u2 });
            // Second save should overwrite the first
            svc.Save(tm2);

            var loaded = svc.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.Units.Count);
            Assert.AreEqual(2, loaded.Units[0].Id);
            Assert.AreEqual("Second", loaded.Units[0].Name);
        }
    }
}
