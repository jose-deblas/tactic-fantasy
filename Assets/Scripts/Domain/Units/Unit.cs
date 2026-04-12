using System;
using System.Collections.Generic;
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
    }

    public class Unit : IUnit
    {
        public const int MaxLevel = 20;
        public const int XpPerLevel = 100;

        public int Id { get; }
        public string Name { get; }
        public Team Team { get; }
        public IClassData Class { get; }
        public CharacterStats CurrentStats { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public (int x, int y) Position { get; private set; }
        public IWeapon EquippedWeapon { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public StatusEffect ActiveStatus { get; private set; }
        /// <summary>False when Sleeping or Stunned.</summary>
        public bool CanAct => IsAlive && (ActiveStatus == null || (ActiveStatus.Type != StatusEffectType.Sleep && ActiveStatus.Type != StatusEffectType.Stun));

        public int Level { get; private set; }
        public int Experience { get; private set; }

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
            Class = classData;
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
