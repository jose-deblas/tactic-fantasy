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

        public CombatResult(
            int damage,
            bool hit,
            bool isCritical,
            int attackerHP,
            int defenderHP,
            bool attackerDoubles,
            bool defenderCounters)
        {
            Damage = damage;
            Hit = hit;
            IsCritical = isCritical;
            AttackerHP = attackerHP;
            DefenderHP = defenderHP;
            AttackerDoubles = attackerDoubles;
            DefenderCounters = defenderCounters;
        }
    }
}
