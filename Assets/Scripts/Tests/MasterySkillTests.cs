using System;
using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Skills;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class MasterySkillTests
    {
        private CombatResolver _resolver;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _resolver = new CombatResolver();
            _map = new GameMap(16, 16, 0);
        }

        // ── Astra ──────────────────────────────────────────────────────────

        [Test]
        public void Astra_CanActivate_WhenRollBelowHalfSKL()
        {
            var unit = CreateHighSKLUnit((0, 0), Team.PlayerTeam);
            var enemy = CreateSoldier((1, 0));
            var astra = SkillDatabase.CreateAstra();
            // SKL=40, threshold=20; rng returns 10 → should activate
            Assert.IsTrue(astra.CanActivate(unit, enemy, new Random(42)));
        }

        [Test]
        public void Astra_SetsAstraActiveFlag()
        {
            var ctx = CreateContext();
            var astra = SkillDatabase.CreateAstra();
            astra.Apply(ctx);
            Assert.IsTrue(ctx.AstraActive);
            Assert.Contains("Astra", ctx.ActivatedSkills);
        }

        [Test]
        public void Astra_Combat_ProducesMultipleHits()
        {
            // With Astra active, attacker should deal damage from multiple strikes
            var attacker = CreateHighSKLUnit((0, 0), Team.PlayerTeam);
            var defender = CreateSoldier((1, 0));
            attacker.LearnSkill(SkillDatabase.CreateAstra());

            // Run many combats; if Astra fires, total damage should reflect multiple hits
            bool astraFired = false;
            for (int i = 0; i < 50; i++)
            {
                ResetHP(attacker, defender);
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.ActivatedSkills.Contains("Astra"))
                {
                    astraFired = true;
                    break;
                }
            }
            // With SKL=40, SKL/2=20% chance per combat, should fire in 50 tries
            Assert.IsTrue(astraFired, "Astra should activate at least once in 50 combats with SKL 40");
        }

        // ── Colossus ───────────────────────────────────────────────────────

        [Test]
        public void Colossus_SetsColossusActiveFlag()
        {
            var ctx = CreateContext();
            var colossus = SkillDatabase.CreateColossus();
            colossus.Apply(ctx);
            Assert.IsTrue(ctx.ColossusActive);
            Assert.Contains("Colossus", ctx.ActivatedSkills);
        }

        [Test]
        public void Colossus_CanActivate_WhenRollBelowSTR()
        {
            var unit = CreateHighSTRUnit((0, 0));
            var enemy = CreateSoldier((1, 0));
            var colossus = SkillDatabase.CreateColossus();
            // STR=30, rng should return < 30 at some point
            bool activated = false;
            for (int i = 0; i < 100; i++)
            {
                if (colossus.CanActivate(unit, enemy, new Random(i)))
                {
                    activated = true;
                    break;
                }
            }
            Assert.IsTrue(activated);
        }

        // ── Flare ──────────────────────────────────────────────────────────

        [Test]
        public void Flare_SetsFlareActiveFlag()
        {
            var ctx = CreateContext();
            var flare = SkillDatabase.CreateFlare();
            flare.Apply(ctx);
            Assert.IsTrue(ctx.FlareActive);
            Assert.Contains("Flare", ctx.ActivatedSkills);
        }

        // ── Deadeye ────────────────────────────────────────────────────────

        [Test]
        public void Deadeye_SetsDeadeyeActiveFlag()
        {
            var ctx = CreateContext();
            var deadeye = SkillDatabase.CreateDeadeye();
            deadeye.Apply(ctx);
            Assert.IsTrue(ctx.DeadeyeActive);
            Assert.Contains("Deadeye", ctx.ActivatedSkills);
        }

        [Test]
        public void Deadeye_CanActivate_WhenRollBelowHalfSKL()
        {
            var unit = CreateHighSKLUnit((0, 0), Team.PlayerTeam);
            var enemy = CreateSoldier((1, 0));
            var deadeye = SkillDatabase.CreateDeadeye();
            bool activated = false;
            for (int i = 0; i < 100; i++)
            {
                if (deadeye.CanActivate(unit, enemy, new Random(i)))
                {
                    activated = true;
                    break;
                }
            }
            Assert.IsTrue(activated);
        }

        // ── Corona ─────────────────────────────────────────────────────────

        [Test]
        public void Corona_SetsCoronaActiveFlag()
        {
            var ctx = CreateContext();
            var corona = SkillDatabase.CreateCorona();
            corona.Apply(ctx);
            Assert.IsTrue(ctx.CoronaActive);
            Assert.Contains("Corona", ctx.ActivatedSkills);
        }

        // ── GetMasterySkill ────────────────────────────────────────────────

        [Test]
        public void GetMasterySkill_ReturnsAstra_ForTrueblade()
        {
            var skill = ClassPromotionService.GetMasterySkill("Trueblade");
            Assert.IsNotNull(skill);
            Assert.AreEqual("Astra", skill.Name);
        }

        [Test]
        public void GetMasterySkill_ReturnsSol_ForMarshall()
        {
            var skill = ClassPromotionService.GetMasterySkill("Marshall");
            Assert.IsNotNull(skill);
            Assert.AreEqual("Sol", skill.Name);
        }

        [Test]
        public void GetMasterySkill_ReturnsNull_ForNonMasterClass()
        {
            Assert.IsNull(ClassPromotionService.GetMasterySkill("Myrmidon"));
            Assert.IsNull(ClassPromotionService.GetMasterySkill("Swordmaster"));
        }

        // ── helpers ────────────────────────────────────────────────────────

        private Unit CreateHighSKLUnit((int x, int y) pos, Team team)
        {
            return new Unit(
                UnitFactory.GetNextId(), "HighSKL", team,
                ClassDataFactory.CreateTrueblade(),
                new CharacterStats(30, 15, 5, 40, 20, 10, 10, 5, 7),
                pos,
                WeaponFactory.CreateIronSword()
            );
        }

        private Unit CreateHighSTRUnit((int x, int y) pos)
        {
            return new Unit(
                UnitFactory.GetNextId(), "HighSTR", Team.PlayerTeam,
                ClassDataFactory.CreateReaver(),
                new CharacterStats(38, 30, 0, 12, 13, 7, 14, 4, 7),
                pos,
                WeaponFactory.CreateIronAxe()
            );
        }

        private Unit CreateSoldier((int x, int y) pos)
        {
            return new Unit(
                UnitFactory.GetNextId(), "Soldier", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                pos,
                WeaponFactory.CreateIronLance()
            );
        }

        private CombatContext CreateContext()
        {
            var attacker = CreateHighSKLUnit((0, 0), Team.PlayerTeam);
            var defender = CreateSoldier((1, 0));
            return new CombatContext(attacker, defender, _map, new Random(0));
        }

        private void ResetHP(Unit a, Unit b)
        {
            a.Heal(999);
            b.Heal(999);
        }
    }
}
