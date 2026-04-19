using NUnit.Framework;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class TerrainTypeTests
    {
        // ── Movement costs ─────────────────────────────────────────────────

        [Test]
        public void Desert_Infantry_Cost3()
        {
            Assert.AreEqual(3, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Infantry));
        }

        [Test]
        public void Desert_Cavalry_Cost4()
        {
            Assert.AreEqual(4, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Cavalry));
        }

        [Test]
        public void Desert_Flying_Cost1()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Flying));
        }

        [Test]
        public void Desert_Mage_Cost1()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Infantry, isMage: true));
        }

        [Test]
        public void Desert_Armored_Cost3()
        {
            Assert.AreEqual(3, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Armored));
        }

        [Test]
        public void Bridge_AllTypes_Cost1()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Bridge, MoveType.Infantry));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Bridge, MoveType.Cavalry));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Bridge, MoveType.Flying));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Bridge, MoveType.Armored));
        }

        [Test]
        public void Throne_Cost1()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Throne, MoveType.Infantry));
        }

        [Test]
        public void Chest_Cost1()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Chest, MoveType.Infantry));
        }

        [Test]
        public void Door_Impassable()
        {
            Assert.AreEqual(int.MaxValue, TerrainProperties.GetMovementCost(TerrainType.Door, MoveType.Infantry));
            Assert.IsFalse(TerrainProperties.IsPassable(TerrainType.Door, MoveType.Infantry));
        }

        [Test]
        public void Door_Impassable_EvenForFlying()
        {
            Assert.AreEqual(int.MaxValue, TerrainProperties.GetMovementCost(TerrainType.Door, MoveType.Flying));
        }

        [Test]
        public void Flying_IgnoresTerrainCost_ExceptWallAndDoor()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Forest, MoveType.Flying));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Mountain, MoveType.Flying));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Desert, MoveType.Flying));
            Assert.AreEqual(int.MaxValue, TerrainProperties.GetMovementCost(TerrainType.Wall, MoveType.Flying));
            Assert.AreEqual(int.MaxValue, TerrainProperties.GetMovementCost(TerrainType.Door, MoveType.Flying));
        }

        // ── Existing terrain backward compat ────────────────────────────────

        [Test]
        public void Mountain_InfantryPassable_CavalryImpassable()
        {
            Assert.AreEqual(3, TerrainProperties.GetMovementCost(TerrainType.Mountain, MoveType.Infantry));
            Assert.AreEqual(int.MaxValue, TerrainProperties.GetMovementCost(TerrainType.Mountain, MoveType.Cavalry));
        }

        [Test]
        public void Plain_Cost1_AllTypes()
        {
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Plain, MoveType.Infantry));
            Assert.AreEqual(1, TerrainProperties.GetMovementCost(TerrainType.Plain, MoveType.Cavalry));
        }

        // ── Defense bonuses ─────────────────────────────────────────────────

        [Test]
        public void Throne_DefenseBonus3()
        {
            Assert.AreEqual(3, TerrainProperties.GetDefenseBonus(TerrainType.Throne));
        }

        [Test]
        public void Desert_DefenseBonus0()
        {
            Assert.AreEqual(0, TerrainProperties.GetDefenseBonus(TerrainType.Desert));
        }

        [Test]
        public void Bridge_DefenseBonus0()
        {
            Assert.AreEqual(0, TerrainProperties.GetDefenseBonus(TerrainType.Bridge));
        }

        // ── Avoid bonuses ───────────────────────────────────────────────────

        [Test]
        public void Throne_AvoidBonus30()
        {
            Assert.AreEqual(30, TerrainProperties.GetAvoidBonus(TerrainType.Throne));
        }

        [Test]
        public void Desert_AvoidBonus5()
        {
            Assert.AreEqual(5, TerrainProperties.GetAvoidBonus(TerrainType.Desert));
        }

        [Test]
        public void Bridge_AvoidBonus0()
        {
            Assert.AreEqual(0, TerrainProperties.GetAvoidBonus(TerrainType.Bridge));
        }

        // ── Heal percent ────────────────────────────────────────────────────

        [Test]
        public void Throne_Heals30Percent()
        {
            Assert.AreEqual(30, TerrainProperties.GetHealPercent(TerrainType.Throne));
        }

        [Test]
        public void Desert_NoHeal()
        {
            Assert.AreEqual(0, TerrainProperties.GetHealPercent(TerrainType.Desert));
        }

        // ── InteractableTile ────────────────────────────────────────────────

        [Test]
        public void InteractableTile_StartsClosedByDefault()
        {
            var tile = new InteractableTile(3, 4, TerrainType.Door);
            Assert.IsFalse(tile.IsOpened);
        }

        [Test]
        public void InteractableTile_Open_SetsIsOpenedTrue()
        {
            var tile = new InteractableTile(3, 4, TerrainType.Door);
            tile.Open();
            Assert.IsTrue(tile.IsOpened);
        }

        [Test]
        public void InteractableTile_Chest_ContainsItem()
        {
            var item = ConsumableFactory.CreateVulnerary();
            var tile = new InteractableTile(5, 6, TerrainType.Chest, item);

            Assert.AreEqual("Vulnerary", tile.ContainedItem.Name);
        }

        [Test]
        public void InteractableTile_Door_NoContainedItem()
        {
            var tile = new InteractableTile(3, 4, TerrainType.Door);
            Assert.IsNull(tile.ContainedItem);
        }

        [Test]
        public void InteractableTile_ImplementsITile()
        {
            var tile = new InteractableTile(2, 3, TerrainType.Door);
            Assert.AreEqual(2, tile.X);
            Assert.AreEqual(3, tile.Y);
            Assert.AreEqual(TerrainType.Door, tile.Terrain);
        }
    }
}
