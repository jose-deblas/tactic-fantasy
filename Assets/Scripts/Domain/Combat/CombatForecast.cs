using System;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Combat
{
    /// <summary>
    /// Immutable value object representing anticipated combat statistics
    /// before a battle is actually resolved.
    /// Pure domain type — no Unity dependency.
    /// </summary>
    public sealed class CombatForecast
    {
        /// <summary>Damage the attacker would deal on a hit (before crits).</summary>
        public int AttackerDamage { get; }

        /// <summary>Displayed hit% (0–100) for the attacker.</summary>
        public int AttackerHitRate { get; }

        /// <summary>Displayed crit% (0–100) for the attacker.</summary>
        public int AttackerCritRate { get; }

        /// <summary>Whether the attacker will double-attack.</summary>
        public bool AttackerDoubles { get; }

        /// <summary>Damage the defender would deal on a counter-hit.</summary>
        public int DefenderDamage { get; }

        /// <summary>Displayed hit% (0–100) for the defender's counter-attack.</summary>
        public int DefenderHitRate { get; }

        /// <summary>Displayed crit% (0–100) for the defender's counter-attack.</summary>
        public int DefenderCritRate { get; }

        /// <summary>Whether the defender will double-attack on counter.</summary>
        public bool DefenderDoubles { get; }

        /// <summary>Whether the defender can counter-attack at all.</summary>
        public bool DefenderCanCounter { get; }

        public CombatForecast(
            int attackerDamage, int attackerHitRate, int attackerCritRate, bool attackerDoubles,
            int defenderDamage, int defenderHitRate, int defenderCritRate, bool defenderDoubles,
            bool defenderCanCounter)
        {
            AttackerDamage    = attackerDamage;
            AttackerHitRate   = attackerHitRate;
            AttackerCritRate  = attackerCritRate;
            AttackerDoubles   = attackerDoubles;
            DefenderDamage    = defenderDamage;
            DefenderHitRate   = defenderHitRate;
            DefenderCritRate  = defenderCritRate;
            DefenderDoubles   = defenderDoubles;
            DefenderCanCounter = defenderCanCounter;
        }

        /// <summary>
        /// One-line human-readable summary for UI display.
        /// e.g. "DMG: 8  HIT: 82%  CRIT: 5%  x2"
        /// </summary>
        public string FormatSummary()
        {
            string doublesTag = AttackerDoubles ? "  ×2" : "";
            return $"DMG: {AttackerDamage}  HIT: {AttackerHitRate}%  CRIT: {AttackerCritRate}%{doublesTag}";
        }

        /// <summary>
        /// Full two-sided panel text for a Fire-Emblem-style battle forecast panel.
        /// </summary>
        public string FormatFull(string attackerName, string defenderName)
        {
            string atkDoubles = AttackerDoubles   ? " ×2" : "";
            string defDoubles = DefenderDoubles   ? " ×2" : "";
            string noCounter  = DefenderCanCounter ? "" : " (no counter)";

            return
                $"── BATTLE FORECAST ──\n" +
                $"{attackerName}\n" +
                $"  DMG  {AttackerDamage,3}{atkDoubles}\n" +
                $"  HIT  {AttackerHitRate,3}%\n" +
                $"  CRIT {AttackerCritRate,3}%\n" +
                $"── vs ──\n" +
                $"{defenderName}{noCounter}\n" +
                $"  DMG  {DefenderDamage,3}{defDoubles}\n" +
                $"  HIT  {DefenderHitRate,3}%\n" +
                $"  CRIT {DefenderCritRate,3}%\n";
        }
    }

    /// <summary>
    /// Pure domain service that computes <see cref="CombatForecast"/> without
    /// rolling any dice — deterministic calculation of expected values.
    /// </summary>
    public sealed class CombatForecastService
    {
        private readonly ICombatResolver _resolver;

        public CombatForecastService(ICombatResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// Calculates the anticipated combat statistics for a potential engagement.
        /// Does NOT modify unit state; purely read-only.
        /// </summary>
        public CombatForecast Calculate(IUnit attacker, IUnit defender, IGameMap map)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defender == null) throw new ArgumentNullException(nameof(defender));
            if (map      == null) throw new ArgumentNullException(nameof(map));

            int atkDmg   = _resolver.CalculateDamage(attacker, defender, map);
            int atkHit   = CalculateHitRate(attacker, defender, map);
            int atkCrit  = CalculateCritRate(attacker, defender);
            bool atkDoubles = _resolver.CanDoubleAttack(attacker, defender);

            bool defCanCounter = _resolver.CanCounterAttack(attacker, defender);
            int defDmg   = defCanCounter ? _resolver.CalculateDamage(defender, attacker, map) : 0;
            int defHit   = defCanCounter ? CalculateHitRate(defender, attacker, map) : 0;
            int defCrit  = defCanCounter ? CalculateCritRate(defender, attacker) : 0;
            bool defDoubles = defCanCounter && _resolver.CanDoubleAttack(defender, attacker);

            return new CombatForecast(
                atkDmg, atkHit, atkCrit, atkDoubles,
                defDmg, defHit, defCrit, defDoubles,
                defCanCounter);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Calculates displayed hit rate (0–100), mirroring CombatResolver logic
        /// but without the True Hit dice roll.
        /// </summary>
        private int CalculateHitRate(IUnit attacker, IUnit defender, IGameMap map)
        {
            int attackAS = _resolver.CalculateAttackSpeed(attacker);
            int defenderAS = _resolver.CalculateAttackSpeed(defender);

            // Re-use weapon-triangle calculation via damage calculation helpers is not directly
            // accessible, so we compute hit ourselves using the same formula as CombatResolver.
            int hit    = (attacker.CurrentStats.SKL * 2) + (attacker.CurrentStats.LCK / 2)
                       + attacker.EquippedWeapon.Hit;

            var (_, triangleBonus) = WeaponTriangle.GetTriangleModifiers(
                attacker.EquippedWeapon, defender.EquippedWeapon);
            hit += triangleBonus;

            int avoid  = (defenderAS * 2) + defender.CurrentStats.LCK
                       + TerrainProperties.GetAvoidBonus(
                             map.GetTile(defender.Position.x, defender.Position.y).Terrain);

            return Math.Max(0, Math.Min(100, hit - avoid));
        }

        private int CalculateCritRate(IUnit attacker, IUnit defender)
        {
            int crit = (attacker.CurrentStats.SKL / 2) + attacker.EquippedWeapon.Crit
                     - defender.CurrentStats.LCK;
            return Math.Max(0, Math.Min(100, crit));
        }
    }
}
