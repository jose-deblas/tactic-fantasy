namespace TacticFantasy.Domain.Items
{
    public static class StatBoosterFactory
    {
        public static StatBooster CreateEnergyDrop()
        {
            return new StatBooster("Energy Drop", u => u.ApplyStatBoost(0, 2, 0, 0, 0, 0, 0, 0, 0));
        }

        public static StatBooster CreateSpiritDust()
        {
            return new StatBooster("Spirit Dust", u => u.ApplyStatBoost(0, 0, 2, 0, 0, 0, 0, 0, 0));
        }

        public static StatBooster CreateSpeedwing()
        {
            return new StatBooster("Speedwing", u => u.ApplyStatBoost(0, 0, 0, 0, 2, 0, 0, 0, 0));
        }

        public static StatBooster CreateSecretBook()
        {
            return new StatBooster("Secret Book", u => u.ApplyStatBoost(0, 0, 0, 2, 0, 0, 0, 0, 0));
        }

        public static StatBooster CreateGoddessIcon()
        {
            return new StatBooster("Goddess Icon", u => u.ApplyStatBoost(0, 0, 0, 0, 0, 2, 0, 0, 0));
        }

        public static StatBooster CreateDracoshield()
        {
            return new StatBooster("Dracoshield", u => u.ApplyStatBoost(0, 0, 0, 0, 0, 0, 2, 0, 0));
        }

        public static StatBooster CreateTalisman()
        {
            return new StatBooster("Talisman", u => u.ApplyStatBoost(0, 0, 0, 0, 0, 0, 0, 2, 0));
        }

        public static StatBooster CreateSeraphRobe()
        {
            return new StatBooster("Seraph Robe", u => u.ApplyStatBoost(7, 0, 0, 0, 0, 0, 0, 0, 0));
        }

        public static StatBooster CreateBootsItem()
        {
            return new StatBooster("Boots", u => u.ApplyStatBoost(0, 0, 0, 0, 0, 0, 0, 0, 2));
        }
    }
}
