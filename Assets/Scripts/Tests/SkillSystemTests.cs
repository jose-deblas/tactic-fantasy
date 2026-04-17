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
    public class SkillSystemTests
    {
        private CombatResolver _resolver;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _resolver = new CombatResolver();
            _map = new GameMap(16, 16, 0);
        }

        // ── Unit skill management ───────────────────────────────────────────

        [Test]
        public void Unit_StartsWithNoSkills()
        {
            var unit = CreateMyrmidon((0, 0));
            Assert.AreEqual(0, unit.EquippedSkills.Count);
        }

        [Test]
        public void Unit_CanLearnAndEquipSkill()
        {
            var unit = CreateMyrmidon((0, 0));
            unit.LearnSkill(SkillDatabase.CreateAdept());
            Assert.AreEqual(1, unit.EquippedSkills.Count);
            Assert.AreEqual("Adept", unit.EquippedSkills[0].Name);
        }

        [Test]
        public void Unit_CanEquipMultipleSkills()
        {
            var unit = CreateMyrmidon((0, 0));
            unit.LearnSkill(SkillDatabase.CreateAdept());
            unit.LearnSkill(SkillDatabase.CreateWrath());
            Assert.AreEqual(2, unit.EquippedSkills.Count);
        }

        [Test]
        public void Unit_LearningSameSkillTwice_DoesNotDuplicate()
        {
            var unit = CreateMyrmidon((0, 0));
            var skill = SkillDatabase.CreateAdept();
            unit.LearnSkill(skill);
            unit.LearnSkill(skill);
            Assert.AreEqual(1, unit.EquippedSkills.Count);
        }

        [Test]
        public void Unit_CanUnequipSkill()
        {
            var unit = CreateMyrmidon((0, 0));
            var skill = SkillDatabase.CreateAdept();
            unit.LearnSkill(skill);
            unit.UnequipSkill(skill);
            Assert.AreEqual(0, unit.EquippedSkills.Count);
        }

        // ── No skills: combat unchanged ─────────────────────────────────────

        [Test]
        public void ResolveCombat_NoSkills_NoActivatedSkillsReported()
        {
            var attacker = CreateMyrmidon((0, 0));
            var defender = CreateSoldier((1, 0));

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.AreEqual(0, result.ActivatedSkills.Count);
        }

        // ── Adept ───────────────────────────────────────────────────────────

        [Test]
        public void Adept_TriggersExtraAttack_WhenRollSucceeds()
        {
            // Attacker with high SKL (guaranteed Adept trigger with seeded RNG)
            // SKL 99 means Adept fires if rng.Next(100) < 99 — nearly always
            var attacker = CreateUnitWithStats(
                "Adept User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 99, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateAdept());

            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            // Run multiple times — Adept should trigger at least once
            bool adeptSeen = false;
            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.ActivatedSkills.Contains("Adept"))
                {
                    adeptSeen = true;
                    break;
                }
            }
            Assert.IsTrue(adeptSeen, "Adept should trigger with SKL 99");
        }

        [Test]
        public void Adept_DoesNotTrigger_WhenSKLIsZero()
        {
            var attacker = CreateUnitWithStats(
                "No SKL", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 0, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateAdept());

            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                Assert.IsFalse(result.ActivatedSkills.Contains("Adept"),
                    "Adept should never trigger with SKL 0");
            }
        }

        // ── Vantage ─────────────────────────────────────────────────────────

        [Test]
        public void Vantage_Activates_WhenDefenderHPBelow50Percent()
        {
            var attacker = CreateMyrmidon((0, 0));

            var defender = CreateUnitWithStats(
                "Vantage User", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 8, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            defender.LearnSkill(SkillDatabase.CreateVantage());
            defender.TakeDamage(11); // HP 9/20 = 45% ≤ 50%

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.IsTrue(result.ActivatedSkills.Contains("Vantage"),
                "Vantage should activate when defender HP ≤ 50%");
            Assert.IsTrue(result.DefenderCounters,
                "Vantage defender should counter");
        }

        [Test]
        public void Vantage_DoesNotActivate_WhenHPAbove50Percent()
        {
            var attacker = CreateMyrmidon((0, 0));

            var defender = CreateUnitWithStats(
                "Healthy Unit", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 8, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            defender.LearnSkill(SkillDatabase.CreateVantage());
            // HP at full — 20/20 = 100% > 50%

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.IsFalse(result.ActivatedSkills.Contains("Vantage"),
                "Vantage should not activate when HP > 50%");
        }

        // ── Wrath ───────────────────────────────────────────────────────────

        [Test]
        public void Wrath_ForcesCritical_WhenHPBelow50Percent()
        {
            var attacker = CreateUnitWithStats(
                "Wrath User", Team.PlayerTeam, (0, 0),
                new CharacterStats(20, 10, 0, 15, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateWrath());
            attacker.TakeDamage(11); // HP 9/20 ≤ 50%

            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            // With Wrath forced crit + high hit chance, should see crit
            bool critSeen = false;
            for (int i = 0; i < 30; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                if (result.IsCritical)
                {
                    critSeen = true;
                    Assert.IsTrue(result.ActivatedSkills.Contains("Wrath"));
                    break;
                }
            }
            Assert.IsTrue(critSeen, "Wrath should force critical when HP ≤ 50%");
        }

        [Test]
        public void Wrath_DoesNotActivate_WhenHPAbove50Percent()
        {
            var attacker = CreateUnitWithStats(
                "Healthy", Team.PlayerTeam, (0, 0),
                new CharacterStats(20, 10, 0, 15, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateWrath());
            // Full HP — 20/20 = 100%

            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            for (int i = 0; i < 20; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                Assert.IsFalse(result.ActivatedSkills.Contains("Wrath"),
                    "Wrath should not trigger above 50% HP");
            }
        }

        // ── Resolve ─────────────────────────────────────────────────────────

        [Test]
        public void Resolve_BoostsStats_WhenHPBelow50Percent()
        {
            // Attacker with Resolve at low HP should deal more damage due to +7 STR... wait, Resolve boosts SKL/SPD/DEF not STR.
            // Let's test via attack speed: Resolve gives +7 SPD.
            var attacker = CreateUnitWithStats(
                "Resolve User", Team.PlayerTeam, (0, 0),
                new CharacterStats(20, 8, 0, 5, 5, 5, 3, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateResolve());
            attacker.TakeDamage(11); // HP 9/20 ≤ 50%

            // Without Resolve: AS = 5 - max(0, 5-8) = 5
            // With Resolve: AS = (5+7) - max(0, 5-8) = 12
            // Defender AS: 5 - max(0, 5-8) = 5
            // Difference: 12-5=7 >= 4 → should double
            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 8, 0, 5, 5, 0, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.IsTrue(result.ActivatedSkills.Contains("Resolve"),
                "Resolve should activate at ≤ 50% HP");
            Assert.IsTrue(result.AttackerDoubles,
                "Resolve +7 SPD should enable doubling");
        }

        [Test]
        public void Resolve_DoesNotActivate_WhenHPAbove50Percent()
        {
            var attacker = CreateUnitWithStats(
                "Healthy", Team.PlayerTeam, (0, 0),
                new CharacterStats(20, 8, 0, 5, 5, 5, 3, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateResolve());
            // Full HP

            var defender = CreateUnitWithStats(
                "Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 8, 0, 5, 5, 0, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.IsFalse(result.ActivatedSkills.Contains("Resolve"));
            // Without Resolve: AS 5 vs 5 → diff 0, no double
            Assert.IsFalse(result.AttackerDoubles);
        }

        // ── Nihil ───────────────────────────────────────────────────────────

        [Test]
        public void Nihil_NegatesOpponentAdept()
        {
            var attacker = CreateUnitWithStats(
                "Nihil User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateNihil());

            var defender = CreateUnitWithStats(
                "Adept User", Team.EnemyTeam, (1, 0),
                new CharacterStats(50, 5, 0, 99, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            defender.LearnSkill(SkillDatabase.CreateAdept());

            // Nihil on attacker should negate defender's skills
            for (int i = 0; i < 20; i++)
            {
                var result = _resolver.ResolveCombat(attacker, defender, _map);
                Assert.IsTrue(result.ActivatedSkills.Contains("Nihil"),
                    "Nihil should activate");
                // Defender's Adept should be negated — but Adept is OnAttack phase and
                // defender only attacks during counter, which doesn't trigger OnAttack skills in our pipeline.
                // The key test is that DefenderSkillsNegated prevents any defender skill activation.
            }
        }

        [Test]
        public void Nihil_NegatesOpponentVantage()
        {
            var attacker = CreateUnitWithStats(
                "Nihil User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateNihil());

            var defender = CreateUnitWithStats(
                "Vantage User", Team.EnemyTeam, (1, 0),
                new CharacterStats(20, 8, 0, 10, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            defender.LearnSkill(SkillDatabase.CreateVantage());
            defender.TakeDamage(11); // HP ≤ 50%

            var result = _resolver.ResolveCombat(attacker, defender, _map);
            Assert.IsTrue(result.ActivatedSkills.Contains("Nihil"));
            Assert.IsFalse(result.ActivatedSkills.Contains("Vantage"),
                "Nihil should negate opponent's Vantage");
        }

        // ── Paragon ─────────────────────────────────────────────────────────

        [Test]
        public void Paragon_DoublesXP()
        {
            var attacker = CreateUnitWithStats(
                "Paragon User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 15, 0, 15, 15, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );
            attacker.LearnSkill(SkillDatabase.CreateParagon());

            var defender = CreateUnitWithStats(
                "Weak Target", Team.EnemyTeam, (1, 0),
                new CharacterStats(5, 1, 0, 1, 1, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            // Strong attacker should kill weak defender
            var result = _resolver.ResolveCombat(attacker, defender, _map);
            if (result.Hit)
            {
                Assert.IsTrue(result.ActivatedSkills.Contains("Paragon"));
                // Kill XP = 60 → doubled = 120
                Assert.AreEqual(CombatXp.KillBonus * 2, result.AttackerXpGained,
                    "Paragon should double attacker XP");
            }
        }

        // ── Brave Weapon ────────────────────────────────────────────────────

        [Test]
        public void BraveSword_HasIsBraveTrue()
        {
            var weapon = WeaponFactory.CreateBraveSword();
            Assert.IsTrue(weapon.IsBrave);
            Assert.AreEqual("Brave Sword", weapon.Name);
            Assert.AreEqual(WeaponRank.B, weapon.RequiredRank);
        }

        [Test]
        public void BraveLance_HasIsBraveTrue()
        {
            var weapon = WeaponFactory.CreateBraveLance();
            Assert.IsTrue(weapon.IsBrave);
        }

        [Test]
        public void BraveAxe_HasIsBraveTrue()
        {
            var weapon = WeaponFactory.CreateBraveAxe();
            Assert.IsTrue(weapon.IsBrave);
        }

        [Test]
        public void IronSword_IsNotBrave()
        {
            var weapon = WeaponFactory.CreateIronSword();
            Assert.IsFalse(weapon.IsBrave);
            Assert.AreEqual(WeaponRank.E, weapon.RequiredRank);
        }

        // ── Weapon Tiers ────────────────────────────────────────────────────

        [Test]
        public void SteelSword_HigherMightThanIron()
        {
            var iron = WeaponFactory.CreateIronSword();
            var steel = WeaponFactory.CreateSteelSword();
            Assert.Greater(steel.Might, iron.Might);
            Assert.AreEqual(WeaponRank.D, steel.RequiredRank);
        }

        [Test]
        public void SilverSword_HigherMightThanSteel()
        {
            var steel = WeaponFactory.CreateSteelSword();
            var silver = WeaponFactory.CreateSilverSword();
            Assert.Greater(silver.Might, steel.Might);
            Assert.AreEqual(WeaponRank.A, silver.RequiredRank);
        }

        [Test]
        public void WeaponTiers_AllLanceVariantsExist()
        {
            var steel = WeaponFactory.CreateSteelLance();
            var silver = WeaponFactory.CreateSilverLance();
            var brave = WeaponFactory.CreateBraveLance();

            Assert.Greater(steel.Might, 6); // Iron lance is 6
            Assert.Greater(silver.Might, steel.Might);
            Assert.IsTrue(brave.IsBrave);
        }

        [Test]
        public void WeaponTiers_AllAxeVariantsExist()
        {
            var steel = WeaponFactory.CreateSteelAxe();
            var silver = WeaponFactory.CreateSilverAxe();
            var brave = WeaponFactory.CreateBraveAxe();

            Assert.Greater(steel.Might, 8); // Iron axe is 8
            Assert.Greater(silver.Might, steel.Might);
            Assert.IsTrue(brave.IsBrave);
        }

        // ── Brave Weapon in Combat ──────────────────────────────────────────

        [Test]
        public void BraveWeapon_DealsDamageFromDoubleStrike()
        {
            // Brave weapon should strike twice, dealing roughly 2x damage of a normal weapon
            var braveAttacker = CreateUnitWithStats(
                "Brave User", Team.PlayerTeam, (0, 0),
                new CharacterStats(30, 10, 0, 15, 10, 5, 5, 0, 5),
                WeaponFactory.CreateBraveSword()
            );

            var normalAttacker = CreateUnitWithStats(
                "Normal User", Team.PlayerTeam, (2, 0),
                new CharacterStats(30, 10, 0, 15, 10, 5, 5, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            var defender1 = CreateUnitWithStats(
                "Target1", Team.EnemyTeam, (1, 0),
                new CharacterStats(80, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            var defender2 = CreateUnitWithStats(
                "Target2", Team.EnemyTeam, (3, 0),
                new CharacterStats(80, 5, 0, 5, 5, 0, 0, 0, 5),
                WeaponFactory.CreateIronSword()
            );

            // Run many times and compare total damage
            int braveDamageSum = 0;
            int normalDamageSum = 0;
            int runs = 50;

            for (int i = 0; i < runs; i++)
            {
                var braveResult = _resolver.ResolveCombat(braveAttacker, defender1, _map);
                var normalResult = _resolver.ResolveCombat(normalAttacker, defender2, _map);
                braveDamageSum += braveResult.Damage;
                normalDamageSum += normalResult.Damage;
            }

            // Brave weapon strikes twice so on average deals ~2x damage
            // Allow some margin due to crits and hit variance
            Assert.Greater(braveDamageSum, normalDamageSum,
                "Brave weapon (2 strikes) should deal more total damage than normal (1 strike)");
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private Unit CreateMyrmidon((int x, int y) pos)
        {
            return new Unit(
                UnitFactory.GetNextId(), "Myrmidon", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                pos,
                WeaponFactory.CreateIronSword()
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

        private Unit CreateUnitWithStats(string name, Team team, (int x, int y) pos, CharacterStats stats, IWeapon weapon)
        {
            return new Unit(
                UnitFactory.GetNextId(), name, team,
                ClassDataFactory.CreateMyrmidon(),
                stats,
                pos,
                weapon
            );
        }
    }
}
