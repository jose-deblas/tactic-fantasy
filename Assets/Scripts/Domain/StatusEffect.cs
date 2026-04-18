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
    }

    public class PoisonEffect : IStatusEffect
    {
        public string Name => "Poison";
        // Duration represents remaining time in seconds
        public float Duration { get; private set; }
        public float DamagePerSecond { get; }

        public PoisonEffect(float duration, float dps)
        {
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
}