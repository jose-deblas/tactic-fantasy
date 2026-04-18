using NUnit.Framework;
using TacticFantasy.Domain.Utils;

namespace DomainTests
{
    public class FloatUtilsTests
    {
        [Test]
        public void Clamp_ValueWithinRange_ReturnsValue()
        {
            Assert.AreEqual(5f, FloatUtils.Clamp(5f, 0f, 10f));
        }

        [Test]
        public void Clamp_ValueBelowMin_ReturnsMin()
        {
            Assert.AreEqual(0f, FloatUtils.Clamp(-1f, 0f, 10f));
        }

        [Test]
        public void Clamp_ValueAboveMax_ReturnsMax()
        {
            Assert.AreEqual(10f, FloatUtils.Clamp(11f, 0f, 10f));
        }

        [Test]
        public void Clamp_MinGreaterThanMax_SwapsAndClamps()
        {
            // min and max swapped - should still clamp correctly
            Assert.AreEqual(3f, FloatUtils.Clamp(3f, 5f, 1f));
            Assert.AreEqual(1f, FloatUtils.Clamp(0f, 5f, 1f));
            Assert.AreEqual(5f, FloatUtils.Clamp(10f, 5f, 1f));
        }
    }
}