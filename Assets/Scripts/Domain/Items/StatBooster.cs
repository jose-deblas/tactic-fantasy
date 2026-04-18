using System;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public class StatBooster : IItem
    {
        public string Name { get; }
        public ItemType ItemType => ItemType.Consumable;
        public int MaxUses => 1;
        public int CurrentUses { get; private set; }
        public bool IsUsable => CurrentUses > 0;

        private readonly Action<IUnit> _effect;

        public StatBooster(string name, Action<IUnit> effect)
        {
            Name = name;
            CurrentUses = 1;
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
