using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class SolarCleaningConfigTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";
        private const string Body =
            "minigame.solar.lanes;3;carriles;diseno\n" +
            "minigame.solar.row_seconds;1.5;s_reales;diseno\n" +
            "minigame.solar.rows;8;filas;diseno\n" +
            "minigame.solar.dirty_probability;0.25;fraccion;diseno\n" +
            "minigame.solar.shot_kwh;0.5;kWh;diseno\n";

        [Test]
        public void FromBalance_ReadsAllKeys()
        {
            var config = SolarCleaningConfig.FromBalance(BalanceLoader.Parse(Header + Body));
            Assert.That(config.Lanes, Is.EqualTo(3));
            Assert.That(config.RowSeconds, Is.EqualTo(1.5f));
            Assert.That(config.Rows, Is.EqualTo(8));
            Assert.That(config.DirtyProbability, Is.EqualTo(0.25f));
            Assert.That(config.ShotKwh, Is.EqualTo(0.5f));
        }

        [TestCase("minigame.solar.lanes")]
        [TestCase("minigame.solar.row_seconds")]
        [TestCase("minigame.solar.rows")]
        [TestCase("minigame.solar.dirty_probability")]
        [TestCase("minigame.solar.shot_kwh")]
        public void FromBalance_MissingKeyThrowsWithKeyName(string key)
        {
            var lines = new List<string>(Body.Split('\n'));
            lines.RemoveAll(l => l.StartsWith(key + ";") || l.Length == 0);
            var data = BalanceLoader.Parse(Header + string.Join("\n", lines));
            var ex = Assert.Throws<KeyNotFoundException>(() => SolarCleaningConfig.FromBalance(data));
            Assert.That(ex.Message, Does.Contain(key));
        }

        [Test]
        public void FromBalance_NullDataThrows()
        {
            Assert.Throws<ArgumentNullException>(() => SolarCleaningConfig.FromBalance(null));
        }

        [Test]
        public void FromBalance_NonIntegerLanesThrows()
        {
            var data = BalanceLoader.Parse(Header + Body.Replace("lanes;3", "lanes;2.5"));
            Assert.Throws<ArgumentOutOfRangeException>(() => SolarCleaningConfig.FromBalance(data));
        }

        [TestCase(0, 1f, 1, 0.5f, 1f)]
        [TestCase(3, 0f, 1, 0.5f, 1f)]
        [TestCase(3, float.NaN, 1, 0.5f, 1f)]
        [TestCase(3, 1f, 0, 0.5f, 1f)]
        [TestCase(3, 1f, 1, -0.1f, 1f)]
        [TestCase(3, 1f, 1, 1.1f, 1f)]
        [TestCase(3, 1f, 1, 0.5f, -1f)]
        [TestCase(3, 1f, 1, 0.5f, float.PositiveInfinity)]
        public void Constructor_InvalidValuesThrow(int lanes, float row, int rows, float prob, float shot)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SolarCleaningConfig(lanes, row, rows, prob, shot));
        }
    }
}
