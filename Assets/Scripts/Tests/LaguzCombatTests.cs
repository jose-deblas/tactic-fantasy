using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class LaguzCombatTests
    {
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _map = new GameMap(16, 16);
        }

        private Unit CreateLaguzUnit(int id, string name, Team team, LaguzClassData classData, (int, int) pos, int initialGauge = 0)
        {
            var weapon = LaguzWeaponFactory.CreateForRace(classData.Race);
            var unit = new Unit(id, name, team, classData, classData.UntransformedStats, pos, weapon);
            unit.InitLaguzGauge(classData.GaugeFillRate, classData.GaugeDrainRate, initialGauge);
            return unit;
        }

        // ── Laguz in combat ─────────────────────────────────────────────────

        [Test]
        public void TransformedLaguz_DealsMoreDamage_ThanUntransformed()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var resolver = new CombatResolver();

            var laguzUntransformed = CreateLaguzUnit(1, "Cat1", Team.PlayerTeam, catClass, (0, 0));
            var laguzTransformed = CreateLaguzUnit(2, "Cat2", Team.PlayerTeam, catClass, (2, 0));
            laguzTransformed.LaguzGauge.FillToMax();
            laguzTransformed.Transform();

            var enemy = new Unit(3, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                (1, 0), WeaponFactory.CreateIronLance());

            int dmgUntransformed = resolver.CalculateDamage(laguzUntransformed, enemy, _map);
            int dmgTransformed = resolver.CalculateDamage(laguzTransformed, enemy, _map);

            Assert.Greater(dmgTransformed, dmgUntransformed);
        }

        [Test]
        public void LaguzWeapon_IsStrikeType_NoTriangleAdvantage()
        {
            var weapon = LaguzWeaponFactory.CreateClaw();
            var sword = WeaponFactory.CreateIronSword();

            var (dmgBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(weapon, sword);

            Assert.AreEqual(0, dmgBonus);
            Assert.AreEqual(0, hitBonus);
        }

        // ── TurnManager gauge ticking ───────────────────────────────────────

        [Test]
        public void TurnManager_TicksPlayerLaguzGauges_OnPhaseAdvance()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var laguz = CreateLaguzUnit(1, "Cat", Team.PlayerTeam, catClass, (0, 0));
            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { laguz, enemy });

            Assert.AreEqual(0, laguz.LaguzGauge.Current);

            // Advance from Player → Enemy phase should tick player Laguz gauges
            tm.AdvancePhase();

            Assert.AreEqual(catClass.GaugeFillRate, laguz.LaguzGauge.Current);
        }

        [Test]
        public void TurnManager_TicksEnemyLaguzGauges_OnPhaseAdvance()
        {
            var tigerClass = LaguzClassDataFactory.CreateTiger();
            var laguzEnemy = CreateLaguzUnit(1, "Tiger", Team.EnemyTeam, tigerClass, (5, 5));
            var player = new Unit(2, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { player, laguzEnemy });

            // Player phase → Enemy phase
            tm.AdvancePhase();
            Assert.AreEqual(0, laguzEnemy.LaguzGauge.Current);

            // Enemy phase → Player phase (should tick enemy Laguz gauges)
            tm.AdvancePhase();
            Assert.AreEqual(tigerClass.GaugeFillRate, laguzEnemy.LaguzGauge.Current);
        }

        [Test]
        public void TurnManager_AutoTransforms_WhenGaugeFills()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            // Cat fills at 8/turn; start at 24 so next tick fills to 30+
            var laguz = CreateLaguzUnit(1, "Cat", Team.PlayerTeam, catClass, (0, 0), initialGauge: 24);
            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { laguz, enemy });

            Assert.IsFalse(laguz.IsTransformed);

            tm.AdvancePhase(); // Player → Enemy (ticks player gauges)

            Assert.IsTrue(laguz.IsTransformed);
            Assert.AreEqual(catClass.TransformedStats.STR, laguz.CurrentStats.STR);
        }

        [Test]
        public void TurnManager_AutoReverts_WhenGaugeDrains()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var laguz = CreateLaguzUnit(1, "Cat", Team.PlayerTeam, catClass, (0, 0), initialGauge: 30);
            laguz.Transform();
            // Gauge at 30, drain at 2/turn → set gauge low to trigger revert
            // Override gauge to 1 by draining
            for (int i = 0; i < 14; i++)
                laguz.LaguzGauge.Tick(isTransformed: true);
            // Now gauge is at 2

            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                ClassDataFactory.CreateFighter().BaseStats,
                (5, 5), WeaponFactory.CreateIronAxe());

            var tm = new TurnManager();
            tm.Initialize(new List<IUnit> { laguz, enemy });

            Assert.IsTrue(laguz.IsTransformed);
            Assert.AreEqual(2, laguz.LaguzGauge.Current);

            tm.AdvancePhase(); // Player → Enemy (ticks player gauges, gauge hits 0)

            Assert.IsFalse(laguz.IsTransformed);
            Assert.AreEqual(catClass.UntransformedStats.STR, laguz.CurrentStats.STR);
        }

        // ── Laguz items ─────────────────────────────────────────────────────

        [Test]
        public void LaguzStone_FillsGaugeToMax()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(1, "Cat", Team.PlayerTeam, catClass, (0, 0));

            Assert.AreEqual(0, unit.LaguzGauge.Current);

            var stone = LaguzItemFactory.CreateLaguzStone();
            stone.Use(unit);

            Assert.AreEqual(TransformGauge.MaxGauge, unit.LaguzGauge.Current);
            Assert.AreEqual(0, stone.CurrentUses);
        }

        [Test]
        public void OliviGrass_AddsGaugePoints()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(1, "Cat", Team.PlayerTeam, catClass, (0, 0), initialGauge: 10);

            var grass = LaguzItemFactory.CreateOliviGrass();
            grass.Use(unit);

            Assert.AreEqual(25, unit.LaguzGauge.Current);
            Assert.AreEqual(2, grass.CurrentUses);
        }

        [Test]
        public void LaguzStone_DoesNothing_OnBeorc()
        {
            var beorc = new Unit(1, "Beorc", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            var stone = LaguzItemFactory.CreateLaguzStone();
            stone.Use(beorc);

            Assert.IsNull(beorc.LaguzGauge);
        }

        // ── Dragon breath is magical damage ─────────────────────────────────

        [Test]
        public void DragonBreath_UsesMagicalDamage()
        {
            var breath = LaguzWeaponFactory.CreateBreath();
            Assert.AreEqual(DamageType.Magical, breath.DamageType);
        }

        [Test]
        public void Dragon_Transformed_HasHighDEF()
        {
            var dragonClass = LaguzClassDataFactory.CreateRedDragon();
            var dragon = CreateLaguzUnit(1, "Dragon", Team.PlayerTeam, dragonClass, (0, 0));

            dragon.Transform();

            Assert.AreEqual(18, dragon.CurrentStats.DEF);
            Assert.AreEqual(12, dragon.CurrentStats.RES);
        }

        // ── AI behavior ─────────────────────────────────────────────────────

        [Test]
        public void AI_UntransformedLaguz_RetreatsFromEnemies()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var laguz = CreateLaguzUnit(1, "Cat", Team.EnemyTeam, catClass, (3, 3));

            var player = new Unit(2, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (4, 3), WeaponFactory.CreateIronSword());

            var allUnits = new List<IUnit> { laguz, player };
            var ai = new AIController(new CombatResolver());
            var pathFinder = new PathFinder();

            ai.DecideAction(laguz, allUnits, _map, pathFinder,
                out var moveTarget, out var attackTarget, out var isHeal);

            // Untransformed Laguz should retreat (move away), not attack
            Assert.IsNull(attackTarget);
            Assert.IsFalse(isHeal);
            if (moveTarget.HasValue)
            {
                int distBefore = _map.GetDistance(3, 3, player.Position.x, player.Position.y);
                int distAfter = _map.GetDistance(moveTarget.Value.x, moveTarget.Value.y, player.Position.x, player.Position.y);
                Assert.GreaterOrEqual(distAfter, distBefore);
            }
        }

        [Test]
        public void AI_TransformedLaguz_AttacksEnemy()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var laguz = CreateLaguzUnit(1, "Cat", Team.EnemyTeam, catClass, (3, 3), initialGauge: 30);
            laguz.Transform();

            var player = new Unit(2, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (4, 3), WeaponFactory.CreateIronSword());

            var allUnits = new List<IUnit> { laguz, player };
            var ai = new AIController(new CombatResolver());
            var pathFinder = new PathFinder();

            ai.DecideAction(laguz, allUnits, _map, pathFinder,
                out var moveTarget, out var attackTarget, out var isHeal);

            // Transformed Laguz should attack
            Assert.IsNotNull(attackTarget);
            Assert.AreEqual(player, attackTarget);
        }
    }
}
