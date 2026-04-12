namespace TacticFantasy.Domain.Combat
{
    public class CombatResult
    {
        public int Damage { get; }
        public bool Hit { get; }
        public bool IsCritical { get; }
        public int AttackerHP { get; }
        public int DefenderHP { get; }
        public bool AttackerDoubles { get; }
        public bool DefenderCounters { get; }

        /// <summary>XP awarded to the attacker after this combat exchange.</summary>
        public int AttackerXpGained { get; }

        /// <summary>XP awarded to the defender after this combat exchange.</summary>
        public int DefenderXpGained { get; }

        public CombatResult(
            int damage,
            bool hit,
            bool isCritical,
            int attackerHP,
            int defenderHP,
            bool attackerDoubles,
            bool defenderCounters,
            int attackerXpGained = 0,
            int defenderXpGained = 0)
        {
            Damage = damage;
            Hit = hit;
            IsCritical = isCritical;
            AttackerHP = attackerHP;
            DefenderHP = defenderHP;
            AttackerDoubles = attackerDoubles;
            DefenderCounters = defenderCounters;
            AttackerXpGained = attackerXpGained;
            DefenderXpGained = defenderXpGained;
        }
    }
}
