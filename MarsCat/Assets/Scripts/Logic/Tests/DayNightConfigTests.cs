// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Clock;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class DayNightConfigTests
    {
        [Test]
        public void Constructor_PreservesProvidedValues()
        {
            var config = new DayNightConfig(12f, 4f, 2f, 0.75f);

            Assert.That(config.DaySeconds, Is.EqualTo(12f));
            Assert.That(config.NightSeconds, Is.EqualTo(4f));
            Assert.That(config.NightTimeScale, Is.EqualTo(2f));
            Assert.That(config.StartProgress, Is.EqualTo(0.75f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidDurationsAndScale(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightConfig(invalid, 4f, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightConfig(12f, invalid, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightConfig(12f, 4f, invalid));
        }

        [TestCase(-0.1f)]
        [TestCase(1f)]
        [TestCase(2f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidStartProgress(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayNightConfig(12f, 4f, 2f, invalid));
        }

        [Test]
        public void Constructor_RejectsUnrepresentableGameDuration()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DayNightConfig(float.MaxValue, float.MaxValue, 2f));
        }
    }
}
