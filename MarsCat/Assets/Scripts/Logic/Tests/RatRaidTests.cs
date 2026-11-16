using System;
using MarsCat.Logic.Food;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class RatRaidTests
    {
        private LooseRats _loose;
        private RatFarm _farm;
        private RatRaid _raid;

        [SetUp]
        public void SetUp()
        {
            _loose = new LooseRats(20);
            _farm = new RatFarm(RatFarmTests.Config(min: 3, max: 10), RatFarmTests.Pool(0f));
            _raid = new RatRaid(_loose, _farm);
        }

        [Test]
        public void MarkAndCapture_MovesRatFromLooseToFarm()
        {
            Assert.That(_raid.Mark(4), Is.True);
            Assert.That(_raid.IsMarked(4), Is.True);
            Assert.That(_raid.Capture(4), Is.True);
            Assert.That(_farm.Rats, Is.EqualTo(1));
            Assert.That(_loose.Count, Is.EqualTo(19));
            Assert.That(_loose.IsLoose(4), Is.False);
            Assert.That(_raid.IsMarked(4), Is.False);
        }

        [Test]
        public void Mark_RejectsUnknownTakenOrAlreadyMarkedRats()
        {
            Assert.That(_raid.Mark(99), Is.False);
            Assert.That(_raid.Mark(1), Is.True);
            Assert.That(_raid.Mark(1), Is.False);
            _loose.TryTake(2);
            Assert.That(_raid.Mark(2), Is.False);
        }

        [Test]
        public void Capture_RequiresMark()
        {
            Assert.That(_raid.Capture(0), Is.False);
            Assert.That(_farm.Rats, Is.Zero);
            Assert.That(_loose.IsLoose(0), Is.True);
        }

        [Test]
        public void Capture_FailsIfAnotherCatHuntedTheRatFirst()
        {
            _raid.Mark(3);
            _loose.TryTake(3);
            Assert.That(_raid.Capture(3), Is.False);
            Assert.That(_farm.Rats, Is.Zero);
            Assert.That(_raid.IsMarked(3), Is.False);
        }

        [Test]
        public void Capture_FailsAndKeepsRatLooseWhenFarmIsFull()
        {
            _farm.AddRats(10);
            _raid.Mark(5);
            Assert.That(_raid.Capture(5), Is.False);
            Assert.That(_loose.IsLoose(5), Is.True);
            Assert.That(_farm.Rats, Is.EqualTo(10));
        }

        [Test]
        public void Unmark_AllowsMarkingAgain()
        {
            _raid.Mark(7);
            Assert.That(_raid.Unmark(7), Is.True);
            Assert.That(_raid.Unmark(7), Is.False);
            Assert.That(_raid.Mark(7), Is.True);
        }

        [Test]
        public void Result_FailsUntilFarmExceedsBreedingStock()
        {
            for (int id = 0; id < 3; id++)
            {
                _raid.Mark(id);
                _raid.Capture(id);
            }
            Assert.That(_raid.Result.Starts, Is.False);
            Assert.That(_raid.Result.ReasonKey, Is.EqualTo("prep.rat_farm.need_rats"));
            _raid.Mark(3);
            _raid.Capture(3);
            Assert.That(_raid.Result.Starts, Is.True);
            Assert.That(_raid.Result.Yield, Is.EqualTo(1f));
        }

        [Test]
        public void Constructor_RejectsNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new RatRaid(null, _farm));
            Assert.Throws<ArgumentNullException>(() => new RatRaid(_loose, null));
        }
    }
}
