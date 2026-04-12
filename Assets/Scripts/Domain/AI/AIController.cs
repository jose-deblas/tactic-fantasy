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
            var path = pathFinder.FindPath(unit.Position.x, unit.Position.y,
                closest.Position.x, closest.Position.y, unit.CurrentStats.MOV, unit, map);

            if (path.Count > 1)
                moveTarget = path[path.Count - 1];
        }

        private void DecideAttackAction(IUnit unit, List<IUnit> allUnits, IGameMap map, IPathFinder pathFinder,
            out (int x, int y)? moveTarget, out IUnit attackTarget)
        {
            moveTarget = null;
            attackTarget = null;

            var playerUnits = allUnits.Where(u => u.Team == Team.PlayerTeam && u.IsAlive).ToList();

            if (playerUnits.Count == 0)
                return;

            var reachable = pathFinder.GetMovementRange(unit.Position.x, unit.Position.y, unit.CurrentStats.MOV, unit, map);

            var bestAttackOption = FindBestAttackPosition(unit, playerUnits, reachable, map);

            if (bestAttackOption.HasValue)
            {
                var option = bestAttackOption.Value;
                moveTarget = option.position;
                attackTarget = option.target;
            }
            else
            {
                var closestEnemy = playerUnits
                    .OrderBy(e => map.GetDistance(unit.Position.x, unit.Position.y, e.Position.x, e.Position.y))
                    .First();

                var pathToEnemy = pathFinder.FindPath(unit.Position.x, unit.Position.y,
                    closestEnemy.Position.x, closestEnemy.Position.y, unit.CurrentStats.MOV, unit, map);

                if (pathToEnemy.Count > 1)
                {
                    moveTarget = pathToEnemy[pathToEnemy.Count - 1];
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
                var pathToAlly = pathFinder.FindPath(unit.Position.x, unit.Position.y,
                    targetAlly.Position.x, targetAlly.Position.y, unit.CurrentStats.MOV, unit, map);

                if (pathToAlly.Count > 1)
                {
                    moveTarget = pathToAlly[pathToAlly.Count - 1];
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
    }
}
