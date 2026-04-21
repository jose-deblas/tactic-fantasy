namespace TacticFantasy.Domain.Narrative
{
    public class DialogueLine
    {
        public string Speaker { get; }
        public string Text { get; }
        public string PortraitId { get; }

        public DialogueLine(string speaker, string text, string portraitId = null)
        {
            Speaker = speaker;
            Text = text;
            PortraitId = portraitId;
        }
    }
}
