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
            if (target == null) throw new ArgumentNullException(nameof(target));
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

    public class RegenerationEffect : IStatusEffect
    {
        public string Name => "Regeneration";
        public float Duration { get; private set; }
        public float HealPerSecond { get; }

        public RegenerationEffect(float duration, float hps)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be non-negative");
            if (hps < 0f) throw new ArgumentOutOfRangeException(nameof(hps), "Heal per second must be non-negative");

            Duration = duration;
            HealPerSecond = hps;
        }

        public void Tick(float deltaTime, IStatusTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (IsExpired) return;

            var effective = Math.Min(deltaTime, Duration);
            var heal = HealPerSecond * effective;

            // Healing implemented as negative damage for simplicity
            target.TakeDamage(-heal);

            Duration -= effective;
        }

        public bool IsExpired => Duration <= 0f;
    }

    public class ShieldEffect : IStatusEffect
    {
        public string Name => "Shield";
        public float Duration { get; private set; }
        public float Amount { get; }

        // Tracks how much shield remains; applied to target.Health when activated
        private float remainingShield = 0f;
        private bool applied = false;
        private float baseHealth = 0f;

        public ShieldEffect(float duration, float amount)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be non-negative");
            if (amount < 0f) throw new ArgumentOutOfRangeException(nameof(amount), "Shield amount must be non-negative");

            Duration = duration;
            Amount = amount;
        }

        public void Tick(float deltaTime, IStatusTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!applied)
            {
                // Apply shield as temporary extra health
                target.TakeDamage(-Amount);
                remainingShield = Amount;
                // Record base health prior to shield application so we can compute how much
                // of the shield remains after incoming damage (supports simple IStatusTarget fakes)
                baseHealth = target.Health - Amount;
                applied = true;
            }

            var effective = Math.Min(deltaTime, Duration);
            Duration -= effective;

            // If the effect expired during this tick, remove any remaining shield immediately
            if (IsExpired)
            {
                // Remaining shield is whatever extra health is still above the base health
                var remaining = Math.Max(0f, target.Health - baseHealth);
                if (remaining > 0f)
                {
                    var reduction = Math.Min(remaining, target.Health);
                    target.TakeDamage(reduction);
                    remainingShield = 0f;
                }
                return;
            }
        }

        public bool IsExpired => Duration <= 0f;
    }
}


