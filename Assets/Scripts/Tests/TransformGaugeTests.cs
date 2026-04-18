using NUnit.Framework;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class TransformGaugeTests
    {
        [Test]
        public void Constructor_SetsInitialValues()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 10);

            Assert.AreEqual(10, gauge.Current);
            Assert.AreEqual(5, gauge.FillRate);
            Assert.AreEqual(2, gauge.DrainRate);
            Assert.IsFalse(gauge.IsFull);
            Assert.IsFalse(gauge.IsEmpty);
        }

        [Test]
        public void Constructor_DefaultsToZero()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2);

            Assert.AreEqual(0, gauge.Current);
            Assert.IsTrue(gauge.IsEmpty);
        }

        [Test]
        public void Constructor_ClampsInitialValue()
        {
            var over = new TransformGauge(5, 2, initialValue: 50);
            Assert.AreEqual(TransformGauge.MaxGauge, over.Current);

            var under = new TransformGauge(5, 2, initialValue: -10);
            Assert.AreEqual(TransformGauge.MinGauge, under.Current);
        }

        // ── Tick untransformed (fill) ───────────────────────────────────────

        [Test]
        public void Tick_Untransformed_FillsByFillRate()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 0);

            gauge.Tick(isTransformed: false);

            Assert.AreEqual(5, gauge.Current);
        }

        [Test]
        public void Tick_Untransformed_ClampsAtMax()
        {
            var gauge = new TransformGauge(fillRate: 8, drainRate: 2, initialValue: 28);

            gauge.Tick(isTransformed: false);

            Assert.AreEqual(TransformGauge.MaxGauge, gauge.Current);
        }

        [Test]
        public void Tick_Untransformed_ReturnsTrueWhenFull()
        {
            var gauge = new TransformGauge(fillRate: 10, drainRate: 2, initialValue: 25);

            bool stateChange = gauge.Tick(isTransformed: false);

            Assert.IsTrue(stateChange);
            Assert.IsTrue(gauge.IsFull);
        }

        [Test]
        public void Tick_Untransformed_ReturnsFalseWhenNotFull()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 0);

            bool stateChange = gauge.Tick(isTransformed: false);

            Assert.IsFalse(stateChange);
        }

        // ── Tick transformed (drain) ──────���─────────────────────────────────

        [Test]
        public void Tick_Transformed_DrainsByDrainRate()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 30);

            gauge.Tick(isTransformed: true);

            Assert.AreEqual(28, gauge.Current);
        }

        [Test]
        public void Tick_Transformed_ClampsAtMin()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 5, initialValue: 3);

            gauge.Tick(isTransformed: true);

            Assert.AreEqual(TransformGauge.MinGauge, gauge.Current);
        }

        [Test]
        public void Tick_Transformed_ReturnsTrueWhenEmpty()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 5, initialValue: 3);

            bool stateChange = gauge.Tick(isTransformed: true);

            Assert.IsTrue(stateChange);
            Assert.IsTrue(gauge.IsEmpty);
        }

        [Test]
        public void Tick_Transformed_ReturnsFalseWhenNotEmpty()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 30);

            bool stateChange = gauge.Tick(isTransformed: true);

            Assert.IsFalse(stateChange);
        }

        // ── FillToMax / AddPoints ───────────────────────────────────────────

        [Test]
        public void FillToMax_SetsGaugeToMax()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 0);

            gauge.FillToMax();

            Assert.AreEqual(TransformGauge.MaxGauge, gauge.Current);
            Assert.IsTrue(gauge.IsFull);
        }

        [Test]
        public void AddPoints_IncreasesGauge()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 10);

            gauge.AddPoints(15);

            Assert.AreEqual(25, gauge.Current);
        }

        [Test]
        public void AddPoints_ClampsAtMax()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 20);

            gauge.AddPoints(15);

            Assert.AreEqual(TransformGauge.MaxGauge, gauge.Current);
        }

        [Test]
        public void AddPoints_NegativeValuesIgnored()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 15);

            gauge.AddPoints(-5);

            Assert.AreEqual(15, gauge.Current);
        }

        // ── Cat fills fastest (8/turn), Dragon fills slowest (2/turn) ───────

        [Test]
        public void Cat_FillsInFourTurns()
        {
            var gauge = new TransformGauge(fillRate: 8, drainRate: 2, initialValue: 0);

            for (int i = 0; i < 3; i++)
                gauge.Tick(isTransformed: false);

            Assert.AreEqual(24, gauge.Current);
            Assert.IsFalse(gauge.IsFull);

            gauge.Tick(isTransformed: false);
            Assert.IsTrue(gauge.IsFull);
        }

        [Test]
        public void RedDragon_FillsInFifteenTurns()
        {
            var gauge = new TransformGauge(fillRate: 2, drainRate: 2, initialValue: 0);

            for (int i = 0; i < 14; i++)
                gauge.Tick(isTransformed: false);

            Assert.AreEqual(28, gauge.Current);
            Assert.IsFalse(gauge.IsFull);

            gauge.Tick(isTransformed: false);
            Assert.IsTrue(gauge.IsFull);
        }

        [Test]
        public void Transformed_DrainsOverFifteenTurns()
        {
            var gauge = new TransformGauge(fillRate: 5, drainRate: 2, initialValue: 30);

            for (int i = 0; i < 14; i++)
                gauge.Tick(isTransformed: true);

            Assert.AreEqual(2, gauge.Current);

            bool stateChange = gauge.Tick(isTransformed: true);
            Assert.IsTrue(stateChange);
            Assert.IsTrue(gauge.IsEmpty);
        }
    }
}
