using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for the class promotion system.
    /// Fire Emblem: units at Lv.20 may promote to an advanced class.
    /// </summary>
    [TestFixture]
    public class ClassPromotionTests
    {
        // ── helpers ────────────────────────────────────────────────────────────

        private Unit CreateMaxLevelMyrmidon()
        {
            return new Unit(1, "Ryu", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: 20);
        }

        private Unit CreateLv1Myrmidon()
        {
            return new Unit(1, "Ryu", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: 1);
        }

        // ── CanPromote ─────────────────────────────────────────────────────────

        [Test]
        public void CanPromote_ReturnsFalse_WhenUnitIsNotMaxLevel()
        {
            var unit = CreateLv1Myrmidon();
            Assert.IsFalse(ClassPromotionService.CanPromote(unit));
        }

        [Test]
        public void CanPromote_ReturnsTrue_WhenMyrmidonIsLv20()
        {
            var unit = CreateMaxLevelMyrmidon();
            Assert.IsTrue(ClassPromotionService.CanPromote(unit));
        }

        [Test]
        public void CanPromote_ReturnsFalse_WhenUnitHasNoPromotionPath()
        {
            // Swordmaster (promoted) has no further promotion
            var unit = new Unit(1, "Ryu", Team.PlayerTeam,
                ClassDataFactory.CreateSwordmaster(),
                ClassDataFactory.CreateSwordmaster().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: 20);
            Assert.IsFalse(ClassPromotionService.CanPromote(unit));
        }

        // ── Promote ────────────────────────────────────────────────────────────

        [Test]
        public void Promote_ResetsLevelToOne()
        {
            var unit = CreateMaxLevelMyrmidon();
            ClassPromotionService.Promote(unit);
            Assert.AreEqual(1, unit.Level);
        }

        [Test]
        public void Promote_ResetsExperienceToZero()
        {
            var unit = CreateMaxLevelMyrmidon();
            ClassPromotionService.Promote(unit);
            Assert.AreEqual(0, unit.Experience);
        }

        [Test]
        public void Promote_ChangesClassToSwordmaster_ForMyrmidon()
        {
            var unit = CreateMaxLevelMyrmidon();
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Swordmaster", unit.Class.Name);
        }

        [Test]
        public void Promote_IncreasesStats_AfterPromotion()
        {
            var unit = CreateMaxLevelMyrmidon();
            int strBefore = unit.CurrentStats.STR;
            ClassPromotionService.Promote(unit);
            // Swordmaster base STR > Myrmidon base STR
            Assert.GreaterOrEqual(unit.CurrentStats.STR, strBefore);
        }

        [Test]
        public void Promote_IncreasesMaxHP_AfterPromotion()
        {
            var unit = CreateMaxLevelMyrmidon();
            int hpBefore = unit.MaxHP;
            ClassPromotionService.Promote(unit);
            Assert.GreaterOrEqual(unit.MaxHP, hpBefore);
        }

        [Test]
        public void Promote_ThrowsWhenUnitCannotPromote()
        {
            var unit = CreateLv1Myrmidon();
            Assert.Throws<System.InvalidOperationException>(() => ClassPromotionService.Promote(unit));
        }

        // ── Promotion paths ────────────────────────────────────────────────────

        [Test]
        public void PromotedClass_Soldier_IsGeneral()
        {
            var unit = new Unit(2, "Lars", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                (0, 0), WeaponFactory.CreateIronLance(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("General", unit.Class.Name);
        }

        [Test]
        public void PromotedClass_Fighter_IsWarrior()
        {
            var unit = new Unit(3, "Guts", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (0, 0), WeaponFactory.CreateIronAxe(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Warrior", unit.Class.Name);
        }

        [Test]
        public void PromotedClass_Mage_IsSage()
        {
            var unit = new Unit(4, "Lysithea", Team.PlayerTeam,
                ClassDataFactory.CreateMage(),
                ClassDataFactory.CreateMage().BaseStats,
                (0, 0), WeaponFactory.CreateFireTome(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Sage", unit.Class.Name);
        }

        [Test]
        public void PromotedClass_Archer_IsSniper()
        {
            var unit = new Unit(5, "Wil", Team.PlayerTeam,
                ClassDataFactory.CreateArcher(),
                ClassDataFactory.CreateArcher().BaseStats,
                (0, 0), WeaponFactory.CreateIronBow(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Sniper", unit.Class.Name);
        }

        [Test]
        public void PromotedClass_Cleric_IsBishop()
        {
            var unit = new Unit(6, "Serra", Team.PlayerTeam,
                ClassDataFactory.CreateCleric(),
                ClassDataFactory.CreateCleric().BaseStats,
                (0, 0), WeaponFactory.CreateHealStaff(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Bishop", unit.Class.Name);
        }
    }
}
