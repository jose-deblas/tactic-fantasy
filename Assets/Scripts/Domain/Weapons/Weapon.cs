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

        /// <summary>
        /// Maximum number of uses for this weapon. -1 means unlimited.
        /// </summary>
        int MaxUses { get; }

        /// <summary>
        /// Current remaining uses. When it reaches 0 the weapon is broken.
        /// Unlimited weapons always return -1.
        /// </summary>
        int CurrentUses { get; }

        /// <summary>True when CurrentUses == 0 (and MaxUses != -1).</summary>
        bool IsBroken { get; }

        /// <summary>True for Brave weapons that strike twice before the counter.</summary>
        bool IsBrave { get; }

        /// <summary>Minimum weapon rank required to equip.</summary>
        WeaponRank RequiredRank { get; }

        /// <summary>Consumes one use. No-op for unlimited or already-broken weapons.</summary>
        void ConsumeUse();
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

        public int MaxUses { get; }
        public int CurrentUses { get; private set; }
        public bool IsBroken => MaxUses != -1 && CurrentUses <= 0;
        public bool IsBrave { get; }
        public WeaponRank RequiredRank { get; }

        /// <summary>
        /// Creates a weapon. Pass <paramref name="uses"/> = -1 (default) for unlimited durability.
        /// </summary>
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
            int onHitStatusDuration = 0,
            int uses = -1,
            bool isBrave = false,
            WeaponRank requiredRank = WeaponRank.E)
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
            MaxUses = uses;
            CurrentUses = uses;
            IsBrave = isBrave;
            RequiredRank = requiredRank;
        }

        /// <inheritdoc/>
        public void ConsumeUse()
        {
            if (MaxUses == -1) return;        // unlimited
            if (CurrentUses <= 0) return;     // already broken
            CurrentUses--;
        }
    }
}
