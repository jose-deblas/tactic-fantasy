using System;

namespace TacticFantasy.Domain.Chapter
{
    public class ArmyGold
    {
        public int Gold { get; private set; }

        public ArmyGold(int initialGold = 0)
        {
            if (initialGold < 0)
                throw new ArgumentOutOfRangeException(nameof(initialGold), "Gold cannot be negative");
            Gold = initialGold;
        }

        public bool CanAfford(int cost)
        {
            return cost >= 0 && Gold >= cost;
        }

        public void Spend(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Spend amount cannot be negative");
            if (!CanAfford(amount))
                throw new InvalidOperationException($"Cannot spend {amount} gold; only {Gold} available");
            Gold -= amount;
        }

        public void Earn(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Earn amount cannot be negative");
            Gold += amount;
        }
    }
}
