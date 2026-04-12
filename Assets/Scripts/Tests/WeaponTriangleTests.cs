using NUnit.Framework;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class WeaponTriangleTests
    {
        [Test]
        public void GetTriangleModifiers_SwordVsAxe_ReturnsAdvantage()
        {
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
            var axe = new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(sword, axe);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_AxeVsLance_ReturnsAdvantage()
        {
            var axe = new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1);
            var lance = new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(axe, lance);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_LanceVsSword_ReturnsAdvantage()
        {
            var lance = new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1);
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(lance, sword);

            Assert.AreEqual(WeaponTriangle.ADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.ADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_AxeVsSword_ReturnsDisadvantage()
        {
            var axe = new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1);
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(axe, sword);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_LanceVsAxe_ReturnsDisadvantage()
        {
            var lance = new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1);
            var axe = new Weapon("Iron Axe", WeaponType.AXE, DamageType.Physical, 8, 9, 70, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(lance, axe);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_SwordVsLance_ReturnsDisadvantage()
        {
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);
            var lance = new Weapon("Iron Lance", WeaponType.LANCE, DamageType.Physical, 6, 6, 80, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(sword, lance);

            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_DAMAGE, damageBonus);
            Assert.AreEqual(WeaponTriangle.DISADVANTAGE_HIT, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_MagicalWeapons_ReturnsNoModifier()
        {
            var fire = new Weapon("Fire", WeaponType.FIRE, DamageType.Magical, 5, 4, 85, 0, 1, 2);
            var staff = new Weapon("Heal Staff", WeaponType.STAFF, DamageType.Magical, 0, 3, 100, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(fire, staff);

            Assert.AreEqual(0, damageBonus);
            Assert.AreEqual(0, hitBonus);
        }

        [Test]
        public void GetTriangleModifiers_BowVsOtherWeapons_ReturnsNoModifier()
        {
            var bow = new Weapon("Iron Bow", WeaponType.BOW, DamageType.Physical, 6, 5, 85, 0, 2, 2);
            var sword = new Weapon("Iron Sword", WeaponType.SWORD, DamageType.Physical, 5, 5, 90, 0, 1, 1);

            var (damageBonus, hitBonus) = WeaponTriangle.GetTriangleModifiers(bow, sword);

            Assert.AreEqual(0, damageBonus);
            Assert.AreEqual(0, hitBonus);
        }
    }
}
