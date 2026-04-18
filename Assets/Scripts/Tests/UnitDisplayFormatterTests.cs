using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class UnitDisplayFormatterTests
    {
        private Unit CreateUnit()
        {
            var stats = new CharacterStats(30, 5, 0, 8, 8, 3, 5, 0, 5);
            return new Unit(1, "Ira", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(), stats, (0, 0), WeaponFactory.CreateIronSword());
        }

        // --- FormatStatus ---

        [Test]
        public void FormatStatus_NoStatus_ReturnsEmpty()
        {
            var unit = CreateUnit();
            Assert.AreEqual("", UnitDisplayFormatter.FormatStatus(unit));
        }

        [Test]
        public void FormatStatus_NullUnit_ReturnsEmpty()
        {
            Assert.AreEqual("", UnitDisplayFormatter.FormatStatus(null));
        }

        [Test]
        public void FormatStatus_Poison_ContainsPoisonAndTurns()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));
            var result = UnitDisplayFormatter.FormatStatus(unit);
            StringAssert.Contains("Poisoned", result);
            StringAssert.Contains("3", result);
        }

        [Test]
        public void FormatStatus_Sleep_ContainsSleepAndTurns()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Sleep, 2));
            var result = UnitDisplayFormatter.FormatStatus(unit);
            StringAssert.Contains("Sleep", result);
            StringAssert.Contains("2", result);
        }

        [Test]
        public void FormatStatus_Stun_ContainsStun()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));
            var result = UnitDisplayFormatter.FormatStatus(unit);
            StringAssert.Contains("Stun", result);
        }

        [Test]
        public void FormatStatus_AfterStatusExpires_ReturnsEmpty()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));
            unit.TickStatus(); // expires
            Assert.AreEqual("", UnitDisplayFormatter.FormatStatus(unit));
        }

        // --- FormatUnitInfo ---

        [Test]
        public void FormatUnitInfo_NullUnit_ReturnsSelectPrompt()
        {
            var result = UnitDisplayFormatter.FormatUnitInfo(null);
            Assert.AreEqual("Select a unit", result);
        }

        [Test]
        public void FormatUnitInfo_NoStatus_DoesNotContainStatusLine()
        {
            var unit = CreateUnit();
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("Ira", result);
            StringAssert.Contains("HP:", result);
            StringAssert.DoesNotContain("Poisoned", result);
            StringAssert.DoesNotContain("Sleep", result);
        }

        [Test]
        public void FormatUnitInfo_WithPoison_AppendsPoisonLine()
        {
            var unit = CreateUnit();
            unit.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 2));
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("Poisoned", result);
        }

        [Test]
        public void FormatUnitInfo_ContainsWeaponName()
        {
            var unit = CreateUnit();
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("Weapon:", result);
        }

        // --- Level & Experience in FormatUnitInfo ---

        [Test]
        public void FormatUnitInfo_ContainsLevelAndExp()
        {
            var unit = CreateUnit(); // starts at level 1, 0 XP
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("Lv", result);
            StringAssert.Contains("EXP", result);
        }

        [Test]
        public void FormatUnitInfo_ShowsCorrectLevel()
        {
            var unit = CreateUnit();
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("Lv:1", result);
        }

        [Test]
        public void FormatUnitInfo_ShowsCorrectExp_AfterGainingXP()
        {
            var unit = CreateUnit();
            unit.GainExperience(42);
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("42", result);
        }

        [Test]
        public void FormatUnitInfo_ShowsExpOutOf100()
        {
            var unit = CreateUnit();
            unit.GainExperience(30);
            var result = UnitDisplayFormatter.FormatUnitInfo(unit);
            StringAssert.Contains("/100", result);
        }

        // --- FormatLevelInfo standalone ---

        [Test]
        public void FormatLevelInfo_NullUnit_ReturnsEmpty()
        {
            Assert.AreEqual("", UnitDisplayFormatter.FormatLevelInfo(null));
        }

        [Test]
        public void FormatLevelInfo_ReturnsLvAndExp()
        {
            var unit = CreateUnit();
            var result = UnitDisplayFormatter.FormatLevelInfo(unit);
            StringAssert.Contains("Lv", result);
            StringAssert.Contains("EXP", result);
        }

        [Test]
        public void FormatLevelInfo_AtMaxLevel_ShowsMaxLabel()
        {
            // Create a unit at max level by leveling up
            var stats = new CharacterStats(30, 5, 0, 8, 8, 3, 5, 0, 5);
            var unit = new Unit(2, "Max", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(), stats, (0,0),
                WeaponFactory.CreateIronSword(), levelOverride: Unit.MaxLevel);
            var result = UnitDisplayFormatter.FormatLevelInfo(unit);
            StringAssert.Contains("MAX", result);
        }
    }
}
