namespace TacticFantasy.Domain.Units
{
    /// <summary>
    /// Defines conditions for recruiting an NPC unit.
    /// The specified recruiter must be adjacent to the NPC and use the Talk action.
    /// </summary>
    public class RecruitmentCondition
    {
        /// <summary>The Id of the AllyNPC unit that can be recruited.</summary>
        public int NPCUnitId { get; }

        /// <summary>The Id of the player unit that must initiate the Talk action.</summary>
        public int RecruiterUnitId { get; }

        public RecruitmentCondition(int npcUnitId, int recruiterUnitId)
        {
            NPCUnitId = npcUnitId;
            RecruiterUnitId = recruiterUnitId;
        }

        public bool CanRecruit(IUnit recruiter, IUnit npc)
        {
            if (recruiter == null || npc == null) return false;
            if (recruiter.Id != RecruiterUnitId) return false;
            if (npc.Id != NPCUnitId) return false;
            if (npc.Team != Team.AllyNPC) return false;
            if (recruiter.Team != Team.PlayerTeam) return false;
            if (!recruiter.IsAlive || !npc.IsAlive) return false;

            int distance = System.Math.Abs(recruiter.Position.x - npc.Position.x)
                         + System.Math.Abs(recruiter.Position.y - npc.Position.y);
            return distance == 1;
        }
    }
}
