using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;
using System.Collections.Generic;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class GuardTests
    {
        private ICombatResolver _combatResolver;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _combatResolver = new CombatResolver();
            _map = new GameMap(8, 8, 0);
        }

        [Test]
        public void SetGuarding_True_SetsFlag()
        {
            var unit = new Unit(1, "Guard", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (0, 0), WeaponFactory.CreateIronLance());

            Assert.IsFalse(unit.IsGuarding);
            unit.SetGuarding(true);
            Assert.IsTrue(unit.IsGuarding);
        }

        [Test]
        public void SetGuarding_False_ClearsFlag()
        {
            var unit = new Unit(1, "Guard", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (0, 0), WeaponFactory.CreateIronLance());

            unit.SetGuarding(true);
            unit.SetGuarding(false);
            Assert.IsFalse(unit.IsGuarding);
        }

        [Test]
        public void CombatResolver_DefenderGuarding_ReducesDamageBy2()
        {
            var attacker = new Unit(1, "Attacker", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                (0, 0), WeaponFactory.CreateIronAxe());

            var defender = new Unit(2, "Defender", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0), WeaponFactory.CreateIronLance());

            int damageWithoutGuard = _combatResolver.CalculateDamage(attacker, defender, _map);

            defender.SetGuarding(true);
            int damageWithGuard = _combatResolver.CalculateDamage(attacker, defender, _map);

            Assert.AreEqual(2, damageWithoutGuard - damageWithGuard);
        }

        [Test]
        public void CombatResolver_DefenderGuarding_ReducesMagicDamageBy2()
        {
            var attacker = new Unit(1, "Mage", Team.EnemyTeam,
                ClassDataFactory.CreateMage(),
                new CharacterStats(16, 0, 8, 7, 7, 4, 3, 5, 5),
                (0, 0), WeaponFactory.CreateFireTome());

            var defender = new Unit(2, "Defender", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0), WeaponFactory.CreateIronLance());

            int damageWithoutGuard = _combatResolver.CalculateDamage(attacker, defender, _map);

            defender.SetGuarding(true);
            int damageWithGuard = _combatResolver.CalculateDamage(attacker, defender, _map);

            Assert.AreEqual(2, damageWithoutGuard - damageWithGuard);
        }

        [Test]
        public void Guard_DoesNotAffectAttackerStats()
        {
            var attacker = new Unit(1, "Attacker", Team.PlayerTeam,
                ClassDataFactory.CreateFighter(),
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                (0, 0), WeaponFactory.CreateIronAxe());

            var defender = new Unit(2, "Defender", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (1, 0), WeaponFactory.CreateIronLance());

            attacker.SetGuarding(true);
            int damage = _combatResolver.CalculateDamage(attacker, defender, _map);

            attacker.SetGuarding(false);
            int damageNoGuard = _combatResolver.CalculateDamage(attacker, defender, _map);

            Assert.AreEqual(damageNoGuard, damage);
        }

        [Test]
        public void TurnManager_AdvancePhase_ClearsPlayerGuardFlags()
        {
            var player = new Unit(1, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                (0, 0), WeaponFactory.CreateIronLance());

            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { player, enemy });

            player.SetGuarding(true);
            Assert.IsTrue(player.IsGuarding);

            // Advance through full cycle: Player → Ally → Enemy → Player
            tm.AdvancePhase(); // to AllyPhase
            Assert.IsTrue(player.IsGuarding, "Guard persists during ally phase");

            tm.AdvancePhase(); // to EnemyPhase
            Assert.IsTrue(player.IsGuarding, "Guard persists during enemy phase");

            tm.AdvancePhase(); // back to PlayerPhase — guard clears
            Assert.IsFalse(player.IsGuarding);
        }

        [Test]
        public void TurnManager_AdvancePhase_ClearsEnemyGuardFlags()
        {
            var player = new Unit(1, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                (0, 0), WeaponFactory.CreateIronLance());

            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { player, enemy });

            enemy.SetGuarding(true);
            Assert.IsTrue(enemy.IsGuarding);

            // Advance Player → Ally (enemy guard clears here since enemy phase starts next)
            tm.AdvancePhase(); // to AllyPhase
            Assert.IsTrue(enemy.IsGuarding, "Enemy guard persists during ally phase");

            // Advance Ally → Enemy (enemy guard clears at start of their phase)
            tm.AdvancePhase(); // to EnemyPhase
            Assert.IsFalse(enemy.IsGuarding);
        }
    }
}
