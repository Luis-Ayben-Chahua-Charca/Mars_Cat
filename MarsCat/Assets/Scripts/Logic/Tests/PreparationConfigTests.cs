using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Preparation;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class PreparationConfigTests
    {
        private const string Csv = "clave;valor;unidad;fuente\n" +
            "minigame.soil.wash.batch_kg;0.25;kg;diseno\n" +
            "minigame.soil.wash.initial_percent;0.5;porcentaje;nasa\n" +
            "minigame.soil.wash.threshold_percent;0.1;porcentaje;tierra\n" +
            "minigame.soil.wash.guide;1;L;derivado\n" +
            "minigame.soil.wash.min;1;L;derivado\n" +
            "minigame.soil.wash.max;2;L;derivado\n" +
            "minigame.soil.wash.weight;1;peso;diseno\n" +
            "minigame.soil.mix.batch_kg;1;kg;diseno\n" +
            "minigame.soil.mix.compost.guide;20;porcentaje;tierra\n" +
            "minigame.soil.mix.compost.min;0;porcentaje;tierra\n" +
            "minigame.soil.mix.compost.max;30;porcentaje;tierra\n" +
            "minigame.soil.mix.bacteria.guide;5;porcentaje;diseno\n" +
            "minigame.soil.mix.bacteria.min;1;porcentaje;diseno\n" +
            "minigame.soil.mix.bacteria.max;10;porcentaje;diseno\n" +
            "minigame.soil.mix.weight;2;peso;diseno\n" +
            "yield.min;0.8;factor;diseno\n" +
            "yield.max;1.2;factor;diseno\n" +
            "yield.plateau;0.1;fraccion;diseno\n" +
            "yield.robot;1;factor;diseno\n";

        private static string Without(string key)
        {
            var lines = new List<string>(Csv.Split('\n'));
            lines.RemoveAll(line => line.StartsWith(key + ";", StringComparison.Ordinal));
            return string.Join("\n", lines);
        }

        [Test]
        public void YieldConfig_ReadsYieldKeysAndPerStepWeights()
        {
            var config = YieldConfig.FromBalance(BalanceLoader.Parse(Csv), "soil", new[] { "wash", "mix" });
            Assert.That(config.Min, Is.EqualTo(0.8f));
            Assert.That(config.Max, Is.EqualTo(1.2f));
            Assert.That(config.Plateau, Is.EqualTo(0.1f));
            Assert.That(config.RobotYield, Is.EqualTo(1f));
            Assert.That(config.WeightOf("wash"), Is.EqualTo(1f));
            Assert.That(config.WeightOf("mix"), Is.EqualTo(2f));
            Assert.That(config.WeightOf("light"), Is.EqualTo(1f));
        }

        [Test]
        public void YieldConfig_ReportsTheMissingKey()
        {
            var data = BalanceLoader.Parse(Without("yield.plateau"));
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                YieldConfig.FromBalance(data, "soil", new string[0])).Message, Does.Contain("yield.plateau"));
            var noWeight = BalanceLoader.Parse(Without("minigame.soil.mix.weight"));
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                YieldConfig.FromBalance(noWeight, "soil", new[] { "wash", "mix" })).Message,
                Does.Contain("minigame.soil.mix.weight"));
        }

        [Test]
        public void YieldConfig_RejectsInvalidValuesAndNull()
        {
            Assert.Throws<ArgumentNullException>(() => YieldConfig.FromBalance(null, "soil", null));
            Assert.Throws<ArgumentException>(() => YieldConfig.FromBalance(BalanceLoader.Parse(Csv), " ", null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new YieldConfig(-1f, 1f, 0.1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new YieldConfig(1f, 0.5f, 0.1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new YieldConfig(0.8f, 1.2f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new YieldConfig(0.8f, 1.2f, 0.1f, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new YieldConfig(0.8f, 1.2f, 0.1f, 1f,
                new Dictionary<string, float> { { "a", -1f } }));
            Assert.Throws<ArgumentException>(() => new YieldConfig(0.8f, 1.2f, 0.1f, 1f).WeightOf(""));
        }

        [Test]
        public void WashConfig_ReadsTheSoilWashKeys()
        {
            var config = WashConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.BatchKg, Is.EqualTo(0.25f));
            Assert.That(config.InitialPercent, Is.EqualTo(0.5f));
            Assert.That(config.ThresholdPercent, Is.EqualTo(0.1f));
            Assert.That(config.GuideLiters, Is.EqualTo(1f));
            Assert.That(config.MinLiters, Is.EqualTo(1f));
            Assert.That(config.MaxLiters, Is.EqualTo(2f));
            Assert.That(config.Plateau, Is.EqualTo(0.1f));
        }

        [TestCase("minigame.soil.wash.batch_kg")]
        [TestCase("minigame.soil.wash.initial_percent")]
        [TestCase("minigame.soil.wash.threshold_percent")]
        [TestCase("minigame.soil.wash.guide")]
        [TestCase("minigame.soil.wash.min")]
        [TestCase("minigame.soil.wash.max")]
        [TestCase("yield.plateau")]
        public void WashConfig_ReportsTheMissingKey(string key)
        {
            var data = BalanceLoader.Parse(Without(key));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => WashConfig.FromBalance(data)).Message,
                Does.Contain(key));
        }

        [Test]
        public void WashConfig_RejectsInvalidValues()
        {
            Assert.Throws<ArgumentNullException>(() => WashConfig.FromBalance(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WashConfig(0f, 0.5f, 0.1f, 1f, 1f, 2f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WashConfig(0.25f, -1f, 0.1f, 1f, 1f, 2f, 0.1f));
            Assert.Throws<ArgumentException>(() => new WashConfig(0.25f, 0.5f, 0.1f, 3f, 1f, 2f, 0.1f));
        }

        [Test]
        public void MixConfig_ReadsTheSoilMixKeys()
        {
            var config = MixConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.BatchKg, Is.EqualTo(1f));
            Assert.That(config.CompostGuide, Is.EqualTo(20f));
            Assert.That(config.CompostMin, Is.EqualTo(0f));
            Assert.That(config.CompostMax, Is.EqualTo(30f));
            Assert.That(config.BacteriaGuide, Is.EqualTo(5f));
            Assert.That(config.BacteriaMin, Is.EqualTo(1f));
            Assert.That(config.BacteriaMax, Is.EqualTo(10f));
            Assert.That(config.Plateau, Is.EqualTo(0.1f));
        }

        [TestCase("minigame.soil.mix.batch_kg")]
        [TestCase("minigame.soil.mix.compost.guide")]
        [TestCase("minigame.soil.mix.compost.min")]
        [TestCase("minigame.soil.mix.compost.max")]
        [TestCase("minigame.soil.mix.bacteria.guide")]
        [TestCase("minigame.soil.mix.bacteria.min")]
        [TestCase("minigame.soil.mix.bacteria.max")]
        public void MixConfig_ReportsTheMissingKey(string key)
        {
            var data = BalanceLoader.Parse(Without(key));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => MixConfig.FromBalance(data)).Message,
                Does.Contain(key));
        }

        [Test]
        public void MixConfig_RejectsInvalidValues()
        {
            Assert.Throws<ArgumentNullException>(() => MixConfig.FromBalance(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MixConfig(0f, 20f, 0f, 30f, 5f, 1f, 10f, 0.1f));
            Assert.Throws<ArgumentException>(() => new MixConfig(1f, 40f, 0f, 30f, 5f, 1f, 10f, 0.1f));
            Assert.Throws<ArgumentException>(() => new MixConfig(1f, 20f, 0f, 30f, 0.5f, 1f, 10f, 0.1f));
        }
    }
}
