using System.Linq;
using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class MultiWeaponClassTests
    {
        [Test]
        public void General_HasLanceAndSword()
        {
            var general = ClassDataFactory.CreateGeneral();
            Assert.AreEqual(2, general.UsableWeaponTypes.Count);
            Assert.Contains(WeaponType.LANCE, general.UsableWeaponTypes.ToList());
            Assert.Contains(WeaponType.SWORD, general.UsableWeaponTypes.ToList());
        }

        [Test]
        public void Warrior_HasAxeAndBow()
        {
            var warrior = ClassDataFactory.CreateWarrior();
            Assert.AreEqual(2, warrior.UsableWeaponTypes.Count);
            Assert.Contains(WeaponType.AXE, warrior.UsableWeaponTypes.ToList());
            Assert.Contains(WeaponType.BOW, warrior.UsableWeaponTypes.ToList());
        }

        [Test]
        public void Sage_HasFireAndStaff()
        {
            var sage = ClassDataFactory.CreateSage();
            Assert.AreEqual(2, sage.UsableWeaponTypes.Count);
            Assert.Contains(WeaponType.FIRE, sage.UsableWeaponTypes.ToList());
            Assert.Contains(WeaponType.STAFF, sage.UsableWeaponTypes.ToList());
        }

        [Test]
        public void Bishop_HasStaffAndFire()
        {
            var bishop = ClassDataFactory.CreateBishop();
            Assert.AreEqual(2, bishop.UsableWeaponTypes.Count);
            Assert.Contains(WeaponType.STAFF, bishop.UsableWeaponTypes.ToList());
            Assert.Contains(WeaponType.FIRE, bishop.UsableWeaponTypes.ToList());
        }

        [Test]
        public void Myrmidon_HasOnlySword()
        {
            var myrmidon = ClassDataFactory.CreateMyrmidon();
            Assert.AreEqual(1, myrmidon.UsableWeaponTypes.Count);
            Assert.AreEqual(WeaponType.SWORD, myrmidon.UsableWeaponTypes[0]);
        }

        [Test]
        public void ClassData_WeaponType_ReturnsPrimary()
        {
            var general = ClassDataFactory.CreateGeneral();
            Assert.AreEqual(WeaponType.LANCE, general.WeaponType);
        }

        [Test]
        public void ClassData_SingleWeaponConstructor_CreatesOneElementList()
        {
            var myrmidon = ClassDataFactory.CreateMyrmidon();
            Assert.AreEqual(1, myrmidon.UsableWeaponTypes.Count);
        }

        [Test]
        public void Unit_WithGeneral_CanEquipSwordAndLance()
        {
            var unit = new Unit(1, "Gatrie", Team.PlayerTeam, ClassDataFactory.CreateGeneral(),
                new CharacterStats(26, 12, 0, 10, 8, 4, 15, 8, 4), (0, 0),
                WeaponFactory.CreateIronLance());

            Assert.IsTrue(unit.CanEquip(WeaponFactory.CreateIronSword()));
            Assert.IsTrue(unit.CanEquip(WeaponFactory.CreateIronLance()));
            Assert.IsFalse(unit.CanEquip(WeaponFactory.CreateIronAxe()));
        }
    }
}
