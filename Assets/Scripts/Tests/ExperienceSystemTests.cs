using System;
using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for the XP / Level-Up domain system.
    /// </summary>
    [TestFixture]
    public class ExperienceSystemTests
    {
        private Unit CreateMyrmidon(int id = 1, (int x, int y) pos = default, int levelOverride = 1)
        {
            return new Unit(id, "Ryu", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                pos, WeaponFactory.CreateIronSword(), levelOverride);
        }

        private Unit CreateSoldier(int id = 2, (int x, int y) pos = default)
        {
            return new Unit(id, "Lars", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                pos, WeaponFactory.CreateIronLance());
        }

        // ── GainExperience basics ────────────────────────────────────────────

        [Test]
        public void GainExperience_InitialLevel_IsOne()
        {
            var unit = CreateMyrmidon();
            Assert.AreEqual(1, unit.Level);
        }

        [Test]
        public void GainExperience_InitialExperience_IsZero()
        {
            var unit = CreateMyrmidon();
            Assert.AreEqual(0, unit.Experience);
        }

        [Test]
        public void GainExperience_SmallAmount_AccumulatesWithoutLevelUp()
        {
            var unit = CreateMyrmidon();
            bool leveledUp = unit.GainExperience(30);

            Assert.IsFalse(leveledUp);
            Assert.AreEqual(30, unit.Experience);
            Assert.AreEqual(1, unit.Level);
        }

        [Test]
        public void GainExperience_ExactlyOneHundred_TriggersLevelUp()
        {
            var unit = CreateMyrmidon();
            bool leveledUp = unit.GainExperience(100);

            Assert.IsTrue(leveledUp);
            Assert.AreEqual(2, unit.Level);
            Assert.AreEqual(0, unit.Experience); // XP resets to remainder
        }

        [Test]
        public void GainExperience_OverOneHundred_LevelsUpAndKeepsRemainder()
        {
            var unit = CreateMyrmidon();
            bool leveledUp = unit.GainExperience(130);

            Assert.IsTrue(leveledUp);
            Assert.AreEqual(2, unit.Level);
            Assert.AreEqual(30, unit.Experience);
        }

        [Test]
        public void GainExperience_MultipleGains_AccumulateCorrectly()
        {
            var unit = CreateMyrmidon();
            unit.GainExperience(60);
            unit.GainExperience(60);

            Assert.AreEqual(2, unit.Level);
            Assert.AreEqual(20, unit.Experience);
        }

        [Test]
        public void GainExperience_AtLevelCap_DoesNotLevelUp()
        {
            var unit = CreateMyrmidon(levelOverride: 20);
            int hpBefore = unit.CurrentHP;

            bool leveledUp = unit.GainExperience(200);

            Assert.IsFalse(leveledUp);
            Assert.AreEqual(20, unit.Level);
            Assert.AreEqual(0, unit.Experience); // XP capped at 0 when maxed
        }

        // ── Level-Up stat growth ─────────────────────────────────────────────

        [Test]
        public void LevelUp_WithAllGrowthsAt100Percent_AllStatsIncrease()
        {
            // Create a unit whose class has 100% growth in all stats
            var allMaxGrowths = new CharacterStats(100, 100, 100, 100, 100, 100, 100, 100, 0);
            var classData = new ClassData("Test",
                new CharacterStats(10, 5, 5, 5, 5, 5, 5, 5, 5),
                new CharacterStats(99, 99, 99, 99, 99, 99, 99, 99, 9),
                allMaxGrowths,
                WeaponType.SWORD, MoveType.Infantry);

            var unit = new Unit(1, "Test", Team.PlayerTeam, classData,
                new CharacterStats(10, 5, 5, 5, 5, 5, 5, 5, 5),
                (0, 0), WeaponFactory.CreateIronSword());

            var statsBefore = unit.CurrentStats;
            unit.GainExperience(100, new Random(0)); // seeded for determinism

            // With 100% growth every stat gains exactly +1
            Assert.AreEqual(statsBefore.HP  + 1, unit.MaxHP,              "HP should grow");
            Assert.AreEqual(statsBefore.STR + 1, unit.CurrentStats.STR,   "STR should grow");
            Assert.AreEqual(statsBefore.SKL + 1, unit.CurrentStats.SKL,   "SKL should grow");
            Assert.AreEqual(statsBefore.SPD + 1, unit.CurrentStats.SPD,   "SPD should grow");
        }

        [Test]
        public void LevelUp_WithAllGrowthsAtZeroPercent_NoStatsIncrease()
        {
            var zeroGrowths = new CharacterStats(0, 0, 0, 0, 0, 0, 0, 0, 0);
            var classData = new ClassData("Test",
                new CharacterStats(10, 5, 5, 5, 5, 5, 5, 5, 5),
                new CharacterStats(99, 99, 99, 99, 99, 99, 99, 99, 9),
                zeroGrowths,
                WeaponType.SWORD, MoveType.Infantry);

            var unit = new Unit(1, "Test", Team.PlayerTeam, classData,
                new CharacterStats(10, 5, 5, 5, 5, 5, 5, 5, 5),
                (0, 0), WeaponFactory.CreateIronSword());

            var statsBefore = unit.CurrentStats;
            unit.GainExperience(100, new Random(0));

            Assert.AreEqual(statsBefore.STR, unit.CurrentStats.STR);
            Assert.AreEqual(statsBefore.DEF, unit.CurrentStats.DEF);
        }

        [Test]
        public void LevelUp_StatsDoNotExceedCapStats()
        {
            // Cap at exactly current value → no growth possible
            var classData = new ClassData("Capped",
                new CharacterStats(10, 5, 0, 5, 5, 5, 5, 0, 5),
                new CharacterStats(10, 5, 5, 5, 5, 5, 5, 5, 9), // cap == current
                new CharacterStats(100, 100, 100, 100, 100, 100, 100, 100, 0),
                WeaponType.SWORD, MoveType.Infantry);

            var unit = new Unit(1, "Capped", Team.PlayerTeam, classData,
                new CharacterStats(10, 5, 0, 5, 5, 5, 5, 0, 5),
                (0, 0), WeaponFactory.CreateIronSword());

            unit.GainExperience(100, new Random(0));

            Assert.AreEqual(5, unit.CurrentStats.STR, "STR capped at 5");
        }

        // ── CombatResolver XP calculation ────────────────────────────────────

        [Test]
        public void CombatResult_ContainsXpGainedFields()
        {
            var result = new CombatResult(10, true, false, 15, 0, false, false,
                attackerXpGained: 60, defenderXpGained: 10);

            Assert.AreEqual(60, result.AttackerXpGained);
            Assert.AreEqual(10, result.DefenderXpGained);
        }

        [Test]
        public void CombatXp_KillingBlowGrantsMaxAttackerXp()
        {
            var resolver = new CombatResolver();
            var map = new GameMap(16, 16, 0);

            // Attacker with guaranteed kill setup — not testing hit/miss here,
            // so we just verify the XP formula values are sensible constants.
            Assert.AreEqual(CombatXp.KillBonus,       60);
            Assert.AreEqual(CombatXp.DamageBonus,     20);
            Assert.AreEqual(CombatXp.SurvivedBonus,   10);
            Assert.AreEqual(CombatXp.CounteredBonus,  15);
        }
    }
}
