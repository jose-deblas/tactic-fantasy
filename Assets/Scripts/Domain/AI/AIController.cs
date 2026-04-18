using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.AI
{
    public interface IAIController
    {
        void DecideAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder, out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction);
    }

    public class AIController : IAIController
    {
        private readonly ICombatResolver _combatResolver;
        private readonly Random _rng = new Random();

        public AIController(ICombatResolver combatResolver)
        {
            _combatResolver = combatResolver;
        }

        public void DecideAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction)
        {
            moveTarget = null;
            attackTarget = null;
            isHealAction = false;

            // A unit with a broken weapon cannot attack or heal — just advance.
            if (unit.HasBrokenWeapon)
            {
                AdvanceTowardNearestEnemy(unit, allUnits, map, pathFinder, out moveTarget);
                return;
            }

            // Self-preservation: if HP is critically low and a healing Fort is reachable, retreat.
            if (TryRetreatToFort(unit, allUnits, map, pathFinder, out moveTarget))
                return;

            if (unit.EquippedWeapon.Type == WeaponType.STAFF)
            {
                DecideHealAction(unit, allUnits, map, pathFinder, out moveTarget, out attackTarget, out isHealAction);
            }
            else
            {
                DecideAttackAction(unit, allUnits, map, pathFinder, out moveTarget, out attackTarget);
            }
        }

        /// <summary>
        /// Moves the unit one step toward the nearest enemy team member. Used
        /// when the unit cannot attack (e.g., broken weapon).
        /// </summary>
        private void AdvanceTowardNearestEnemy(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget)
        {
            moveTarget = null;
            var opponents = allUnits
                .Where(u => u.Team != unit.Team && u.IsAlive)
                .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                .ToList();

            if (opponents.Count == 0)
                return;

            var closest = opponents.First();
            var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map, allUnits);

            var bestTile = reachable
                .OrderBy(t => map.GetDistance(t.Item1, t.Item2, closest.Position.x, closest.Position.y))
                .First();

            if (bestTile != (unit.Position.x, unit.Position.y))
                moveTarget = bestTile;
        }

        private void DecideAttackAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget)
        {
            moveTarget = null;
            attackTarget = null;

            var playerUnits = allUnits.Where(u => u.Team == Team.PlayerTeam && u.IsAlive).ToList();

            if (playerUnits.Count == 0)
                return;

            var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map, allUnits);

            var bestAttackOption = FindBestAttackPosition(unit, playerUnits, reachable, map);

            if (bestAttackOption.HasValue)
            {
                var option = bestAttackOption.Value;
                moveTarget = option.position;
                attackTarget = option.target;
            }
            else
            {
                // Move toward closest enemy using the closest reachable tile
                var closestEnemy = playerUnits
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .First();

                var bestTile = reachable
                    .OrderBy(t => map.GetDistance(t.Item1, t.Item2, closestEnemy.Position.x, closestEnemy.Position.y))
                    .First();

                if (bestTile != (unit.Position.x, unit.Position.y))
                {
                    moveTarget = bestTile;
                }
            }
        }

        private void DecideHealAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction)
        {
            moveTarget = null;
            attackTarget = null;
            isHealAction = false;

            var allyUnits = allUnits.Where(u => u.Team == Team.EnemyTeam && u.IsAlive && u.Id != unit.Id).ToList();
            var injuredAllies = allyUnits.Where(u => u.CurrentHP < u.MaxHP).OrderBy(u => u.CurrentHP).ToList();

            if (injuredAllies.Count == 0)
                return;

            var targetAlly = injuredAllies.First();
            int distance = map.GetDistance(unit.Position.x, unit.Position.y, targetAlly.Position.x, targetAlly.Position.y);

            if (distance <= unit.EquippedWeapon.MaxRange)
            {
                attackTarget = targetAlly;
                isHealAction = true;
            }
            else
            {
                var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map, allUnits);

                var bestTile = reachable
                    .OrderBy(t => map.GetDistance(t.Item1, t.Item2, targetAlly.Position.x, targetAlly.Position.y))
                    .First();

                if (bestTile != (unit.Position.x, unit.Position.y))
                {
                    moveTarget = bestTile;
                }
            }
        }

        /// <summary>
        /// Scores a potential attack option for AI decision-making.
        /// Lower score = more attractive target.
        ///
        /// Scoring factors (additive penalties/bonuses):
        ///  - Base: target's current HP (lower HP → easier kill → preferred)
        ///  - Triangle advantage  : -15 bonus  (strongly prefer advantaged targets)
        ///  - Triangle disadvantage: +30 penalty (avoid unfavorable matchups)
        ///  - Target is sleeping/stunned (no counter risk): -20 bonus
        ///  - Target already has the same status our weapon inflicts: +20 penalty (redundant)
        /// A near-dead unit (HP ≤ weapon power) scores so low that it beats any
        /// triangle consideration — the finisher heuristic.
        /// </summary>
        private int ScoreAttackOption(IUnit attacker, IUnit target)
        {
            // Finisher heuristic: if the target can be killed by a single attack,
            // give it a very low score so it beats other considerations.
            if (target.CurrentHP <= attacker.EquippedWeapon.Power)
            {
                return int.MinValue / 2; // very strong preference
            }

            int score = target.CurrentHP;

            var (dmgBonus, _) = WeaponTriangle.GetTriangleModifiers(attacker.EquippedWeapon, target.EquippedWeapon);

            if (dmgBonus > 0)
                score -= TriangleAdvantageBias;      // advantage: more attractive
            else if (dmgBonus < 0)
                score += TriangleDisadvantagePenalty; // disadvantage: less attractive

            // Prefer targets that cannot counter-attack (sleep/stun)
            if (target.ActiveStatus != null &&
                (target.ActiveStatus.Type == StatusEffectType.Sleep ||
                 target.ActiveStatus.Type == StatusEffectType.Stun))
            {
                score -= NoCounterBias;
            }

            // Avoid redundant status application (already has the same status our weapon inflicts)
            if (attacker.EquippedWeapon.OnHitStatus != null &&
                target.ActiveStatus != null &&
                target.ActiveStatus.Type == attacker.EquippedWeapon.OnHitStatus.Value)
            {
                score += RedundantStatusPenalty;
            }

            return score;
        }

        // Tuning constants for the target-selection heuristic
        private const int TriangleAdvantageBias      = 15;
        private const int TriangleDisadvantagePenalty = 30;
        private const int NoCounterBias               = 20; // bonus for targets that can't counter (sleep/stun)
        private const int RedundantStatusPenalty      = 20; // penalty for applying a status the target already has

        private ((int x, int y) position, IUnit target)? FindBestAttackPosition(IUnit unit, List<IUnit> enemies, HashSet<(int, int)> reachable, IGameMap map)
        {
            var validTargets = new List<(int x, int y, IUnit target, int score)>();

            foreach (var position in reachable)
            {
                foreach (var enemy in enemies)
                {
                    int distance = map.GetDistance(position.Item1, position.Item2, enemy.Position.x, enemy.Position.y);

                    if (distance >= unit.EquippedWeapon.MinRange && distance <= unit.EquippedWeapon.MaxRange)
                    {
                        int score = ScoreAttackOption(unit, enemy);
                        score -= ScoreTerrainBonus(position.Item1, position.Item2, map);
                        validTargets.Add((position.Item1, position.Item2, enemy, score));
                    }
                }
            }

            if (validTargets.Count == 0)
                return null;

            var selected = validTargets.OrderBy(t => t.score).First();
            return ((selected.x, selected.y), selected.target);
        }

        /// <summary>
        /// Returns a bonus (lower score = more attractive) based on the defense
        /// value of the tile at the given position. This makes the AI prefer
        /// defensive terrain (Fort, Forest, Mountain) when attacking from it.
        /// Bonus scale matches <see cref="TriangleAdvantageBias"/> to allow
        /// meaningful trade-offs between terrain and target quality.
        /// </summary>
        private int ScoreTerrainBonus(int x, int y, IGameMap map)
        {
            int defBonus = TerrainProperties.GetDefenseBonus(map.GetTile(x, y).Terrain);
            // Each point of defense is worth TerrainDefenseBiasPerPoint in the score
            return defBonus * TerrainDefenseBiasPerPoint;
        }

        private const int TerrainDefenseBiasPerPoint = 8;

        /// <summary>
        /// Returns true and sets <paramref name="moveTarget"/> when the unit is below
        /// <see cref="LowHpThresholdPercent"/> of its max HP AND can reach a Fort tile.
        /// The unit retreats to the nearest reachable Fort to benefit from its healing.
        /// </summary>
        private bool TryRetreatToFort(IUnit unit, List<IUnit> allUnits, IGameMap map,
            IPathFinder pathFinder, out (int x, int y)? moveTarget)
        {
            moveTarget = null;

            // Only trigger when HP is critically low
            if (unit.CurrentHP > unit.MaxHP * LowHpThresholdPercent / 100)
                return false;

            var reachable = pathFinder.GetMovementRange(
                unit.Position.x, unit.Position.y,
                unit.CurrentStats.MOV, unit, map, allUnits);

            // Find the closest reachable Fort tile (excluding current tile)
            (int x, int y)? bestFort = null;
            int bestDist = int.MaxValue;
            foreach (var tile in reachable)
            {
                if (tile == (unit.Position.x, unit.Position.y))
                    continue;
                if (map.GetTile(tile.Item1, tile.Item2).Terrain == TerrainType.Fort)
                {
                    int dist = map.GetDistance(unit.Position.x, unit.Position.y, tile.Item1, tile.Item2);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestFort = tile;
                    }
                }
            }

            if (!bestFort.HasValue)
                return false;

            moveTarget = bestFort;
            return true;
        }

        /// <summary>
        /// HP percentage threshold (inclusive) below which a unit will attempt
        /// to retreat to a healing Fort if one is reachable.
        /// </summary>
        private const int LowHpThresholdPercent = 30;
    }
}
