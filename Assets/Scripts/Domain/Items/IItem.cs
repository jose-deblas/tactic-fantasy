using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public enum ItemType { Weapon, Consumable, KeyItem }

    public interface IItem
    {
        string Name { get; }
        ItemType ItemType { get; }
        int MaxUses { get; }
        int CurrentUses { get; }
        bool IsUsable { get; }
        void Use(IUnit unit);
    }
}
