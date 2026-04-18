namespace TacticFantasy.Domain.Units
{
    /// <summary>
    /// Pure domain service: converts unit state to human-readable display strings.
    /// No Unity dependency — fully testable.
    /// </summary>
    public static class UnitDisplayFormatter
    {
        /// <summary>
        /// Returns a one-line status string such as "☠ Poisoned (2 turns)" or "" if no status.
        /// </summary>
        public static string FormatStatus(IUnit unit)
        {
            if (unit?.ActiveStatus == null)
                return "";

            return unit.ActiveStatus.Type switch
            {
                StatusEffectType.Poison => $"Poisoned ({unit.ActiveStatus.RemainingTurns} turns)",
                StatusEffectType.Sleep  => $"Sleep ({unit.ActiveStatus.RemainingTurns} turns)",
                StatusEffectType.Stun   => $"Stun ({unit.ActiveStatus.RemainingTurns} turn)",
                _                       => ""
            };
        }

        /// <summary>
        /// Returns the full info block text for a unit panel (newline-separated).
        /// </summary>
        public static string FormatUnitInfo(IUnit unit)
        {
            if (unit == null)
                return "Select a unit";

            string lines = string.Join("\n", new[]
            {
                unit.Name,
                $"Class: {unit.Class.Name}",
                $"HP: {unit.CurrentHP}/{unit.MaxHP}",
                $"STR: {unit.CurrentStats.STR} SPD: {unit.CurrentStats.SPD}",
                $"DEF: {unit.CurrentStats.DEF} RES: {unit.CurrentStats.RES}",
                $"Weapon: {unit.EquippedWeapon.Name}"
            });

            string statusLine = FormatStatus(unit);
            if (statusLine.Length > 0)
                lines += $"\n{statusLine}";

            return lines;
        }

        /// <summary>
        /// Returns a short level/XP string such as "Lv 5  EXP 42/100" or "Lv 20 (MAX)".
        /// </summary>
        public static string FormatLevelInfo(IUnit unit)
        {
            if (unit == null)
                return "";

            if (unit.Level >= Unit.MaxLevel)
                return $"Lv {unit.Level} (MAX)";

            return $"Lv {unit.Level}  EXP {unit.Experience}/{Unit.XpPerLevel}";
        }
    }
}
