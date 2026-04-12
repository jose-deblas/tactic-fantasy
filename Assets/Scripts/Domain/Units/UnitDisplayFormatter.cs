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

            string lines =
                $"{unit.Name}\n" +
                $"Class: {unit.Class.Name}\n" +
                $"HP: {unit.CurrentHP}/{unit.MaxHP}\n" +
                $"STR: {unit.CurrentStats.STR} SPD: {unit.CurrentStats.SPD}\n" +
                $"DEF: {unit.CurrentStats.DEF} RES: {unit.CurrentStats.RES}\n" +
                $"Weapon: {unit.EquippedWeapon.Name}";

            string statusLine = FormatStatus(unit);
            if (statusLine.Length > 0)
                lines += $"\n{statusLine}";

            return lines;
        }
    }
}
