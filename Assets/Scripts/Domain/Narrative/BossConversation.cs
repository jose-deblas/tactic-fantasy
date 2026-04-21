namespace TacticFantasy.Domain.Narrative
{
    public class BossConversation
    {
        public string UnitName { get; }
        public string BossName { get; }
        public DialogueScript Dialogue { get; }
        public int AttackerAtkBonus { get; }
        public int AttackerHitBonus { get; }

        public BossConversation(
            string unitName,
            string bossName,
            DialogueScript dialogue,
            int attackerAtkBonus = 0,
            int attackerHitBonus = 0)
        {
            UnitName = unitName;
            BossName = bossName;
            Dialogue = dialogue;
            AttackerAtkBonus = attackerAtkBonus;
            AttackerHitBonus = attackerHitBonus;
        }

        public bool IsTriggeredBy(string attackerName, string defenderName)
        {
            return (attackerName == UnitName && defenderName == BossName) ||
                   (attackerName == BossName && defenderName == UnitName);
        }

        public bool IsAttackerTheHero(string attackerName)
        {
            return attackerName == UnitName;
        }
    }
}
