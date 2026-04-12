using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    /// <summary>
    /// TDD tests for CombatForecastService — pure domain service that calculates
    /// expected combat stats (damage, hit%, crit%, doubles, counter) before a battle occurs.
    /// </summary>
    [TestFixture]
    public class CombatForecastTests
    {
        private CombatForecastService _forecast;
        private IGameMap _map;

        [SetUp]
        public void Setup()
        {
            _forecast = new CombatForecastService(new CombatResolver());
            _map = new GameMap(16, 16, 0);
        }

        private IUnit MakeSword(int id, int x, int y, Team team = Team.PlayerTeam)
        {
            var unit = new Unit(
                id, "Warrior", team,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(hp: 20, str: 10, mag: 0, skl: 10, spd: 10, lck: 5, def: 5, res: 3, mov: 5),
                (x, y),
                WeaponFactory.CreateIronSword()
            );
            return unit;
        }

        private IUnit MakeLance(int id, int x, int y, Team team = Team.EnemyTeam)
        {
            var unit = new Unit(
                id, "Knight", team,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(hp: 24, str: 8, mag: 0, skl: 7, spd: 5, lck: 2, def: 10, res: 2, mov: 5),
                (x, y),
                WeaponFactory.CreateIronLance()
            );
            return unit;
        }

        // ─── Basic structure ──────────────────────────────────────────────────

        [Test]
        public void Calculate_ReturnsNonNullForecast()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsNotNull(result);
        }

        [Test]
        public void Calculate_AttackerDamage_IsNonNegative()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.GreaterOrEqual(result.AttackerDamage, 0);
        }

        [Test]
        public void Calculate_DefenderDamage_IsNonNegative()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.GreaterOrEqual(result.DefenderDamage, 0);
        }

        // ─── Hit rate clamping ────────────────────────────────────────────────

        [Test]
        public void Calculate_AttackerHitRate_IsClamped0To100()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsTrue(result.AttackerHitRate >= 0 && result.AttackerHitRate <= 100,
                $"AttackerHitRate out of range: {result.AttackerHitRate}");
        }

        [Test]
        public void Calculate_DefenderHitRate_IsClamped0To100()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsTrue(result.DefenderHitRate >= 0 && result.DefenderHitRate <= 100,
                $"DefenderHitRate out of range: {result.DefenderHitRate}");
        }

        // ─── Crit rate clamping ───────────────────────────────────────────────

        [Test]
        public void Calculate_AttackerCritRate_IsClamped0To100()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsTrue(result.AttackerCritRate >= 0 && result.AttackerCritRate <= 100);
        }

        // ─── Double attack flag ───────────────────────────────────────────────

        [Test]
        public void Calculate_AttackerDoubles_WhenSpeedAdvantage4Plus()
        {
            // attacker SPD=10, defender SPD=5 => AS diff = 5 >= 4 => doubles
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0); // Knight has SPD 5
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsTrue(result.AttackerDoubles);
        }

        [Test]
        public void Calculate_DefenderDoesNotDouble_WhenSlower()
        {
            var attacker = MakeSword(1, 0, 0); // SPD 10
            var defender = MakeLance(2, 1, 0); // SPD 5
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsFalse(result.DefenderDoubles);
        }

        // ─── Counter attack flag ──────────────────────────────────────────────

        [Test]
        public void Calculate_DefenderCanCounter_WhenInRange()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsTrue(result.DefenderCanCounter);
        }

        [Test]
        public void Calculate_DefenderCannotCounter_WhenOutOfRange()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 5, 0); // distance 5, sword range=1
            var result = _forecast.Calculate(attacker, defender, _map);
            Assert.IsFalse(result.DefenderCanCounter);
        }

        // ─── Weapon triangle reflected in damage ─────────────────────────────

        [Test]
        public void Calculate_SwordVsAxe_AttackerHasHigherHitThanSwordVsSword()
        {
            // Sword beats Axe → higher hit rate expected
            var attacker = MakeSword(1, 0, 0);
            var axeEnemy = new Unit(
                2, "Brigand", Team.EnemyTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(20, 10, 0, 10, 10, 5, 5, 3, 5),
                (1, 0),
                WeaponFactory.CreateIronAxe()
            );
            var lanceEnemy = MakeLance(3, 1, 0);

            var vsAxe = _forecast.Calculate(attacker, axeEnemy, _map);
            var vsLance = _forecast.Calculate(attacker, lanceEnemy, _map);

            // Sword beats Axe: should have better hit than Sword vs Lance (disadvantage)
            Assert.Greater(vsAxe.AttackerHitRate, vsLance.AttackerHitRate,
                "Sword vs Axe (advantage) should have higher hit rate than Sword vs Lance (disadvantage)");
        }

        // ─── Display formatting ───────────────────────────────────────────────

        [Test]
        public void CombatForecast_FormatSummary_ContainsKeyStats()
        {
            var attacker = MakeSword(1, 0, 0);
            var defender = MakeLance(2, 1, 0);
            var result = _forecast.Calculate(attacker, defender, _map);
            string summary = result.FormatSummary();

            StringAssert.Contains("DMG", summary);
            StringAssert.Contains("HIT", summary);
            StringAssert.Contains("CRIT", summary);
        }
    }
}
