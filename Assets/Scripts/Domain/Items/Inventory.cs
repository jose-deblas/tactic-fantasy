using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Items
{
    public class Inventory
    {
        public const int MaxSlots = 7;
        private readonly List<IItem> _items = new List<IItem>();

        public IReadOnlyList<IItem> Items => _items.AsReadOnly();
        public int Count => _items.Count;
        public bool IsFull => _items.Count >= MaxSlots;

        public Inventory() { }

        public Inventory(IWeapon initialWeapon)
        {
            if (initialWeapon != null)
                _items.Add(initialWeapon);
        }

        public Inventory(IEnumerable<IItem> items)
        {
            foreach (var item in items)
            {
                if (_items.Count >= MaxSlots) break;
                _items.Add(item);
            }
        }

        public bool Add(IItem item)
        {
            if (item == null || IsFull) return false;
            _items.Add(item);
            return true;
        }

        public bool Remove(IItem item)
        {
            return _items.Remove(item);
        }

        public void Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(indexA));
            if (indexB < 0 || indexB >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(indexB));

            var temp = _items[indexA];
            _items[indexA] = _items[indexB];
            _items[indexB] = temp;
        }

        public IReadOnlyList<IItem> GetAll()
        {
            return _items.AsReadOnly();
        }

        public IReadOnlyList<IWeapon> GetWeapons()
        {
            return _items.OfType<IWeapon>().ToList().AsReadOnly();
        }

        public IWeapon GetFirstUsableWeapon()
        {
            return _items.OfType<IWeapon>().FirstOrDefault(w => !w.IsBroken);
        }
    }
}
