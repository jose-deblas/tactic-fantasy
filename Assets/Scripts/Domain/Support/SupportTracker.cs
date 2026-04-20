using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Support
{
    /// <summary>
    /// Tracks support relationships and points between unit pairs.
    /// Support points accumulate when paired units end turns adjacent;
    /// levels unlock at thresholds (C=20, B=60, A=120).
    /// </summary>
    public class SupportTracker
    {
        public const int ThresholdC = 20;
        public const int ThresholdB = 60;
        public const int ThresholdA = 120;
        public const int SupportRange = 3;

        private readonly Dictionary<(int, int), int> _points = new Dictionary<(int, int), int>();
        private readonly HashSet<(int, int)> _registeredPairs = new HashSet<(int, int)>();

        /// <summary>
        /// Registers a potential support pairing. Only registered pairs can gain points.
        /// </summary>
        public void RegisterSupport(int unitId1, int unitId2)
        {
            var key = MakeKey(unitId1, unitId2);
            _registeredPairs.Add(key);
            if (!_points.ContainsKey(key))
                _points[key] = 0;
        }

        /// <summary>Adds support points for a registered pair.</summary>
        public void AddPoints(int unitId1, int unitId2, int points)
        {
            var key = MakeKey(unitId1, unitId2);
            if (!_registeredPairs.Contains(key)) return;
            _points[key] = _points.GetValueOrDefault(key, 0) + points;
        }

        /// <summary>Gets current support level between two units.</summary>
        public SupportLevel GetLevel(int unitId1, int unitId2)
        {
            var key = MakeKey(unitId1, unitId2);
            if (!_points.TryGetValue(key, out int pts)) return SupportLevel.None;

            if (pts >= ThresholdA) return SupportLevel.A;
            if (pts >= ThresholdB) return SupportLevel.B;
            if (pts >= ThresholdC) return SupportLevel.C;
            return SupportLevel.None;
        }

        /// <summary>Gets current accumulated points for a pair.</summary>
        public int GetPoints(int unitId1, int unitId2)
        {
            var key = MakeKey(unitId1, unitId2);
            return _points.GetValueOrDefault(key, 0);
        }

        /// <summary>
        /// Computes total combat bonus for a unit from all same-team allies within support range.
        /// </summary>
        public SupportBonus GetCombatBonus(IUnit unit, IReadOnlyList<IUnit> allies)
        {
            var bonus = SupportBonus.Zero;
            foreach (var ally in allies)
            {
                if (ally.Id == unit.Id) continue;
                if (ally.Team != unit.Team) continue;
                if (!ally.IsAlive) continue;

                int distance = Math.Abs(unit.Position.x - ally.Position.x)
                             + Math.Abs(unit.Position.y - ally.Position.y);
                if (distance > SupportRange) continue;

                var level = GetLevel(unit.Id, ally.Id);
                if (level == SupportLevel.None) continue;

                bonus = bonus + SupportBonus.ForLevel(level);
            }
            return bonus;
        }

        private static (int, int) MakeKey(int id1, int id2)
        {
            return id1 < id2 ? (id1, id2) : (id2, id1);
        }
    }
}
