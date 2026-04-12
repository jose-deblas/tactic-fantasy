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

        // ---- terrain-aware positioning ------------------------------------

        /// <summary>
        /// When two reachable tiles can both attack the same target, the AI
        /// should prefer the tile with the higher defense bonus (Forest > Plain).
        /// </summary>
        [Test]
        public void DecideAction_PrefersDefensiveTile_WhenMultipleAttackPositionsAvailable()
        {
            // Build a controlled 5x1 map:
            //  col0=Plain, col1=Forest, col2=Plain(enemy start), col3=Plain(player)
            var tiles = new ITile[5, 1];
            tiles[0, 0] = new Tile(0, 0, TerrainType.Plain);
            tiles[1, 0] = new Tile(1, 0, TerrainType.Forest);  // defense bonus
            tiles[2, 0] = new Tile(2, 0, TerrainType.Plain);   // enemy starts here
            tiles[3, 0] = new Tile(3, 0, TerrainType.Plain);
            tiles[4, 0] = new Tile(4, 0, TerrainType.Plain);
            var controlledMap = new GameMap(5, 1, tiles);

            // Enemy has MOV=3, so it can reach cols 0-4 from col2.
            // Player is at col3, so col2 and col1 are both within weapon range (1).
            // col1 is Forest (+1 def); col2 is Plain (+0 def) — AI should choose col1.
            IUnit enemy  = MakeEnemy(10, WeaponType.SWORD, pos: (2, 0));
            IUnit player = MakePlayer(1, WeaponType.LANCE,  hp: 20, pos: (3, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, controlledMap, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget, "AI should attack the player");
            Assert.IsNotNull(moveTarget, "AI should move to a specific tile");
            Assert.AreEqual(1, moveTarget.Value.x,
                "AI should prefer the Forest tile (col1) for its defense bonus");
        }

        /// <summary>
        /// Fort tiles (heal% + defense) should be preferred over Forest tiles.
        /// </summary>
        [Test]
        public void DecideAction_PrefersFortTile_OverForestTile()
        {
            var tiles = new ITile[5, 1];
            tiles[0, 0] = new Tile(0, 0, TerrainType.Plain);
            tiles[1, 0] = new Tile(1, 0, TerrainType.Forest); // def +1
            tiles[2, 0] = new Tile(2, 0, TerrainType.Fort);   // def +2  ← should be chosen
            tiles[3, 0] = new Tile(3, 0, TerrainType.Plain);  // player here
            tiles[4, 0] = new Tile(4, 0, TerrainType.Plain);
            var controlledMap = new GameMap(5, 1, tiles);

            // Enemy at col0, player at col3 — enemy can move to col1 or col2 to attack
            IUnit enemy  = MakeEnemy(10, WeaponType.SWORD, pos: (0, 0));
            IUnit player = MakePlayer(1, WeaponType.LANCE,  hp: 20, pos: (3, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, controlledMap, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.IsNotNull(moveTarget);
            Assert.AreEqual(2, moveTarget.Value.x,
                "AI should prefer the Fort tile (col2) over the Forest tile (col1)");
        }

        // ---- status-aware target selection ---------------------------------

        /// <summary>
        /// A sleeping target cannot counter-attack, so the AI should prefer
        /// attacking the sleeping unit over an equally healthy, non-sleeping one.
        /// </summary>
        [Test]
        public void DecideAction_PrefersAttacking_SleepingTarget()
        {
            // Two adjacent player units, same HP. One is asleep.
            IUnit enemy        = MakeEnemy(10, WeaponType.SWORD, pos: (3, 3));
            IUnit awakePlayer  = MakePlayer(1,  WeaponType.SWORD, hp: 20, pos: (2, 3));
            IUnit asleepPlayer = MakePlayer(2,  WeaponType.SWORD, hp: 20, pos: (4, 3));

            // Apply Sleep to the second player
            asleepPlayer.ApplyStatus(new StatusEffect(StatusEffectType.Sleep, 3));

            var allUnits = new List<IUnit> { enemy, awakePlayer, asleepPlayer };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget, "AI must select an attack target");
            Assert.AreEqual(asleepPlayer.Id, attackTarget.Id,
                "AI should prefer the sleeping target (no counter-attack risk)");
        }

        /// <summary>
        /// A stunned target cannot counter-attack, so it should also be preferred
        /// over a healthy, non-stunned one.
        /// </summary>
        [Test]
        public void DecideAction_PrefersAttacking_StunnedTarget()
        {
            IUnit enemy        = MakeEnemy(10, WeaponType.SWORD, pos: (3, 3));
            IUnit healthyPlayer = MakePlayer(1,  WeaponType.SWORD, hp: 20, pos: (2, 3));
            IUnit stunnedPlayer = MakePlayer(2,  WeaponType.SWORD, hp: 20, pos: (4, 3));

            stunnedPlayer.ApplyStatus(new StatusEffect(StatusEffectType.Stun, 1));

            var allUnits = new List<IUnit> { enemy, healthyPlayer, stunnedPlayer };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.AreEqual(stunnedPlayer.Id, attackTarget.Id,
                "AI should prefer the stunned target (no counter-attack risk)");
        }

        /// <summary>
        /// A target already poisoned should be deprioritized when the attacker
        /// uses a poison weapon (redundant application). A healthy target should
        /// be attacked instead.
        /// </summary>
        [Test]
        public void DecideAction_Deprioritsizes_AlreadyPoisonedTarget_WhenAttackerHasPoisonWeapon()
        {
            // Create a poison sword attacker
            var poisonWeapon = new Weapon("PoisonSword", WeaponType.SWORD, DamageType.Physical,
                5, 0, 80, 0, 1, 1, onHitStatus: StatusEffectType.Poison);
            var stats = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit enemy = new Unit(10, "PoisonEnemy", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), stats, (3, 3), poisonWeapon);

            IUnit healthyPlayer  = MakePlayer(1, WeaponType.SWORD, hp: 20, pos: (2, 3));
            IUnit poisonedPlayer = MakePlayer(2, WeaponType.SWORD, hp: 20, pos: (4, 3));
            poisonedPlayer.ApplyStatus(new StatusEffect(StatusEffectType.Poison, 3));

            var allUnits = new List<IUnit> { enemy, healthyPlayer, poisonedPlayer };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.AreEqual(healthyPlayer.Id, attackTarget.Id,
                "AI with poison weapon should attack the healthy target, not the already-poisoned one");
        }
    }
}

