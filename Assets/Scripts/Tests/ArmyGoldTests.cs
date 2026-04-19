using System;
using NUnit.Framework;
using TacticFantasy.Domain.Chapter;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ArmyGoldTests
    {
        [Test]
        public void Constructor_InitializesWithGivenAmount()
        {
            var gold = new ArmyGold(500);
            Assert.AreEqual(500, gold.Gold);
        }

        [Test]
        public void Constructor_DefaultsToZero()
        {
            var gold = new ArmyGold();
            Assert.AreEqual(0, gold.Gold);
        }

        [Test]
        public void Constructor_NegativeInitial_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ArmyGold(-1));
        }

        [Test]
        public void Earn_AddsGold()
        {
            var gold = new ArmyGold(100);
            gold.Earn(50);
            Assert.AreEqual(150, gold.Gold);
        }

        [Test]
        public void Earn_NegativeAmount_Throws()
        {
            var gold = new ArmyGold(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => gold.Earn(-10));
        }

        [Test]
        public void Spend_DeductsGold()
        {
            var gold = new ArmyGold(100);
            gold.Spend(40);
            Assert.AreEqual(60, gold.Gold);
        }

        [Test]
        public void Spend_ExactAmount_LeavesZero()
        {
            var gold = new ArmyGold(100);
            gold.Spend(100);
            Assert.AreEqual(0, gold.Gold);
        }

        [Test]
        public void Spend_MoreThanAvailable_Throws()
        {
            var gold = new ArmyGold(100);
            Assert.Throws<InvalidOperationException>(() => gold.Spend(101));
        }

        [Test]
        public void Spend_NegativeAmount_Throws()
        {
            var gold = new ArmyGold(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => gold.Spend(-5));
        }

        [Test]
        public void CanAfford_WithEnoughGold_ReturnsTrue()
        {
            var gold = new ArmyGold(100);
            Assert.IsTrue(gold.CanAfford(100));
            Assert.IsTrue(gold.CanAfford(50));
        }

        [Test]
        public void CanAfford_WithInsufficientGold_ReturnsFalse()
        {
            var gold = new ArmyGold(100);
            Assert.IsFalse(gold.CanAfford(101));
        }

        [Test]
        public void CanAfford_ZeroCost_ReturnsTrue()
        {
            var gold = new ArmyGold(0);
            Assert.IsTrue(gold.CanAfford(0));
        }
    }
}
