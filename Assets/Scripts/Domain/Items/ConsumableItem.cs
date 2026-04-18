using System;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public class ConsumableItem : IItem
    {
        public string Name { get; }
        public ItemType ItemType => ItemType.Consumable;
        public int MaxUses { get; }
        public int CurrentUses { get; private set; }
        public bool IsUsable => CurrentUses > 0;

        private readonly Action<IUnit> _effect;

        public ConsumableItem(string name, int uses, Action<IUnit> effect)
        {
            Name = name;
            MaxUses = uses;
            CurrentUses = uses;
            _effect = effect;
        }

        public void Use(IUnit unit)
        {
            if (!IsUsable || unit == null) return;
            _effect(unit);
            CurrentUses--;
        }
    }
}
