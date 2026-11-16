// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Water;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MeltingChamberSessionTests
    {
        private static ResourcePool Pool(float electricity)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Electricity] = electricity;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        private static MeltingConfig Config(float oscillation = 0f)
        {
            return new MeltingConfig(0.4f, 5f, 0.2f, 0.2f, oscillation, 2f, 0.5f);
        }

        private static MeltingChamberSession Session(ResourcePool pool, float oscillation = 0f, int seed = 1)
        {
            return new MeltingChamberSession(Config(oscillation), pool, seed);
        }

        private static void Play(MeltingChamberSession session, float seconds, float input)
        {
            float elapsed = 0f;
            while (elapsed < seconds - 0.0001f)
            {
                session.Tick(0.1f, input);
                elapsed += 0.1f;
            }
        }

        [Test]
        public void Start_BeginsPressurizingAtInitialProgress()
        {
            var session = Session(Pool(0f));
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Pressurize));
            Assert.That(session.Progress, Is.EqualTo(0.4f));
            Assert.That(session.IsFinished, Is.False);
        }

        [Test]
        public void Pressurize_WithNeedleCenteredFillsInFiveSeconds()
        {
            var session = Session(Pool(0f));
            MeltPhase completed = MeltPhase.Lost;
            session.OnPhaseCompleted += phase => completed = phase;
            Play(session, 4.8f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Pressurize));
            Play(session, 0.4f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Heat));
            Assert.That(completed, Is.EqualTo(MeltPhase.Pressurize));
            Assert.That(session.Progress, Is.LessThan(0.5f));
        }

        [Test]
        public void Accelerate_HalvesTheTimeAndChargesElectricity()
        {
            var pool = Pool(1f);
            var session = Session(pool);
            session.SetAccelerated(true);
            Assert.That(session.IsAccelerated, Is.True);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(0.5f).Within(0.0001f));
            Play(session, 2.3f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Pressurize));
            Play(session, 0.3f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Heat));
        }

        [Test]
        public void Accelerate_WithoutElectricityDoesNotSpeedUpAndChargesNothing()
        {
            var pool = Pool(0.4f);
            var session = Session(pool);
            session.SetAccelerated(true);
            Assert.That(session.IsAccelerated, Is.False);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(0.4f));
            Play(session, 2.6f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Pressurize));
        }

        [Test]
        public void Accelerate_ChargesOnlyOncePerSession()
        {
            var pool = Pool(1f);
            var session = Session(pool);
            session.SetAccelerated(true);
            session.SetAccelerated(false);
            session.SetAccelerated(true);
            Assert.That(session.IsAccelerated, Is.True);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void Pressurize_FailureRestartsPhaseWithoutLosingTheBatch()
        {
            var session = Session(Pool(0f));
            int failed = 0;
            int finished = 0;
            session.OnPhaseFailed += phase => { if (phase == MeltPhase.Pressurize) failed++; };
            session.OnFinished += result => finished++;
            Play(session, 2.2f, 1f);
            Assert.That(failed, Is.EqualTo(1));
            Assert.That(finished, Is.Zero);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Pressurize));
            Assert.That(session.Progress, Is.LessThan(0.4f).And.GreaterThan(0f));
            Play(session, 5.6f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Heat));
        }

        [Test]
        public void Heat_FailureBoilsAndLosesTheBatch()
        {
            var session = Session(Pool(0f));
            Play(session, 5.2f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Heat));
            MeltPhase failed = MeltPhase.Done;
            PreparationResult result = PreparationResult.Ok(1f);
            session.OnPhaseFailed += phase => failed = phase;
            session.OnFinished += value => result = value;
            Play(session, 2.2f, -1f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Lost));
            Assert.That(failed, Is.EqualTo(MeltPhase.Heat));
            Assert.That(result.Starts, Is.False);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.water.boiled"));
        }

        [Test]
        public void Heat_CompletionFinishesWithNormalYield()
        {
            var session = Session(Pool(0f));
            var completed = new System.Collections.Generic.List<MeltPhase>();
            int finished = 0;
            PreparationResult result = PreparationResult.Fail("x");
            session.OnPhaseCompleted += completed.Add;
            session.OnFinished += value => { finished++; result = value; };
            Play(session, 10.4f, 0f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Done));
            Assert.That(session.IsFinished, Is.True);
            Assert.That(completed, Is.EqualTo(new[] { MeltPhase.Pressurize, MeltPhase.Heat }));
            Assert.That(finished, Is.EqualTo(1));
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(1f));
        }

        [Test]
        public void Needle_FollowsInputAndStaysInRange()
        {
            var session = Session(Pool(0f));
            session.Tick(0.01f, 0.2f);
            Assert.That(session.Needle, Is.EqualTo(0.6f).Within(0.0001f));
            session.Tick(0.01f, 1f);
            Assert.That(session.Needle, Is.EqualTo(1f).Within(0.0001f));
            session.Tick(0.01f, -1f);
            Assert.That(session.Needle, Is.Zero.Within(0.0001f));
        }

        [Test]
        public void Oscillation_MovesTheNeedleAndRepeatsWithTheSameSeed()
        {
            var a = Session(Pool(0f), 0.1f, 7);
            var b = Session(Pool(0f), 0.1f, 7);
            var c = Session(Pool(0f), 0.1f, 8);
            float min = 1f;
            float max = 0f;
            for (int i = 0; i < 50; i++)
            {
                a.Tick(0.1f, 0f);
                b.Tick(0.1f, 0f);
                c.Tick(0.1f, 0f);
                Assert.That(a.Needle, Is.EqualTo(b.Needle));
                min = Math.Min(min, a.Needle);
                max = Math.Max(max, a.Needle);
            }
            Assert.That(max - min, Is.GreaterThan(0.05f));
            Assert.That(c.Needle, Is.Not.EqualTo(a.Needle));
        }

        [Test]
        public void Accelerate_MakesTheNeedleMoreUnstable()
        {
            float Swing(bool accelerated)
            {
                var session = Session(Pool(10f), 0.05f, 3);
                if (accelerated)
                    session.SetAccelerated(true);
                float min = 1f;
                float max = 0f;
                for (int i = 0; i < 50; i++)
                {
                    session.Tick(0.05f, 0f);
                    min = Math.Min(min, session.Needle);
                    max = Math.Max(max, session.Needle);
                }
                return max - min;
            }

            Assert.That(Swing(true), Is.GreaterThan(Swing(false)));
        }

        [Test]
        public void Tick_AfterFinishingDoesNothing()
        {
            var session = Session(Pool(0f));
            Play(session, 10.4f, 0f);
            int events = 0;
            session.OnPhaseFailed += phase => events++;
            session.OnFinished += result => events++;
            session.Tick(5f, 1f);
            Assert.That(session.Phase, Is.EqualTo(MeltPhase.Done));
            Assert.That(events, Is.Zero);
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            var session = Session(Pool(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(-1f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(float.NaN, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(1f, 1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Tick(1f, float.NaN));
            Assert.Throws<ArgumentNullException>(() => new MeltingChamberSession(null, Pool(0f), 1));
            Assert.Throws<ArgumentNullException>(() => new MeltingChamberSession(Config(), null, 1));
        }
    }
}
