// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Recycling;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class RecyclingConfigTests
    {
        // Los valores de temp.start, temp.ambient, temp.max_heat y humidity.* son de prueba.
        private const string Csv = "clave;valor;unidad;fuente\n" +
            "yield.plateau;0.1;fraccion;diseno\n" +
            "minigame.compost.mix.cat_waste.cn;10;C_N;estimacion\n" +
            "minigame.compost.mix.cat_waste.n_fraction;1;relativo;diseno\n" +
            "minigame.compost.mix.rat_waste.cn;75;C_N;derivado\n" +
            "minigame.compost.mix.rat_waste.n_fraction;1;relativo;diseno\n" +
            "minigame.compost.mix.crop_residue.cn;20;C_N;tierra\n" +
            "minigame.compost.mix.crop_residue.n_fraction;1;relativo;diseno\n" +
            "minigame.compost.mix.guide;30;C_N;tierra\n" +
            "minigame.compost.mix.min;20;C_N;tierra\n" +
            "minigame.compost.mix.max;40;C_N;tierra\n" +
            "minigame.compost.batch_kg;1;kg;diseno\n" +
            "minigame.compost.yield_ratio;0.6;fraccion;tierra\n" +
            "minigame.compost.cycle_seconds;60;s_reales;diseno\n" +
            "minigame.compost.temp.green_min;55;C;tierra\n" +
            "minigame.compost.temp.green_max;65;C;tierra\n" +
            "minigame.compost.temp.overheat;70;C;tierra\n" +
            "minigame.compost.humidity.green_min;50;porcentaje;tierra\n" +
            "minigame.compost.humidity.green_max;60;porcentaje;tierra\n" +
            "minigame.compost.humidity.drown;65;porcentaje;tierra\n" +
            "minigame.compost.humidity.dry;40;porcentaje;tierra\n" +
            "minigame.compost.cooling_per_second;0.5;C_por_s;diseno\n" +
            "minigame.compost.inertia;0.3;fraccion_por_s;diseno\n" +
            "minigame.compost.heat_kwh_per_second;0.005;kWh_por_s;diseno\n" +
            "minigame.compost.sprinkler_l_per_second;0.01;L_por_s;diseno\n" +
            "minigame.compost.temp.start;20;C;placeholder\n" +
            "minigame.compost.temp.ambient;5;C;placeholder\n" +
            "minigame.compost.temp.max_heat;80;C;placeholder\n" +
            "minigame.compost.humidity.start;55;porcentaje;placeholder\n" +
            "minigame.compost.humidity.sprinkler_per_second;2;porcentaje_por_s;placeholder\n" +
            "minigame.compost.humidity.dry_per_second;0.1;porcentaje_por_s;placeholder\n";

        private static string Without(string key)
        {
            var lines = new List<string>(Csv.Split('\n'));
            lines.RemoveAll(line => line.StartsWith(key + ";", StringComparison.Ordinal));
            return string.Join("\n", lines);
        }

        [Test]
        public void CompostMixConfig_ReadsInputsAndTarget()
        {
            var config = CompostMixConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.CarbonToNitrogen(ResourceId.CatWaste), Is.EqualTo(10f));
            Assert.That(config.CarbonToNitrogen(ResourceId.RatWaste), Is.EqualTo(75f));
            Assert.That(config.CarbonToNitrogen(ResourceId.CropResidue), Is.EqualTo(20f));
            Assert.That(config.NitrogenFraction(ResourceId.RatWaste), Is.EqualTo(1f));
            Assert.That(config.Guide, Is.EqualTo(30f));
            Assert.That(config.Min, Is.EqualTo(20f));
            Assert.That(config.Max, Is.EqualTo(40f));
            Assert.That(config.Plateau, Is.EqualTo(0.1f));
        }

        [TestCase("minigame.compost.mix.cat_waste.cn")]
        [TestCase("minigame.compost.mix.rat_waste.n_fraction")]
        [TestCase("minigame.compost.mix.crop_residue.cn")]
        [TestCase("minigame.compost.mix.guide")]
        [TestCase("minigame.compost.mix.min")]
        [TestCase("minigame.compost.mix.max")]
        [TestCase("yield.plateau")]
        public void CompostMixConfig_ReportsTheMissingKey(string key)
        {
            var data = BalanceLoader.Parse(Without(key));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => CompostMixConfig.FromBalance(data)).Message,
                Does.Contain(key));
        }

        [Test]
        public void CompostMixConfig_RejectsInvalidValuesAndUnknownInputs()
        {
            Assert.Throws<ArgumentNullException>(() => CompostMixConfig.FromBalance(null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CompostMixConfig(0f, 1f, 75f, 1f, 20f, 1f, 30f, 20f, 40f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CompostMixConfig(10f, 0f, 75f, 1f, 20f, 1f, 30f, 20f, 40f, 0.1f));
            Assert.Throws<ArgumentException>(() =>
                new CompostMixConfig(10f, 1f, 75f, 1f, 20f, 1f, 50f, 20f, 40f, 0.1f));
            var config = CompostMixConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.Throws<ArgumentException>(() => config.CarbonToNitrogen(ResourceId.Water));
            Assert.That(CompostMixConfig.IsInput(ResourceId.CatWaste), Is.True);
            Assert.That(CompostMixConfig.IsInput(ResourceId.Compost), Is.False);
        }

        [Test]
        public void CompostConfig_ReadsEveryKey()
        {
            var config = CompostConfig.FromBalance(BalanceLoader.Parse(Csv));
            Assert.That(config.TempGreenMin, Is.EqualTo(55f));
            Assert.That(config.TempGreenMax, Is.EqualTo(65f));
            Assert.That(config.OverheatTemp, Is.EqualTo(70f));
            Assert.That(config.HumidityGreenMin, Is.EqualTo(50f));
            Assert.That(config.HumidityGreenMax, Is.EqualTo(60f));
            Assert.That(config.DrownHumidity, Is.EqualTo(65f));
            Assert.That(config.DryHumidity, Is.EqualTo(40f));
            Assert.That(config.CoolingPerSecond, Is.EqualTo(0.5f));
            Assert.That(config.Inertia, Is.EqualTo(0.3f));
            Assert.That(config.HeatKwhPerSecond, Is.EqualTo(0.005f));
            Assert.That(config.SprinklerLitersPerSecond, Is.EqualTo(0.01f));
            Assert.That(config.CycleSeconds, Is.EqualTo(60f));
            Assert.That(config.BatchKg, Is.EqualTo(1f));
            Assert.That(config.YieldRatio, Is.EqualTo(0.6f));
            Assert.That(config.StartTemp, Is.EqualTo(20f));
            Assert.That(config.AmbientTemp, Is.EqualTo(5f));
            Assert.That(config.MaxHeatTemp, Is.EqualTo(80f));
            Assert.That(config.StartHumidity, Is.EqualTo(55f));
            Assert.That(config.SprinklerHumidityPerSecond, Is.EqualTo(2f));
            Assert.That(config.DryingPerSecond, Is.EqualTo(0.1f));
        }

        [TestCase("minigame.compost.temp.green_min")]
        [TestCase("minigame.compost.temp.overheat")]
        [TestCase("minigame.compost.humidity.drown")]
        [TestCase("minigame.compost.cooling_per_second")]
        [TestCase("minigame.compost.inertia")]
        [TestCase("minigame.compost.heat_kwh_per_second")]
        [TestCase("minigame.compost.sprinkler_l_per_second")]
        [TestCase("minigame.compost.cycle_seconds")]
        [TestCase("minigame.compost.batch_kg")]
        [TestCase("minigame.compost.yield_ratio")]
        [TestCase("minigame.compost.temp.start")]
        [TestCase("minigame.compost.temp.ambient")]
        [TestCase("minigame.compost.temp.max_heat")]
        [TestCase("minigame.compost.humidity.start")]
        [TestCase("minigame.compost.humidity.sprinkler_per_second")]
        [TestCase("minigame.compost.humidity.dry_per_second")]
        public void CompostConfig_ReportsTheMissingKey(string key)
        {
            var data = BalanceLoader.Parse(Without(key));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => CompostConfig.FromBalance(data)).Message,
                Does.Contain(key));
        }

        [Test]
        public void CompostConfig_RejectsInconsistentZonesAndInvalidValues()
        {
            Assert.Throws<ArgumentNullException>(() => CompostConfig.FromBalance(null));
            Assert.Throws<ArgumentException>(() => new CompostConfig(65f, 55f, 70f, 50f, 60f, 65f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, 60f, 1f, 0.6f, 20f, 5f, 80f, 55f, 2f, 0.1f));
            Assert.Throws<ArgumentException>(() => new CompostConfig(55f, 65f, 60f, 50f, 60f, 65f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, 60f, 1f, 0.6f, 20f, 5f, 80f, 55f, 2f, 0.1f));
            Assert.Throws<ArgumentException>(() => new CompostConfig(55f, 65f, 70f, 50f, 60f, 55f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, 60f, 1f, 0.6f, 20f, 5f, 80f, 55f, 2f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompostConfig(55f, 65f, 70f, 50f, 60f, 65f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, 0f, 1f, 0.6f, 20f, 5f, 80f, 55f, 2f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompostConfig(55f, 65f, 70f, 50f, 60f, 65f, 40f,
                -0.5f, 0.3f, 0.005f, 0.01f, 60f, 1f, 0.6f, 20f, 5f, 80f, 55f, 2f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompostConfig(55f, 65f, 70f, 50f, 60f, 65f, 40f,
                0.5f, 0.3f, 0.005f, 0.01f, 60f, 1f, 0.6f, 20f, 5f, 80f, 120f, 2f, 0.1f));
        }
    }
}
