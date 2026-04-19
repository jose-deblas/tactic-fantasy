using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public static class ConsumableFactory
    {
        public static ConsumableItem CreateVulnerary()
        {
            return new ConsumableItem("Vulnerary", 3, u => u.Heal(10));
        }

        public static ConsumableItem CreateElixir()
        {
            return new ConsumableItem("Elixir", 3, u => u.Heal(u.MaxHP));
        }

        public static ConsumableItem CreateAntitoxin()
        {
            return new ConsumableItem("Antitoxin", 1, u =>
            {
                if (u.ActiveStatus != null && u.ActiveStatus.Type == StatusEffectType.Poison)
                    u.ClearStatus();
            });
        }

        public static ConsumableItem CreatePureWater()
        {
            return new ConsumableItem("Pure Water", 1, u =>
            {
                u.ApplyStatBoost(0, 0, 0, 0, 0, 0, 0, 7, 0);
            });
        }

        public static ConsumableItem CreateTorch()
        {
            return new ConsumableItem("Torch", 1, u => { });
        }
    }
}
