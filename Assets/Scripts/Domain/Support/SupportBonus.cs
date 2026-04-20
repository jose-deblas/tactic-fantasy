namespace TacticFantasy.Domain.Support
{
    /// <summary>
    /// Stat bonuses granted by support adjacency during combat.
    /// </summary>
    public struct SupportBonus
    {
        public int Attack;
        public int Defense;
        public int Hit;
        public int Avoid;

        public SupportBonus(int attack, int defense, int hit, int avoid)
        {
            Attack = attack;
            Defense = defense;
            Hit = hit;
            Avoid = avoid;
        }

        /// <summary>Returns the combat bonus for a given support level.</summary>
        public static SupportBonus ForLevel(SupportLevel level)
        {
            return level switch
            {
                SupportLevel.C => new SupportBonus(1, 1, 5, 5),
                SupportLevel.B => new SupportBonus(2, 2, 10, 10),
                SupportLevel.A => new SupportBonus(3, 3, 15, 15),
                _ => new SupportBonus(0, 0, 0, 0)
            };
        }

        public static SupportBonus operator +(SupportBonus a, SupportBonus b)
        {
            return new SupportBonus(
                a.Attack + b.Attack,
                a.Defense + b.Defense,
                a.Hit + b.Hit,
                a.Avoid + b.Avoid);
        }

        public static SupportBonus Zero => new SupportBonus(0, 0, 0, 0);
    }
}
