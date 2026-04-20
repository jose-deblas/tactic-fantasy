using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Support;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace DomainTests
{
    [TestFixture]
    public class SupportTrackerTests
    {
        private SupportTracker _tracker;

        [SetUp]
        public void Setup()
        {
            _tracker = new SupportTracker();
        }

        [Test]
        public void UnregisteredPair_ReturnsNone()
        {
            Assert.AreEqual(SupportLevel.None, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void RegisteredPair_StartsAtNone()
        {
            _tracker.RegisterSupport(1, 2);

            Assert.AreEqual(SupportLevel.None, _tracker.GetLevel(1, 2));
            Assert.AreEqual(0, _tracker.GetPoints(1, 2));
        }

        [Test]
        public void AddPoints_BelowThresholdC_StaysNone()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdC - 1);

            Assert.AreEqual(SupportLevel.None, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void AddPoints_ReachesThresholdC_PromotesToC()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdC);

            Assert.AreEqual(SupportLevel.C, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void AddPoints_ReachesThresholdB_PromotesToB()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdB);

            Assert.AreEqual(SupportLevel.B, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void AddPoints_ReachesThresholdA_PromotesToA()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdA);

            Assert.AreEqual(SupportLevel.A, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void AddPoints_Incremental_AccumulatesCorrectly()
        {
            _tracker.RegisterSupport(1, 2);

            _tracker.AddPoints(1, 2, 10);
            Assert.AreEqual(SupportLevel.None, _tracker.GetLevel(1, 2));

            _tracker.AddPoints(1, 2, 10);
            Assert.AreEqual(SupportLevel.C, _tracker.GetLevel(1, 2));

            _tracker.AddPoints(1, 2, 40);
            Assert.AreEqual(SupportLevel.B, _tracker.GetLevel(1, 2));

            _tracker.AddPoints(1, 2, 60);
            Assert.AreEqual(SupportLevel.A, _tracker.GetLevel(1, 2));
        }

        [Test]
        public void AddPoints_ToUnregisteredPair_DoesNothing()
        {
            _tracker.AddPoints(1, 2, 100);

            Assert.AreEqual(SupportLevel.None, _tracker.GetLevel(1, 2));
            Assert.AreEqual(0, _tracker.GetPoints(1, 2));
        }

        [Test]
        public void GetLevel_SymmetricAccess_ReturnsSameLevel()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(2, 1, SupportTracker.ThresholdC);

            Assert.AreEqual(SupportLevel.C, _tracker.GetLevel(1, 2));
            Assert.AreEqual(SupportLevel.C, _tracker.GetLevel(2, 1));
        }

        [Test]
        public void GetCombatBonus_AdjacentAllyWithSupport_ReturnsBonus()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdC);

            var unit = CreateUnit(1, "Hero", Team.PlayerTeam, (5, 5));
            var ally = CreateUnit(2, "Ally", Team.PlayerTeam, (5, 6));
            var allies = new List<IUnit> { unit, ally };

            var bonus = _tracker.GetCombatBonus(unit, allies);

            Assert.AreEqual(1, bonus.Attack);
            Assert.AreEqual(1, bonus.Defense);
        }

        [Test]
        public void GetCombatBonus_AllyOutOfRange_ReturnsZero()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdA);

            var unit = CreateUnit(1, "Hero", Team.PlayerTeam, (0, 0));
            var ally = CreateUnit(2, "Ally", Team.PlayerTeam, (10, 10));
            var allies = new List<IUnit> { unit, ally };

            var bonus = _tracker.GetCombatBonus(unit, allies);

            Assert.AreEqual(0, bonus.Attack);
        }

        [Test]
        public void GetCombatBonus_MultipleAllies_StacksBonuses()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.RegisterSupport(1, 3);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdC);
            _tracker.AddPoints(1, 3, SupportTracker.ThresholdB);

            var unit = CreateUnit(1, "Hero", Team.PlayerTeam, (5, 5));
            var ally1 = CreateUnit(2, "Ally1", Team.PlayerTeam, (5, 6));
            var ally2 = CreateUnit(3, "Ally2", Team.PlayerTeam, (6, 5));
            var allies = new List<IUnit> { unit, ally1, ally2 };

            var bonus = _tracker.GetCombatBonus(unit, allies);

            // C(1) + B(2) = 3 attack
            Assert.AreEqual(3, bonus.Attack);
            Assert.AreEqual(3, bonus.Defense);
        }

        [Test]
        public void GetCombatBonus_DeadAlly_Excluded()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdA);

            var unit = CreateUnit(1, "Hero", Team.PlayerTeam, (5, 5));
            var ally = CreateUnit(2, "Ally", Team.PlayerTeam, (5, 6));
            ally.TakeDamage(ally.CurrentHP); // kill the ally

            var allies = new List<IUnit> { unit, ally };
            var bonus = _tracker.GetCombatBonus(unit, allies);

            Assert.AreEqual(0, bonus.Attack);
        }

        [Test]
        public void GetCombatBonus_EnemyUnit_Excluded()
        {
            _tracker.RegisterSupport(1, 2);
            _tracker.AddPoints(1, 2, SupportTracker.ThresholdA);

            var unit = CreateUnit(1, "Hero", Team.PlayerTeam, (5, 5));
            var enemy = CreateUnit(2, "Enemy", Team.EnemyTeam, (5, 6));
            var allies = new List<IUnit> { unit, enemy };

            var bonus = _tracker.GetCombatBonus(unit, allies);

            Assert.AreEqual(0, bonus.Attack);
        }

        private static Unit CreateUnit(int id, string name, Team team, (int x, int y) pos)
        {
            var stats = new CharacterStats(20, 8, 0, 8, 8, 5, 6, 3, 5);
            var classData = new ClassData(
                "Fighter", stats, stats, stats,
                WeaponType.SWORD, MoveType.Infantry);
            var weapon = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
            return new Unit(id, name, team, classData, stats, pos, weapon);
        }
    }
}
