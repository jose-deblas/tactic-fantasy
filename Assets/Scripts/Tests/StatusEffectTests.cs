using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class StatusEffectTests
    {
        private Unit CreateUnit(int maxHp = 30)
        {
            var stats = new CharacterStats(maxHp, 5, 0, 8, 8, 3, 5, 0, 5);
            return new Unit(1, "Hero", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(), stats, (0, 0), WeaponFactory.CreateIronSword());
        }

        // --- StatusEffect value object ---

        [Test]
        public void StatusEffect_NewEffect_IsActive()
        {
            var effect = new StatusEffect(StatusEffectType.Poison, 3);
            Assert.IsTrue(effect.IsActive);
            Assert.AreEqual(3, effect.RemainingTurns);
        }

        [Test]
        public void StatusEffect_DecrementToZero_IsNotActive()
        {
            var effect = new StatusEffect(StatusEffectType.Sleep, 1);
            effect.DecrementTurn();
            Assert.IsFalse(effect.IsActive);
            Assert.AreEqual(0, effect.RemainingTurns);
        }

        [Test]
        public void StatusEffect_DecrementBelowZero_StaysAtZero()
        {
            var effect = new StatusEffect(StatusEffectType.Stun, 1);
            effect.DecrementTurn();
            effect.DecrementTurn(); // extra call, should not go negative
            Assert.AreEqual(0, effect.RemainingTurns);
        }

        // --- Unit.ApplyStatus / ClearStatus ---

        [Test]
        public void Unit_StartsWithNoStatus()
        {
            var unit = CreateUnit();
            Assert.IsNull(unit.ActiveStatus);
        }

        [Test]
        public void Unit_ApplyStatus_SetsActiveStatus()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));
            Assert.IsNotNull(unit.ActiveStatus);
            Assert.AreEqual(StatusEffectType.Poison, unit.ActiveStatus.Type);
        }

        [Test]
        public void Unit_ClearStatus_RemovesActiveStatus()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));
            unit.ClearStatus();
            Assert.IsNull(unit.ActiveStatus);
        }

        // --- Unit.CanAct ---

        [Test]
        public void Unit_WithNoStatus_CanAct()
        {
            var unit = CreateUnit();
            Assert.IsTrue(unit.CanAct);
        }

        [Test]
        public void Unit_WithPoison_CanAct()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));
            Assert.IsTrue(unit.CanAct); // Poison does not prevent action
        }

        [Test]
        public void Unit_WithSleep_CannotAct()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Sleep, 2));
            Assert.IsFalse(unit.CanAct);
        }

        [Test]
        public void Unit_WithStun_CannotAct()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));
            Assert.IsFalse(unit.CanAct);
        }

        [Test]
        public void Unit_DeadUnit_CannotActRegardlessOfStatus()
        {
            var unit = CreateUnit(10);
            unit.TakeDamage(10); // kill
            Assert.IsFalse(unit.CanAct);
        }

        // --- Unit.TickStatus ---

        [Test]
        public void Unit_TickPoison_DealsDamageAndDecrements()
        {
            var unit = CreateUnit(30); // 30 MaxHP → 10% = 3 damage
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));
            int hpBefore = unit.CurrentHP;
            unit.TickStatus();
            Assert.AreEqual(hpBefore - 3, unit.CurrentHP);
            Assert.AreEqual(2, unit.ActiveStatus.RemainingTurns);
        }

        [Test]
        public void Unit_TickPoison_LastTurn_ClearsStatus()
        {
            var unit = CreateUnit(30);
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 1));
            unit.TickStatus();
            Assert.IsNull(unit.ActiveStatus); // expired, should be cleared
        }

        [Test]
        public void Unit_TickSleep_DecrementsDurationWithoutDamage()
        {
            var unit = CreateUnit(30);
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Sleep, 2));
            int hpBefore = unit.CurrentHP;
            unit.TickStatus();
            Assert.AreEqual(hpBefore, unit.CurrentHP); // no damage from sleep
            Assert.AreEqual(1, unit.ActiveStatus.RemainingTurns);
        }

        [Test]
        public void Unit_TickSleep_LastTurn_ClearsStatus()
        {
            var unit = CreateUnit(30);
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Sleep, 1));
            unit.TickStatus();
            Assert.IsNull(unit.ActiveStatus);
        }

        [Test]
        public void Unit_TickWithNoStatus_DoesNotThrow()
        {
            var unit = CreateUnit();
            Assert.DoesNotThrow(() => unit.TickStatus());
        }

        [Test]
        public void Unit_TickStun_ClearsAfterOneTurn()
        {
            var unit = CreateUnit(30);
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));
            unit.TickStatus();
            Assert.IsNull(unit.ActiveStatus);
            Assert.IsTrue(unit.CanAct); // now free to act
        }

        [Test]
        public void Unit_PoisonDamage_MinimumOneDamage_WhenMaxHpVeryLow()
        {
            // MaxHP = 5, 10% = 0.5 → floor 0, but clamped to 1
            var stats = new CharacterStats(5, 3, 0, 5, 5, 2, 2, 0, 3);
            var unit = new Unit(2, "Weakling", Team.EnemyTeam, ClassDataFactory.CreateSoldier(), stats, (0, 0), WeaponFactory.CreateIronLance());
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 1));
            unit.TickStatus();
            // Should take at least 1 damage
            Assert.Less(unit.CurrentHP, 5);
        }
    }
}
