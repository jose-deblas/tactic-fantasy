using System;
using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Combat;
using TacticFantasy.Domain.Items;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class NPCUnitTests
    {
        private TurnManager _turnManager;

        private Unit MakeUnit(int id, Team team, (int x, int y) pos)
        {
            return new Unit(id, $"Unit{id}", team,
                ClassDataFactory.CreateSoldier(),
                ClassDataFactory.CreateSoldier().BaseStats,
                pos, WeaponFactory.CreateIronLance());
        }

        [Test]
        public void TurnManager_ThreePhaseRotation_PlayerAllyEnemy()
        {
            var tm = new TurnManager();
            var units = new List<IUnit>
            {
                MakeUnit(1, Team.PlayerTeam, (0, 0)),
                MakeUnit(2, Team.AllyNPC, (1, 0)),
                MakeUnit(3, Team.EnemyTeam, (5, 5))
            };
            tm.Initialize(units);

            Assert.AreEqual(Phase.PlayerPhase, tm.CurrentPhase);
            tm.AdvancePhase();
            Assert.AreEqual(Phase.AllyPhase, tm.CurrentPhase);
            tm.AdvancePhase();
            Assert.AreEqual(Phase.EnemyPhase, tm.CurrentPhase);
            tm.AdvancePhase();
            Assert.AreEqual(Phase.PlayerPhase, tm.CurrentPhase);
        }

        [Test]
        public void TurnManager_AllyPhase_OnlyAllyNPCUnitsAct()
        {
            var tm = new TurnManager();
            var player = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var ally = MakeUnit(2, Team.AllyNPC, (1, 0));
            var enemy = MakeUnit(3, Team.EnemyTeam, (5, 5));
            tm.Initialize(new List<IUnit> { player, ally, enemy });

            tm.AdvancePhase(); // → AllyPhase
            Assert.AreEqual(Phase.AllyPhase, tm.CurrentPhase);

            var currentUnit = tm.CurrentUnit;
            Assert.IsNotNull(currentUnit);
            Assert.AreEqual(Team.AllyNPC, currentUnit.Team);
        }

        [Test]
        public void TurnManager_HaveAllAllyUnitsActed_TracksCorrectly()
        {
            var tm = new TurnManager();
            var player = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var ally = MakeUnit(2, Team.AllyNPC, (1, 0));
            var enemy = MakeUnit(3, Team.EnemyTeam, (5, 5));
            tm.Initialize(new List<IUnit> { player, ally, enemy });

            tm.AdvancePhase(); // → AllyPhase
            Assert.IsFalse(tm.HaveAllAllyUnitsActed());

            tm.MarkUnitAsActed(ally.Id);
            Assert.IsTrue(tm.HaveAllAllyUnitsActed());
        }

        [Test]
        public void AIController_NPCUnit_TargetsEnemyTeam()
        {
            var combatResolver = new CombatResolver();
            var ai = new AIController(combatResolver);
            var map = new GameMap(8, 8, 0);
            var pathFinder = new PathFinder();

            var npc = new Unit(1, "NPC", Team.AllyNPC,
                ClassDataFactory.CreateSoldier(),
                new CharacterStats(20, 7, 0, 8, 8, 3, 7, 2, 5),
                (3, 3), WeaponFactory.CreateIronLance());
            var enemy = new Unit(2, "Enemy", Team.EnemyTeam,
                ClassDataFactory.CreateFighter(),
                new CharacterStats(22, 9, 0, 5, 7, 4, 6, 0, 5),
                (4, 3), WeaponFactory.CreateIronAxe());
            var player = new Unit(3, "Player", Team.PlayerTeam,
                ClassDataFactory.CreateMyrmidon(),
                ClassDataFactory.CreateMyrmidon().BaseStats,
                (0, 0), WeaponFactory.CreateIronSword());

            var allUnits = new List<IUnit> { npc, enemy, player };

            ai.DecideAction(npc, allUnits, map, pathFinder,
                out var moveTarget, out var attackTarget, out var isHealAction);

            // NPC should target the enemy, not the player
            Assert.AreEqual(enemy, attackTarget);
        }

        [Test]
        public void AIController_AreHostile_EnemyVsPlayer_True()
        {
            Assert.IsTrue(TeamRelations.AreHostile(Team.EnemyTeam, Team.PlayerTeam));
            Assert.IsTrue(TeamRelations.AreHostile(Team.PlayerTeam, Team.EnemyTeam));
        }

        [Test]
        public void AIController_AreHostile_EnemyVsAllyNPC_True()
        {
            Assert.IsTrue(TeamRelations.AreHostile(Team.EnemyTeam, Team.AllyNPC));
            Assert.IsTrue(TeamRelations.AreHostile(Team.AllyNPC, Team.EnemyTeam));
        }

        [Test]
        public void AIController_AreHostile_PlayerVsAllyNPC_False()
        {
            Assert.IsFalse(TeamRelations.AreHostile(Team.PlayerTeam, Team.AllyNPC));
            Assert.IsFalse(TeamRelations.AreHostile(Team.AllyNPC, Team.PlayerTeam));
        }

        [Test]
        public void Unit_Recruit_ChangesTeamFromAllyNPCToPlayer()
        {
            var npc = MakeUnit(1, Team.AllyNPC, (0, 0));
            Assert.AreEqual(Team.AllyNPC, npc.Team);

            npc.Recruit();
            Assert.AreEqual(Team.PlayerTeam, npc.Team);
        }

        [Test]
        public void Unit_Recruit_ThrowsIfNotAllyNPC()
        {
            var enemy = MakeUnit(1, Team.EnemyTeam, (0, 0));

            Assert.Throws<InvalidOperationException>(() => enemy.Recruit());
        }

        [Test]
        public void Unit_Recruit_ThrowsIfAlreadyPlayer()
        {
            var player = MakeUnit(1, Team.PlayerTeam, (0, 0));

            Assert.Throws<InvalidOperationException>(() => player.Recruit());
        }

        [Test]
        public void RecruitmentCondition_CanRecruit_ValidConditions_ReturnsTrue()
        {
            var player = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var npc = MakeUnit(2, Team.AllyNPC, (3, 4));

            var condition = new RecruitmentCondition(npcUnitId: 2, recruiterUnitId: 1);
            Assert.IsTrue(condition.CanRecruit(player, npc));
        }

        [Test]
        public void RecruitmentCondition_CanRecruit_WrongRecruiter_ReturnsFalse()
        {
            var wrongPlayer = MakeUnit(99, Team.PlayerTeam, (3, 3));
            var npc = MakeUnit(2, Team.AllyNPC, (3, 4));

            var condition = new RecruitmentCondition(npcUnitId: 2, recruiterUnitId: 1);
            Assert.IsFalse(condition.CanRecruit(wrongPlayer, npc));
        }

        [Test]
        public void RecruitmentCondition_CanRecruit_NotAdjacent_ReturnsFalse()
        {
            var player = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var npc = MakeUnit(2, Team.AllyNPC, (5, 5));

            var condition = new RecruitmentCondition(npcUnitId: 2, recruiterUnitId: 1);
            Assert.IsFalse(condition.CanRecruit(player, npc));
        }

        [Test]
        public void TradeService_PlayerAndAllyNPC_CanTrade()
        {
            var tradeService = new TradeService();
            var player = MakeUnit(1, Team.PlayerTeam, (3, 3));
            var npc = MakeUnit(2, Team.AllyNPC, (3, 4));

            Assert.IsTrue(tradeService.CanTrade(player, npc));
        }

        [Test]
        public void GetGameState_AllyNPCDeathDoesNotCausePlayerLoss()
        {
            var tm = new TurnManager();
            var player = MakeUnit(1, Team.PlayerTeam, (0, 0));
            var npc = MakeUnit(2, Team.AllyNPC, (1, 0));
            var enemy = MakeUnit(3, Team.EnemyTeam, (5, 5));
            tm.Initialize(new List<IUnit> { player, npc, enemy });

            // Kill the NPC
            npc.TakeDamage(999);
            Assert.IsFalse(npc.IsAlive);

            // Game should still be in progress
            Assert.AreEqual(GameState.InProgress, tm.GetGameState());
        }

        [Test]
        public void TurnManager_TurnCountIncrements_AfterFullCycle()
        {
            var tm = new TurnManager();
            tm.Initialize(new List<IUnit>
            {
                MakeUnit(1, Team.PlayerTeam, (0, 0)),
                MakeUnit(2, Team.AllyNPC, (1, 0)),
                MakeUnit(3, Team.EnemyTeam, (5, 5))
            });

            Assert.AreEqual(1, tm.TurnCount);
            tm.AdvancePhase(); // Player → Ally
            Assert.AreEqual(1, tm.TurnCount);
            tm.AdvancePhase(); // Ally → Enemy
            Assert.AreEqual(1, tm.TurnCount);
            tm.AdvancePhase(); // Enemy → Player (TurnCount++)
            Assert.AreEqual(2, tm.TurnCount);
        }
    }
}
