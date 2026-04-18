using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// Serialisable snapshot of a single unit's mutable state.
    /// Captures only what changes at runtime (HP, position, status, inventory).
    /// Identity data (id, name, team, class) is preserved to allow
    /// reconstruction via UnitFactory on load.
    /// </summary>
    public class UnitSnapshot
    {
        public int Id              { get; }
        public string Name         { get; }
        public Team Team           { get; }
        public string ClassName    { get; }

        /// <summary>Ordered list of item names in the unit's inventory.</summary>
        public IReadOnlyList<string> InventoryItemNames { get; }

        /// <summary>Backward-compat: returns the first inventory item name, or empty string.</summary>
        public string WeaponName => InventoryItemNames.Count > 0 ? InventoryItemNames[0] : string.Empty;

        public int CurrentHP       { get; }
        public int PositionX       { get; }
        public int PositionY       { get; }
        public int Level            { get; }
        public int Experience       { get; }

        public StatusEffectType StatusType          { get; }
        public int              StatusRemainingTurns { get; }

        private UnitSnapshot(
            int id, string name, Team team, string className,
            IReadOnlyList<string> inventoryItemNames,
            int currentHP, int posX, int posY,
            StatusEffectType statusType, int statusTurns,
            int level, int experience)
        {
            Id                   = id;
            Name                 = name;
            Team                 = team;
            ClassName            = className;
            InventoryItemNames   = inventoryItemNames;
            CurrentHP            = currentHP;
            PositionX            = posX;
            PositionY            = posY;
            StatusType           = statusType;
            StatusRemainingTurns = statusTurns;
            Level                = level;
            Experience           = experience;
        }

        /// <summary>Captures the mutable runtime state of a unit.</summary>
        public static UnitSnapshot Capture(IUnit unit)
        {
            var status      = unit.ActiveStatus;
            var statusType  = status?.Type  ?? StatusEffectType.None;
            var statusTurns = status?.RemainingTurns ?? 0;

            var itemNames = unit.Inventory.Items
                .Select(i => i.Name)
                .ToList()
                .AsReadOnly();

            return new UnitSnapshot(
                unit.Id, unit.Name, unit.Team,
                unit.Class?.Name ?? string.Empty,
                itemNames,
                unit.CurrentHP,
                unit.Position.x, unit.Position.y,
                statusType, statusTurns,
                unit.Level, unit.Experience);
        }

        /// <summary>
        /// Reconstructs a snapshot from persisted data with inventory support.
        /// </summary>
        public static UnitSnapshot Rebuild(
            int id, string name, Team team, string className,
            IReadOnlyList<string> inventoryItemNames,
            int currentHP, int posX, int posY,
            StatusEffectType statusType, int statusTurns,
            int level, int experience)
        {
            return new UnitSnapshot(id, name, team, className,
                inventoryItemNames,
                currentHP, posX, posY, statusType, statusTurns, level, experience);
        }

        /// <summary>
        /// Backward-compatible rebuild from old save format with single weapon name.
        /// </summary>
        public static UnitSnapshot Rebuild(
            int id, string name, Team team, string className, string weaponName,
            int currentHP, int posX, int posY,
            StatusEffectType statusType, int statusTurns,
            int level, int experience)
        {
            var itemNames = string.IsNullOrEmpty(weaponName)
                ? new List<string>().AsReadOnly()
                : new List<string> { weaponName }.AsReadOnly();

            return new UnitSnapshot(id, name, team, className,
                itemNames,
                currentHP, posX, posY, statusType, statusTurns, level, experience);
        }
    }
}
