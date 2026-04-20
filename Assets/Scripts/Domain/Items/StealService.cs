using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Items
{
    public interface IStealService
    {
        bool CanSteal(IUnit thief, IUnit target);
        IItem Steal(IUnit thief, IUnit target);
    }

    public class StealService : IStealService
    {
        private static readonly HashSet<string> StealCapableClasses = new HashSet<string>
        {
            "Thief", "Rogue"
        };

        public bool CanSteal(IUnit thief, IUnit target)
        {
            if (thief == null || target == null) return false;
            if (!thief.IsAlive || !target.IsAlive) return false;
            if (thief.Team == target.Team) return false;
            if (!StealCapableClasses.Contains(thief.Class.Name)) return false;
            if (thief.CurrentStats.SPD <= target.CurrentStats.SPD) return false;
            if (thief.Inventory.IsFull) return false;

            int distance = Math.Abs(thief.Position.x - target.Position.x)
                         + Math.Abs(thief.Position.y - target.Position.y);
            if (distance != 1) return false;

            return target.Inventory.Items.Any(item => item.ItemType != ItemType.Weapon);
        }

        public IItem Steal(IUnit thief, IUnit target)
        {
            if (!CanSteal(thief, target))
                throw new InvalidOperationException("Cannot steal: conditions not met.");

            var item = target.Inventory.Items.First(i => i.ItemType != ItemType.Weapon);
            target.Inventory.Remove(item);
            thief.Inventory.Add(item);
            return item;
        }
    }
}
