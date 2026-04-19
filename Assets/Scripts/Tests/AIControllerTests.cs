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

        // ---- broken weapon handling ----------------------------------------

        /// <summary>
        /// An AI unit with a broken weapon cannot attack. DecideAction should
        /// set a moveTarget toward the nearest enemy but leave attackTarget null.
        /// </summary>
        [Test]
        public void DecideAction_BrokenWeaponUnit_DoesNotAttack_ButStillMoves()
        {
            // Create enemy with a 1-use weapon, then exhaust it
            var brokenWeapon = new Weapon("BrokenSword", WeaponType.SWORD, DamageType.Physical,
                5, 0, 80, 0, 1, 1, uses: 1);
            brokenWeapon.ConsumeUse(); // now IsBroken == true

            var stats = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit enemy = new Unit(10, "BrokenEnemy", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), stats, (0, 0), brokenWeapon);

            IUnit player = MakePlayer(1, WeaponType.SWORD, hp: 20, pos: (3, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNull(attackTarget, "Unit with broken weapon must not select an attack target");
            Assert.IsNotNull(moveTarget, "Unit with broken weapon should still advance toward the enemy");
        }

        /// <summary>
        /// An AI healer with a broken staff cannot heal. DecideAction should
        /// leave both moveTarget and attackTarget null (no-op).
        /// </summary>
        [Test]
        public void DecideAction_BrokenStaffHealer_DoesNothing()
        {
            var brokenStaff = new Weapon("BrokenStaff", WeaponType.STAFF, DamageType.Magical,
                5, 0, 80, 0, 1, 1, uses: 1);
            brokenStaff.ConsumeUse();

            var stats = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit healer = new Unit(20, "BrokenHealer", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), stats, (0, 0), brokenStaff);

            // Injured ally nearby
            var allyWeapon = new Weapon("Sword", WeaponType.SWORD, DamageType.Physical, 5, 0, 80, 0, 1, 1);
            var allyStats = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit injuredAlly = new Unit(21, "InjuredAlly", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), allyStats, (1, 0), allyWeapon);
            injuredAlly.TakeDamage(10);

            var allUnits = new List<IUnit> { healer, injuredAlly };

            _ai.DecideAction(healer, allUnits, _map, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNull(attackTarget, "Healer with broken staff must not select a heal target");
        }

        // ---- self-preservation: retreat to Fort when low HP -----------------

        /// <summary>
        /// When an enemy has very low HP (≤ 30% of max) AND a Fort tile is
        /// reachable within its movement range, the AI should retreat to the
        /// Fort instead of attacking, to benefit from its healing effect.
        /// </summary>
        [Test]
        public void DecideAction_LowHpUnit_RetreatsToFort_InsteadOfAttacking()
        {
            // Map: col0=Fort, col1=Plain(enemy, low HP), col2=Plain, col3=Plain(player)
            var tiles = new ITile[5, 1];
            tiles[0, 0] = new Tile(0, 0, TerrainType.Fort);   // healing retreat destination
            tiles[1, 0] = new Tile(1, 0, TerrainType.Plain);  // enemy starts here
            tiles[2, 0] = new Tile(2, 0, TerrainType.Plain);
            tiles[3, 0] = new Tile(3, 0, TerrainType.Plain);  // player here
            tiles[4, 0] = new Tile(4, 0, TerrainType.Plain);
            var controlledMap = new GameMap(5, 1, tiles);

            // Enemy at 5 HP out of 20 (25% — below 30% threshold)
            var weapon = new Weapon("Sword", WeaponType.SWORD, DamageType.Physical, 5, 0, 80, 0, 1, 1);
            var stats  = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit enemy  = new Unit(10, "LowHpEnemy", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), stats, (1, 0), weapon);
            enemy.TakeDamage(15); // leaves 5 HP (25%)

            IUnit player = MakePlayer(1, WeaponType.LANCE, hp: 20, pos: (3, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, controlledMap, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNull(attackTarget, "Low-HP unit should NOT attack when a Fort is reachable");
            Assert.IsNotNull(moveTarget, "Low-HP unit should move to the Fort tile");
            Assert.AreEqual(0, moveTarget.Value.x,
                "Low-HP unit should retreat to the Fort at col0");
        }

        /// <summary>
        /// When an enemy has low HP but NO Fort is reachable, it should still
        /// attack normally — retreat is only triggered when there's a safe tile.
        /// </summary>
        [Test]
        public void DecideAction_LowHpUnit_AttacksNormally_WhenNoFortReachable()
        {
            // Map: all Plain tiles — no Fort within reach
            IUnit enemy  = MakeEnemy(10, WeaponType.SWORD, hp: 20, pos: (3, 3));
            enemy.TakeDamage(15); // 5 HP (25%)

            IUnit player = MakePlayer(1, WeaponType.LANCE, hp: 20, pos: (4, 3));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget,
                "Low-HP unit with no Fort nearby should still attack");
        }

        /// <summary>
        /// A unit at exactly 31% HP (above threshold) should NOT retreat — it
        /// should fight normally.
        /// </summary>
        [Test]
        public void DecideAction_UnitAboveLowHpThreshold_DoesNotRetreat()
        {
            var tiles = new ITile[5, 1];
            tiles[0, 0] = new Tile(0, 0, TerrainType.Fort);
            tiles[1, 0] = new Tile(1, 0, TerrainType.Plain);  // enemy starts here
            tiles[2, 0] = new Tile(2, 0, TerrainType.Plain);
            tiles[3, 0] = new Tile(3, 0, TerrainType.Plain);  // player here
            tiles[4, 0] = new Tile(4, 0, TerrainType.Plain);
            var controlledMap = new GameMap(5, 1, tiles);

            var weapon = new Weapon("Sword", WeaponType.SWORD, DamageType.Physical, 5, 0, 80, 0, 1, 1);
            var stats  = new CharacterStats(20, 5, 0, 5, 5, 5, 5, 0, 5);
            IUnit enemy  = new Unit(10, "OkHpEnemy", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(), stats, (1, 0), weapon);
            enemy.TakeDamage(13); // leaves 7 HP (~35% — above 30% threshold)

            IUnit player = MakePlayer(1, WeaponType.LANCE, hp: 20, pos: (3, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, controlledMap, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget,
                "Unit above the low-HP threshold should attack, not retreat");
        }

        /// <summary>
        /// When two targets are otherwise equal (HP, triangle, status), the AI
        /// should prefer attacking the higher-ATK (more dangerous) target.
        /// </summary>
        [Test]
        public void DecideAction_PrefersHigherAttackStatTarget_WhenOtherFactorsEqual()
        {
            IUnit enemy = MakeEnemy(10, WeaponType.SWORD, pos: (3, 3));

            // Two adjacent players, same HP and weapon type
            IUnit weakPlayer  = MakePlayer(1, WeaponType.SWORD, hp: 20, pos: (2, 3));
            IUnit strongPlayer = MakePlayer(2, WeaponType.SWORD, hp: 20, pos: (4, 3));

            // Bump the STR stat of the strong player to make it higher threat
            strongPlayer.ApplyStatBoost(0, 5, 0, 0, 0, 0, 0, 0, 0);

            var allUnits = new List<IUnit> { enemy, weakPlayer, strongPlayer };

            _ai.DecideAction(enemy, allUnits, _map, _pathFinder,
                out _, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.AreEqual(strongPlayer.Id, attackTarget.Id,
                "AI should prefer the higher-ATK (more dangerous) target when other factors are equal");
        }

        /// <summary>
        /// When two reachable tiles give equal score and terrain defense, prefer
        /// the tile closer to the attacker to avoid unnecessary movement.
        /// </summary>
        [Test]
        public void DecideAction_PrefersCloserTile_WhenDefenseAndScoreEqual()
        {
            // Map: cols 0..4 plain, but we'll set col1 and col2 both Plain
            var tiles = new ITile[5, 1];
            for (int i = 0; i < 5; i++) tiles[i, 0] = new Tile(i, 0, TerrainType.Plain);
            var controlledMap = new GameMap(5, 1, tiles);

            // Enemy at col0, player at col4. Enemy can move to col1 or col2 to attack (both plain)
            IUnit enemy  = MakeEnemy(10, WeaponType.SWORD, pos: (0, 0));
            IUnit player = MakePlayer(1, WeaponType.LANCE,  hp: 20, pos: (4, 0));
            var allUnits = new List<IUnit> { enemy, player };

            _ai.DecideAction(enemy, allUnits, controlledMap, _pathFinder,
                out (int x, int y)? moveTarget, out IUnit attackTarget, out _);

            Assert.IsNotNull(attackTarget);
            Assert.IsNotNull(moveTarget);
            // Expect the AI to pick col1 (closer) over col2 when everything else equal
            Assert.AreEqual(1, moveTarget.Value.x,
                "AI should prefer the closer attack tile when defense and score are equal");
        }
    }
}

