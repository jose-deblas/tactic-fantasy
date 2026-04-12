using NUnit.Framework;
using System.Collections.Generic;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// Tests for AIController target-selection logic.
    /// </summary>
    [TestFixture]
    public class AIControllerTests
    {
        private IAIController _ai;
        private ICombatResolver _combatResolver;
        private IGameMap _map;
        private IPathFinder _pathFinder;

        [SetUp]
        public void Setup()
        {
            _combatResolver = new CombatResolver();
            _ai = new AIController(_combatResolver);
            _map = new GameMap(8, 8, 0);
            _pathFinder = new PathFinder();
        }

        // ---- helpers -------------------------------------------------------

        private IUnit MakeEnemy(int id, WeaponType wt, int hp = 20, (int x, int y) pos = default)
        {
            var weapon = new Weapon($"W{id}", wt, DamageType.Physical, 5, 0, 80, 0, 1, 1);
            var stats = new CharacterStats(hp, 5, 0, 5, 5, 5, 5, 0, 5);
            return new Unit(id, $"Enemy{id}", Team.EnemyTeam, ClassDataFactory.CreateMyrmidon(), stats, pos, weapon);
        }

        private IUnit MakePlayer(int id, WeaponType wt, int hp = 20, (int x, int y) pos = default)
        {
            var weapon = new Weapon($"W{id}", wt, DamageType.Physical, 5, 0, 80, 0, 1, 1);
            var stats = new CharacterStats(hp, 5, 0, 5, 5, 5, 5, 0, 5);
            return new Unit(id, $"Player{id}", Team.PlayerTeam, ClassDataFactory.CreateMyrmidon(), stats, pos, weapon);
        }

        // ---- weapon-triangle target selection ------------------------------

        /// <summary>
        /// An enemy wielding a Sword should prefer to attack the Axe-user
        /// (triangle advantage) over an equally-healthy Lance-user.
        /// </summary>
        [Test]
        public void DecideAction_SwordEnemy_PrefersAxeTargetOverLanceTarget()
        {
            // Axe user adjacent left, Lance user adjacent right — same HP
            IUnit swordEnemy = MakeEnemy(10, WeaponType.SWORD, pos: (3, 3));
            IUnit axePlayer  = MakePlayer(1, WeaponType.AXE,   hp: 20, pos: (2, 3));
            IUnit lancePlayer= MakePlayer(2, WeaponType.LANCE,  hp: 20, pos: (4, 3));

            var allUnits = new List<IUnit> { swordEnemy, axePlayer, lancePlayer };

            _ai.DecideAction(swordEnemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget, "AI should choose an attack target");
            Assert.AreEqual(axePlayer.Id, attackTarget.Id,
                "AI with Sword should target Axe wielder (triangle advantage)");
        }

        /// <summary>
        /// An enemy wielding an Axe should avoid attacking a Sword-user
        /// (triangle disadvantage) if a neutral target is available.
        /// </summary>
        [Test]
        public void DecideAction_AxeEnemy_AvoidsDisadvantageTarget_WhenNeutralExists()
        {
            IUnit axeEnemy    = MakeEnemy(10, WeaponType.AXE, pos: (3, 3));
            IUnit swordPlayer = MakePlayer(1, WeaponType.SWORD, hp: 20, pos: (2, 3)); // disadvantage
            IUnit bowPlayer   = MakePlayer(2, WeaponType.BOW,   hp: 20, pos: (4, 3)); // neutral

            var allUnits = new List<IUnit> { axeEnemy, swordPlayer, bowPlayer };

            _ai.DecideAction(axeEnemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget, "AI should still attack someone");
            Assert.AreEqual(bowPlayer.Id, attackTarget.Id,
                "AI with Axe should avoid Sword target (disadvantage) and prefer neutral BOW target");
        }

        /// <summary>
        /// When a triangle-favored enemy has very high HP and a disadvantaged enemy is
        /// near-dead, the AI should still prefer the near-kill (finisher priority).
        /// </summary>
        [Test]
        public void DecideAction_PrefersFinalBlow_OverTriangleAdvantageWhenHpIsVeryLow()
        {
            IUnit swordEnemy  = MakeEnemy(10, WeaponType.SWORD, pos: (3, 3));
            IUnit axePlayer   = MakePlayer(1, WeaponType.AXE,   hp: 20, pos: (2, 3)); // advantage target, full HP
            IUnit lancePlayer = MakePlayer(2, WeaponType.LANCE,  hp: 1,  pos: (4, 3)); // disadvantage but nearly dead

            var allUnits = new List<IUnit> { swordEnemy, axePlayer, lancePlayer };

            _ai.DecideAction(swordEnemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget, "AI should always pick someone");
            Assert.AreEqual(lancePlayer.Id, attackTarget.Id,
                "Finishing a near-dead unit should outweigh weapon triangle advantage");
        }

        // ---- regression: existing behavior still works --------------------

        [Test]
        public void DecideAction_WithNoPlayerUnits_SetsNoTarget()
        {
            IUnit enemy = MakeEnemy(10, WeaponType.SWORD, pos: (0, 0));
            var allUnits = new List<IUnit> { enemy };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out var move, out var attack, out _);

            Assert.IsNull(move);
            Assert.IsNull(attack);
        }

        [Test]
        public void DecideAction_WithSingleTarget_SelectsIt()
        {
            IUnit enemy  = MakeEnemy(10, WeaponType.SWORD, pos: (0, 0));
            IUnit player = MakePlayer(1, WeaponType.LANCE,  hp: 15, pos: (1, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.AreEqual(player.Id, attackTarget.Id);
        }
    }
}
