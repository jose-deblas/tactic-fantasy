using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class MagicTriangleTests
    {
        [Test]
        public void GetTriangleModifiers_FireVsWind_ReturnsAdvantage()
        {
            var fire = WeaponFactory.CreateFireTome();
            var wind = WeaponFactory.CreateWindTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(fire, wind);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_WindVsThunder_ReturnsAdvantage()
        {
            var wind = WeaponFactory.CreateWindTome();
            var thunder = WeaponFactory.CreateThunderTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(wind, thunder);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_ThunderVsFire_ReturnsAdvantage()
        {
            var thunder = WeaponFactory.CreateThunderTome();
            var fire = WeaponFactory.CreateFireTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(thunder, fire);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_WindVsFire_ReturnsDisadvantage()
        {
            var wind = WeaponFactory.CreateWindTome();
            var fire = WeaponFactory.CreateFireTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(wind, fire);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_ThunderVsWind_ReturnsDisadvantage()
        {
            var thunder = WeaponFactory.CreateThunderTome();
            var wind = WeaponFactory.CreateWindTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(thunder, wind);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_FireVsThunder_ReturnsDisadvantage()
        {
            var fire = WeaponFactory.CreateFireTome();
            var thunder = WeaponFactory.CreateThunderTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(fire, thunder);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_FireVsSword_ReturnsNoModifier()
        {
            var fire = WeaponFactory.CreateFireTome();
            var sword = WeaponFactory.CreateIronSword();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(fire, sword);

            Assert.AreEqual(0, damageBonus);
            Assert.AreEqual(0, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_ThunderVsBow_ReturnsNoModifier()
        {
            var thunder = WeaponFactory.CreateThunderTome();
            var bow = WeaponFactory.CreateIronBow();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(thunder, bow);

            Assert.AreEqual(0, damageBonus);
            Assert.AreEqual(0, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_SameElement_ReturnsNoModifier()
        {
            var fire1 = WeaponFactory.CreateFireTome();
            var fire2 = WeaponFactory.CreateSteelFireTome();

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(fire1, fire2);

            Assert.AreEqual(0, damageBonus);
            Assert.AreEqual(0, hitBonus);
        }

        [Test]
        public void Sage_CanEquip_AllThreeMagicTypes()
        {
            var sage = ClassDataFactory.CreateSage();

            Assert.IsTrue(sage.UsableWeaponTypes.Contains(WeaponType.FIRE));
            Assert.IsTrue(sage.UsableWeaponTypes.Contains(WeaponType.WIND));
            Assert.IsTrue(sage.UsableWeaponTypes.Contains(WeaponType.THUNDER));
            Assert.IsTrue(sage.UsableWeaponTypes.Contains(WeaponType.STAFF));
        }

        [Test]
        public void Archsage_CanEquip_AllThreeMagicTypesAndStaff()
        {
            var archsage = ClassDataFactory.CreateArchsage();

            Assert.IsTrue(archsage.UsableWeaponTypes.Contains(WeaponType.FIRE));
            Assert.IsTrue(archsage.UsableWeaponTypes.Contains(WeaponType.WIND));
            Assert.IsTrue(archsage.UsableWeaponTypes.Contains(WeaponType.THUNDER));
            Assert.IsTrue(archsage.UsableWeaponTypes.Contains(WeaponType.STAFF));
        }

        [Test]
        public void WindTome_HasCorrectStats()
        {
            var wind = WeaponFactory.CreateWindTome();

            Assert.AreEqual("Wind", wind.Name);
            Assert.AreEqual(WeaponType.WIND, wind.Type);
            Assert.AreEqual(DamageType.Magical, wind.DamageType);
            Assert.AreEqual(4, wind.Might);
            Assert.AreEqual(90, wind.Hit);
            Assert.AreEqual(1, wind.MinRange);
            Assert.AreEqual(2, wind.MaxRange);
        }

        [Test]
        public void ThunderTome_HasCorrectStats()
        {
            var thunder = WeaponFactory.CreateThunderTome();

            Assert.AreEqual("Thunder", thunder.Name);
            Assert.AreEqual(WeaponType.THUNDER, thunder.Type);
            Assert.AreEqual(DamageType.Magical, thunder.DamageType);
            Assert.AreEqual(6, thunder.Might);
            Assert.AreEqual(80, thunder.Hit);
            Assert.AreEqual(1, thunder.MinRange);
            Assert.AreEqual(2, thunder.MaxRange);
        }
    }
}
