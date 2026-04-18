using System;

namespace TacticFantasy.Domain.Utils
{
    public static class PositionUtils
    {
        /// <summary>
        /// Returns the Chebyshev distance between two integer grid positions.
        /// Chebyshev distance is appropriate for movement where diagonal steps cost 1.
        /// </summary>
        public static int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }
    }
}