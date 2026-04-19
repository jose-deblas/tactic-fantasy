using System;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Chapter
{
    public class BexpDistributor
    {
        /// <summary>BEXP cost per level-up (RD standard).</summary>
        public const int BexpPerLevel = 50;

        /// <summary>
        /// Allocates BEXP to a unit. Returns the number of level-ups that occurred.
        /// Each level-up costs 50 BEXP and uses deterministic growth (top-3 growth stats).
        /// Stops when the unit reaches max level or BEXP runs out.
        /// </summary>
        public int Allocate(Unit unit, ref int bexpPool)
        {
            if (bexpPool < 0)
                throw new ArgumentOutOfRangeException(nameof(bexpPool), "BEXP pool cannot be negative");

            int levelsGained = 0;

            while (bexpPool >= BexpPerLevel && unit.Level < Unit.MaxLevel)
            {
                bexpPool -= BexpPerLevel;
                unit.GainLevelBexp();
                levelsGained++;
            }

            return levelsGained;
        }

        /// <summary>
        /// Returns the BEXP cost for <paramref name="levels"/> level-ups.
        /// </summary>
        public static int CostForLevels(int levels)
        {
            return levels * BexpPerLevel;
        }
    }
}
