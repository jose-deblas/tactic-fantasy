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
            AttackerEffectiveStats = attacker.CurrentStats;
            DefenderEffectiveStats = defender.CurrentStats;
            ActivatedSkills = new List<string>();
        }
    }
}
