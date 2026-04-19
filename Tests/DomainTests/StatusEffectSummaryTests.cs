using NUnit.Framework;
using TacticFantasy.Domain;

namespace DomainTests
{
    public class StatusEffectSummaryTests
    {
        class DummyEffect : IStatusEffect
        {
            public string Name { get; } = "Dummy";
            public float Duration { get; private set; }
            public bool IsExpired => Duration <= 0f;
            public void Tick(float deltaTime, IStatusTarget target) { Duration -= deltaTime; }

            public DummyEffect(float duration) { Duration = duration; }
        }

        [Test]
        public void SummaryShowsNameAndRemaining()
        {
            var e = new DummyEffect(5f);
            var s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (5.00s)", s.ToString());

            e.Tick(1.2345f);
            s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (3.77s)", s.ToString());
        }

        [Test]
        public void SummaryShowsExpiredWhenDurationZeroOrLess()
        {
            var e = new DummyEffect(1f);
            e.Tick(1f);
            var s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (expired)", s.ToString());

            e = new DummyEffect(0f);
            s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (expired)", s.ToString());
        }

        [Test]
        public void SummaryFormatsSubsecondAsMilliseconds()
        {
            var e = new DummyEffect(0.345f);
            var s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (345ms)", s.ToString());

            e = new DummyEffect(0.999f);
            s = StatusEffectSummary.From(e);
            Assert.AreEqual("Dummy (999ms)", s.ToString());
        }
    }
}
