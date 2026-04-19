using System;

namespace TacticFantasy.Domain
{
    public interface IStatusEffect
    {
        string Name { get; }
        float Duration { get; }
        void Tick(float deltaTime, IStatusTarget target);
        bool IsExpired { get; }
    }

    public interface IStatusTarget
    {
        float Health { get; set; }
        void TakeDamage(float amount);
        // Whether the unit can perform actions (move/attack). Status effects like Stun may toggle this.
        bool CanAct { get; set; }
    }

    public class PoisonEffect : IStatusEffect
    {
        public string Name => "Poison";
        // Duration represents remaining time in seconds
        public float Duration { get; private set; }
        public float DamagePerSecond { get; }

        public PoisonEffect(float duration, float dps)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be non-negative");
            if (dps < 0f) throw new ArgumentOutOfRangeException(nameof(dps), "Damage per second must be non-negative");

            Duration = duration;
            DamagePerSecond = dps;
        }

        public void Tick(float deltaTime, IStatusTarget target)
        {
            if (IsExpired) return;

            // Only apply up to the remaining duration
            var effective = Math.Min(deltaTime, Duration);
            var damage = DamagePerSecond * effective;
            target.TakeDamage(damage);

            // Decrease remaining duration
            Duration -= effective;
        }

        public bool IsExpired => Duration <= 0f;
    }

    public class StunEffect : IStatusEffect
    {
        public string Name => "Stun";
        public float Duration { get; private set; }

        public StunEffect(float duration)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be non-negative");
            Duration = duration;
        }

        public void Tick(float deltaTime, IStatusTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (IsExpired)
            {
                // Ensure the target can act again once expired
                target.CanAct = true;
                return;
            }

            // While stunned, target cannot act
            target.CanAct = false;

            var effective = Math.Min(deltaTime, Duration);
            Duration -= effective;

            if (IsExpired)
            {
                target.CanAct = true;
            }
        }

        public bool IsExpired => Duration <= 0f;
    }
}
