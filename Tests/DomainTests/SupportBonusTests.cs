using NUnit.Framework;
using TacticFantasy.Domain.Support;

namespace DomainTests
{
    [TestFixture]
    public class SupportBonusTests
    {
        [Test]
        public void ForLevel_None_ReturnsZeroBonuses()
        {
            var bonus = SupportBonus.ForLevel(SupportLevel.None);

            Assert.AreEqual(0, bonus.Attack);
            Assert.AreEqual(0, bonus.Defense);
            Assert.AreEqual(0, bonus.Hit);
            Assert.AreEqual(0, bonus.Avoid);
        }

        [Test]
        public void ForLevel_C_ReturnsSmallBonuses()
        {
            var bonus = SupportBonus.ForLevel(SupportLevel.C);

            Assert.AreEqual(1, bonus.Attack);
            Assert.AreEqual(1, bonus.Defense);
            Assert.AreEqual(5, bonus.Hit);
            Assert.AreEqual(5, bonus.Avoid);
        }

        [Test]
        public void ForLevel_B_ReturnsMediumBonuses()
        {
            var bonus = SupportBonus.ForLevel(SupportLevel.B);

            Assert.AreEqual(2, bonus.Attack);
            Assert.AreEqual(2, bonus.Defense);
            Assert.AreEqual(10, bonus.Hit);
            Assert.AreEqual(10, bonus.Avoid);
        }

        [Test]
        public void ForLevel_A_ReturnsLargeBonuses()
        {
            var bonus = SupportBonus.ForLevel(SupportLevel.A);

            Assert.AreEqual(3, bonus.Attack);
            Assert.AreEqual(3, bonus.Defense);
            Assert.AreEqual(15, bonus.Hit);
            Assert.AreEqual(15, bonus.Avoid);
        }

        [Test]
        public void Addition_CombinesTwoBonuses()
        {
            var a = SupportBonus.ForLevel(SupportLevel.C);
            var b = SupportBonus.ForLevel(SupportLevel.B);

            var sum = a + b;

            Assert.AreEqual(3, sum.Attack);
            Assert.AreEqual(3, sum.Defense);
            Assert.AreEqual(15, sum.Hit);
            Assert.AreEqual(15, sum.Avoid);
        }

        [Test]
        public void Zero_ReturnsAllZeroes()
        {
            var zero = SupportBonus.Zero;

            Assert.AreEqual(0, zero.Attack);
            Assert.AreEqual(0, zero.Defense);
            Assert.AreEqual(0, zero.Hit);
            Assert.AreEqual(0, zero.Avoid);
        }

        [Test]
        public void BonusesScaleWithLevel()
        {
            var c = SupportBonus.ForLevel(SupportLevel.C);
            var b = SupportBonus.ForLevel(SupportLevel.B);
            var a = SupportBonus.ForLevel(SupportLevel.A);

            Assert.Less(c.Attack, b.Attack);
            Assert.Less(b.Attack, a.Attack);
            Assert.Less(c.Hit, b.Hit);
            Assert.Less(b.Hit, a.Hit);
        }
    }
}
