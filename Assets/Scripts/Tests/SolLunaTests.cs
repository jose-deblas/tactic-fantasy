using System;
using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Skills;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for Sol and Luna skills (Phase 1B of Radiant Dawn skill plan).
    ///
    ///   Sol  (OnDamageDealt, SKL/2% chance): heals attacker for damage dealt on a hit.
    ///   Luna (OnAttack,      SKL/2% chance): ignores half of defender DEF/RES on this strike.
    /// </summary>
    [TestFixture]
    public class SolLunaTests
    {
        private CombatResolver _resolver;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _resolver = new CombatResolver();
            _map = new GameMap(16, 16, 0);
        }

        // ── Sol: basic properties ────────────────────────────────────────────

        [Test]
        public void Sol_ActivationPhase_IsOnDamageDealt()
        {
            var sol = SkillDatabase.CreateSol();
            Assert.AreEqual(SkillActivationPhase.OnDamageDealt, sol.ActivationPhase);
        }

        [Test]
        public void Sol_Name_IsSol()
        {
            var sol = SkillDatabase.CreateSol();
            Assert.AreEqual("Sol", sol.Name);
        }

        // ── Sol: CanActivate ─────────────────────────────────────────────────

        [Test]
        public void Sol_CanActivate_ReturnsFalse_WhenSKLIsZero()
        {
            var sol = SkillDatabase.CreateSol();
            var owner = CreateUnitWithStats("SKL0", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 0, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            var opp = CreateUnitWithStats("Opp", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword());

            // SKL 0 → activation chance = 0% → never fires
            var rng = new Random(42);
            bool anyActivation = false;
            for (int i = 0; i < 100; i++)
                if (sol.CanActivate(owner, opp, rng))
                    anyActivation = true;

            Assert.IsFalse(anyActivation);
        }

        [Test]
        public void Sol_CanActivate_ReturnsTrueEventually_WhenSKLIsHigh()
        {
            var sol = SkillDatabase.CreateSol();
            var owner = CreateUnitWithStats("HighSKL", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 99, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            var opp = CreateUnitWithStats("Opp", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword());

            // SKL 99 → activation chance ≈ 49% → fires many times in 100 rolls
            var rng = new Random(0);
            bool fired = false;
            for (int i = 0; i < 100; i++)
                if (sol.CanActivate(owner, opp, rng)) { fired = true; break; }

            Assert.IsTrue(fired, "Sol should activate with high SKL");
        }

        // ── Sol: heal mechanic in combat ─────────────────────────────────────

        [Test]
        public void Sol_WhenActivated_ReportsHealInResult()
        {
            // Attacker starts at half HP; Sol should restore some HP when it fires.
            // We use a fixed seed and high SKL to maximise chance of Sol firing.
            var attacker = CreateUnitWithStats("Sol User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 12, 0, 99, 12, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attacker.LearnSkill(SkillDatabase.CreateSol());
            attacker.TakeDamage(15); // HP = 15/30

            var defender = CreateUnitWithStats("Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(60, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword());

            bool solSeen = false;
            for (int i = 0; i < 60; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.Hit && result.ActivatedSkills.Contains("Sol"))
                {
                    // When Sol fires and a hit lands, attacker should have gained HP
                    Assert.Greater(result.AttackerHealedHP, 0,
                        "Sol should heal attacker when it activates on a hit");
                    solSeen = true;
                    break;
                }
            }
            Assert.IsTrue(solSeen, "Sol should trigger at least once with SKL 99 in 60 runs");
        }

        [Test]
        public void Sol_DoesNotHeal_WhenAttackMisses()
        {
            // Force a miss by giving the attacker 0 Hit on a no-hit weapon
            var attacker = CreateUnitWithStats("Sol Miss", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 12, 0, 99, 12, 5, 5, 0, 5),
                new Weapon("Awful Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 0, 0, 1, 1));
            attacker.LearnSkill(SkillDatabase.CreateSol());

            var defender = CreateUnitWithStats("Dodge", Team.EnemyTeam, (1, 0),
                new CharacterStats(60, 5, 0, 99, 99, 99, 30, 0, 5),
                WeaponFactory.CreateIronSword());

            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (!result.Hit)
                    Assert.AreEqual(0, result.AttackerHealedHP,
                        "Sol should not heal on a miss");
            }
        }

        // ── Luna: basic properties ───────────────────────────────────────────

        [Test]
        public void Luna_ActivationPhase_IsOnAttack()
        {
            var luna = SkillDatabase.CreateLuna();
            Assert.AreEqual(SkillActivationPhase.OnAttack, luna.ActivationPhase);
        }

        [Test]
        public void Luna_Name_IsLuna()
        {
            var luna = SkillDatabase.CreateLuna();
            Assert.AreEqual("Luna", luna.Name);
        }

        // ── Luna: CanActivate ────────────────────────────────────────────────

        [Test]
        public void Luna_CanActivate_ReturnsFalse_WhenSKLIsZero()
        {
            var luna = SkillDatabase.CreateLuna();
            var owner = CreateUnitWithStats("SKL0", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 0, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            var opp = CreateUnitWithStats("Opp", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword());

            var rng = new Random(42);
            bool anyActivation = false;
            for (int i = 0; i < 100; i++)
                if (luna.CanActivate(owner, opp, rng))
                    anyActivation = true;

            Assert.IsFalse(anyActivation);
        }

        // ── Luna: defense-halving in combat ──────────────────────────────────

        [Test]
        public void Luna_WhenActivated_DealsMoreDamageThanNormal()
        {
            // Luna halves defender DEF → attacker with same weapon vs high-DEF target
            // should deal more damage across many runs when Luna fires vs not.

            var attackerWithLuna = CreateUnitWithStats("Luna User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 99, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attackerWithLuna.LearnSkill(SkillDatabase.CreateLuna());

            var attackerNoSkill = CreateUnitWithStats("Normal", Team.PlayerTeam, (2, 0),
                new CharacterStats(30, 10, 0, 99, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());

            // High-DEF defender to make Luna's effect visible
            var defender1 = CreateUnitWithStats("Tank1", Team.EnemyTeam, (1, 0),
                new CharacterStats(80, 5, 0, 5, 5, 0, 20, 0, 5),
                WeaponFactory.CreateIronSword());
            var defender2 = CreateUnitWithStats("Tank2", Team.EnemyTeam, (3, 0),
                new CharacterStats(80, 5, 0, 5, 5, 0, 20, 0, 5),
                WeaponFactory.CreateIronSword());

            int lunaTotal = 0, normalTotal = 0;
            int runs = 80;
            for (int i = 0; i < runs; i++)
            {
                lunaTotal += _resolver.ResolveCombat(attackerWithLuna, defender1, _map).Damage;
                normalTotal += _resolver.ResolveCombat(attackerNoSkill, defender2, _map).Damage;
            }

            Assert.Greater(lunaTotal, normalTotal,
                "Luna user should deal more total damage than a user without Luna against high-DEF target");
        }

        [Test]
        public void Luna_ReportsActivation_InActivatedSkills()
        {
            var attacker = CreateUnitWithStats("Luna User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 99, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attacker.LearnSkill(SkillDatabase.CreateLuna());

            var defender = CreateUnitWithStats("Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(60, 5, 0, 5, 5, 0, 5, 0, 5),
                WeaponFactory.CreateIronSword());

            bool lunaSeen = false;
            for (int i = 0; i < 60; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.ActivatedSkills.Contains("Luna")) { lunaSeen = true; break; }
            }
            Assert.IsTrue(lunaSeen, "Luna should appear in ActivatedSkills when triggered");
        }

        // ── Nihil negates Sol and Luna ────────────────────────────────────────

        [Test]
        public void Nihil_NegatesSol_OnDefender()
        {
            var attacker = CreateUnitWithStats("Nihil User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attacker.LearnSkill(SkillDatabase.CreateNihil());

            var defender = CreateUnitWithStats("Sol User", Team.EnemyTeam, (1, 0),
                new CharacterStats(30, 8, 0, 99, 8, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            defender.LearnSkill(SkillDatabase.CreateSol());
            defender.TakeDamage(15); // low HP to make Sol more likely

            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                Assert.IsFalse(result.ActivatedSkills.Contains("Sol"),
                    "Nihil should prevent defender's Sol from activating");
            }
        }

        [Test]
        public void Nihil_NegatesLuna_OnDefender()
        {
            var attacker = CreateUnitWithStats("Nihil User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attacker.LearnSkill(SkillDatabase.CreateNihil());

            var defender = CreateUnitWithStats("Luna User", Team.EnemyTeam, (1, 0),
                new CharacterStats(30, 8, 0, 99, 8, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            defender.LearnSkill(SkillDatabase.CreateLuna());

            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                Assert.IsFalse(result.ActivatedSkills.Contains("Luna"),
                    "Nihil should prevent defender's Luna from activating");
            }
        }

        // ── Sol + Wrath combo ────────────────────────────────────────────────

        [Test]
        public void SolPlusWrath_BothCanActivate_InSameCombat()
        {
            var attacker = CreateUnitWithStats("Combo", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 12, 0, 99, 12, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword());
            attacker.LearnSkill(SkillDatabase.CreateWrath());
            attacker.LearnSkill(SkillDatabase.CreateSol());
            attacker.TakeDamage(16); // ≤50% HP to trigger Wrath and Sol conditions

            var defender = CreateUnitWithStats("Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(80, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword());

            bool bothSeen = false;
            for (int i = 0; i < 80; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.ActivatedSkills.Contains("Wrath") && result.ActivatedSkills.Contains("Sol"))
                {
                    bothSeen = true;
                    break;
                }
            }
            Assert.IsTrue(bothSeen, "Wrath and Sol should both be able to activate in the same combat");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Unit CreateUnitWithStats(string name, Team team, (int x, int y) pos, CharacterStats stats, IWeapon weapon)
        {
            return new Unit(
                UnitFactory.GetNextId(), name, team,
                ClassDataFactory.CreateMyrmidon(),
                stats, pos, weapon);
        }
    }
}
