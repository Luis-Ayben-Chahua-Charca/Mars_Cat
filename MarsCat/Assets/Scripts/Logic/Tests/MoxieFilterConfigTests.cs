// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Air;
using MarsCat.Logic.Data;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MoxieFilterConfigTests
    {
        private const string Csv = "clave;valor;unidad;fuente\n" +
            "minigame.moxie.filter_count;5;filtros;diseno\n" +
            "minigame.moxie.dirty_interval_seconds;144;s_juego;diseno\n" +
            "minigame.moxie.cleaning_seconds_per_filter;10;s_juego;diseno\n";

        [Test]
        public void FromBalance_ReadsAgreedValuesAndAllowsDifferentDurations()
        {
            var config = MoxieFilterConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.FilterCount, Is.EqualTo(5));
            Assert.That(config.DirtyIntervalSeconds, Is.EqualTo(144f));
            Assert.That(config.CleaningSecondsPerFilter, Is.EqualTo(10f));
            var adjusted = MoxieFilterConfig.FromBalance(BalanceLoader.Parse(
                Csv.Replace(";144;", ";200;").Replace(";10;", ";8;")));
            Assert.That(adjusted.DirtyIntervalSeconds, Is.EqualTo(200f));
            Assert.That(adjusted.CleaningSecondsPerFilter, Is.EqualTo(8f));
        }

        [Test]
        public void FromBalance_RejectsMissingKeysAndNull()
        {
            Assert.Throws<ArgumentNullException>(() => MoxieFilterConfig.FromBalance(null));
            Assert.Throws<KeyNotFoundException>(() => MoxieFilterConfig.FromBalance(
                BalanceLoader.Parse(Csv.Replace("minigame.moxie.cleaning_seconds_per_filter;10;s_juego;diseno\n", ""))));
        }

        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("1.5")]
        [TestCase("2147483648")]
        public void FromBalance_RejectsInvalidFilterCount(string count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MoxieFilterConfig.FromBalance(
                BalanceLoader.Parse(Csv.Replace("filter_count;5", "filter_count;" + count))));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidDurations(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MoxieFilterConfig(5, value, 10f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MoxieFilterConfig(5, 144f, value));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_RejectsInvalidFilterCount(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MoxieFilterConfig(count, 144f, 10f));
        }
    }
}
