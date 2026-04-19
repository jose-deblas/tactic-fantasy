using NUnit.Framework;
using TacticFantasy.Domain;

namespace DomainTests
{
    class FakeUnit : IStatusTarget
    {
        public float Health { get; set; }
        public bool CanAct { get; set; } = true;
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

        [Test]
        public void StunPreventsActionsAndRestores()
        {
            var unit = new FakeUnit { Health = 100f };
            var stun = new StunEffect(duration: 2f);

            Assert.IsTrue(unit.CanAct);

            stun.Tick(1f, unit);
            Assert.IsFalse(unit.CanAct);
            Assert.IsFalse(stun.IsExpired);

            stun.Tick(1f, unit);
            Assert.IsTrue(stun.IsExpired);
            Assert.IsTrue(unit.CanAct);

            // Further ticks should keep unit able to act
            stun.Tick(1f, unit);
            Assert.IsTrue(unit.CanAct);
        }

        [Test]
        public void PoisonConstructor_RejectsNegativeParams()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PoisonEffect(-1f, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PoisonEffect(1f, -0.5f));
        }

        [Test]
        public void StunConstructor_RejectsNegativeDuration()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new StunEffect(-0.1f));
        }

        [Test]
        public void PoisonTick_ThrowsOnNullTarget()
        {
            var poison = new PoisonEffect(duration: 1f, dps: 1f);
            Assert.Throws<System.ArgumentNullException>(() => poison.Tick(1f, null));
        }

        [Test]
        public void StunTick_ThrowsOnNullTarget()
        {
            var stun = new StunEffect(duration: 1f);
            Assert.Throws<System.ArgumentNullException>(() => stun.Tick(1f, null));
        }
    }
}