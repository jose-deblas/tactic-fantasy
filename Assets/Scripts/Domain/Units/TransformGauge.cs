using System;

namespace TacticFantasy.Domain.Units
{
    public class TransformGauge
    {
        public const int MaxGauge = 30;
        public const int MinGauge = 0;

        public int Current { get; private set; }
        public int FillRate { get; }
        public int DrainRate { get; }
        public bool IsFull => Current >= MaxGauge;
        public bool IsEmpty => Current <= MinGauge;

        public TransformGauge(int fillRate, int drainRate, int initialValue = 0)
        {
            FillRate = fillRate;
            DrainRate = drainRate;
            Current = Math.Max(MinGauge, Math.Min(MaxGauge, initialValue));
        }

        /// <summary>
        /// Ticks the gauge each turn. Fills when untransformed, drains when transformed.
        /// Returns true if a state change should occur (full → transform, empty → revert).
        /// </summary>
        public bool Tick(bool isTransformed)
        {
            if (isTransformed)
            {
                Current = Math.Max(MinGauge, Current - DrainRate);
                return IsEmpty;
            }
            else
            {
                Current = Math.Min(MaxGauge, Current + FillRate);
                return IsFull;
            }
        }

        /// <summary>Sets gauge to max (used by Laguz Stone).</summary>
        public void FillToMax()
        {
            Current = MaxGauge;
        }

        /// <summary>Adds points to the gauge (used by Olivi Grass). Clamped to max.</summary>
        public void AddPoints(int points)
        {
            Current = Math.Min(MaxGauge, Current + Math.Max(0, points));
        }
    }
}
