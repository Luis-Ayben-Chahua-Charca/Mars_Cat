// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Water;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MeltingConfigTests
    {
        private const string Csv =
            "clave;valor;unidad;fuente\n" +
            "minigame.melt.initial_progress;0.3;fraccion;diseno\n" +
            "minigame.melt.fill_seconds;4;s_reales;diseno\n" +
            "minigame.melt.drain_per_second;0.25;fraccion_por_s;diseno\n" +
            "minigame.melt.green_width;0.4;fraccion;diseno\n" +
            "minigame.melt.oscillation;0.05;fraccion;diseno\n" +
            "minigame.melt.accelerate_factor;3;factor;diseno\n" +
            "minigame.melt.accelerate_extra_kwh;0.75;kWh;diseno\n" +
            "minigame.water.vein.liters;8;L_hielo;derivado\n" +
            "minigame.water.vein.purity;0.5;fraccion;nasa\n";

        [Test]
        public void FromBalance_ReadsEveryMeltKey()
        {
            var config = MeltingConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.InitialProgress, Is.EqualTo(0.3f));
            Assert.That(config.FillSeconds, Is.EqualTo(4f));
            Assert.That(config.DrainPerSecond, Is.EqualTo(0.25f));
            Assert.That(config.GreenWidth, Is.EqualTo(0.4f));
            Assert.That(config.Oscillation, Is.EqualTo(0.05f));
            Assert.That(config.AccelerateFactor, Is.EqualTo(3f));
            Assert.That(config.AccelerateExtraKwh, Is.EqualTo(0.75f));
        }

        [TestCase("minigame.melt.initial_progress")]
        [TestCase("minigame.melt.fill_seconds")]
        [TestCase("minigame.melt.drain_per_second")]
        [TestCase("minigame.melt.green_width")]
        [TestCase("minigame.melt.oscillation")]
        [TestCase("minigame.melt.accelerate_factor")]
        [TestCase("minigame.melt.accelerate_extra_kwh")]
        public void FromBalance_MissingKeyNamesTheKey(string key)
        {
            var lines = new List<string>(Csv.Split('\n'));
            lines.RemoveAll(line => line.StartsWith(key + ";", StringComparison.Ordinal));
            var data = BalanceLoader.Parse(string.Join("\n", lines));
            var ex = Assert.Throws<KeyNotFoundException>(() => MeltingConfig.FromBalance(data));
            Assert.That(ex.Message, Does.Contain(key));
        }

        [Test]
        public void FromBalance_NullDataThrows()
        {
            Assert.Throws<ArgumentNullException>(() => MeltingConfig.FromBalance(null));
        }

        [TestCase(0f, 5f, 0.2f, 0.2f, 0.1f, 2f, 0.5f)]
        [TestCase(1f, 5f, 0.2f, 0.2f, 0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 0f, 0.2f, 0.2f, 0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 5f, -0.1f, 0.2f, 0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 5f, 0.2f, 0f, 0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 5f, 0.2f, 1.5f, 0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 5f, 0.2f, 0.2f, -0.1f, 2f, 0.5f)]
        [TestCase(0.4f, 5f, 0.2f, 0.2f, 0.1f, 0.5f, 0.5f)]
        [TestCase(0.4f, 5f, 0.2f, 0.2f, 0.1f, 2f, -1f)]
        [TestCase(float.NaN, 5f, 0.2f, 0.2f, 0.1f, 2f, 0.5f)]
        public void Constructor_InvalidValuesThrow(float initial, float fill, float drain,
            float green, float oscillation, float factor, float extra)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MeltingConfig(initial, fill, drain, green, oscillation, factor, extra));
        }
    }
}
