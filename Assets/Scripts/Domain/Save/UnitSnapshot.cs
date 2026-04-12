using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Save
{
    /// <summary>
    /// Serialisable snapshot of a single unit's mutable state.
    /// Captures only what changes at runtime (HP, position, status).
    /// Identity data (id, name, team, class, weapon) is preserved to allow
    /// reconstruction via UnitFactory on load.
    /// </summary>
    public class UnitSnapshot
    {
        public int Id              { get; }
        public string Name         { get; }
        public Team Team           { get; }
        public string ClassName    { get; }
        public string WeaponName   { get; }

        public int CurrentHP       { get; }
        public int PositionX       { get; }
        public int PositionY       { get; }
        public int Level            { get; }
        public int Experience       { get; }

        public StatusEffectType StatusType          { get; }
        public int              StatusRemainingTurns { get; }

        private UnitSnapshot(
            int id, string name, Team team, string className, string weaponName,
            int currentHP, int posX, int posY,
            StatusEffectType statusType, int statusTurns,
            int level, int experience)
        {
            Id                   = id;
            Name                 = name;
            Team                 = team;
            ClassName            = className;
            WeaponName           = weaponName;
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

            return new UnitSnapshot(
                unit.Id, unit.Name, unit.Team,
                unit.Class?.Name ?? string.Empty,
                unit.EquippedWeapon?.Name ?? string.Empty,
                unit.CurrentHP,
                unit.Position.x, unit.Position.y,
                statusType, statusTurns,
                unit.Level, unit.Experience);
        }

        /// <summary>
        /// Reconstructs a snapshot from persisted data (e.g. loaded from disk).
        /// Used by persistence adapters; avoids exposing a public constructor.
        /// </summary>
        public static UnitSnapshot Rebuild(
            int id, string name, Team team, string className, string weaponName,
            int currentHP, int posX, int posY,
            StatusEffectType statusType, int statusTurns,
            int level, int experience)
        {
            return new UnitSnapshot(id, name, team, className, weaponName,
                currentHP, posX, posY, statusType, statusTurns, level, experience);
        }
    }
}
