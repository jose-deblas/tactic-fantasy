using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class WeatherEffectsTests
    {
        // ── WeatherEffects static methods ───────────────────────────────────

        [Test]
        public void Clear_NoHitModifier()
        {
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Clear, WeaponType.BOW));
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Clear, WeaponType.SWORD));
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Clear, WeaponType.FIRE));
        }

        [Test]
        public void Clear_NoDamageModifier()
        {
            Assert.AreEqual(0, WeatherEffects.GetDamageModifier(Weather.Clear, WeaponType.FIRE));
        }

        [Test]
        public void Clear_NoAvoidModifier()
        {
            Assert.AreEqual(0, WeatherEffects.GetAvoidModifier(Weather.Clear));
        }

        [Test]
        public void Rain_BowHitPenalty()
        {
            Assert.AreEqual(WeatherEffects.RAIN_BOW_HIT_PENALTY, WeatherEffects.GetHitModifier(Weather.Rain, WeaponType.BOW));
        }

        [Test]
        public void Rain_NonBowNoHitPenalty()
        {
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Rain, WeaponType.SWORD));
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Rain, WeaponType.LANCE));
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Rain, WeaponType.FIRE));
        }

        [Test]
        public void Rain_FireDamagePenalty()
        {
            Assert.AreEqual(WeatherEffects.RAIN_FIRE_DAMAGE_PENALTY, WeatherEffects.GetDamageModifier(Weather.Rain, WeaponType.FIRE));
        }

        [Test]
        public void Rain_NonFireNoDamagePenalty()
        {
            Assert.AreEqual(0, WeatherEffects.GetDamageModifier(Weather.Rain, WeaponType.THUNDER));
            Assert.AreEqual(0, WeatherEffects.GetDamageModifier(Weather.Rain, WeaponType.SWORD));
        }

        [Test]
        public void Rain_MoveCostIncrease()
        {
            Assert.AreEqual(WeatherEffects.RAIN_MOVE_COST_INCREASE, WeatherEffects.GetMoveCostIncrease(Weather.Rain));
        }

        [Test]
        public void Snow_AvoidPenalty()
        {
            Assert.AreEqual(WeatherEffects.SNOW_AVOID_PENALTY, WeatherEffects.GetAvoidModifier(Weather.Snow));
        }

        [Test]
        public void Snow_MoveCostIncrease()
        {
            Assert.AreEqual(WeatherEffects.SNOW_MOVE_COST_INCREASE, WeatherEffects.GetMoveCostIncrease(Weather.Snow));
        }

        [Test]
        public void Snow_NoHitPenalty()
        {
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Snow, WeaponType.BOW));
            Assert.AreEqual(0, WeatherEffects.GetHitModifier(Weather.Snow, WeaponType.SWORD));
        }

        [Test]
        public void Sandstorm_HitPenalty_AllWeapons()
        {
            Assert.AreEqual(WeatherEffects.SANDSTORM_HIT_PENALTY, WeatherEffects.GetHitModifier(Weather.Sandstorm, WeaponType.SWORD));
            Assert.AreEqual(WeatherEffects.SANDSTORM_HIT_PENALTY, WeatherEffects.GetHitModifier(Weather.Sandstorm, WeaponType.BOW));
            Assert.AreEqual(WeatherEffects.SANDSTORM_HIT_PENALTY, WeatherEffects.GetHitModifier(Weather.Sandstorm, WeaponType.FIRE));
        }

        [Test]
        public void Sandstorm_VisionPenalty()
        {
            Assert.AreEqual(WeatherEffects.SANDSTORM_VISION_PENALTY, WeatherEffects.GetVisionPenalty(Weather.Sandstorm));
        }

        [Test]
        public void Sandstorm_NoMoveCostIncrease()
        {
            Assert.AreEqual(0, WeatherEffects.GetMoveCostIncrease(Weather.Sandstorm));
        }

        // ── GameMap weather integration ─────────────────────────────────────

        [Test]
        public void GameMap_DefaultWeather_IsClear()
        {
            var map = new GameMap(8, 8, 42);
            Assert.AreEqual(Weather.Clear, map.CurrentWeather);
        }

        [Test]
        public void GameMap_SetWeather_ChangesCurrentWeather()
        {
            var map = new GameMap(8, 8, 42);
            map.SetWeather(Weather.Rain);
            Assert.AreEqual(Weather.Rain, map.CurrentWeather);
        }

        [Test]
        public void GameMap_WeatherCanChangeMultipleTimes()
        {
            var map = new GameMap(8, 8, 42);
            map.SetWeather(Weather.Snow);
            Assert.AreEqual(Weather.Snow, map.CurrentWeather);
            map.SetWeather(Weather.Sandstorm);
            Assert.AreEqual(Weather.Sandstorm, map.CurrentWeather);
            map.SetWeather(Weather.Clear);
            Assert.AreEqual(Weather.Clear, map.CurrentWeather);
        }

        // ── CombatResolver weather integration ──────────────────────────────

        private static IUnit CreateUnit(IClassData classData, CharacterStats stats, (int, int) pos, IWeapon weapon)
        {
            return new Unit(1, "Test", Team.PlayerTeam, classData, stats, pos, weapon);
        }

        [Test]
        public void Rain_ReducesFireMageDamage()
        {
            var map = new GameMap(8, 8, 42);
            var stats = new CharacterStats(20, 0, 10, 10, 10, 5, 5, 5, 5);
            var mage = CreateUnit(ClassDataFactory.CreateMage(), stats, (0, 0), WeaponFactory.CreateFireTome());
            var defender = CreateUnit(ClassDataFactory.CreateSoldier(), stats, (1, 0), WeaponFactory.CreateIronLance());

            var resolver = new CombatResolver();

            // Clear weather damage
            int clearDamage = resolver.CalculateDamage(mage, defender, map);

            // Rain weather damage
            map.SetWeather(Weather.Rain);
            int rainDamage = resolver.CalculateDamage(mage, defender, map);

            Assert.AreEqual(clearDamage - 2, rainDamage);
        }

        [Test]
        public void Rain_DoesNotAffectThunderDamage()
        {
            var map = new GameMap(8, 8, 42);
            var stats = new CharacterStats(20, 0, 10, 10, 10, 5, 5, 5, 5);
            var mage = CreateUnit(ClassDataFactory.CreateSage(), stats, (0, 0), WeaponFactory.CreateThunderTome());
            var defender = CreateUnit(ClassDataFactory.CreateSoldier(), stats, (1, 0), WeaponFactory.CreateIronLance());

            var resolver = new CombatResolver();

            int clearDamage = resolver.CalculateDamage(mage, defender, map);

            map.SetWeather(Weather.Rain);
            int rainDamage = resolver.CalculateDamage(mage, defender, map);

            Assert.AreEqual(clearDamage, rainDamage);
        }
    }
}
