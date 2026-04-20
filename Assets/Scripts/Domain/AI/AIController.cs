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
        void DecideAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder, out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction, IFogOfWar fogOfWar = null);
    }

    public class AIController : IAIController
    {
        private readonly ICombatResolver _combatResolver;
        private readonly Random _rng = new Random();
        private readonly Dictionary<int, (int x, int y)> _lastKnownPositions = new Dictionary<int, (int, int)>();

        public AIController(ICombatResolver combatResolver)
        {
            _combatResolver = combatResolver;
        }

        /// <summary>Returns true if the two teams are hostile to each other.</summary>
        public static bool AreHostile(Team a, Team b) => TeamRelations.AreHostile(a, b);

        /// <summary>Returns true if the two teams are allied (same side).</summary>
        public static bool AreAllied(Team a, Team b) => TeamRelations.AreAllied(a, b);

        public void DecideAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction, IFogOfWar fogOfWar = null)
        {
            moveTarget = null;
            attackTarget = null;
            isHealAction = false;

            // Update last-known positions for visible opponents
            if (fogOfWar != null)
            {
                UpdateLastKnownPositions(unit, allUnits, fogOfWar);
            }

            // A unit with a broken weapon cannot attack or heal — just advance.
            if (unit.HasBrokenWeapon)
            {
                AdvanceTowardNearestEnemy(unit, allUnits, map, pathFinder, out moveTarget, fogOfWar);
                return;
            }

            // Untransformed Laguz retreat toward safety — they have halved stats and are vulnerable.
            if (unit.IsLaguz && !unit.IsTransformed)
            {
                RetreatFromEnemies(unit, allUnits, map, pathFinder, out moveTarget);
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
                DecideAttackAction(unit, allUnits, map, pathFinder, out moveTarget, out attackTarget, fogOfWar);
            }
        }

        /// <summary>
        /// Moves the unit one step toward the nearest enemy team member. Used
        /// when the unit cannot attack (e.g., broken weapon).
        /// </summary>
        private void AdvanceTowardNearestEnemy(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, IFogOfWar fogOfWar = null)
        {
            moveTarget = null;

            // If fog is active, try to use last-known positions for non-visible opponents
            (int x, int y)? targetPos = null;
            var opponents = allUnits
                .Where(u => AreHostile(unit.Team, u.Team) && u.IsAlive)
                .ToList();

            if (fogOfWar != null)
            {
                // Only consider visible opponents for direct targeting
                var visibleOpponents = opponents
                    .Where(u => fogOfWar.IsTileVisible(u.Position.x, u.Position.y, unit.Team))
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .ToList();

                if (visibleOpponents.Count > 0)
                {
                    targetPos = visibleOpponents.First().Position;
                }
                else
                {
                    // Move toward last-known position
                    targetPos = GetClosestLastKnownPosition(unit, map);
                }
            }
            else
            {
                var sorted = opponents
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .ToList();
                if (sorted.Count > 0)
                    targetPos = sorted.First().Position;
            }

            if (!targetPos.HasValue)
                return;

            var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map, allUnits);

            var bestTile = reachable
                .OrderBy(t => map.GetDistance(t.Item1, t.Item2, targetPos.Value.x, targetPos.Value.y))
                .First();

            if (bestTile != (unit.Position.x, unit.Position.y))
                moveTarget = bestTile;
        }

        private void DecideAttackAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget, IFogOfWar fogOfWar = null)
        {
            moveTarget = null;
            attackTarget = null;

            var hostileUnits = allUnits.Where(u => AreHostile(unit.Team, u.Team) && u.IsAlive).ToList();

            if (hostileUnits.Count == 0)
                return;

            // When fog is active, only attack visible targets
            var targetableUnits = fogOfWar != null
                ? hostileUnits.Where(u => fogOfWar.IsTileVisible(u.Position.x, u.Position.y, unit.Team)).ToList()
                : hostileUnits;

            var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map, allUnits);

            if (targetableUnits.Count > 0)
            {
                var bestAttackOption = FindBestAttackPosition(unit, targetableUnits, reachable, map);

                if (bestAttackOption.HasValue)
                {
                    var option = bestAttackOption.Value;
                    moveTarget = option.position;
                    attackTarget = option.target;
                    return;
                }
            }

            // No attack possible — move toward closest known enemy
            (int x, int y)? advanceTarget = null;
            if (fogOfWar != null)
            {
                var visibleEnemies = hostileUnits
                    .Where(u => fogOfWar.IsTileVisible(u.Position.x, u.Position.y, unit.Team))
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .ToList();

                advanceTarget = visibleEnemies.Count > 0
                    ? visibleEnemies.First().Position
                    : GetClosestLastKnownPosition(unit, map);
            }
            else
            {
                var closestEnemy = hostileUnits
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .First();
                advanceTarget = closestEnemy.Position;
            }

            if (!advanceTarget.HasValue)
                return;

            var bestTile = reachable
                .OrderBy(t => map.GetDistance(t.Item1, t.Item2, advanceTarget.Value.x, advanceTarget.Value.y))
                .First();

            if (bestTile != (unit.Position.x, unit.Position.y))
            {
                moveTarget = bestTile;
            }
        }

        private void DecideHealAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget, out bool isHealAction)
        {
            moveTarget = null;
            attackTarget = null;
            isHealAction = false;

            var allyUnits = allUnits.Where(u => AreAllied(unit.Team, u.Team) && u.IsAlive && u.Id != unit.Id).ToList();
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
            if (target.CurrentHP <= attacker.EquippedWeapon.Might)
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

            // Prefer attacking higher-attack (threat) units when other factors are equal.
            // Higher target ATK reduces the score (more attractive to eliminate high-threat units).
            score -= target.CurrentStats.STR * AttackStatBias;

            return score;
        }

        // Tuning constants for the target-selection heuristic
        private const int TriangleAdvantageBias      = 15;
        private const int TriangleDisadvantagePenalty = 30;
        private const int NoCounterBias               = 20; // bonus for targets that can't counter (sleep/stun)
        private const int RedundantStatusPenalty      = 20; // penalty for applying a status the target already has
        private const int AttackStatBias              = 2;  // weight for preferring high-ATK targets

        private ((int x, int y) position, IUnit target)? FindBestAttackPosition(IUnit unit, List<IUnit> enemies, HashSet<(int, int)> reachable, IGameMap map)
        {
            // Include terrain defense and distance in the candidate tuple to allow richer tie-breaking.
            var validTargets = new List<(int x, int y, IUnit target, int score, int terrainDef, int distance)>();

            foreach (var position in reachable)
            {
                foreach (var enemy in enemies)
                {
                    int distance = map.GetDistance(position.Item1, position.Item2, enemy.Position.x, enemy.Position.y);

                    if (distance >= unit.EquippedWeapon.MinRange && distance <= unit.EquippedWeapon.MaxRange)
                    {
                        int score = ScoreAttackOption(unit, enemy);
                        int terrainDef = TerrainProperties.GetDefenseBonus(map.GetTile(position.Item1, position.Item2).Terrain);
                        score -= ScoreTerrainBonus(position.Item1, position.Item2, map);
                        validTargets.Add((position.Item1, position.Item2, enemy, score, terrainDef, distance));
                    }
                }
            }

            if (validTargets.Count == 0)
                return null;

            // Choose lowest score. Tie-breakers, in order:
            // 1) lower target.CurrentHP (prefer finishers)
            // 2) higher terrain defense (prefer defensive tiles)
            // 3) shorter movement distance from current unit position (prefer closer tiles)
            var selected = validTargets
                .OrderBy(t => t.score)
                .ThenBy(t => t.target.CurrentHP)
                .ThenByDescending(t => t.terrainDef)
                .ThenBy(t => t.distance)
                .First();
            return ((selected.x, selected.y), selected.target);
        }

        // Public helper for tests and future AI policies: given a set of reachable tiles, pick the best
        // attack position and target. This wraps the internal FindBestAttackPosition logic and is covered
        // by unit tests.
        public ((int x, int y) position, IUnit target)? GetBestAttackOptionForReachable(IUnit unit, List<IUnit> enemies, HashSet<(int, int)> reachable, IGameMap map)
        {
            return FindBestAttackPosition(unit, enemies, reachable, map);
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

        /// <summary>
        /// Moves an untransformed Laguz unit away from enemies. Prefers tiles that
        /// maximise distance from the nearest opponent while favouring defensive terrain.
        /// </summary>
        private void RetreatFromEnemies(IUnit unit, List<IUnit> allUnits, IGameMap map,
            IPathFinder pathFinder, out (int x, int y)? moveTarget)
        {
            moveTarget = null;
            var opponents = allUnits
                .Where(u => AreHostile(unit.Team, u.Team) && u.IsAlive)
                .ToList();

            if (opponents.Count == 0)
                return;

            var reachable = pathFinder.GetMovementRange(
                unit.Position.x, unit.Position.y,
                unit.CurrentStats.MOV, unit, map, allUnits);

            // Pick the tile that is farthest from the nearest enemy, with terrain defense as tiebreaker
            (int x, int y)? best = null;
            int bestMinDist = -1;
            int bestTerrainDef = -1;

            foreach (var tile in reachable)
            {
                int minDistToEnemy = opponents
                    .Min(e => map.GetDistance(tile.Item1, tile.Item2, e.Position.x, e.Position.y));
                int terrainDef = TerrainProperties.GetDefenseBonus(map.GetTile(tile.Item1, tile.Item2).Terrain);

                if (minDistToEnemy > bestMinDist ||
                    (minDistToEnemy == bestMinDist && terrainDef > bestTerrainDef))
                {
                    bestMinDist = minDistToEnemy;
                    bestTerrainDef = terrainDef;
                    best = tile;
                }
            }

            if (best.HasValue && best.Value != (unit.Position.x, unit.Position.y))
                moveTarget = best;
        }

        private void UpdateLastKnownPositions(IUnit unit, List<IUnit> allUnits, IFogOfWar fogOfWar)
        {
            foreach (var opponent in allUnits.Where(u => AreHostile(unit.Team, u.Team) && u.IsAlive))
            {
                if (fogOfWar.IsTileVisible(opponent.Position.x, opponent.Position.y, unit.Team))
                {
                    _lastKnownPositions[opponent.Id] = opponent.Position;
                }
            }
        }

        private (int x, int y)? GetClosestLastKnownPosition(IUnit unit, IGameMap map)
        {
            if (_lastKnownPositions.Count == 0)
                return null;

            return _lastKnownPositions.Values
                .OrderBy(pos => map.GetDistance(unit.Position.x, unit.Position.y, pos.x, pos.y))
                .Cast<(int x, int y)?>()
                .FirstOrDefault();
        }
    }
}
