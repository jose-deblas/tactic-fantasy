using System.Collections.Generic;

namespace TacticFantasy.Domain.Narrative
{
    public class DialogueScript
    {
        public IReadOnlyList<DialogueLine> Lines { get; }

        public DialogueScript(IReadOnlyList<DialogueLine> lines)
        {
            Lines = lines ?? new List<DialogueLine>();
        }

        public bool HasLines => Lines.Count > 0;
    }
}
