using System.Collections.Generic;
using NUnit.Framework;
using TacticFantasy.Domain.Chapter;
using TacticFantasy.Domain.Narrative;

namespace TacticFantasy.Tests
{
    [TestFixture]
    public class BossConversationTriggerTests
    {
        // ── DialogueLine ────────────────────────────────────────────────────

        [Test]
        public void DialogueLine_StoresSpeakerAndText()
        {
            var line = new DialogueLine("Ike", "Prepare yourself!");

            Assert.AreEqual("Ike", line.Speaker);
            Assert.AreEqual("Prepare yourself!", line.Text);
            Assert.IsNull(line.PortraitId);
        }

        [Test]
        public void DialogueLine_OptionalPortraitId()
        {
            var line = new DialogueLine("Ike", "Prepare yourself!", "ike_angry");

            Assert.AreEqual("ike_angry", line.PortraitId);
        }

        // ── DialogueScript ──────────────────────────────────────────────────

        [Test]
        public void DialogueScript_HasLines_ReturnsTrueWhenNotEmpty()
        {
            var script = new DialogueScript(new List<DialogueLine>
            {
                new DialogueLine("Ike", "We'll fight our way through!"),
                new DialogueLine("Soren", "Understood.")
            });

            Assert.IsTrue(script.HasLines);
            Assert.AreEqual(2, script.Lines.Count);
        }

        [Test]
        public void DialogueScript_HasLines_ReturnsFalseWhenEmpty()
        {
            var script = new DialogueScript(new List<DialogueLine>());
            Assert.IsFalse(script.HasLines);
        }

        [Test]
        public void DialogueScript_NullLines_DefaultsToEmpty()
        {
            var script = new DialogueScript(null);
            Assert.IsFalse(script.HasLines);
            Assert.AreEqual(0, script.Lines.Count);
        }

        // ── BossConversation ────────────────────────────────────────────────

        [Test]
        public void BossConversation_IsTriggeredBy_MatchesAttackerVsBoss()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            Assert.IsTrue(conv.IsTriggeredBy("Ike", "Black Knight"));
        }

        [Test]
        public void BossConversation_IsTriggeredBy_MatchesReverseOrder()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            Assert.IsTrue(conv.IsTriggeredBy("Black Knight", "Ike"));
        }

        [Test]
        public void BossConversation_IsTriggeredBy_DoesNotMatchUnrelatedUnits()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            Assert.IsFalse(conv.IsTriggeredBy("Soren", "Black Knight"));
            Assert.IsFalse(conv.IsTriggeredBy("Ike", "Ashnard"));
            Assert.IsFalse(conv.IsTriggeredBy("Soren", "Mia"));
        }

        [Test]
        public void BossConversation_IsAttackerTheHero_CorrectlyIdentifies()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            Assert.IsTrue(conv.IsAttackerTheHero("Ike"));
            Assert.IsFalse(conv.IsAttackerTheHero("Black Knight"));
        }

        [Test]
        public void BossConversation_StatBonuses_DefaultToZero()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            Assert.AreEqual(0, conv.AttackerAtkBonus);
            Assert.AreEqual(0, conv.AttackerHitBonus);
        }

        [Test]
        public void BossConversation_StatBonuses_CanBeSet()
        {
            var dialogue = new DialogueScript(new List<DialogueLine>
            {
                new DialogueLine("Ike", "I will defeat you!")
            });
            var conv = new BossConversation("Ike", "Black Knight", dialogue,
                attackerAtkBonus: 3, attackerHitBonus: 15);

            Assert.AreEqual(3, conv.AttackerAtkBonus);
            Assert.AreEqual(15, conv.AttackerHitBonus);
        }

        // ── ChapterData boss conversation lookup ────────────────────────────

        [Test]
        public void ChapterData_FindBossConversation_ReturnsMatchingConversation()
        {
            var conv1 = CreateTestConversation("Ike", "Black Knight");
            var conv2 = CreateTestConversation("Micaiah", "Jarod");

            var chapter = new ChapterData("Chapter 1", 0, 200, 10,
                bossConversations: new List<BossConversation> { conv1, conv2 });

            var result = chapter.FindBossConversation("Ike", "Black Knight");
            Assert.IsNotNull(result);
            Assert.AreEqual("Ike", result.UnitName);
            Assert.AreEqual("Black Knight", result.BossName);
        }

        [Test]
        public void ChapterData_FindBossConversation_ReturnsNullWhenNoMatch()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            var chapter = new ChapterData("Chapter 1", 0, 200, 10,
                bossConversations: new List<BossConversation> { conv });

            Assert.IsNull(chapter.FindBossConversation("Soren", "Ashnard"));
        }

        [Test]
        public void ChapterData_FindBossConversation_WorksInReverseOrder()
        {
            var conv = CreateTestConversation("Ike", "Black Knight");

            var chapter = new ChapterData("Chapter 1", 0, 200, 10,
                bossConversations: new List<BossConversation> { conv });

            var result = chapter.FindBossConversation("Black Knight", "Ike");
            Assert.IsNotNull(result);
        }

        [Test]
        public void ChapterData_NoBossConversations_DefaultsToEmpty()
        {
            var chapter = new ChapterData("Chapter 1", 0, 200, 10);

            Assert.IsNotNull(chapter.BossConversations);
            Assert.AreEqual(0, chapter.BossConversations.Count);
            Assert.IsNull(chapter.FindBossConversation("Ike", "Anyone"));
        }

        [Test]
        public void ChapterData_IntroOutroDialogue_CanBeSet()
        {
            var intro = new DialogueScript(new List<DialogueLine>
            {
                new DialogueLine("Narrator", "The dawn rises over Daein...")
            });
            var outro = new DialogueScript(new List<DialogueLine>
            {
                new DialogueLine("Ike", "The battle is won.")
            });

            var chapter = new ChapterData("Chapter 1", 0, 200, 10,
                introDialogue: intro, outroDialogue: outro);

            Assert.IsNotNull(chapter.IntroDialogue);
            Assert.IsTrue(chapter.IntroDialogue.HasLines);
            Assert.IsNotNull(chapter.OutroDialogue);
            Assert.IsTrue(chapter.OutroDialogue.HasLines);
        }

        [Test]
        public void ChapterData_IntroOutroDialogue_DefaultToNull()
        {
            var chapter = new ChapterData("Chapter 1", 0, 200, 10);

            Assert.IsNull(chapter.IntroDialogue);
            Assert.IsNull(chapter.OutroDialogue);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static BossConversation CreateTestConversation(string unitName, string bossName)
        {
            var dialogue = new DialogueScript(new List<DialogueLine>
            {
                new DialogueLine(unitName, $"Face me, {bossName}!"),
                new DialogueLine(bossName, "You dare challenge me?")
            });
            return new BossConversation(unitName, bossName, dialogue);
        }
    }
}
