using System;
using System.Collections.Generic;
using System.Linq;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Skills;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Combat
{
    public interface ICombatResolver
    {
        CombatResult ResolveCombat(IUnit attacker, IUnit defender, IGameMap map);
        int CalculateDamage(IUnit attacker, IUnit defender, IGameMap map);
        bool CalculateHit(IUnit attacker, IUnit defender, IGameMap map);
        bool CalculateCritical(IUnit attacker, IUnit defender);
        int CalculateAttackSpeed(IUnit unit);
        bool CanDoubleAttack(IUnit attacker, IUnit defender);
        bool CanCounterAttack(IUnit attacker, IUnit defender);
    }

    public class CombatResolver : ICombatResolver
    {
        private readonly Random _rng = new Random();

        public CombatResult ResolveCombat(IUnit attacker, IUnit defender, IGameMap map)
        {
            var ctx = new CombatContext(attacker, defender, map, _rng);

            // Phase 1: Apply passive skills (Resolve, Paragon)
            ApplyPassiveSkills(ctx, attacker, isAttacker: true);
            ApplyPassiveSkills(ctx, defender, isAttacker: false);

            // Phase 2: Apply pre-combat skills (Nihil first, then Vantage)
            ApplyPreCombatSkills(ctx, attacker, isAttacker: true);
            ApplyPreCombatSkills(ctx, defender, isAttacker: false);

            int attackerHP = attacker.CurrentHP;
            int defenderHP = defender.CurrentHP;

            bool anyAttackerHit = false;
            bool anyAttackerCrit = false;
            int totalDamage = 0;
            bool defenderCounterLanded = false;
            int braveStrikes = attacker.EquippedWeapon.IsBrave ? 2 : 1;

            // Phase 3: Determine attack order
            bool defenderCounters = CanCounterAttack(attacker, defender);
            bool attackerDoubles = CanDoubleAttackWithStats(ctx.AttackerEffectiveStats, ctx.DefenderEffectiveStats, attacker, defender);

            if (ctx.VantageActivated && defenderCounters && defenderHP > 0)
            {
                // Vantage: defender strikes first
                var counterResult = ExecuteStrike(ctx, defender, attacker, defenderHP, attackerHP, isCounter: true);
                attackerHP = counterResult.targetHP;
                if (counterResult.hit) defenderCounterLanded = true;
                defenderCounters = false; // already countered
            }

            // Phase 4: Attacker strikes (Brave = 2x, normal = 1x)
            if (attackerHP > 0)
            {
                // Check OnAttack skills for attacker
                int extraAttacks = 0;
                if (!ctx.AttackerSkillsNegated)
                {
                    foreach (var skill in attacker.EquippedSkills.Where(s => s.ActivationPhase == SkillActivationPhase.OnAttack))
                    {
                        if (skill.CanActivate(attacker, defender, ctx.Rng))
                            skill.Apply(ctx);
                    }
                    extraAttacks = ctx.ExtraAttacks;
                    ctx.ExtraAttacks = 0; // consume
                }

                int totalStrikes = braveStrikes + extraAttacks;
                for (int i = 0; i < totalStrikes && defenderHP > 0 && attackerHP > 0; i++)
                {
                    var strikeResult = ExecuteStrike(ctx, attacker, defender, attackerHP, defenderHP, isCounter: false);
                    defenderHP = strikeResult.targetHP;
                    if (strikeResult.hit)
                    {
                        anyAttackerHit = true;
                        totalDamage += strikeResult.damage;
                        if (strikeResult.crit) anyAttackerCrit = true;
                    }
                }
            }

            // Phase 5: Defender counter (if not already done via Vantage)
            if (defenderCounters && defenderHP > 0 && attackerHP > 0)
            {
                var counterResult = ExecuteStrike(ctx, defender, attacker, defenderHP, attackerHP, isCounter: true);
                attackerHP = counterResult.targetHP;
                if (counterResult.hit) defenderCounterLanded = true;
            }

            // Phase 6: Follow-up attacks (doubles)
            if (attackerDoubles && defenderHP > 0 && attackerHP > 0)
            {
                int followUpStrikes = braveStrikes;
                for (int i = 0; i < followUpStrikes && defenderHP > 0 && attackerHP > 0; i++)
                {
                    var strikeResult = ExecuteStrike(ctx, attacker, defender, attackerHP, defenderHP, isCounter: false);
                    defenderHP = strikeResult.targetHP;
                    if (strikeResult.hit)
                    {
                        anyAttackerHit = true;
                        totalDamage += strikeResult.damage;
                        if (strikeResult.crit) anyAttackerCrit = true;
                    }
                }
            }

            // Phase 7: XP calculation
            bool defenderKilled = defenderHP <= 0;
            bool attackerKilled = attackerHP <= 0;

            int attackerXp = 0;
            int defenderXp = 0;

            if (anyAttackerHit)
            {
                attackerXp = defenderKilled ? CombatXp.KillBonus : CombatXp.DamageBonus;
            }

            if (!attackerKilled)
            {
                defenderXp += CombatXp.SurvivedBonus;
                if (defenderCounterLanded)
                    defenderXp += CombatXp.CounteredBonus;
            }

            // Paragon doubles XP
            if (ctx.HasParagon)
                attackerXp *= 2;

            // Phase 8: Weapon durability
            attacker.EquippedWeapon.ConsumeUse();
            if (defenderCounterLanded || ctx.VantageActivated)
                defender.EquippedWeapon.ConsumeUse();

            return new CombatResult(totalDamage, anyAttackerHit, anyAttackerCrit, attackerHP, defenderHP,
                attackerDoubles, defenderCounters || ctx.VantageActivated,
                attackerXpGained: attackerXp,
                defenderXpGained: defenderXp,
                defenderStatusApplied: ResolveOnHitStatus(attacker, anyAttackerHit, defenderHP),
                attackerStatusApplied: ResolveOnHitStatus(defender, defenderCounterLanded, attackerHP),
                activatedSkills: ctx.ActivatedSkills);
        }

        private (bool hit, bool crit, int damage, int targetHP) ExecuteStrike(
            CombatContext ctx, IUnit striker, IUnit target,
            int strikerHP, int targetHP, bool isCounter)
        {
            CharacterStats strikerStats = striker == ctx.Attacker ? ctx.AttackerEffectiveStats : ctx.DefenderEffectiveStats;
            CharacterStats targetStats = target == ctx.Attacker ? ctx.AttackerEffectiveStats : ctx.DefenderEffectiveStats;

            bool hit = CalculateHitWithStats(strikerStats, targetStats, striker, target, ctx.Map);
            bool crit = false;
            int damage = 0;

            if (hit)
            {
                crit = ctx.ForceCritical && striker == ctx.Attacker && !isCounter
                    ? true
                    : CalculateCriticalWithStats(strikerStats, striker, target);
                damage = CalculateDamageWithStats(strikerStats, targetStats, striker, target, ctx.Map);
                if (crit)
                    damage *= 3;
                targetHP -= damage;
            }

            return (hit, crit, damage, targetHP);
        }

        private void ApplyPassiveSkills(CombatContext ctx, IUnit unit, bool isAttacker)
        {
            bool negated = isAttacker ? ctx.AttackerSkillsNegated : ctx.DefenderSkillsNegated;
            if (negated) return;

            foreach (var skill in unit.EquippedSkills.Where(s => s.ActivationPhase == SkillActivationPhase.Passive))
            {
                if (skill.CanActivate(unit, isAttacker ? ctx.Defender : ctx.Attacker, ctx.Rng))
                {
                    // Resolve applies to the unit's own effective stats
                    if (!isAttacker)
                    {
                        // Temporarily swap so Apply modifies defender stats
                        var temp = ctx.AttackerEffectiveStats;
                        ctx.AttackerEffectiveStats = ctx.DefenderEffectiveStats;
                        skill.Apply(ctx);
                        ctx.DefenderEffectiveStats = ctx.AttackerEffectiveStats;
                        ctx.AttackerEffectiveStats = temp;
                    }
                    else
                    {
                        skill.Apply(ctx);
                    }
                }
            }
        }

        private void ApplyPreCombatSkills(CombatContext ctx, IUnit unit, bool isAttacker)
        {
            bool negated = isAttacker ? ctx.AttackerSkillsNegated : ctx.DefenderSkillsNegated;
            if (negated) return;

            foreach (var skill in unit.EquippedSkills.Where(s => s.ActivationPhase == SkillActivationPhase.PreCombat))
            {
                if (skill.CanActivate(unit, isAttacker ? ctx.Defender : ctx.Attacker, ctx.Rng))
                {
                    if (!isAttacker)
                    {
                        // Nihil on defender should negate attacker's skills, not defender's.
                        // Swap perspective so Apply targets the correct side.
                        bool prevAtk = ctx.AttackerSkillsNegated;
                        bool prevDef = ctx.DefenderSkillsNegated;
                        skill.Apply(ctx);
                        if (ctx.DefenderSkillsNegated != prevDef)
                        {
                            // Skill tried to negate "defender" but owner IS the defender — redirect to attacker
                            ctx.DefenderSkillsNegated = prevDef;
                            ctx.AttackerSkillsNegated = true;
                        }
                    }
                    else
                    {
                        skill.Apply(ctx);
                    }
                }
            }
        }

        // ── Public methods (unchanged signatures for backward compat) ────────

        public int CalculateDamage(IUnit attacker, IUnit defender, IGameMap map)
        {
            return CalculateDamageWithStats(attacker.CurrentStats, defender.CurrentStats, attacker, defender, map);
        }

        public bool CalculateHit(IUnit attacker, IUnit defender, IGameMap map)
        {
            return CalculateHitWithStats(attacker.CurrentStats, defender.CurrentStats, attacker, defender, map);
        }

        public bool CalculateCritical(IUnit attacker, IUnit defender)
        {
            return CalculateCriticalWithStats(attacker.CurrentStats, attacker, defender);
        }

        public int CalculateAttackSpeed(IUnit unit)
        {
            return CalculateAttackSpeedWithStats(unit.CurrentStats, unit);
        }

        public bool CanDoubleAttack(IUnit attacker, IUnit defender)
        {
            return CanDoubleAttackWithStats(attacker.CurrentStats, defender.CurrentStats, attacker, defender);
        }

        public bool CanCounterAttack(IUnit attacker, IUnit defender)
        {
            int distance = GetDistance(attacker, defender);
            return distance >= defender.EquippedWeapon.MinRange
                && distance <= defender.EquippedWeapon.MaxRange
                && defender.EquippedWeapon.Type != WeaponType.STAFF
                && defender.EquippedWeapon.Type != WeaponType.REFRESH;
        }

        // ── Stats-aware calculation methods ──────────────────────────────────

        private int CalculateDamageWithStats(CharacterStats attackerStats, CharacterStats defenderStats, IUnit attacker, IUnit defender, IGameMap map)
        {
            int attackPower = CalculateAttackPowerWithStats(attackerStats, attacker, defender, map);
            int defensePower = CalculateDefensePowerWithStats(defenderStats, attacker, defender, map);
            return Math.Max(0, attackPower - defensePower);
        }

        private bool CalculateHitWithStats(CharacterStats attackerStats, CharacterStats defenderStats, IUnit attacker, IUnit defender, IGameMap map)
        {
            int hitRate = CalculateHitRateWithStats(attackerStats, defenderStats, attacker, defender, map);
            hitRate = Math.Max(0, Math.Min(100, hitRate));

            int roll1 = _rng.Next(100);
            int roll2 = _rng.Next(100);
            int averageRoll = (roll1 + roll2) / 2;

            return averageRoll < hitRate;
        }

        private bool CalculateCriticalWithStats(CharacterStats attackerStats, IUnit attacker, IUnit defender)
        {
            int critRate = CalculateCritRateWithStats(attackerStats, attacker, defender);
            critRate = Math.Max(0, Math.Min(100, critRate));

            int roll = _rng.Next(100);
            return roll < critRate;
        }

        private int CalculateAttackSpeedWithStats(CharacterStats stats, IUnit unit)
        {
            int weight = unit.EquippedWeapon.Weight;
            return stats.SPD - Math.Max(0, weight - stats.STR);
        }

        private bool CanDoubleAttackWithStats(CharacterStats attackerStats, CharacterStats defenderStats, IUnit attacker, IUnit defender)
        {
            int attackerAS = CalculateAttackSpeedWithStats(attackerStats, attacker);
            int defenderAS = CalculateAttackSpeedWithStats(defenderStats, defender);
            return (attackerAS - defenderAS) >= 4;
        }

        private int CalculateAttackPowerWithStats(CharacterStats attackerStats, IUnit attacker, IUnit defender, IGameMap map)
        {
            int basePower = attacker.EquippedWeapon.DamageType == DamageType.Physical
                ? attackerStats.STR
                : attackerStats.MAG;

            int power = basePower + attacker.EquippedWeapon.Might;
            power += TerrainProperties.GetDefenseBonus(map.GetTile(attacker.Position.x, attacker.Position.y).Terrain);

            var (damageBonus, _) = WeaponTriangle.GetTriangleModifiers(attacker.EquippedWeapon, defender.EquippedWeapon);
            power += damageBonus;

            return power;
        }

        private int CalculateDefensePowerWithStats(CharacterStats defenderStats, IUnit attacker, IUnit defender, IGameMap map)
        {
            int defense = attacker.EquippedWeapon.DamageType == DamageType.Physical
                ? defenderStats.DEF
                : defenderStats.RES;

            defense += TerrainProperties.GetDefenseBonus(map.GetTile(defender.Position.x, defender.Position.y).Terrain);

            return defense;
        }

        private int CalculateHitRateWithStats(CharacterStats attackerStats, CharacterStats defenderStats, IUnit attacker, IUnit defender, IGameMap map)
        {
            int hit = (attackerStats.SKL * 2) + (attackerStats.LCK / 2) + attacker.EquippedWeapon.Hit;
            var (_, triangleHitBonus) = WeaponTriangle.GetTriangleModifiers(attacker.EquippedWeapon, defender.EquippedWeapon);
            hit += triangleHitBonus;

            int defenderAS = CalculateAttackSpeedWithStats(defenderStats, defender);
            int avoid = (defenderAS * 2) + defenderStats.LCK +
                        TerrainProperties.GetAvoidBonus(map.GetTile(defender.Position.x, defender.Position.y).Terrain);

            return hit - avoid;
        }

        private int CalculateCritRateWithStats(CharacterStats attackerStats, IUnit attacker, IUnit defender)
        {
            return (attackerStats.SKL / 2) + attacker.EquippedWeapon.Crit - defender.CurrentStats.LCK;
        }

        private int GetDistance(IUnit unit1, IUnit unit2)
        {
            int dx = Math.Abs(unit1.Position.x - unit2.Position.x);
            int dy = Math.Abs(unit1.Position.y - unit2.Position.y);
            return Math.Max(dx, dy);
        }

        private static StatusEffectType? ResolveOnHitStatus(IUnit striker, bool didHit, int targetRemainingHP)
        {
            if (!didHit) return null;
            if (targetRemainingHP <= 0) return null;
            if (striker.EquippedWeapon.OnHitStatus == null) return null;
            return striker.EquippedWeapon.OnHitStatus;
        }
    }
}
