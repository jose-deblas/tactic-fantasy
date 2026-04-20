using NUnit.Framework;
using TacticFantasy.Domain.Support;

namespace DomainTests
{
    [TestFixture]
    public class BiorhythmTests
    {
        [Test]
        public void GetPhase_Turn0_DefaultOffset_IsHigh()
        {
            var bio = new Biorhythm(offset: 0);

            Assert.AreEqual(BiorhythmPhase.High, bio.GetPhase(0));
        }

        [Test]
        public void GetPhase_CyclesCorrectly()
        {
            var bio = new Biorhythm(offset: 0);

            Assert.AreEqual(BiorhythmPhase.High, bio.GetPhase(0));
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(1));
            Assert.AreEqual(BiorhythmPhase.Low, bio.GetPhase(2));
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(3));
        }

        [Test]
        public void GetPhase_RepeatsCycleAfterPeriod()
        {
            var bio = new Biorhythm(offset: 0);

            Assert.AreEqual(BiorhythmPhase.High, bio.GetPhase(4));
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(5));
            Assert.AreEqual(BiorhythmPhase.Low, bio.GetPhase(6));
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(7));
        }

        [Test]
        public void GetPhase_WithOffset_ShiftsCycle()
        {
            var bio = new Biorhythm(offset: 1);

            // offset=1: phase = (turn+1) % 4
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(0)); // (0+1)%4=1 → Normal
            Assert.AreEqual(BiorhythmPhase.Low, bio.GetPhase(1));    // (1+1)%4=2 → Low
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(2)); // (2+1)%4=3 → Normal
            Assert.AreEqual(BiorhythmPhase.High, bio.GetPhase(3));   // (3+1)%4=0 → High
        }

        [Test]
        public void GetPhase_DifferentOffsets_ProduceDifferentPhases()
        {
            var bio0 = new Biorhythm(offset: 0);
            var bio2 = new Biorhythm(offset: 2);

            // At turn 0: offset0=High, offset2=Low
            Assert.AreEqual(BiorhythmPhase.High, bio0.GetPhase(0));
            Assert.AreEqual(BiorhythmPhase.Low, bio2.GetPhase(0));
        }

        [Test]
        public void GetModifier_HighPhase_ReturnsPositiveBonus()
        {
            var bio = new Biorhythm(offset: 0);

            int mod = bio.GetModifier(0); // High phase

            Assert.AreEqual(Biorhythm.HighBonus, mod);
            Assert.Greater(mod, 0);
        }

        [Test]
        public void GetModifier_NormalPhase_ReturnsZero()
        {
            var bio = new Biorhythm(offset: 0);

            int mod = bio.GetModifier(1); // Normal phase

            Assert.AreEqual(0, mod);
        }

        [Test]
        public void GetModifier_LowPhase_ReturnsNegativePenalty()
        {
            var bio = new Biorhythm(offset: 0);

            int mod = bio.GetModifier(2); // Low phase

            Assert.AreEqual(Biorhythm.LowPenalty, mod);
            Assert.Less(mod, 0);
        }

        [Test]
        public void GetPhase_NegativeOffset_HandlesGracefully()
        {
            var bio = new Biorhythm(offset: -1);

            // (-1 + 0) % 4 = -1 → wraps to 3 → Normal
            Assert.AreEqual(BiorhythmPhase.Normal, bio.GetPhase(0));
            Assert.AreEqual(BiorhythmPhase.High, bio.GetPhase(1));
        }

        [Test]
        public void Deterministic_SameTurnSameOffset_AlwaysSameResult()
        {
            var bio1 = new Biorhythm(offset: 3);
            var bio2 = new Biorhythm(offset: 3);

            for (int turn = 0; turn < 20; turn++)
            {
                Assert.AreEqual(bio1.GetPhase(turn), bio2.GetPhase(turn));
                Assert.AreEqual(bio1.GetModifier(turn), bio2.GetModifier(turn));
            }
        }
    }
}
