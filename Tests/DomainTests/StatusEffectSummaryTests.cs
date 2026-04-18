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
            public void Tick(float deltaTime, IUnit target) { Duration -= deltaTime; }

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
    }
}
