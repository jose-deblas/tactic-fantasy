using System.Collections.Generic;

namespace TacticFantasy.Domain.Map
{
    public enum TriggerCondition
    {
        OnTurn,
        OnTileSteppedOn,
        OnUnitDeath
    }

    public class ReinforcementTrigger
    {
        public TriggerCondition Condition { get; }
        public int TurnNumber { get; }
        public (int x, int y)? TriggerTile { get; }
        public int? TriggerUnitId { get; }
        public List<UnitPlacement> UnitsToSpawn { get; }
        public bool HasFired { get; set; }

        private ReinforcementTrigger(
            TriggerCondition condition,
            List<UnitPlacement> unitsToSpawn,
            int turnNumber = 0,
            (int, int)? triggerTile = null,
            int? triggerUnitId = null)
        {
            Condition = condition;
            UnitsToSpawn = unitsToSpawn;
            TurnNumber = turnNumber;
            TriggerTile = triggerTile;
            TriggerUnitId = triggerUnitId;
            HasFired = false;
        }

        public static ReinforcementTrigger OnTurn(int turn, List<UnitPlacement> units)
        {
            return new ReinforcementTrigger(TriggerCondition.OnTurn, units, turnNumber: turn);
        }

        public static ReinforcementTrigger OnTileSteppedOn((int, int) tile, List<UnitPlacement> units)
        {
            return new ReinforcementTrigger(TriggerCondition.OnTileSteppedOn, units, triggerTile: tile);
        }

        public static ReinforcementTrigger OnUnitDeath(int unitId, List<UnitPlacement> units)
        {
            return new ReinforcementTrigger(TriggerCondition.OnUnitDeath, units, triggerUnitId: unitId);
        }
    }
}
