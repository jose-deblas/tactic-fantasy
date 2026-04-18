using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Weapons
{
    public static class LaguzWeaponFactory
    {
        public static IWeapon CreateStrike() =>
            new Weapon("Strike", WeaponType.STRIKE, DamageType.Physical,
                might: 4, weight: 3, hit: 90, crit: 0, minRange: 1, maxRange: 1);

        public static IWeapon CreateClaw() =>
            new Weapon("Claw", WeaponType.STRIKE, DamageType.Physical,
                might: 6, weight: 5, hit: 85, crit: 5, minRange: 1, maxRange: 1);

        public static IWeapon CreateFang() =>
            new Weapon("Fang", WeaponType.STRIKE, DamageType.Physical,
                might: 10, weight: 8, hit: 80, crit: 5, minRange: 1, maxRange: 1);

        public static IWeapon CreateTalon() =>
            new Weapon("Talon", WeaponType.STRIKE, DamageType.Physical,
                might: 8, weight: 6, hit: 85, crit: 5, minRange: 1, maxRange: 1);

        public static IWeapon CreateBeak() =>
            new Weapon("Beak", WeaponType.STRIKE, DamageType.Physical,
                might: 5, weight: 3, hit: 95, crit: 10, minRange: 1, maxRange: 1);

        public static IWeapon CreateBreath() =>
            new Weapon("Breath", WeaponType.STRIKE, DamageType.Magical,
                might: 14, weight: 10, hit: 75, crit: 0, minRange: 1, maxRange: 1);

        /// <summary>Returns the appropriate natural weapon for a Laguz race.</summary>
        public static IWeapon CreateForRace(LaguzRace race)
        {
            return race switch
            {
                LaguzRace.Cat => CreateStrike(),
                LaguzRace.Tiger => CreateClaw(),
                LaguzRace.Lion => CreateFang(),
                LaguzRace.Hawk => CreateTalon(),
                LaguzRace.Raven => CreateBeak(),
                LaguzRace.RedDragon => CreateBreath(),
                LaguzRace.WhiteDragon => CreateBreath(),
                LaguzRace.Heron => CreateStrike(),
                _ => CreateStrike()
            };
        }
    }
}
