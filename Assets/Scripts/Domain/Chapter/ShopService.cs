using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Chapter
{
    public class ShopService
    {
        private readonly Dictionary<string, int> _catalog = new Dictionary<string, int>();
        private readonly Dictionary<string, Func<IItem>> _itemFactories = new Dictionary<string, Func<IItem>>();

        /// <summary>Sell price is always 50% of the buy price.</summary>
        public const int SellPricePercent = 50;

        public void RegisterItem(string name, int buyPrice, Func<IItem> factory)
        {
            _catalog[name] = buyPrice;
            _itemFactories[name] = factory;
        }

        public int GetBuyPrice(string itemName)
        {
            if (!_catalog.TryGetValue(itemName, out int price))
                throw new InvalidOperationException($"Item '{itemName}' is not in the shop catalog");
            return price;
        }

        public int GetSellPrice(string itemName)
        {
            return GetBuyPrice(itemName) * SellPricePercent / 100;
        }

        public bool IsInStock(string itemName)
        {
            return _catalog.ContainsKey(itemName);
        }

        public void Buy(IUnit unit, string itemName, ArmyGold gold)
        {
            if (!IsInStock(itemName))
                throw new InvalidOperationException($"Item '{itemName}' is not in the shop catalog");

            int price = GetBuyPrice(itemName);
            if (!gold.CanAfford(price))
                throw new InvalidOperationException($"Cannot afford {itemName} (costs {price}, have {gold.Gold})");

            if (unit.Inventory.IsFull)
                throw new InvalidOperationException($"{unit.Name}'s inventory is full");

            var item = _itemFactories[itemName]();
            gold.Spend(price);
            unit.Inventory.Add(item);
        }

        public void Sell(IUnit unit, IItem item, ArmyGold gold)
        {
            if (!unit.Inventory.Items.Contains(item))
                throw new InvalidOperationException($"{unit.Name} does not have {item.Name}");

            if (!_catalog.ContainsKey(item.Name))
                throw new InvalidOperationException($"Item '{item.Name}' has no catalog entry for pricing");

            int sellPrice = GetSellPrice(item.Name);
            unit.Inventory.Remove(item);
            gold.Earn(sellPrice);
        }
    }
}
