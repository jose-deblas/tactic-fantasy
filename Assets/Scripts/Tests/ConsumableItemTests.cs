using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ConsumableItemTests
    {
        private Unit CreateUnit(int currentHP, int maxHP)
        {
            var classData = ClassDataFactory.CreateMyrmidon();
            var stats = new CharacterStats(maxHP, 10, 0, 10, 10, 5, 5, 2, 5);
            var unit = new Unit(1, "Test", Team.PlayerTeam, classData, stats, (0, 0), WeaponFactory.CreateIronSword());
            unit.TakeDamage(maxHP - currentHP);
            return unit;
        }

        [Test]
        public void Vulnerary_Heals10HP()
        {
            var unit = CreateUnit(10, 30);
            var vulnerary = ConsumableFactory.CreateVulnerary();
            vulnerary.Use(unit);
            Assert.AreEqual(20, unit.CurrentHP);
        }

        [Test]
        public void Vulnerary_ThreeUses_ThenUnusable()
        {
            var unit = CreateUnit(10, 30);
            var vulnerary = ConsumableFactory.CreateVulnerary();
            vulnerary.Use(unit);
            vulnerary.Use(unit);
            vulnerary.Use(unit);
            Assert.IsFalse(vulnerary.IsUsable);
            Assert.AreEqual(0, vulnerary.CurrentUses);
        }

        [Test]
        public void Vulnerary_Use_WhenExhausted_NoEffect()
        {
            var unit = CreateUnit(10, 30);
            var vulnerary = ConsumableFactory.CreateVulnerary();
            vulnerary.Use(unit);
            vulnerary.Use(unit);
            vulnerary.Use(unit);
            // Fourth use should have no effect
            unit.TakeDamage(10); // bring HP down
            int hpBefore = unit.CurrentHP;
            vulnerary.Use(unit);
            Assert.AreEqual(hpBefore, unit.CurrentHP);
        }

        [Test]
        public void Elixir_HealsToFull()
        {
            var unit = CreateUnit(1, 30);
            var elixir = ConsumableFactory.CreateElixir();
            elixir.Use(unit);
            Assert.AreEqual(30, unit.CurrentHP);
        }

        [Test]
        public void Antitoxin_CuresPoison()
        {
            var unit = CreateUnit(20, 30);
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));
            Assert.AreEqual(StatusEffectType.Poison, unit.ActiveStatus.Type);

            var antitoxin = ConsumableFactory.CreateAntitoxin();
            antitoxin.Use(unit);
            Assert.IsNull(unit.ActiveStatus);
        }

        [Test]
        public void Antitoxin_NoEffectWithoutPoison()
        {
            var unit = CreateUnit(20, 30);
            var antitoxin = ConsumableFactory.CreateAntitoxin();
            antitoxin.Use(unit);
            Assert.IsNull(unit.ActiveStatus);
        }

        [Test]
        public void ConsumableItem_Use_DecrementsCount()
        {
            var unit = CreateUnit(10, 30);
            var vulnerary = ConsumableFactory.CreateVulnerary();
            Assert.AreEqual(3, vulnerary.CurrentUses);
            vulnerary.Use(unit);
            Assert.AreEqual(2, vulnerary.CurrentUses);
        }

        [Test]
        public void StatBooster_EnergyDrop_AddsSTR()
        {
            var unit = CreateUnit(30, 30);
            int strBefore = unit.CurrentStats.STR;
            var energyDrop = StatBoosterFactory.CreateEnergyDrop();
            energyDrop.Use(unit);
            Assert.AreEqual(strBefore + 2, unit.CurrentStats.STR);
            Assert.IsFalse(energyDrop.IsUsable);
        }

        [Test]
        public void StatBooster_SeraphRobe_AddsHP()
        {
            var unit = CreateUnit(30, 30);
            int maxHPBefore = unit.MaxHP;
            var seraphRobe = StatBoosterFactory.CreateSeraphRobe();
            seraphRobe.Use(unit);
            Assert.AreEqual(maxHPBefore + 7, unit.MaxHP);
            Assert.AreEqual(maxHPBefore + 7, unit.CurrentHP);
        }

        [Test]
        public void StatBooster_CanOnlyBeUsedOnce()
        {
            var unit = CreateUnit(30, 30);
            var energyDrop = StatBoosterFactory.CreateEnergyDrop();
            energyDrop.Use(unit);
            int strAfterFirst = unit.CurrentStats.STR;
            energyDrop.Use(unit);
            Assert.AreEqual(strAfterFirst, unit.CurrentStats.STR);
        }
    }
}
