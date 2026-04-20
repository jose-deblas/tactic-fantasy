namespace TacticFantasy.Domain.Units
{
    public enum Team
    {
        PlayerTeam = 0,
        EnemyTeam = 1,
        AllyNPC = 2
    }

    public static class TeamRelations
    {
        /// <summary>Returns true if the two teams are hostile to each other.</summary>
        public static bool AreHostile(Team a, Team b)
        {
            if (a == b) return false;
            // PlayerTeam and AllyNPC are allied; both hostile to EnemyTeam
            if ((a == Team.PlayerTeam || a == Team.AllyNPC) && (b == Team.PlayerTeam || b == Team.AllyNPC))
                return false;
            return true;
        }

        /// <summary>Returns true if the two teams are allied (same side).</summary>
        public static bool AreAllied(Team a, Team b)
        {
            return !AreHostile(a, b);
        }
    }
}
