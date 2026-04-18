using System;

namespace TacticFantasy.Domain
{
    public sealed class StatusEffectSummary
    {
        public string Name { get; }
        public float Remaining { get; }

        public StatusEffectSummary(string name, float remaining)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Remaining = remaining;
        }

        public static StatusEffectSummary From(IStatusEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            // Remaining is Duration - elapsed; but IStatusEffect doesn't expose elapsed.
            // Here we approximate by using Duration: callers should pass an effect whose Duration is remaining time.
            return new StatusEffectSummary(effect.Name, effect.Duration);
        }

        public override string ToString()
        {
            return $"{Name} ({Remaining:0.00}s)";
        }
    }
}
