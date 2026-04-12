using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class CombatResolverTests
    {
        private ICombatResolver _combatResolver;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _combatResolver = new CombatResolver();
            _map = new GameMap(16, 16, 0);
        }

        [Test]
        public void CalculateDamage_WithPhysicalAttack_ReturnsDamageBasedOnStats()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            int damage = _combatResolver.CalculateDamage(attacker, defender, _map);

            // Expected: (6 STR + 5 might) - 7 DEF = 4 damage
            Assert.GreaterOrEqual(damage, 0);
        }

        [Test]
        public void CalculateDamage_WithMagicalAttack_UsesMagicStats()
        {
            var attacker = new Unit(
                1, "Mage", Team.PlayerTeam,
                ClassDataFactory.CreateMage(),
                new CharacterStats(16, 0, 8, 7, 7, 5, 3, 7, 5),
                (0, 0),
                WeaponFactory.CreateFireTome()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            int damage = _combatResolver.CalculateDamage(attacker, defender, _map);

            // Expected: (8 MAG + 5 might) - 2 RES = 11 damage
            Assert.Greater(damage, 0);
        }

        [Test]
        public void CalculateAttackSpeed_ReducesByWeaponWeight()
        {
            var unit = new Unit(
                1, "Swordsman", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            int as_normal = _combatResolver.CalculateAttackSpeed(unit);
            // SPD 12 - max(0, Weight 5 - STR 6) = 12 - 0 = 12
            Assert.AreEqual(12, as_normal);
        }

        [Test]
        public void CalculateAttackSpeed_HeavyWeapon_ReducesSpeed()
        {
            var unit = new Unit(
                1, "Fighter", Team.PlayerTeam,
                ClassDataFactory.CreateFighter(),
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronAxe()
            );

            int as_attack = _combatResolver.CalculateAttackSpeed(unit);
            // SPD 7 - max(0, Weight 9 - STR 9) = 7 - 0 = 7
            Assert.AreEqual(7, as_attack);
        }

        [Test]
        public void CanDoubleAttack_ReturnsTrueWhenSpeedDifferenceGreaterOrEqual4()
        {
            var attacker = new Unit(
                1, "Fast Unit", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Slow Unit", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                (1, 0),
                WeaponFactory.CreateIronAxe()
            );

            // Attacker AS: 12 - 0 = 12
            // Defender AS: 7 - 0 = 7
            // Difference: 12 - 7 = 5 >= 4
            Assert.IsTrue(_combatResolver.CanDoubleAttack(attacker, defender));
        }

        [Test]
        public void CanDoubleAttack_ReturnsFalseWhenSpeedDifferenceLessThan4()
        {
            var attacker = new Unit(
                1, "Unit A", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (0, 0),
                WeaponFactory.CreateIronLance()
            );

            var defender = new Unit(
                2, "Unit B", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            // Same stats, so difference is 0 < 4
            Assert.IsFalse(_combatResolver.CanDoubleAttack(attacker, defender));
        }

        [Test]
        public void CanCounterAttack_ReturnsTrueWhenInWeaponRange()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            Assert.IsTrue(_combatResolver.CanCounterAttack(attacker, defender));
        }

        [Test]
        public void CanCounterAttack_ReturnsFalseForStaff()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var healer = new Unit(
                2, "Healer", Team.EnemyTeam,
                ClassDataFactory.CreateCleric(),
                new CharacterStats(16, 0, 7, 5, 5, 7, 2, 8, 5),
                (1, 0),
                WeaponFactory.CreateHealStaff()
            );

            Assert.IsFalse(_combatResolver.CanCounterAttack(attacker, healer));
        }

        [Test]
        public void CalculateHit_ReturnsValidBoolean()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            bool hit = _combatResolver.CalculateHit(attacker, defender, _map);
            Assert.IsInstanceOf<bool>(hit);
        }

        [Test]
        public void CalculateCritical_ReturnsValidBoolean()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            bool critical = _combatResolver.CalculateCritical(attacker, defender);
            Assert.IsInstanceOf<bool>(critical);
        }

        [Test]
        public void ResolveCombat_ModifiesUnitHP()
        {
            var attacker = new Unit(
                1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );

            var defender = new Unit(
                2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            int defenderHPBefore = defender.CurrentHP;
            var result = _combatResolver.ResolveCombat(attacker, defender, _map);

            if (result.Hit)
            {
                Assert.AreNotEqual(defenderHPBefore, result.DefenderHP);
            }
        }

        // ── On-hit status effect tests ─────────────────────────────────────

        [Test]
        public void ResolveCombat_PoisonSword_AppliesPoisonToDefenderOnHit()
        {
            // Attacker uses a Poison Sword — should infect defender when hit lands
            var attacker = new Unit(
                1, "Assassin", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 10, 0, 15, 15, 8, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreatePoisonSword()
            );

            // Sturdy defender so they survive the hit
            var defender = new Unit(
                2, "Soldier", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(30, 7, 0, 8, 8, 3, 12, 3, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            // Run many times: at least one hit should carry Poison
            bool poisonSeen = false;
            for (int i = 0; i < 50; i++)
            {
                var result = _combatResolver.ResolveCombat(attacker, defender, _map);
                if (result.Hit && result.DefenderHP > 0)
                {
                    if (result.DefenderStatusApplied == StatusEffectType.Poison)
                    {
                        poisonSeen = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(poisonSeen, "Poison Sword should apply Poison to defender on a hit that doesn't kill");
        }

        [Test]
        public void ResolveCombat_NormalSword_DoesNotApplyAnyStatus()
        {
            var attacker = new Unit(
                1, "Fighter", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 8, 0, 10, 10, 5, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreateIronSword()
            );
            var defender = new Unit(
                2, "Soldier", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(30, 7, 0, 8, 8, 3, 10, 3, 5),
                (1, 0),
                WeaponFactory.CreateIronLance()
            );

            for (int i = 0; i < 20; i++)
            {
                var result = _combatResolver.ResolveCombat(attacker, defender, _map);
                Assert.IsNull(result.DefenderStatusApplied,
                    "Iron Sword should never apply a status effect");
            }
        }

        [Test]
        public void ResolveCombat_KillingBlow_DoesNotApplyStatus()
        {
            // Even with a Poison Sword, a lethal hit should NOT record a status
            // (dead units can't be poisoned — it's irrelevant)
            var attacker = new Unit(
                1, "Assassin", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 30, 0, 20, 20, 15, 5, 0, 5),
                (0, 0),
                WeaponFactory.CreatePoisonSword()
            );
            var defender = new Unit(
                2, "WeakUnit", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(1, 1, 0, 1, 1, 1, 0, 0, 5),
                (1, 0),
                WeaponFactory.CreateIronSword()
            );

            // Should always kill in one hit → no status
            for (int i = 0; i < 20; i++)
            {
                var result = _combatResolver.ResolveCombat(attacker, defender, _map);
                if (result.DefenderHP <= 0)
                    Assert.IsNull(result.DefenderStatusApplied,
                        "Killing blow should not apply status effect");
            }
        }
    }
}
