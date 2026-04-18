using System.Collections.Generic;
using TacticFantasy.Domain.Units;

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

        /// <summary>Status effect applied to the defender on hit, if any.</summary>
        public StatusEffectType? DefenderStatusApplied { get; }

        /// <summary>Status effect applied to the attacker via counter-hit, if any.</summary>
        public StatusEffectType? AttackerStatusApplied { get; }

        /// <summary>HP healed by the attacker via Sol (0 if Sol did not fire).</summary>
        public int AttackerHealedHP { get; }

        /// <summary>Names of skills that activated during this combat.</summary>
        public IReadOnlyList<string> ActivatedSkills { get; }

        public CombatResult(
            int damage,
            bool hit,
            bool isCritical,
            int attackerHP,
            int defenderHP,
            bool attackerDoubles,
            bool defenderCounters,
            int attackerXpGained = 0,
            int defenderXpGained = 0,
            StatusEffectType? defenderStatusApplied = null,
            StatusEffectType? attackerStatusApplied = null,
            IReadOnlyList<string> activatedSkills = null,
            int attackerHealedHP = 0)
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
            DefenderStatusApplied = defenderStatusApplied;
            AttackerStatusApplied = attackerStatusApplied;
            ActivatedSkills = activatedSkills ?? new List<string>();
            AttackerHealedHP = attackerHealedHP;
        }
    }
}
