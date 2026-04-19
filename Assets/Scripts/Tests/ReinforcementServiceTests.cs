using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class ReinforcementServiceTests
    {
        private ReinforcementService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ReinforcementService(new MapLoader(), startingId: 100);
        }

        [Test]
        public void OnTurn_FiresOnCorrectTurn()
        {
            var trigger = ReinforcementTrigger.OnTurn(3, new List<UnitPlacement>
            {
                new UnitPlacement("Reinforcement", "Soldier", "Iron Lance", Team.EnemyTeam, (5, 5))
            });

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 3, new List<IUnit>());

            Assert.AreEqual(1, spawned.Count);
            Assert.AreEqual("Reinforcement", spawned[0].Name);
            Assert.AreEqual(Team.EnemyTeam, spawned[0].Team);
            Assert.AreEqual((5, 5), spawned[0].Position);
        }

        [Test]
        public void OnTurn_DoesNotFireBeforeCorrectTurn()
        {
            var trigger = ReinforcementTrigger.OnTurn(3, new List<UnitPlacement>
            {
                new UnitPlacement("Reinforcement", "Soldier", "Iron Lance", Team.EnemyTeam, (5, 5))
            });

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 2, new List<IUnit>());

            Assert.AreEqual(0, spawned.Count);
            Assert.IsFalse(trigger.HasFired);
        }

        [Test]
        public void OnTurn_DoesNotFireTwice()
        {
            var trigger = ReinforcementTrigger.OnTurn(3, new List<UnitPlacement>
            {
                new UnitPlacement("Reinforcement", "Soldier", "Iron Lance", Team.EnemyTeam, (5, 5))
            });

            _service.EvaluateTriggers(new[] { trigger }, 3, new List<IUnit>());
            var spawnedAgain = _service.EvaluateTriggers(new[] { trigger }, 3, new List<IUnit>());

            Assert.AreEqual(0, spawnedAgain.Count);
            Assert.IsTrue(trigger.HasFired);
        }

        [Test]
        public void OnTileSteppedOn_FiresWhenUnitOnTile()
        {
            var trigger = ReinforcementTrigger.OnTileSteppedOn((3, 3), new List<UnitPlacement>
            {
                new UnitPlacement("Ambush", "Fighter", "Iron Axe", Team.EnemyTeam, (4, 4))
            });

            var player = new Unit(1, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (3, 3), WeaponFactory.CreateIronSword());

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit> { player });

            Assert.AreEqual(1, spawned.Count);
            Assert.AreEqual("Ambush", spawned[0].Name);
        }

        [Test]
        public void OnTileSteppedOn_DoesNotFireWhenNoUnitOnTile()
        {
            var trigger = ReinforcementTrigger.OnTileSteppedOn((3, 3), new List<UnitPlacement>
            {
                new UnitPlacement("Ambush", "Fighter", "Iron Axe", Team.EnemyTeam, (4, 4))
            });

            var player = new Unit(1, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                new CharacterStats(18, 6, 0, 11, 12, 5, 5, 0, 5),
                (0, 0), WeaponFactory.CreateIronSword());

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit> { player });

            Assert.AreEqual(0, spawned.Count);
        }

        [Test]
        public void OnUnitDeath_FiresWhenUnitIsDead()
        {
            var trigger = ReinforcementTrigger.OnUnitDeath(1, new List<UnitPlacement>
            {
                new UnitPlacement("Avenger", "Myrmidon", "Iron Sword", Team.EnemyTeam, (7, 7))
            });

            var deadUnit = new Unit(1, "Fallen", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (5, 5), WeaponFactory.CreateIronLance());
            deadUnit.TakeDamage(100);

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit> { deadUnit });

            Assert.AreEqual(1, spawned.Count);
            Assert.AreEqual("Avenger", spawned[0].Name);
        }

        [Test]
        public void OnUnitDeath_DoesNotFireWhenUnitAlive()
        {
            var trigger = ReinforcementTrigger.OnUnitDeath(1, new List<UnitPlacement>
            {
                new UnitPlacement("Avenger", "Myrmidon", "Iron Sword", Team.EnemyTeam, (7, 7))
            });

            var aliveUnit = new Unit(1, "Still Standing", Team.EnemyTeam,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(18, 7, 0, 8, 8, 3, 7, 2, 5),
                (5, 5), WeaponFactory.CreateIronLance());

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit> { aliveUnit });

            Assert.AreEqual(0, spawned.Count);
        }

        [Test]
        public void MultipleTriggers_AllFireWhenConditionsMet()
        {
            var trigger1 = ReinforcementTrigger.OnTurn(2, new List<UnitPlacement>
            {
                new UnitPlacement("Wave1", "Soldier", "Iron Lance", Team.EnemyTeam, (1, 1))
            });
            var trigger2 = ReinforcementTrigger.OnTurn(2, new List<UnitPlacement>
            {
                new UnitPlacement("Wave2", "Fighter", "Iron Axe", Team.EnemyTeam, (2, 2))
            });

            var spawned = _service.EvaluateTriggers(new[] { trigger1, trigger2 }, 2, new List<IUnit>());

            Assert.AreEqual(2, spawned.Count);
        }

        [Test]
        public void SpawnedUnits_HaveUniqueIds()
        {
            var trigger = ReinforcementTrigger.OnTurn(1, new List<UnitPlacement>
            {
                new UnitPlacement("A", "Soldier", "Iron Lance", Team.EnemyTeam, (1, 1)),
                new UnitPlacement("B", "Fighter", "Iron Axe", Team.EnemyTeam, (2, 2))
            });

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit>());

            Assert.AreNotEqual(spawned[0].Id, spawned[1].Id);
        }

        [Test]
        public void NoTriggersFire_WhenConditionsNotMet()
        {
            var trigger = ReinforcementTrigger.OnTurn(5, new List<UnitPlacement>
            {
                new UnitPlacement("Late", "Soldier", "Iron Lance", Team.EnemyTeam, (1, 1))
            });

            var spawned = _service.EvaluateTriggers(new[] { trigger }, 1, new List<IUnit>());

            Assert.AreEqual(0, spawned.Count);
            Assert.IsFalse(trigger.HasFired);
        }
    }
}
