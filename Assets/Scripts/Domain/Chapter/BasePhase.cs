using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Chapter
{
    public class BasePhase
    {
        private readonly List<IUnit> _availableUnits;
        private readonly HashSet<int> _deployedUnitIds = new HashSet<int>();
        private readonly BexpDistributor _bexpDistributor = new BexpDistributor();

        public int BexpPool { get; private set; }
        public ArmyGold Gold { get; }
        public ShopService Shop { get; }
        public int MaxDeployCount { get; }
        public IReadOnlyList<IUnit> AvailableUnits => _availableUnits.AsReadOnly();

        public IReadOnlyList<IUnit> DeployedUnits =>
            _availableUnits.Where(u => _deployedUnitIds.Contains(u.Id)).ToList().AsReadOnly();

        public IReadOnlyList<IUnit> BenchedUnits =>
            _availableUnits.Where(u => !_deployedUnitIds.Contains(u.Id)).ToList().AsReadOnly();

        public BasePhase(
            List<IUnit> availableUnits,
            int bexpPool,
            ArmyGold gold,
            ShopService shop,
            int maxDeployCount)
        {
            _availableUnits = new List<IUnit>(availableUnits);
            BexpPool = bexpPool;
            Gold = gold;
            Shop = shop;
            MaxDeployCount = maxDeployCount;
        }

        /// <summary>
        /// Allocates BEXP to a unit. Returns the number of level-ups.
        /// Deducts from the shared BEXP pool.
        /// </summary>
        public int AllocateBexp(Unit unit, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Cannot allocate negative BEXP");
            if (amount > BexpPool)
                throw new InvalidOperationException(
                    $"Cannot allocate {amount} BEXP; only {BexpPool} available");

            int toSpend = Math.Min(amount, BexpPool);
            int levelsBefore = unit.Level;
            int levelsGained = _bexpDistributor.Allocate(unit, ref toSpend);

            // toSpend now contains leftover BEXP that wasn't used
            int spent = amount - toSpend;
            // If unit hit max level, only deduct what was actually consumed
            if (levelsGained == 0 && unit.Level >= Unit.MaxLevel)
                spent = 0;

            BexpPool -= spent;
            return levelsGained;
        }

        public void DeployUnit(IUnit unit)
        {
            if (!_availableUnits.Any(u => u.Id == unit.Id))
                throw new InvalidOperationException($"{unit.Name} is not in the available roster");
            if (_deployedUnitIds.Count >= MaxDeployCount)
                throw new InvalidOperationException(
                    $"Cannot deploy more than {MaxDeployCount} units");
            _deployedUnitIds.Add(unit.Id);
        }

        public void BenchUnit(IUnit unit)
        {
            _deployedUnitIds.Remove(unit.Id);
        }
    }
}
