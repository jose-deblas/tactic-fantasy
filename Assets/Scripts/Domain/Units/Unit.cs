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

        void TakeDamage(int damage);
        void Heal(int amount);
        void SetPosition(int x, int y);
        void EquipWeapon(IWeapon weapon);
        void ApplyStatus(StatusEffect effect);
        void ClearStatus();
        void TickStatus();
    }

    public class Unit : IUnit
    {
        public int Id { get; }
        public string Name { get; }
        public Team Team { get; }
        public IClassData Class { get; }
        public CharacterStats CurrentStats { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; }
        public (int x, int y) Position { get; private set; }
        public IWeapon EquippedWeapon { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public StatusEffect ActiveStatus { get; private set; }
        /// <summary>False when Sleeping or Stunned.</summary>
        public bool CanAct => IsAlive && (ActiveStatus == null || (ActiveStatus.Type != StatusEffectType.Sleep && ActiveStatus.Type != StatusEffectType.Stun));

        public Unit(
            int id,
            string name,
            Team team,
            IClassData classData,
            CharacterStats stats,
            (int x, int y) position,
            IWeapon weapon)
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
        }

        public void TakeDamage(int damage)
        {
            CurrentHP = System.Math.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
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
                int poisonDmg = System.Math.Max(1, MaxHP * StatusEffect.PoisonDamagePercent / 100);
                TakeDamage(poisonDmg);
            }

            ActiveStatus.DecrementTurn();

            if (!ActiveStatus.IsActive)
                ActiveStatus = null;
        }
    }
}
