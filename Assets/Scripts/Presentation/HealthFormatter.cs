using System;

namespace TacticFantasy.Presentation
{
    public static class HealthFormatter
    {
        // Returns formatted health string: "HP: current/max (percent%)"
        public static string Format(int current, int max)
        {
            if (max <= 0) throw new ArgumentException("max must be > 0", nameof(max));
            var cur = Math.Max(0, Math.Min(current, max));
            var percent = (int)Math.Round((double)cur / max * 100);
            return $"HP: {cur}/{max} ({percent}%)";
        }
    }
}
