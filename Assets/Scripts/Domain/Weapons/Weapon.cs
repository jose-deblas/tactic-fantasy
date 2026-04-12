using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Weapons
{
    public interface IWeapon
    {
        string Name { get; }
        WeaponType Type { get; }
        DamageType DamageType { get; }
        int Might { get; }
        int Weight { get; }
        int Hit { get; }
        int Crit { get; }
        int MinRange { get; }
        int MaxRange { get; }

        /// <summary>
        /// Optional status effect inflicted on the target when this weapon hits.
        /// Null means no status effect.
        /// </summary>
        StatusEffectType? OnHitStatus { get; }

        /// <summary>
        /// Duration (in turns) of the on-hit status effect. Zero if none.
        /// </summary>
        int OnHitStatusDuration { get; }
    }

    public class Weapon : IWeapon
    {
        public string Name { get; }
        public WeaponType Type { get; }
        public DamageType DamageType { get; }
        public int Might { get; }
        public int Weight { get; }
        public int Hit { get; }
        public int Crit { get; }
        public int MinRange { get; }
        public int MaxRange { get; }
        public StatusEffectType? OnHitStatus { get; }
        public int OnHitStatusDuration { get; }

        public Weapon(
            string name,
            WeaponType type,
            DamageType damageType,
            int might,
            int weight,
            int hit,
            int crit,
            int minRange,
            int maxRange,
            StatusEffectType? onHitStatus = null,
            int onHitStatusDuration = 0)
        {
            Name = name;
            Type = type;
            DamageType = damageType;
            Might = might;
            Weight = weight;
            Hit = hit;
            Crit = crit;
            MinRange = minRange;
            MaxRange = maxRange;
            OnHitStatus = onHitStatus;
            OnHitStatusDuration = onHitStatusDuration;
        }
    }
}
