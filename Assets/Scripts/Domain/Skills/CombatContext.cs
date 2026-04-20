using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Support;
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
        /// <summary>Set by Astra; triggers 5 consecutive hits at half damage.</summary>
        public bool AstraActive { get; set; }
        /// <summary>Set by Colossus; adds attacker STR to damage for the current strike.</summary>
        public bool ColossusActive { get; set; }
        /// <summary>Set by Flare; halves enemy RES for the current strike.</summary>
        public bool FlareActive { get; set; }
        /// <summary>Set by Deadeye; deals 2x damage and applies Sleep on hit.</summary>
        public bool DeadeyeActive { get; set; }
        /// <summary>Set by Corona; halves enemy RES and DEF for the current strike.</summary>
        public bool CoronaActive { get; set; }
        public List<string> ActivatedSkills { get; }

        public SupportBonus AttackerSupportBonus { get; set; }
        public SupportBonus DefenderSupportBonus { get; set; }

        public CombatContext(IUnit attacker, IUnit defender, IGameMap map, Random rng,
            SupportBonus attackerSupportBonus = default, SupportBonus defenderSupportBonus = default)
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
            AstraActive = false;
            ColossusActive = false;
            FlareActive = false;
            DeadeyeActive = false;
            CoronaActive = false;
            AttackerSupportBonus = attackerSupportBonus;
            DefenderSupportBonus = defenderSupportBonus;
            ActivatedSkills = new List<string>();

            // Apply support bonuses to effective stats
            var atkStats = attacker.CurrentStats;
            AttackerEffectiveStats = new CharacterStats(
                atkStats.HP, atkStats.STR + attackerSupportBonus.Attack, atkStats.MAG + attackerSupportBonus.Attack,
                atkStats.SKL, atkStats.SPD, atkStats.LCK, atkStats.DEF + attackerSupportBonus.Defense,
                atkStats.RES + attackerSupportBonus.Defense, atkStats.MOV);

            var defStats = defender.CurrentStats;
            int guardDef = defender.IsGuarding ? 2 : 0;
            int guardRes = defender.IsGuarding ? 2 : 0;
            DefenderEffectiveStats = new CharacterStats(
                defStats.HP, defStats.STR + defenderSupportBonus.Attack, defStats.MAG + defenderSupportBonus.Attack,
                defStats.SKL, defStats.SPD, defStats.LCK, defStats.DEF + defenderSupportBonus.Defense + guardDef,
                defStats.RES + defenderSupportBonus.Defense + guardRes, defStats.MOV);
        }
    }
}
