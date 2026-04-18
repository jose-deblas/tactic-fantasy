using NUnit.Framework;
using TacticFantasy.Domain;

namespace DomainTests
{
    class FakeUnit : IUnit
    {
        public float Health { get; set; }
        public void TakeDamage(float amount) { Health -= amount; }
    }

    public class StatusEffectTests
    {
        [Test]
        public void PoisonTicksAndExpires()
        {
            var unit = new FakeUnit { Health = 100f };
            var poison = new PoisonEffect(duration: 3f, dps: 2f); // 2 DPS for 3s => 6 damage

            poison.Tick(1f, unit);
            Assert.AreEqual(98f, unit.Health, 1e-4);
            Assert.IsFalse(poison.IsExpired);

            poison.Tick(2f, unit);
            Assert.AreEqual(94f, unit.Health, 1e-4);
            Assert.IsTrue(poison.IsExpired);

            // Further ticks do nothing
            poison.Tick(1f, unit);
            Assert.AreEqual(94f, unit.Health, 1e-4);
        }
    }
}