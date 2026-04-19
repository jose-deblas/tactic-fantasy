using NUnit.Framework;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class BexpDistributorTests
    {
        private BexpDistributor _distributor;

        [SetUp]
        public void Setup()
        {
            _distributor = new BexpDistributor();
        }

        private Unit CreateUnit(CharacterStats? stats = null, int level = 1)
        {
            var unit = new Unit(1, "TestUnit", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                stats ?? ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword(),
                levelOverride: level);
            return unit;
        }

        [Test]
        public void Allocate_GrantsOneLevelPerFiftyBexp()
        {
            var unit = CreateUnit();
            int pool = 50;
            int startLevel = unit.Level;

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(1, levels);
            Assert.AreEqual(startLevel + 1, unit.Level);
            Assert.AreEqual(0, pool);
        }

        [Test]
        public void Allocate_GrantsMultipleLevels()
        {
            var unit = CreateUnit();
            int pool = 150;

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(3, levels);
            Assert.AreEqual(0, pool);
        }

        [Test]
        public void Allocate_LeftoverBexpRemainsInPool()
        {
            var unit = CreateUnit();
            int pool = 75; // 50 used, 25 leftover

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(1, levels);
            Assert.AreEqual(25, pool);
        }

        [Test]
        public void Allocate_InsufficientBexp_NoLevelUp()
        {
            var unit = CreateUnit();
            int pool = 49;

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(0, levels);
            Assert.AreEqual(49, pool);
        }

        [Test]
        public void Allocate_StopsAtMaxLevel()
        {
            var unit = CreateUnit(level: Unit.MaxLevel - 1);
            int pool = 500; // enough for 10 levels, but only 1 available

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(1, levels);
            Assert.AreEqual(Unit.MaxLevel, unit.Level);
            Assert.AreEqual(450, pool); // only 50 consumed
        }

        [Test]
        public void Allocate_AtMaxLevel_NoLevelUp()
        {
            var unit = CreateUnit(level: Unit.MaxLevel);
            int pool = 100;

            int levels = _distributor.Allocate(unit, ref pool);

            Assert.AreEqual(0, levels);
            Assert.AreEqual(100, pool);
        }

        [Test]
        public void GainLevelBexp_GrantsExactlyThreeStatPoints()
        {
            // Myrmidon growths: HP=55, STR=35, MAG=5, SKL=50, SPD=55, LCK=30, DEF=20, RES=10
            var unit = CreateUnit();
            var statsBefore = unit.CurrentStats;

            unit.GainLevelBexp();

            var statsAfter = unit.CurrentStats;
            int totalGain =
                (statsAfter.HP  - statsBefore.HP) +
                (statsAfter.STR - statsBefore.STR) +
                (statsAfter.MAG - statsBefore.MAG) +
                (statsAfter.SKL - statsBefore.SKL) +
                (statsAfter.SPD - statsBefore.SPD) +
                (statsAfter.LCK - statsBefore.LCK) +
                (statsAfter.DEF - statsBefore.DEF) +
                (statsAfter.RES - statsBefore.RES);

            Assert.AreEqual(3, totalGain, "BEXP level-up must grant exactly 3 stat points");
        }

        [Test]
        public void GainLevelBexp_ChoosesHighestGrowthStats()
        {
            // Myrmidon growths: HP=55, STR=35, MAG=5, SKL=50, SPD=55, LCK=30, DEF=20, RES=10
            // Top 3 by growth rate: SPD(55), HP(55), SKL(50) — tie-break by index: HP(0) before SPD(4)
            var unit = CreateUnit();
            var statsBefore = unit.CurrentStats;

            unit.GainLevelBexp();

            var statsAfter = unit.CurrentStats;
            Assert.AreEqual(1, statsAfter.HP  - statsBefore.HP,  "HP should gain +1 (growth 55, index 0)");
            Assert.AreEqual(1, statsAfter.SPD - statsBefore.SPD, "SPD should gain +1 (growth 55, index 4)");
            Assert.AreEqual(1, statsAfter.SKL - statsBefore.SKL, "SKL should gain +1 (growth 50)");
            Assert.AreEqual(0, statsAfter.STR - statsBefore.STR, "STR should not gain (growth 35, not top 3)");
        }

        [Test]
        public void GainLevelBexp_SkipsCappedStats()
        {
            // Set HP at cap so it gets skipped
            var myrmidon = ClassDataFactory.CreateMyrmidon();
            var caps = myrmidon.CapStats;
            // HP at cap, rest normal. Myrmidon HP cap is 30
            var stats = new CharacterStats(caps.HP, 6, 0, 11, 12, 5, 5, 0, 5);
            var unit = CreateUnit(stats);
            var statsBefore = unit.CurrentStats;

            unit.GainLevelBexp();

            var statsAfter = unit.CurrentStats;
            // HP is capped so should not grow. Top 3 non-capped: SPD(55), SKL(50), STR(35)
            Assert.AreEqual(0, statsAfter.HP  - statsBefore.HP,  "HP at cap should be skipped");
            Assert.AreEqual(1, statsAfter.SPD - statsBefore.SPD, "SPD should gain +1");
            Assert.AreEqual(1, statsAfter.SKL - statsBefore.SKL, "SKL should gain +1");
            Assert.AreEqual(1, statsAfter.STR - statsBefore.STR, "STR should gain +1 (next highest after HP skipped)");
        }

        [Test]
        public void GainLevelBexp_FewerThanThreeUncapped_GrantsAsManyAsPossible()
        {
            var myrmidon = ClassDataFactory.CreateMyrmidon();
            var caps = myrmidon.CapStats;
            // Set all stats at cap except STR and DEF
            var stats = new CharacterStats(
                caps.HP, caps.STR - 1, caps.MAG, caps.SKL, caps.SPD, caps.LCK, caps.DEF - 1, caps.RES, 5);
            var unit = CreateUnit(stats);
            var statsBefore = unit.CurrentStats;

            unit.GainLevelBexp();

            var statsAfter = unit.CurrentStats;
            int totalGain =
                (statsAfter.HP  - statsBefore.HP) +
                (statsAfter.STR - statsBefore.STR) +
                (statsAfter.MAG - statsBefore.MAG) +
                (statsAfter.SKL - statsBefore.SKL) +
                (statsAfter.SPD - statsBefore.SPD) +
                (statsAfter.LCK - statsBefore.LCK) +
                (statsAfter.DEF - statsBefore.DEF) +
                (statsAfter.RES - statsBefore.RES);

            Assert.AreEqual(2, totalGain, "Only 2 stats uncapped, so only 2 should grow");
        }

        [Test]
        public void GainLevelBexp_IsDeterministic()
        {
            var stats = ClassDataFactory.CreateMyrmidon().BaseStats;
            var unit1 = CreateUnit(stats);
            var unit2 = CreateUnit(stats);

            unit1.GainLevelBexp();
            unit2.GainLevelBexp();

            Assert.AreEqual(unit1.CurrentStats.HP,  unit2.CurrentStats.HP);
            Assert.AreEqual(unit1.CurrentStats.STR, unit2.CurrentStats.STR);
            Assert.AreEqual(unit1.CurrentStats.MAG, unit2.CurrentStats.MAG);
            Assert.AreEqual(unit1.CurrentStats.SKL, unit2.CurrentStats.SKL);
            Assert.AreEqual(unit1.CurrentStats.SPD, unit2.CurrentStats.SPD);
            Assert.AreEqual(unit1.CurrentStats.LCK, unit2.CurrentStats.LCK);
            Assert.AreEqual(unit1.CurrentStats.DEF, unit2.CurrentStats.DEF);
            Assert.AreEqual(unit1.CurrentStats.RES, unit2.CurrentStats.RES);
        }

        [Test]
        public void GainLevelBexp_MaxHPUpdatesWithHPGain()
        {
            var unit = CreateUnit();
            int maxHpBefore = unit.MaxHP;

            unit.GainLevelBexp();

            // Myrmidon HP growth is 55 (top 3), so HP should gain +1
            Assert.AreEqual(maxHpBefore + 1, unit.MaxHP);
        }

        [Test]
        public void CostForLevels_ReturnsCorrectAmount()
        {
            Assert.AreEqual(50, BexpDistributor.CostForLevels(1));
            Assert.AreEqual(250, BexpDistributor.CostForLevels(5));
            Assert.AreEqual(0, BexpDistributor.CostForLevels(0));
        }
    }
}
