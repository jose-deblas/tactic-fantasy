namespace TacticFantasy.Domain.Combat
{
    /// <summary>
    /// Defines XP award constants for combat actions.
    /// Centralizes tuning of the experience curve.
    /// </summary>
    public static class CombatXp
    {
        /// <summary>XP awarded to the attacker for killing the defender.</summary>
        public const int KillBonus     = 60;

        /// <summary>XP awarded to the attacker for dealing damage without killing.</summary>
        public const int DamageBonus   = 20;

        /// <summary>XP awarded to the defender for surviving an attack.</summary>
        public const int SurvivedBonus = 10;

        /// <summary>XP awarded to the defender for landing a counter-attack.</summary>
        public const int CounteredBonus = 15;
    }
}
