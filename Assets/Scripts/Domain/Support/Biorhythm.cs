using System;

namespace TacticFantasy.Domain.Support
{
    public enum BiorhythmPhase
    {
        High,
        Normal,
        Low
    }

    /// <summary>
    /// Deterministic biorhythm cycle that provides periodic stat modifiers.
    /// Cycle: High → Normal → Low → Normal → (repeat). Period = 4 turns.
    /// Each unit has an offset so they cycle differently.
    /// </summary>
    public class Biorhythm
    {
        public const int CycleLength = 4;
        public const int HighBonus = 2;
        public const int LowPenalty = -2;

        public int Offset { get; }

        public Biorhythm(int offset = 0)
        {
            Offset = offset;
        }

        /// <summary>Returns the biorhythm phase for the given turn number.</summary>
        public BiorhythmPhase GetPhase(int currentTurn)
        {
            int phase = ((currentTurn + Offset) % CycleLength + CycleLength) % CycleLength;
            return phase switch
            {
                0 => BiorhythmPhase.High,
                1 => BiorhythmPhase.Normal,
                2 => BiorhythmPhase.Low,
                3 => BiorhythmPhase.Normal,
                _ => BiorhythmPhase.Normal
            };
        }

        /// <summary>
        /// Returns the stat modifier for the given turn.
        /// High = +2 ATK/SKL, Low = -2 ATK/SKL, Normal = 0.
        /// </summary>
        public int GetModifier(int currentTurn)
        {
            return GetPhase(currentTurn) switch
            {
                BiorhythmPhase.High => HighBonus,
                BiorhythmPhase.Low => LowPenalty,
                _ => 0
            };
        }
    }
}
