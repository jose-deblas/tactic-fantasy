namespace TacticFantasy.Domain.Utils
{
    public static class FloatUtils
    {
        /// <summary>
        /// Clamps a value between min and max.
        /// Preserves order if min &gt; max by swapping them.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                var tmp = min; min = max; max = tmp;
            }
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}