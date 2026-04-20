using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Skills;

namespace TacticFantasy.Domain.Units
{
    /// <summary>
    /// Domain service that handles class promotions.
    /// Supports Tier 1→2 and Tier 2→3 promotion paths.
    /// Mastery skills are auto-learned on promotion to Tier 3.
    /// </summary>
    public static class ClassPromotionService
    {
        private static readonly Dictionary<string, Func<IClassData>> _promotionMap =
            new Dictionary<string, Func<IClassData>>
            {
                // Tier 1 → Tier 2
                { "Myrmidon",    ClassDataFactory.CreateSwordmaster },
                { "Soldier",     ClassDataFactory.CreateGeneral     },
                { "Fighter",     ClassDataFactory.CreateWarrior     },
                { "Mage",        ClassDataFactory.CreateSage        },
                { "Archer",      ClassDataFactory.CreateSniper      },
                { "Cleric",      ClassDataFactory.CreateBishop      },
                { "Thief",       ClassDataFactory.CreateRogue       },
                // Tier 2 → Tier 3
                { "Swordmaster", ClassDataFactory.CreateTrueblade   },
                { "General",     ClassDataFactory.CreateMarshall    },
                { "Warrior",     ClassDataFactory.CreateReaver      },
                { "Sage",        ClassDataFactory.CreateArchsage    },
                { "Sniper",      ClassDataFactory.CreateMarksman    },
                { "Bishop",      ClassDataFactory.CreateSaint       },
            };

        /// <summary>Maps third-tier class names to the mastery skill learned on promotion.</summary>
        private static readonly Dictionary<string, Func<ISkill>> _masterySkillMap =
            new Dictionary<string, Func<ISkill>>
            {
                { "Trueblade", SkillDatabase.CreateAstra    },
                { "Marshall",  SkillDatabase.CreateSol      },
                { "Reaver",    SkillDatabase.CreateColossus },
                { "Archsage",  SkillDatabase.CreateFlare    },
                { "Marksman",  SkillDatabase.CreateDeadeye  },
                { "Saint",     SkillDatabase.CreateCorona   },
            };

        /// <summary>Returns true when the unit is at max level and has a promotion path.</summary>
        public static bool CanPromote(IUnit unit)
        {
            return unit.Level >= Unit.MaxLevel
                && _promotionMap.ContainsKey(unit.Class.Name);
        }

        /// <summary>
        /// Promotes the unit to its next class tier.
        /// Mastery skills are auto-learned on promotion to Tier 3.
        /// Throws <see cref="InvalidOperationException"/> if the unit cannot promote.
        /// </summary>
        public static void Promote(Unit unit)
        {
            if (!CanPromote(unit))
                throw new InvalidOperationException(
                    $"{unit.Name} cannot promote: must be Lv.{Unit.MaxLevel} with a valid promotion path.");

            var promotedClass = _promotionMap[unit.Class.Name]();
            unit.ChangeClass(promotedClass);

            // Auto-learn mastery skill on Tier 3 promotion
            if (_masterySkillMap.TryGetValue(promotedClass.Name, out var skillFactory))
                unit.LearnSkill(skillFactory());
        }

        /// <summary>Returns the mastery skill for a given third-tier class, or null if none.</summary>
        public static ISkill GetMasterySkill(string className)
        {
            return _masterySkillMap.TryGetValue(className, out var factory) ? factory() : null;
        }
    }
}
