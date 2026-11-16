// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Air;
using MarsCat.Logic.Data;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class AtmosphereConfigTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";

        private const string ClockRows =
            "clock.day_seconds;100;s;diseno\nclock.night_seconds;20;s;diseno\n" +
            "clock.night_time_scale;2;factor;diseno\n";

        private const string AirRows =
            "air.initial_volume_m3;40;m3;prueba\n" +
            "air.initial_fraction;0.2;fraccion;prueba\n" +
            "air.density;1.5;kg_por_m3;prueba\n" +
            "air.cat.rest_kg_per_sol;0.14;kg;prueba\n" +
            "air.cat.work_kg_per_sol;0.28;kg;prueba\n" +
            "air.new_module_fraction;0.15;fraccion;prueba\n" +
            "air.critical_seconds;30;s;prueba\n" +
            "air.band.yellow;0.18;fraccion;prueba\n" +
            "air.band.red;0.15;fraccion;prueba\n" +
            "air.band.critical;0.08;fraccion;prueba\n";

        private static AtmosphereConfig Make(float gameSecondsPerSol = 140f, float initial = 0.2f,
            float yellow = 0.18f, float red = 0.15f, float critical = 0.08f, float rest = 0.14f,
            float work = 0.28f, float criticalSeconds = 30f, float density = 1.5f,
            float newModule = 0.15f, float volume = 40f)
        {
            return new AtmosphereConfig(gameSecondsPerSol, initial, yellow, red, critical,
                rest, work, criticalSeconds, density, newModule, volume);
        }

        [Test]
        public void FromBalance_ReadsAirKeysAndConvertsRatesPerSolToPerGameSecond()
        {
            var config = AtmosphereConfig.FromBalance(BalanceLoader.Parse(Header + ClockRows + AirRows));
            Assert.That(config.GameSecondsPerSol, Is.EqualTo(140f));
            Assert.That(config.InitialFraction, Is.EqualTo(0.2f));
            Assert.That(config.YellowBelow, Is.EqualTo(0.18f));
            Assert.That(config.RedBelow, Is.EqualTo(0.15f));
            Assert.That(config.CriticalBelow, Is.EqualTo(0.08f));
            Assert.That(config.RestKgPerSecond, Is.EqualTo(0.14f / 140f).Within(1e-9f));
            Assert.That(config.WorkKgPerSecond, Is.EqualTo(0.28f / 140f).Within(1e-9f));
            Assert.That(config.CriticalSeconds, Is.EqualTo(30f));
            Assert.That(config.AirDensityKgPerM3, Is.EqualTo(1.5f));
            Assert.That(config.NewModuleFraction, Is.EqualTo(0.15f));
            Assert.That(config.InitialVolumeM3, Is.EqualTo(40f));
        }

        [TestCase("air.initial_fraction")]
        [TestCase("air.band.yellow")]
        [TestCase("air.band.red")]
        [TestCase("air.band.critical")]
        [TestCase("air.cat.rest_kg_per_sol")]
        [TestCase("air.cat.work_kg_per_sol")]
        [TestCase("air.critical_seconds")]
        [TestCase("air.density")]
        [TestCase("air.new_module_fraction")]
        [TestCase("air.initial_volume_m3")]
        [TestCase("clock.day_seconds")]
        public void FromBalance_MissingKeyNamesTheKey(string key)
        {
            var rows = new List<string>((ClockRows + AirRows).Split('\n'));
            rows.RemoveAll(row => row.StartsWith(key + ";", StringComparison.Ordinal));
            var data = BalanceLoader.Parse(Header + string.Join("\n", rows));
            var ex = Assert.Throws<KeyNotFoundException>(() => AtmosphereConfig.FromBalance(data));
            Assert.That(ex.Message, Does.Contain(key));
        }

        [Test]
        public void FromBalance_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => AtmosphereConfig.FromBalance(null));
        }

        [Test]
        public void Constructor_RejectsInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(gameSecondsPerSol: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(initial: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(initial: 1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(rest: -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(work: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(criticalSeconds: -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(density: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(newModule: 1.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(volume: 0f));
        }

        [Test]
        public void Constructor_RequiresBandsInDescendingOrder()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(yellow: 0.15f, red: 0.15f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(red: 0.08f, critical: 0.08f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Make(yellow: 0.1f, red: 0.15f));
        }
    }
}
