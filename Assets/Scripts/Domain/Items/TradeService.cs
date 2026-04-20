using System;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public interface ITradeService
    {
        bool CanTrade(IUnit unitA, IUnit unitB);
        void TradeItem(IUnit giver, IUnit receiver, IItem item);
        void SwapItems(IUnit unitA, int slotA, IUnit unitB, int slotB);
    }

    public class TradeService : ITradeService
    {
        public bool CanTrade(IUnit unitA, IUnit unitB)
        {
            if (unitA == null || unitB == null) return false;
            if (!unitA.IsAlive || !unitB.IsAlive) return false;
            if (unitA.Id == unitB.Id) return false;
            if (!TeamRelations.AreAllied(unitA.Team, unitB.Team)) return false;

            int distance = Math.Abs(unitA.Position.x - unitB.Position.x)
                         + Math.Abs(unitA.Position.y - unitB.Position.y);
            return distance == 1;
        }

        public void TradeItem(IUnit giver, IUnit receiver, IItem item)
        {
            if (!CanTrade(giver, receiver))
                throw new InvalidOperationException("Units cannot trade: not adjacent or not allies.");
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!giver.Inventory.Items.Contains(item))
                throw new InvalidOperationException("Item not found in giver's inventory.");
            if (receiver.Inventory.IsFull)
                throw new InvalidOperationException("Receiver's inventory is full.");

            giver.Inventory.Remove(item);
            receiver.Inventory.Add(item);
        }

        public void SwapItems(IUnit unitA, int slotA, IUnit unitB, int slotB)
        {
            if (!CanTrade(unitA, unitB))
                throw new InvalidOperationException("Units cannot trade: not adjacent or not allies.");

            var items_A = unitA.Inventory.Items;
            var items_B = unitB.Inventory.Items;

            if (slotA < 0 || slotA >= items_A.Count)
                throw new ArgumentOutOfRangeException(nameof(slotA));
            if (slotB < 0 || slotB >= items_B.Count)
                throw new ArgumentOutOfRangeException(nameof(slotB));

            var itemA = items_A[slotA];
            var itemB = items_B[slotB];

            unitA.Inventory.Remove(itemA);
            unitB.Inventory.Remove(itemB);
            unitA.Inventory.Add(itemB);
            unitB.Inventory.Add(itemA);
        }
    }
}
