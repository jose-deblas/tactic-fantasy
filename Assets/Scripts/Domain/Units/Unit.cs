using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Skills;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Units
{
    public interface IUnit
    {
        int Id { get; }
        string Name { get; }
        Team Team { get; }
        IClassData Class { get; }
        CharacterStats CurrentStats { get; }
        int CurrentHP { get; }
        int MaxHP { get; }
        (int x, int y) Position { get; }
        IWeapon EquippedWeapon { get; }
        bool IsAlive { get; }
        StatusEffect ActiveStatus { get; }
        bool CanAct { get; }

        /// <summary>True when the equipped weapon is broken (no uses left).</summary>
        bool HasBrokenWeapon { get; }
        int Level { get; }
        int Experience { get; }

        void TakeDamage(int damage);
        void Heal(int amount);
        void SetPosition(int x, int y);
        void EquipWeapon(IWeapon weapon);
        void ApplyStatus(StatusEffect effect);
        void ClearStatus();
        void TickStatus();

        /// <summary>Adds <paramref name="amount"/> XP; returns true if a level-up occurred.</summary>
        bool GainExperience(int amount, Random rng = null);

        /// <summary>Changes the unit's class to <paramref name="newClass"/> and resets level/XP.</summary>
        void ChangeClass(IClassData newClass);

        IReadOnlyList<ISkill> EquippedSkills { get; }
        void LearnSkill(ISkill skill);
        void EquipSkill(ISkill skill);
        void UnequipSkill(ISkill skill);
    }

    public class Unit : IUnit
    {
        public const int MaxLevel = 20;
        public const int XpPerLevel = 100;

        public int Id { get; }
        public string Name { get; }
        public Team Team { get; }
        private IClassData _class;
        public IClassData Class => _class;

        public CharacterStats CurrentStats { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public (int x, int y) Position { get; private set; }
        public IWeapon EquippedWeapon { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public StatusEffect ActiveStatus { get; private set; }
        public bool CanAct => IsAlive && (ActiveStatus == null || (ActiveStatus.Type != StatusEffectType.Sleep && ActiveStatus.Type != StatusEffectType.Stun));
        public bool HasBrokenWeapon => EquippedWeapon != null && EquippedWeapon.IsBroken;

        public int Level { get; private set; }
        public int Experience { get; private set; }

        private readonly List<ISkill> _equippedSkills = new List<ISkill>();
        public IReadOnlyList<ISkill> EquippedSkills => _equippedSkills.AsReadOnly();

        public Unit(
            int id,
            string name,
            Team team,
            IClassData classData,
            CharacterStats stats,
            (int x, int y) position,
            IWeapon weapon,
            int levelOverride = 1)
        {
            Id = id;
            Name = name;
            Team = team;
            _class = classData;
            CurrentStats = stats;
            MaxHP = stats.HP;
            CurrentHP = stats.HP;
            Position = position;
            EquippedWeapon = weapon;
            Level = Math.Max(1, Math.Min(levelOverride, MaxLevel));
            Experience = 0;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP = Math.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
        }

        public void SetPosition(int x, int y)
        {
            Position = (x, y);
        }

        public void EquipWeapon(IWeapon weapon)
        {
            EquippedWeapon = weapon;
        }

        public void ApplyStatus(StatusEffect effect)
        {
            // Don't overwrite a worse status with a weaker one (Sleep > Poison)
            ActiveStatus = effect;
        }

        public void ClearStatus()
        {
            ActiveStatus = null;
        }

        /// <summary>
        /// Called at end of each turn: applies poison damage, decrements duration,
        /// removes expired effects. Taking damage while asleep wakes the unit.
        /// </summary>
        public void TickStatus()
        {
            if (ActiveStatus == null || !ActiveStatus.IsActive)
            {
                ActiveStatus = null;
                return;
            }

            if (ActiveStatus.Type == StatusEffectType.Poison)
            {
                int poisonDmg = Math.Max(1, MaxHP * StatusEffect.PoisonDamagePercent / 100);
                TakeDamage(poisonDmg);
            }

            ActiveStatus.DecrementTurn();

            if (!ActiveStatus.IsActive)
                ActiveStatus = null;
        }

        /// <summary>
        /// Awards experience to this unit. Triggers a level-up when XP reaches 100.
        /// Returns true if the unit leveled up.
        /// At MaxLevel: XP is capped and no level-up occurs.
        /// </summary>
        public bool GainExperience(int amount, Random rng = null)
        {
            if (Level >= MaxLevel)
            {
                Experience = 0; // stay capped
                return false;
            }

            Experience += amount;
            bool leveledUp = false;

            while (Experience >= XpPerLevel && Level < MaxLevel)
            {
                Experience -= XpPerLevel;
                Level++;
                ApplyLevelUpStatGrowths(rng ?? new Random());
                leveledUp = true;
            }

            if (Level >= MaxLevel)
                Experience = 0;

            return leveledUp;
        }

        /// <summary>
        /// Promotes this unit to <paramref name="newClass"/>.
        /// Stats are bumped by the difference between new class base stats and current stats
        /// (only increases are applied). Level and XP reset to 1/0.
        /// </summary>
        public void ChangeClass(IClassData newClass)
        {
            var newBase = newClass.BaseStats;
            var current = CurrentStats;

            // Apply stat bonuses: each stat becomes max(current, newBase)
            int newHP  = Math.Max(current.HP,  newBase.HP);
            int newSTR = Math.Max(current.STR, newBase.STR);
            int newMAG = Math.Max(current.MAG, newBase.MAG);
            int newSKL = Math.Max(current.SKL, newBase.SKL);
            int newSPD = Math.Max(current.SPD, newBase.SPD);
            int newLCK = Math.Max(current.LCK, newBase.LCK);
            int newDEF = Math.Max(current.DEF, newBase.DEF);
            int newRES = Math.Max(current.RES, newBase.RES);
            int newMOV = Math.Max(current.MOV, newBase.MOV);

            int hpGain = newHP - MaxHP;
            CurrentStats = new CharacterStats(newHP, newSTR, newMAG, newSKL, newSPD, newLCK, newDEF, newRES, newMOV);
            MaxHP    += Math.Max(0, hpGain);
            CurrentHP = Math.Min(CurrentHP + Math.Max(0, hpGain), MaxHP);

            _class = newClass;
            Level = 1;
            Experience = 0;
        }

        public void LearnSkill(ISkill skill)
        {
            if (skill == null) return;
            if (!_equippedSkills.Contains(skill))
                _equippedSkills.Add(skill);
        }

        public void EquipSkill(ISkill skill)
        {
            LearnSkill(skill);
        }

        public void UnequipSkill(ISkill skill)
        {
            if (skill == null) return;
            _equippedSkills.Remove(skill);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Rolls stat increases using the class growth rates.
        /// Each stat has a growth% chance (0–100) of gaining +1, capped by CapStats.
        /// </summary>
        private void ApplyLevelUpStatGrowths(Random rng)
        {
            var growths = Class.GrowthRates;
            var caps    = Class.CapStats;

            int newHP  = GrowStat(CurrentStats.HP,  growths.HP,  caps.HP,  rng);
            int newSTR = GrowStat(CurrentStats.STR, growths.STR, caps.STR, rng);
            int newMAG = GrowStat(CurrentStats.MAG, growths.MAG, caps.MAG, rng);
            int newSKL = GrowStat(CurrentStats.SKL, growths.SKL, caps.SKL, rng);
            int newSPD = GrowStat(CurrentStats.SPD, growths.SPD, caps.SPD, rng);
            int newLCK = GrowStat(CurrentStats.LCK, growths.LCK, caps.LCK, rng);
            int newDEF = GrowStat(CurrentStats.DEF, growths.DEF, caps.DEF, rng);
            int newRES = GrowStat(CurrentStats.RES, growths.RES, caps.RES, rng);

            int hpGain = newHP - CurrentStats.HP;

            CurrentStats = new CharacterStats(newHP, newSTR, newMAG, newSKL, newSPD, newLCK, newDEF, newRES, CurrentStats.MOV);

            // MaxHP and CurrentHP grow with HP stat
            MaxHP    += hpGain;
            CurrentHP = Math.Min(CurrentHP + hpGain, MaxHP);
        }

        private static int GrowStat(int current, int growthPercent, int cap, Random rng)
        {
            if (current >= cap) return current;
            int roll = rng.Next(100);
            return roll < growthPercent ? current + 1 : current;
        }
    }
}
