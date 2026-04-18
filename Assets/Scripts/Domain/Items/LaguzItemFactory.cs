using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public static class LaguzItemFactory
    {
        /// <summary>
        /// Laguz Stone: instantly fills transform gauge to max (1 use).
        /// Only affects Laguz units.
        /// </summary>
        public static ConsumableItem CreateLaguzStone()
        {
            return new ConsumableItem("Laguz Stone", 1, unit =>
            {
                if (unit.LaguzGauge != null)
                    unit.LaguzGauge.FillToMax();
            });
        }

        /// <summary>
        /// Olivi Grass: adds +15 transform gauge points (3 uses).
        /// Only affects Laguz units.
        /// </summary>
        public static ConsumableItem CreateOliviGrass()
        {
            return new ConsumableItem("Olivi Grass", 3, unit =>
            {
                if (unit.LaguzGauge != null)
                    unit.LaguzGauge.AddPoints(15);
            });
        }
    }
}
