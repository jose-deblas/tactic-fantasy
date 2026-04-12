using System;
using TacticFantasy.Domain.Map;
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
            bool attackerHits = CalculateHit(attacker, defender, map);
            bool attackerCrits = false;
            int damage = 0;

            if (attackerHits)
            {
                attackerCrits = CalculateCritical(attacker, defender);
                damage = CalculateDamage(attacker, defender, map);
                if (attackerCrits)
                    damage *= 3;
            }

            int defenderHP = defender.CurrentHP - (attackerHits ? damage : 0);
            int attackerHP = attacker.CurrentHP;

            bool attackerDoubles = CanDoubleAttack(attacker, defender);
            bool defenderCounters = attackerHits && CanCounterAttack(attacker, defender);

            if (defenderCounters && defenderHP > 0)
            {
                bool defenderHits = CalculateHit(defender, attacker, map);
                if (defenderHits)
                {
                    bool defenderCrits = CalculateCritical(defender, attacker);
                    int counterDamage = CalculateDamage(defender, attacker, map);
                    if (defenderCrits)
                        counterDamage *= 3;
                    attackerHP -= counterDamage;
                }
            }

            return new CombatResult(damage, attackerHits, attackerCrits, attackerHP, defenderHP, attackerDoubles, defenderCounters);
        }

        public int CalculateDamage(IUnit attacker, IUnit defender, IGameMap map)
        {
            int attackPower = CalculateAttackPower(attacker, defender, map);
            int defensePower = CalculateDefensePower(attacker, defender, map);
            return Math.Max(0, attackPower - defensePower);
        }

        public bool CalculateHit(IUnit attacker, IUnit defender, IGameMap map)
        {
            int hitRate = CalculateHitRate(attacker, defender, map);
            hitRate = Math.Max(0, Math.Min(100, hitRate));

            // True Hit: average of 2 random rolls
            int roll1 = _rng.Next(100);
            int roll2 = _rng.Next(100);
            int averageRoll = (roll1 + roll2) / 2;

            return averageRoll < hitRate;
        }

        public bool CalculateCritical(IUnit attacker, IUnit defender)
        {
            int critRate = CalculateCritRate(attacker, defender);
            critRate = Math.Max(0, Math.Min(100, critRate));

            int roll = _rng.Next(100);
            return roll < critRate;
        }

        public int CalculateAttackSpeed(IUnit unit)
        {
            int weight = unit.EquippedWeapon.Weight;
            int str = unit.CurrentStats.STR;
            return unit.CurrentStats.SPD - Math.Max(0, weight - str);
        }

        public bool CanDoubleAttack(IUnit attacker, IUnit defender)
        {
            int attackerAS = CalculateAttackSpeed(attacker);
            int defenderAS = CalculateAttackSpeed(defender);
            return (attackerAS - defenderAS) >= 4;
        }

        public bool CanCounterAttack(IUnit attacker, IUnit defender)
        {
            int distance = GetDistance(attacker, defender);
            return distance <= defender.EquippedWeapon.MaxRange && defender.EquippedWeapon.Type != WeaponType.STAFF;
        }

        private int CalculateAttackPower(IUnit attacker, IUnit defender, IGameMap map)
        {
            int basePower = attacker.EquippedWeapon.DamageType == DamageType.Physical
                ? attacker.CurrentStats.STR
                : attacker.CurrentStats.MAG;

            int power = basePower + attacker.EquippedWeapon.Might;
            power += TerrainProperties.GetDefenseBonus(map.GetTile(attacker.Position.x, attacker.Position.y).Terrain);

            var (damageBonus, _) = WeaponTriangle.GetTriangleModifiers(attacker.EquippedWeapon, defender.EquippedWeapon);
            power += damageBonus;

            return power;
        }

        private int CalculateDefensePower(IUnit attacker, IUnit defender, IGameMap map)
        {
            int defense = attacker.EquippedWeapon.DamageType == DamageType.Physical
                ? defender.CurrentStats.DEF
                : defender.CurrentStats.RES;

            defense += TerrainProperties.GetDefenseBonus(map.GetTile(defender.Position.x, defender.Position.y).Terrain);

            return defense;
        }

        private int CalculateHitRate(IUnit attacker, IUnit defender, IGameMap map)
        {
            int hit = (attacker.CurrentStats.SKL * 2) + (attacker.CurrentStats.LCK / 2) + attacker.EquippedWeapon.Hit;
            var (_, triangleHitBonus) = WeaponTriangle.GetTriangleModifiers(attacker.EquippedWeapon, defender.EquippedWeapon);
            hit += triangleHitBonus;

            int avoid = (CalculateAttackSpeed(defender) * 2) + defender.CurrentStats.LCK +
                        TerrainProperties.GetAvoidBonus(map.GetTile(defender.Position.x, defender.Position.y).Terrain);

            return hit - avoid;
        }

        private int CalculateCritRate(IUnit attacker, IUnit defender)
        {
            return (attacker.CurrentStats.SKL / 2) + attacker.EquippedWeapon.Crit - defender.CurrentStats.LCK;
        }

        private int GetDistance(IUnit unit1, IUnit unit2)
        {
            int dx = Math.Abs(unit1.Position.x - unit2.Position.x);
            int dy = Math.Abs(unit1.Position.y - unit2.Position.y);
            return Math.Max(dx, dy);
        }
    }
}
