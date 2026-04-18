using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ThirdTierPromotionTests
    {
        // ── Tier property ──────────────────────────────────────────────────

        [Test]
        public void BaseClass_HasTier1()
        {
            Assert.AreEqual(1, ClassDataFactory.CreateMyrmidon().Tier);
            Assert.AreEqual(1, ClassDataFactory.CreateSoldier().Tier);
            Assert.AreEqual(1, ClassDataFactory.CreateFighter().Tier);
            Assert.AreEqual(1, ClassDataFactory.CreateMage().Tier);
            Assert.AreEqual(1, ClassDataFactory.CreateArcher().Tier);
            Assert.AreEqual(1, ClassDataFactory.CreateCleric().Tier);
        }

        [Test]
        public void AdvancedClass_HasTier2()
        {
            Assert.AreEqual(2, ClassDataFactory.CreateSwordmaster().Tier);
            Assert.AreEqual(2, ClassDataFactory.CreateGeneral().Tier);
            Assert.AreEqual(2, ClassDataFactory.CreateWarrior().Tier);
            Assert.AreEqual(2, ClassDataFactory.CreateSage().Tier);
            Assert.AreEqual(2, ClassDataFactory.CreateSniper().Tier);
            Assert.AreEqual(2, ClassDataFactory.CreateBishop().Tier);
        }

        [Test]
        public void MasterClass_HasTier3()
        {
            Assert.AreEqual(3, ClassDataFactory.CreateTrueblade().Tier);
            Assert.AreEqual(3, ClassDataFactory.CreateMarshall().Tier);
            Assert.AreEqual(3, ClassDataFactory.CreateReaver().Tier);
            Assert.AreEqual(3, ClassDataFactory.CreateArchsage().Tier);
            Assert.AreEqual(3, ClassDataFactory.CreateMarksman().Tier);
            Assert.AreEqual(3, ClassDataFactory.CreateSaint().Tier);
        }

        // ── Tier 2 → Tier 3 promotion paths ────────────────────────────────

        [Test]
        public void Swordmaster_PromotesToTrueblade()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSwordmaster(), WeaponFactory.CreateIronSword());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Trueblade", unit.Class.Name);
            Assert.AreEqual(3, unit.Class.Tier);
        }

        [Test]
        public void General_PromotesToMarshall()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateGeneral(), WeaponFactory.CreateIronLance());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Marshall", unit.Class.Name);
        }

        [Test]
        public void Warrior_PromotesToReaver()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateWarrior(), WeaponFactory.CreateIronAxe());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Reaver", unit.Class.Name);
        }

        [Test]
        public void Sage_PromotesToArchsage()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSage(), WeaponFactory.CreateFireTome());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Archsage", unit.Class.Name);
        }

        [Test]
        public void Sniper_PromotesToMarksman()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSniper(), WeaponFactory.CreateIronBow());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Marksman", unit.Class.Name);
        }

        [Test]
        public void Bishop_PromotesToSaint()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateBishop(), WeaponFactory.CreateHealStaff());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual("Saint", unit.Class.Name);
        }

        // ── Promotion mechanics ────────────────────────────────────────────

        [Test]
        public void Promote_ResetsLevelToOne_ForTier3()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSwordmaster(), WeaponFactory.CreateIronSword());
            ClassPromotionService.Promote(unit);
            Assert.AreEqual(1, unit.Level);
        }

        [Test]
        public void Promote_IncreasesStats_ForTier3()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSwordmaster(), WeaponFactory.CreateIronSword());
            int strBefore = unit.CurrentStats.STR;
            ClassPromotionService.Promote(unit);
            Assert.GreaterOrEqual(unit.CurrentStats.STR, strBefore);
        }

        [Test]
        public void Promote_ThirdTierClassHasNoFurtherPromotion()
        {
            var unit = new Unit(1, "Master", Team.PlayerTeam,
                ClassDataFactory.CreateTrueblade(),
                ClassDataFactory.CreateTrueblade().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: 20);
            Assert.IsFalse(ClassPromotionService.CanPromote(unit));
        }

        // ── Mastery skill auto-learn on Tier 3 promotion ───────────────────

        [Test]
        public void Promote_ToTrueblade_LearnsAstra()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSwordmaster(), WeaponFactory.CreateIronSword());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Astra"));
        }

        [Test]
        public void Promote_ToMarshall_LearnsSol()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateGeneral(), WeaponFactory.CreateIronLance());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Sol"));
        }

        [Test]
        public void Promote_ToReaver_LearnsColossus()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateWarrior(), WeaponFactory.CreateIronAxe());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Colossus"));
        }

        [Test]
        public void Promote_ToArchsage_LearnsFlare()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSage(), WeaponFactory.CreateFireTome());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Flare"));
        }

        [Test]
        public void Promote_ToMarksman_LearnsDeadeye()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateSniper(), WeaponFactory.CreateIronBow());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Deadeye"));
        }

        [Test]
        public void Promote_ToSaint_LearnsCorona()
        {
            var unit = CreateMaxLevelUnit(ClassDataFactory.CreateBishop(), WeaponFactory.CreateHealStaff());
            ClassPromotionService.Promote(unit);
            Assert.IsTrue(unit.EquippedSkills.Any(s => s.Name == "Corona"));
        }

        [Test]
        public void Promote_Tier1ToTier2_DoesNotLearnMasterySkill()
        {
            var unit = new Unit(1, "Ryu", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: 20);
            ClassPromotionService.Promote(unit);
            Assert.AreEqual(0, unit.EquippedSkills.Count);
        }

        // ── Multi-weapon on third-tier ─────────────────────────────────────

        [Test]
        public void Marshall_CanUseSwordLanceAxe()
        {
            var marshall = ClassDataFactory.CreateMarshall();
            Assert.Contains(WeaponType.SWORD, (System.Collections.ICollection)marshall.UsableWeaponTypes);
            Assert.Contains(WeaponType.LANCE, (System.Collections.ICollection)marshall.UsableWeaponTypes);
            Assert.Contains(WeaponType.AXE, (System.Collections.ICollection)marshall.UsableWeaponTypes);
        }

        // ── helpers ────────────────────────────────────────────────────────

        private Unit CreateMaxLevelUnit(IClassData classData, IWeapon weapon)
        {
            return new Unit(1, "TestUnit", Team.PlayerTeam,
                classData, classData.BaseStats,
                (0, 0), weapon,
                levelOverride: 20);
        }
    }
}
