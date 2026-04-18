using NUnit.Framework;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class LaguzStatSwapTests
    {
        private Unit CreateLaguzUnit(LaguzClassData classData)
        {
            var weapon = LaguzWeaponFactory.CreateForRace(classData.Race);
            var unit = new Unit(1, "Laguz", Team.PlayerTeam, classData,
                classData.UntransformedStats, (0, 0), weapon);
            unit.InitLaguzGauge(classData.GaugeFillRate, classData.GaugeDrainRate);
            return unit;
        }

        [Test]
        public void NewLaguzUnit_IsNotTransformed()
        {
            var unit = CreateLaguzUnit(LaguzClassDataFactory.CreateCat());

            Assert.IsTrue(unit.IsLaguz);
            Assert.IsFalse(unit.IsTransformed);
        }

        [Test]
        public void NewLaguzUnit_HasUntransformedStats()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            Assert.AreEqual(catClass.UntransformedStats.STR, unit.CurrentStats.STR);
            Assert.AreEqual(catClass.UntransformedStats.DEF, unit.CurrentStats.DEF);
        }

        [Test]
        public void Transform_SwapsToTransformedStats()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            unit.Transform();

            Assert.IsTrue(unit.IsTransformed);
            Assert.AreEqual(catClass.TransformedStats.STR, unit.CurrentStats.STR);
            Assert.AreEqual(catClass.TransformedStats.DEF, unit.CurrentStats.DEF);
        }

        [Test]
        public void Revert_SwapsToUntransformedStats()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            unit.Transform();
            unit.Revert();

            Assert.IsFalse(unit.IsTransformed);
            Assert.AreEqual(catClass.UntransformedStats.STR, unit.CurrentStats.STR);
            Assert.AreEqual(catClass.UntransformedStats.DEF, unit.CurrentStats.DEF);
        }

        [Test]
        public void Transform_DoesNothing_WhenAlreadyTransformed()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            unit.Transform();
            var statsBefore = unit.CurrentStats;
            unit.Transform(); // no-op

            Assert.AreEqual(statsBefore.STR, unit.CurrentStats.STR);
        }

        [Test]
        public void Revert_DoesNothing_WhenAlreadyUntransformed()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            var statsBefore = unit.CurrentStats;
            unit.Revert(); // no-op

            Assert.AreEqual(statsBefore.STR, unit.CurrentStats.STR);
        }

        [Test]
        public void NonLaguzUnit_IsNotLaguz()
        {
            var unit = new Unit(1, "Beorc", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            Assert.IsFalse(unit.IsLaguz);
            Assert.IsFalse(unit.IsTransformed);
            Assert.IsNull(unit.LaguzGauge);
        }

        [Test]
        public void NonLaguzUnit_TransformDoesNothing()
        {
            var unit = new Unit(1, "Beorc", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            var statsBefore = unit.CurrentStats;
            unit.Transform();

            Assert.AreEqual(statsBefore.STR, unit.CurrentStats.STR);
        }

        // ── Stat difference tests (transformed = full, untransformed = halved) ──

        [Test]
        public void Cat_TransformedSTR_IsDoubleUntransformed()
        {
            var catClass = LaguzClassDataFactory.CreateCat();

            Assert.AreEqual(10, catClass.TransformedStats.STR);
            Assert.AreEqual(5, catClass.UntransformedStats.STR);
        }

        [Test]
        public void Tiger_TransformedDEF_IsDoubleUntransformed()
        {
            var tigerClass = LaguzClassDataFactory.CreateTiger();

            Assert.AreEqual(12, tigerClass.TransformedStats.DEF);
            Assert.AreEqual(6, tigerClass.UntransformedStats.DEF);
        }

        [Test]
        public void Lion_HasHighestTransformedSTR_AmongBeasts()
        {
            var cat = LaguzClassDataFactory.CreateCat();
            var tiger = LaguzClassDataFactory.CreateTiger();
            var lion = LaguzClassDataFactory.CreateLion();

            Assert.Greater(lion.TransformedStats.STR, tiger.TransformedStats.STR);
            Assert.Greater(tiger.TransformedStats.STR, cat.TransformedStats.STR);
        }

        // ── TickTransformGauge integration ──────────────────────────────────

        [Test]
        public void TickTransformGauge_FillsAndAutoTransforms()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);
            // Cat fills at 8/turn, needs 30 to transform → 4 turns

            for (int i = 0; i < 3; i++)
                unit.TickTransformGauge();

            Assert.IsFalse(unit.IsTransformed);

            unit.TickTransformGauge();
            Assert.IsTrue(unit.IsTransformed);
            Assert.AreEqual(catClass.TransformedStats.STR, unit.CurrentStats.STR);
        }

        [Test]
        public void TickTransformGauge_DrainsAndAutoReverts()
        {
            var catClass = LaguzClassDataFactory.CreateCat();
            var unit = CreateLaguzUnit(catClass);

            // Force transform
            unit.LaguzGauge.FillToMax();
            unit.Transform();
            Assert.IsTrue(unit.IsTransformed);

            // Drain at 2/turn from 30 → 15 ticks to drain
            for (int i = 0; i < 14; i++)
                unit.TickTransformGauge();
            Assert.IsTrue(unit.IsTransformed);

            unit.TickTransformGauge();
            Assert.IsFalse(unit.IsTransformed);
            Assert.AreEqual(catClass.UntransformedStats.STR, unit.CurrentStats.STR);
        }

        [Test]
        public void TickTransformGauge_ReturnsFalse_ForNonLaguz()
        {
            var unit = new Unit(1, "Beorc", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            bool changed = unit.TickTransformGauge();
            Assert.IsFalse(changed);
        }

        // ── All Laguz races can be created ──────────────────────────────────

        [Test]
        public void AllLaguzRaces_CanBeCreated()
        {
            var classes = new[]
            {
                LaguzClassDataFactory.CreateCat(),
                LaguzClassDataFactory.CreateTiger(),
                LaguzClassDataFactory.CreateLion(),
                LaguzClassDataFactory.CreateHawk(),
                LaguzClassDataFactory.CreateRaven(),
                LaguzClassDataFactory.CreateRedDragon(),
                LaguzClassDataFactory.CreateWhiteDragon(),
                LaguzClassDataFactory.CreateHeron()
            };

            foreach (var cls in classes)
            {
                var unit = CreateLaguzUnit(cls);
                Assert.IsTrue(unit.IsLaguz, $"{cls.Name} should be Laguz");
                Assert.IsFalse(unit.IsTransformed, $"{cls.Name} should start untransformed");
            }
        }

        // ── Bird tribe becomes Flying when transformed ──────────────────────

        [Test]
        public void Hawk_TransformedMoveType_IsFlying()
        {
            var hawkClass = LaguzClassDataFactory.CreateHawk();
            Assert.AreEqual(MoveType.Infantry, hawkClass.UntransformedMoveType);
            Assert.AreEqual(MoveType.Flying, hawkClass.TransformedMoveType);
        }

        [Test]
        public void Raven_TransformedMoveType_IsFlying()
        {
            var ravenClass = LaguzClassDataFactory.CreateRaven();
            Assert.AreEqual(MoveType.Infantry, ravenClass.UntransformedMoveType);
            Assert.AreEqual(MoveType.Flying, ravenClass.TransformedMoveType);
        }

        [Test]
        public void Tiger_MoveType_IsAlwaysInfantry()
        {
            var tigerClass = LaguzClassDataFactory.CreateTiger();
            Assert.AreEqual(MoveType.Infantry, tigerClass.UntransformedMoveType);
            Assert.AreEqual(MoveType.Infantry, tigerClass.TransformedMoveType);
        }

        // ── Natural weapons ─────────────────────────────────────────────────

        [Test]
        public void LaguzWeapon_ForCat_IsStrike()
        {
            var weapon = LaguzWeaponFactory.CreateForRace(LaguzRace.Cat);
            Assert.AreEqual("Strike", weapon.Name);
            Assert.AreEqual(WeaponType.STRIKE, weapon.Type);
        }

        [Test]
        public void LaguzWeapon_ForLion_IsFang()
        {
            var weapon = LaguzWeaponFactory.CreateForRace(LaguzRace.Lion);
            Assert.AreEqual("Fang", weapon.Name);
            Assert.AreEqual(DamageType.Physical, weapon.DamageType);
        }

        [Test]
        public void LaguzWeapon_ForRedDragon_IsBreath()
        {
            var weapon = LaguzWeaponFactory.CreateForRace(LaguzRace.RedDragon);
            Assert.AreEqual("Breath", weapon.Name);
            Assert.AreEqual(DamageType.Magical, weapon.DamageType);
        }

        [Test]
        public void LaguzWeapon_ForHawk_IsTalon()
        {
            var weapon = LaguzWeaponFactory.CreateForRace(LaguzRace.Hawk);
            Assert.AreEqual("Talon", weapon.Name);
        }
    }
}
