// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Clock;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class DayNightClockTests
    {
        private const float Tolerance = 0.00001f;

        [Test]
        public void Constructor_StartsAtDawnOfFirstSol()
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f));

            Assert.That(clock.CurrentSol, Is.EqualTo(1));
            Assert.That(clock.IsNight, Is.False);
            Assert.That(clock.DayProgress, Is.Zero);
            Assert.That(clock.GameDeltaTime, Is.Zero);
        }

        [Test]
        public void Constructor_RejectsNullConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => new DayNightClock(null));
        }

        [TestCase(0.25f, false)]
        [TestCase(0.5f, true)]
        [TestCase(0.75f, true)]
        public void Constructor_UsesGameTimeForStartProgress(float progress, bool isNight)
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f, progress));

            Assert.That(clock.DayProgress, Is.EqualTo(progress).Within(Tolerance));
            Assert.That(clock.IsNight, Is.EqualTo(isNight));
            Assert.That(clock.CurrentSol, Is.EqualTo(1));
        }

        [Test]
        public void Tick_AgreedDurations_StartNightAt360AndNextSolAt480()
        {
            var clock = new DayNightClock(new DayNightConfig(360f, 120f, 3f));
            var events = RecordEvents(clock);

            clock.Tick(359f);
            Assert.That(clock.IsNight, Is.False);
            Assert.That(events, Is.Empty);

            clock.Tick(1f);
            Assert.That(clock.IsNight, Is.True);
            Assert.That(clock.DayProgress, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(events, Is.EqualTo(new[] { "night:1" }));

            clock.Tick(119f);
            Assert.That(clock.IsNight, Is.True);
            Assert.That(clock.CurrentSol, Is.EqualTo(1));

            clock.Tick(1f);
            Assert.That(clock.IsNight, Is.False);
            Assert.That(clock.CurrentSol, Is.EqualTo(2));
            Assert.That(clock.DayProgress, Is.Zero);
            Assert.That(events, Is.EqualTo(new[] { "night:1", "day:2" }));
        }

        [Test]
        public void Tick_AppliesScaleOnlyToNightAndResetsLastDelta()
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f));

            clock.Tick(2f);
            Assert.That(clock.GameDeltaTime, Is.EqualTo(2f));
            clock.Tick(10f);
            clock.Tick(2f);
            Assert.That(clock.GameDeltaTime, Is.EqualTo(6f));
            Assert.That(clock.DayProgress, Is.EqualTo(0.75f).Within(Tolerance));
            clock.Tick(0f);
            Assert.That(clock.GameDeltaTime, Is.Zero);
            Assert.That(clock.DayProgress, Is.EqualTo(0.75f).Within(Tolerance));
        }

        [Test]
        public void Tick_CrossingDusk_WeightsEachRealTimeSegment()
        {
            var clock = new DayNightClock(new DayNightConfig(360f, 120f, 3f));
            clock.Tick(350f);
            var events = RecordEvents(clock);

            clock.Tick(20f);

            Assert.That(clock.GameDeltaTime, Is.EqualTo(40f));
            Assert.That(clock.DayProgress, Is.EqualTo(390f / 720f).Within(Tolerance));
            Assert.That(events, Is.EqualTo(new[] { "night:1" }));
        }

        [Test]
        public void Tick_CrossingDawn_PreservesRemainingDayTime()
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f, 0.75f));
            var events = RecordEvents(clock);

            clock.Tick(5f);

            Assert.That(clock.GameDeltaTime, Is.EqualTo(9f));
            Assert.That(clock.CurrentSol, Is.EqualTo(2));
            Assert.That(clock.IsNight, Is.False);
            Assert.That(clock.DayProgress, Is.EqualTo(3f / 24f).Within(Tolerance));
            Assert.That(events, Is.EqualTo(new[] { "day:2" }));
        }

        [Test]
        public void Tick_CrossingSeveralSols_EmitsEveryTransitionInOrder()
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f));
            var events = RecordEvents(clock);

            clock.Tick(34f);

            Assert.That(clock.CurrentSol, Is.EqualTo(3));
            Assert.That(clock.IsNight, Is.False);
            Assert.That(clock.GameDeltaTime, Is.EqualTo(50f));
            Assert.That(clock.DayProgress, Is.EqualTo(2f / 24f).Within(Tolerance));
            Assert.That(events, Is.EqualTo(new[] { "night:1", "day:2", "night:2", "day:3" }));
        }

        [Test]
        public void Events_ObserveUpdatedStateAtEachBoundary()
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f));
            int nightCount = 0;
            int dayCount = 0;
            clock.OnNightStarted += sol =>
            {
                nightCount++;
                Assert.That(clock.CurrentSol, Is.EqualTo(sol));
                Assert.That(clock.IsNight, Is.True);
                Assert.That(clock.DayProgress, Is.EqualTo(0.5f).Within(Tolerance));
            };
            clock.OnDayStarted += sol =>
            {
                dayCount++;
                Assert.That(clock.CurrentSol, Is.EqualTo(sol));
                Assert.That(clock.IsNight, Is.False);
                Assert.That(clock.DayProgress, Is.Zero);
            };

            clock.Tick(32f);

            Assert.That(nightCount, Is.EqualTo(2));
            Assert.That(dayCount, Is.EqualTo(2));
        }

        [Test]
        public void Tick_UsesConfiguredDurationsAndScale()
        {
            var clock = new DayNightClock(new DayNightConfig(10f, 5f, 2f));
            clock.Tick(10f);
            Assert.That(clock.DayProgress, Is.EqualTo(0.5f).Within(Tolerance));
            clock.Tick(5f);
            Assert.That(clock.GameDeltaTime, Is.EqualTo(10f));
            Assert.That(clock.CurrentSol, Is.EqualTo(2));
        }

        [Test]
        public void Tick_UnequalGamePhases_UsesTheirActualLengths()
        {
            var clock = new DayNightClock(new DayNightConfig(10f, 5f, 4f));
            clock.Tick(10f);
            Assert.That(clock.DayProgress, Is.EqualTo(1f / 3f).Within(Tolerance));
        }

        [Test]
        public void Tick_PartitioningTime_PreservesStateEventsAndTotalGameTime()
        {
            var config = new DayNightConfig(12f, 4f, 3f, 0.25f);
            var single = new DayNightClock(config);
            var partitioned = new DayNightClock(config);
            var singleEvents = RecordEvents(single);
            var partitionedEvents = RecordEvents(partitioned);
            float totalGameTime = 0f;

            single.Tick(40f);
            for (int i = 0; i < 160; i++)
            {
                partitioned.Tick(0.25f);
                totalGameTime += partitioned.GameDeltaTime;
            }

            Assert.That(partitioned.CurrentSol, Is.EqualTo(single.CurrentSol));
            Assert.That(partitioned.IsNight, Is.EqualTo(single.IsNight));
            Assert.That(partitioned.DayProgress, Is.EqualTo(single.DayProgress).Within(Tolerance));
            Assert.That(totalGameTime, Is.EqualTo(single.GameDeltaTime).Within(Tolerance));
            Assert.That(partitionedEvents, Is.EqualTo(singleEvents));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(float.MaxValue)]
        public void Tick_RejectsInvalidAdvanceWithoutChangingState(float invalid)
        {
            var clock = new DayNightClock(new DayNightConfig(12f, 4f, 3f));
            clock.Tick(2f);
            var events = RecordEvents(clock);
            float progress = clock.DayProgress;

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Tick(invalid));

            Assert.That(clock.CurrentSol, Is.EqualTo(1));
            Assert.That(clock.IsNight, Is.False);
            Assert.That(clock.DayProgress, Is.EqualTo(progress));
            Assert.That(clock.GameDeltaTime, Is.EqualTo(2f));
            Assert.That(events, Is.Empty);
        }

        private static List<string> RecordEvents(DayNightClock clock)
        {
            var events = new List<string>();
            clock.OnNightStarted += sol => events.Add("night:" + sol);
            clock.OnDayStarted += sol => events.Add("day:" + sol);
            return events;
        }
    }
}
