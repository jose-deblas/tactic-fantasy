using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Map
{
    public interface IReinforcementService
    {
        List<IUnit> EvaluateTriggers(
            IReadOnlyList<ReinforcementTrigger> triggers,
            int currentTurn,
            IReadOnlyList<IUnit> allUnits);
    }

    public class ReinforcementService : IReinforcementService
    {
        private readonly IMapLoader _mapLoader;
        private int _nextId;

        public ReinforcementService(IMapLoader mapLoader, int startingId = 100)
        {
            _mapLoader = mapLoader;
            _nextId = startingId;
        }

        public List<IUnit> EvaluateTriggers(
            IReadOnlyList<ReinforcementTrigger> triggers,
            int currentTurn,
            IReadOnlyList<IUnit> allUnits)
        {
            var spawned = new List<IUnit>();

            foreach (var trigger in triggers)
            {
                if (trigger.HasFired)
                    continue;

                if (ShouldFire(trigger, currentTurn, allUnits))
                {
                    trigger.HasFired = true;
                    foreach (var placement in trigger.UnitsToSpawn)
                    {
                        var unit = CreateUnitFromPlacement(_nextId++, placement);
                        spawned.Add(unit);
                    }
                }
            }

            return spawned;
        }

        private bool ShouldFire(ReinforcementTrigger trigger, int currentTurn, IReadOnlyList<IUnit> allUnits)
        {
            switch (trigger.Condition)
            {
                case TriggerCondition.OnTurn:
                    return currentTurn == trigger.TurnNumber;

                case TriggerCondition.OnTileSteppedOn:
                    if (!trigger.TriggerTile.HasValue)
                        return false;
                    var (tx, ty) = trigger.TriggerTile.Value;
                    return allUnits.Any(u => u.IsAlive && u.Position == (tx, ty));

                case TriggerCondition.OnUnitDeath:
                    if (!trigger.TriggerUnitId.HasValue)
                        return false;
                    var target = allUnits.FirstOrDefault(u => u.Id == trigger.TriggerUnitId.Value);
                    return target != null && !target.IsAlive;

                default:
                    return false;
            }
        }

        private IUnit CreateUnitFromPlacement(int id, UnitPlacement placement)
        {
            var classData = ResolveClass(placement.ClassName);
            var weapon = WeaponFactory.GetWeaponForClass(classData.WeaponType);
            return new Unit(id, placement.Name, placement.Team, classData, classData.BaseStats, placement.Position, weapon);
        }

        private static IClassData ResolveClass(string className)
        {
            return className.ToLower() switch
            {
                "myrmidon" => ClassDataFactory.CreateMyrmidon(),
                "soldier" => ClassDataFactory.CreateSoldier(),
                "fighter" => ClassDataFactory.CreateFighter(),
                "mage" => ClassDataFactory.CreateMage(),
                "archer" => ClassDataFactory.CreateArcher(),
                "cleric" => ClassDataFactory.CreateCleric(),
                "swordmaster" => ClassDataFactory.CreateSwordmaster(),
                "general" => ClassDataFactory.CreateGeneral(),
                "warrior" => ClassDataFactory.CreateWarrior(),
                "sage" => ClassDataFactory.CreateSage(),
                "sniper" => ClassDataFactory.CreateSniper(),
                "bishop" => ClassDataFactory.CreateBishop(),
                _ => ClassDataFactory.CreateSoldier()
            };
        }
    }
}
