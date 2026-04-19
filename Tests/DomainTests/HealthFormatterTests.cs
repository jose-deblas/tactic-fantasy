using NUnit.Framework;
using TacticFantasy.Presentation;
using System;

namespace DomainTests
{
    public class HealthFormatterTests
    {
        [Test]
        public void Format_NormalValues_ReturnsFormattedString()
        {
            var s = HealthFormatter.Format(30, 50);
            Assert.AreEqual("HP: 30/50 (60%)", s);
        }

        [Test]
        public void Format_ClampBelowZero_ReturnsZero()
        {
            var s = HealthFormatter.Format(-10, 50);
            Assert.AreEqual("HP: 0/50 (0%)", s);
        }

        [Test]
        public void Format_ClampAboveMax_ReturnsMax()
        {
            var s = HealthFormatter.Format(120, 100);
            Assert.AreEqual("HP: 100/100 (100%)", s);
        }

        [Test]
        public void Format_MaxZero_Throws()
        {
            Assert.Throws<ArgumentException>(() => HealthFormatter.Format(10, 0));
        }
    }
}
