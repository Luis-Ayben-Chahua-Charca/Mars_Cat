// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class EnergyConfigTests
    {
        [Test]
        public void FromBalance_ConvertsHeatingUsingGameDurationOfSol()
        {
            var data = BalanceLoader.Parse("clave;valor;unidad;fuente\n" +
                "clock.day_seconds;12;s;diseno\nclock.night_seconds;4;s;diseno\n" +
                "clock.night_time_scale;2;factor;diseno\nenergy.heating.rate_per_sol;40;u;diseno");
            var config = EnergyConfig.FromBalance(data);
            Assert.That(config.GameSecondsPerSol, Is.EqualTo(20f));
            Assert.That(config.HeatingRatePerSecond, Is.EqualTo(2f));
        }

        [Test]
        public void FromBalance_RequiresHeatingKeyInsteadOfInventingValue()
        {
            var data = BalanceLoader.Parse("clave;valor;unidad;fuente\n" +
                "clock.day_seconds;12;s;diseno\nclock.night_seconds;4;s;diseno\nclock.night_time_scale;2;factor;diseno");
            Assert.That(Assert.Throws<KeyNotFoundException>(() => EnergyConfig.FromBalance(data)).Message,
                Does.Contain("energy.heating.rate_per_sol"));
            Assert.Throws<ArgumentNullException>(() => EnergyConfig.FromBalance(null));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidAmounts(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyConfig(invalid, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyConfig(10f, invalid));
        }

        [Test]
        public void Constructor_RejectsZeroSolAndOverflowButAcceptsZeroHeating()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyConfig(0f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyConfig(0.1f, float.MaxValue));
            Assert.That(new EnergyConfig(10f, 0f).HeatingRatePerSecond, Is.Zero);
        }
    }
}
