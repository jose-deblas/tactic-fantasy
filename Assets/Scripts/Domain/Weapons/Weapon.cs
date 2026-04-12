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

        public Weapon(
            string name,
            WeaponType type,
            DamageType damageType,
            int might,
            int weight,
            int hit,
            int crit,
            int minRange,
            int maxRange)
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
        }
    }
}
