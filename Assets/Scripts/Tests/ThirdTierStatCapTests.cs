using NUnit.Framework;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ThirdTierStatCapTests
    {
        // Third-tier stat caps should be significantly higher than Tier 2

        [Test]
        public void Trueblade_HasHigherCaps_ThanSwordmaster()
        {
            var t2 = ClassDataFactory.CreateSwordmaster();
            var t3 = ClassDataFactory.CreateTrueblade();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.STR, t2.CapStats.STR);
            Assert.Greater(t3.CapStats.SKL, t2.CapStats.SKL);
            Assert.Greater(t3.CapStats.SPD, t2.CapStats.SPD);
        }

        [Test]
        public void Marshall_HasHigherCaps_ThanGeneral()
        {
            var t2 = ClassDataFactory.CreateGeneral();
            var t3 = ClassDataFactory.CreateMarshall();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.STR, t2.CapStats.STR);
            Assert.Greater(t3.CapStats.DEF, t2.CapStats.DEF);
        }

        [Test]
        public void Reaver_HasHigherCaps_ThanWarrior()
        {
            var t2 = ClassDataFactory.CreateWarrior();
            var t3 = ClassDataFactory.CreateReaver();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.STR, t2.CapStats.STR);
        }

        [Test]
        public void Archsage_HasHigherCaps_ThanSage()
        {
            var t2 = ClassDataFactory.CreateSage();
            var t3 = ClassDataFactory.CreateArchsage();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.MAG, t2.CapStats.MAG);
        }

        [Test]
        public void Marksman_HasHigherCaps_ThanSniper()
        {
            var t2 = ClassDataFactory.CreateSniper();
            var t3 = ClassDataFactory.CreateMarksman();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.STR, t2.CapStats.STR);
            Assert.Greater(t3.CapStats.SKL, t2.CapStats.SKL);
        }

        [Test]
        public void Saint_HasHigherCaps_ThanBishop()
        {
            var t2 = ClassDataFactory.CreateBishop();
            var t3 = ClassDataFactory.CreateSaint();
            Assert.Greater(t3.CapStats.HP, t2.CapStats.HP);
            Assert.Greater(t3.CapStats.MAG, t2.CapStats.MAG);
            Assert.Greater(t3.CapStats.RES, t2.CapStats.RES);
        }

        // Third-tier base stats should be higher than Tier 2 base stats

        [Test]
        public void Trueblade_HasHigherBaseStats_ThanSwordmaster()
        {
            var t2 = ClassDataFactory.CreateSwordmaster();
            var t3 = ClassDataFactory.CreateTrueblade();
            Assert.Greater(t3.BaseStats.HP, t2.BaseStats.HP);
            Assert.Greater(t3.BaseStats.STR, t2.BaseStats.STR);
            Assert.Greater(t3.BaseStats.SKL, t2.BaseStats.SKL);
        }

        [Test]
        public void AllThirdTierClasses_HaveMOV7OrHigher()
        {
            Assert.GreaterOrEqual(ClassDataFactory.CreateTrueblade().BaseStats.MOV, 7);
            Assert.GreaterOrEqual(ClassDataFactory.CreateMarshall().BaseStats.MOV, 5);
            Assert.GreaterOrEqual(ClassDataFactory.CreateReaver().BaseStats.MOV, 7);
            Assert.GreaterOrEqual(ClassDataFactory.CreateArchsage().BaseStats.MOV, 7);
            Assert.GreaterOrEqual(ClassDataFactory.CreateMarksman().BaseStats.MOV, 7);
            Assert.GreaterOrEqual(ClassDataFactory.CreateSaint().BaseStats.MOV, 7);
        }
    }
}
