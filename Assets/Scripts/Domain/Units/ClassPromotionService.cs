using System;
using System.Collections.Generic;

namespace TacticFantasy.Domain.Units
{
    /// <summary>
    /// Domain service that handles class promotions.
    /// Knows which base classes can promote and to what.
    /// Follows DDD: pure domain logic, no Unity dependencies.
    /// </summary>
    public static class ClassPromotionService
    {
        private static readonly Dictionary<string, Func<IClassData>> _promotionMap =
            new Dictionary<string, Func<IClassData>>
            {
                { "Myrmidon", ClassDataFactory.CreateSwordmaster },
                { "Soldier",  ClassDataFactory.CreateGeneral      },
                { "Fighter",  ClassDataFactory.CreateWarrior      },
                { "Mage",     ClassDataFactory.CreateSage         },
                { "Archer",   ClassDataFactory.CreateSniper       },
                { "Cleric",   ClassDataFactory.CreateBishop       },
            };

        /// <summary>Returns true when the unit is at max level and has a promotion path.</summary>
        public static bool CanPromote(IUnit unit)
        {
            return unit.Level >= Unit.MaxLevel
                && _promotionMap.ContainsKey(unit.Class.Name);
        }

        /// <summary>
        /// Promotes the unit to its advanced class.
        /// Throws <see cref="InvalidOperationException"/> if the unit cannot promote.
        /// </summary>
        public static void Promote(Unit unit)
        {
            if (!CanPromote(unit))
                throw new InvalidOperationException(
                    $"{unit.Name} cannot promote: must be Lv.{Unit.MaxLevel} with a valid promotion path.");

            var promotedClass = _promotionMap[unit.Class.Name]();
            unit.ChangeClass(promotedClass);
        }
    }
}
