using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Skills
{
    public class CombatContext
    {
        public IUnit Attacker { get; }
        public IUnit Defender { get; }
        public IGameMap Map { get; }
        public Random Rng { get; }

        public int ExtraAttacks { get; set; }
        public bool ForceCritical { get; set; }
        public bool AttackerSkillsNegated { get; set; }
        public bool DefenderSkillsNegated { get; set; }
        public bool VantageActivated { get; set; }

        public CharacterStats AttackerEffectiveStats { get; set; }
        public CharacterStats DefenderEffectiveStats { get; set; }

        public bool HasParagon { get; set; }
        /// <summary>Set by Luna; when true, defender DEF/RES is halved for this strike.</summary>
        public bool LunaActive { get; set; }
        /// <summary>HP healed by Sol after a successful hit. Accumulated over strikes.</summary>
        public int SolHealAmount { get; set; }
        /// <summary>Damage dealt by the most recent attacker strike (for Sol to reference).</summary>
        public int LastStrikeDamage { get; set; }
        public List<string> ActivatedSkills { get; }

        public CombatContext(IUnit attacker, IUnit defender, IGameMap map, Random rng)
        {
            Attacker = attacker;
            Defender = defender;
            Map = map;
            Rng = rng;
            ExtraAttacks = 0;
            ForceCritical = false;
            AttackerSkillsNegated = false;
            DefenderSkillsNegated = false;
            VantageActivated = false;
            HasParagon = false;
            LunaActive = false;
            SolHealAmount = 0;
            LastStrikeDamage = 0;
            AttackerEffectiveStats = attacker.CurrentStats;
            DefenderEffectiveStats = defender.CurrentStats;
            ActivatedSkills = new List<string>();
        }
    }
}
