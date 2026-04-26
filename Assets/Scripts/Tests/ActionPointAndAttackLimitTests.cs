using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ActionPointAndAttackLimitTests
    {
        [Test]
        public void Unit_CannotAttackTwice_InSameTurn()
        {
            var tm = new TurnManager();
            var unit = new Unit(1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());
            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(), ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());
            tm.Initialize(new List<IUnit> { unit, enemy });

            Assert.IsFalse(tm.HasUnitAttacked(unit.Id));
            Assert.AreEqual(tm.DefaultActionsPerUnit, tm.GetRemainingActions(unit.Id));

            // Perform first attack: consume action and mark attacked
            Assert.IsTrue(tm.ConsumeAction(unit.Id));
            tm.MarkUnitAsAttacked(unit.Id);

            Assert.IsTrue(tm.HasUnitAttacked(unit.Id));
            Assert.AreEqual(1, tm.GetRemainingActions(unit.Id));

            // The HasUnitAttacked flag should prevent a second attack (GameController checks this flag before attacking)
            Assert.IsTrue(tm.HasUnitAttacked(unit.Id), "Unit should be marked as attacked and prevented from a second attack.");
        }

        [Test]
        public void AttackThenMove_And_MoveThenAttack_ConsumeActionsCorrectly()
        {
            var tm1 = new TurnManager();
            var unit1 = new Unit(1, "Unit1", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());
            tm1.Initialize(new List<IUnit> { unit1 });

            // Attack then Move
            Assert.AreEqual(2, tm1.GetRemainingActions(unit1.Id));
            Assert.IsTrue(tm1.ConsumeAction(unit1.Id)); // attack
            tm1.MarkUnitAsAttacked(unit1.Id);
            Assert.AreEqual(1, tm1.GetRemainingActions(unit1.Id));
            Assert.IsTrue(tm1.ConsumeAction(unit1.Id)); // move
            Assert.AreEqual(0, tm1.GetRemainingActions(unit1.Id));
            Assert.IsTrue(tm1.HasUnitAttacked(unit1.Id));
            Assert.IsTrue(tm1.HasUnitActed(unit1.Id));

            // Move then Attack
            var tm2 = new TurnManager();
            var unit2 = new Unit(2, "Unit2", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());
            tm2.Initialize(new List<IUnit> { unit2 });

            Assert.AreEqual(2, tm2.GetRemainingActions(unit2.Id));
            Assert.IsTrue(tm2.ConsumeAction(unit2.Id)); // move
            Assert.AreEqual(1, tm2.GetRemainingActions(unit2.Id));
            Assert.IsTrue(tm2.ConsumeAction(unit2.Id)); // attack
            tm2.MarkUnitAsAttacked(unit2.Id);
            Assert.AreEqual(0, tm2.GetRemainingActions(unit2.Id));
            Assert.IsTrue(tm2.HasUnitActed(unit2.Id));
            Assert.IsTrue(tm2.HasUnitAttacked(unit2.Id));
        }

        [Test]
        public void Sing_Cantar_Refreshes_Actions_And_Clears_AttackFlag()
        {
            var tm = new TurnManager();
            var heron = new Unit(1, "Heron", Team.PlayerTeam,
                ClassDataFactory.CreateHeron(), ClassDataFactory.CreateHeron().BaseStats,
                (0, 0), WeaponFactory.CreateRefreshStaff());
            var ally = new Unit(2, "Ally", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(), ClassDataFactory.CreateMyrmidon().BaseStats,
                (1, 0), WeaponFactory.CreateIronSword());
            tm.Initialize(new List<IUnit> { heron, ally });

            // Simulate ally has acted and attacked and has no remaining actions
            tm.MarkUnitAsActed(ally.Id);
            tm.MarkUnitAsAttacked(ally.Id);
            Assert.IsTrue(tm.HasUnitActed(ally.Id));
            Assert.IsTrue(tm.HasUnitAttacked(ally.Id));
            Assert.AreEqual(0, tm.GetRemainingActions(ally.Id));

            // Heron sings: refresh ally
            tm.RefreshUnit(ally.Id);

            Assert.IsFalse(tm.HasUnitActed(ally.Id));
            Assert.IsFalse(tm.HasUnitAttacked(ally.Id));
            Assert.AreEqual(tm.DefaultActionsPerUnit, tm.GetRemainingActions(ally.Id));
        }
    }
}
