using NUnit.Framework;
using TacticFantasy.Domain.Utils;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class PositionUtilsTests
    {
        [Test]
        public void Distance_ChebyshevDistance_ReturnsCorrect()
        {
            Assert.AreEqual(0, PositionUtils.Distance(0, 0, 0, 0));
            Assert.AreEqual(3, PositionUtils.Distance(0, 0, 3, 0));
            Assert.AreEqual(4, PositionUtils.Distance(1, 1, -3, 3));
            Assert.AreEqual(5, PositionUtils.Distance(-2, -1, 3, 4));
        }

        [Test]
        public void Distance_Symmetric()
        {
            Assert.AreEqual(PositionUtils.Distance(0,0,5,2), PositionUtils.Distance(5,2,0,0));
        }
    }
}