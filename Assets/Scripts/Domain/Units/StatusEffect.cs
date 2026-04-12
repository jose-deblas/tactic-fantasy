namespace TacticFantasy.Domain.Units
{
    /// <summary>
    /// Types of status effects that can afflict a unit.
    /// </summary>
    public enum StatusEffectType
    {
        None,
        Poison,  // Deals damage at end of each turn
        Sleep,   // Unit cannot act until woken (takes damage wakes it)
        Stun     // Unit skips next turn (one-time effect)
    }

    /// <summary>
    /// An active status effect on a unit, with remaining duration in turns.
    /// </summary>
    public class StatusEffect
    {
        public StatusEffectType Type { get; }
        public int RemainingTurns { get; private set; }

        // Poison damage is % of MaxHP per turn
        public const int PoisonDamagePercent = 10;

        public StatusEffect(StatusEffectType type, int durationTurns)
        {
            Type = type;
            RemainingTurns = durationTurns;
        }

        /// <summary>Returns true if the effect is still active.</summary>
        public bool IsActive => RemainingTurns > 0;

        /// <summary>Decrements the remaining duration by one turn.</summary>
        public void DecrementTurn()
        {
            if (RemainingTurns > 0)
                RemainingTurns--;
        }
    }
}
