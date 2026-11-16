using System;
using System.Collections.Generic;
using MarsCat.Logic.Data;
using MarsCat.Logic.Food;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class RatFarmConfigTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";

        private static string Csv()
        {
            // Valores propios de prueba, sin atribución a NASA ni al balance del juego.
            return Header + "clock.day_seconds;10;s;diseno\nclock.night_seconds;2;s;diseno\n" +
                "clock.night_time_scale;3;factor;diseno\nclock.start_progress;0;fraccion;diseno\n" +
                "building.rat_farm.rations_per_rat;3;raciones;diseno\n" +
                "building.rat_farm.convert_seconds;5;s_juego;diseno\n" +
                "building.rat_farm.forage_per_rat_per_sol;0.32;kg;diseno\n" +
                "building.rat_farm.waste_per_rat_per_sol;0.16;kg;diseno\n" +
                "building.rat_farm.max_rats;20;ratas;diseno\n" +
                "building.rat_farm.min_breeders;6;ratas;diseno\n" +
                "building.rat_farm.breed_rate;0.8;por_sol;diseno\n" +
                "rat.loose.initial;7;ratas;diseno\n";
        }

        [Test]
        public void FromBalance_ConvertsSolRatesToGameSeconds()
        {
            var config = RatFarmConfig.FromBalance(BalanceLoader.Parse(Csv()));
            // Sol de prueba: 10 + 2 * 3 = 16 s de juego.
            Assert.That(config.GameSecondsPerSol, Is.EqualTo(16f));
            Assert.That(config.RationsPerRat, Is.EqualTo(3f));
            Assert.That(config.ConvertSeconds, Is.EqualTo(5f));
            Assert.That(config.ForagePerRatPerSecond, Is.EqualTo(0.02f).Within(1e-6f));
            Assert.That(config.WastePerRatPerSecond, Is.EqualTo(0.01f).Within(1e-6f));
            Assert.That(config.BreedRatePerSecond, Is.EqualTo(0.05f).Within(1e-6f));
            Assert.That(config.MaxRats, Is.EqualTo(20));
            Assert.That(config.MinBreeders, Is.EqualTo(6));
        }

        [Test]
        public void FromBalance_ReportsMissingKey()
        {
            string csv = Csv().Replace("building.rat_farm.breed_rate;0.8;por_sol;diseno\n", "");
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                RatFarmConfig.FromBalance(BalanceLoader.Parse(csv))).Message,
                Does.Contain("building.rat_farm.breed_rate"));
        }

        [Test]
        public void FromBalance_RejectsNullAndFractionalCounts()
        {
            Assert.Throws<ArgumentNullException>(() => RatFarmConfig.FromBalance(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => RatFarmConfig.FromBalance(
                BalanceLoader.Parse(Csv().Replace("max_rats;20", "max_rats;20.5"))));
        }

        [Test]
        public void Constructor_RejectsInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(0f, 5f, 1f, 1f, 20, 6, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 0f, 1f, 1f, 20, 6, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, -1f, 1f, 20, 6, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, 1f, -1f, 20, 6, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, 1f, 1f, 0, 0, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, 1f, 1f, 20, 21, 1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, 1f, 1f, 20, 6, -1f, 16f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(3f, 5f, 1f, 1f, 20, 6, 1f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatFarmConfig(float.NaN, 5f, 1f, 1f, 20, 6, 1f, 16f));
        }

        [Test]
        public void LooseRats_FromBalanceReadsInitialCount()
        {
            var loose = LooseRats.FromBalance(BalanceLoader.Parse(Csv()));
            Assert.That(loose.Count, Is.EqualTo(7));
        }

        [Test]
        public void RatHuntConfig_FromBalanceReadsKeysAndReportsMissing()
        {
            string csv = Header + "rat.hunt_seconds;12;s_reales;diseno\ncat.food.ration;2;raciones;diseno\n";
            var config = RatHuntConfig.FromBalance(BalanceLoader.Parse(csv));
            Assert.That(config.HuntSeconds, Is.EqualTo(12f));
            Assert.That(config.FoodRation, Is.EqualTo(2f));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => RatHuntConfig.FromBalance(
                BalanceLoader.Parse(Header + "cat.food.ration;2;raciones;diseno\n"))).Message,
                Does.Contain("rat.hunt_seconds"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatHuntConfig(0f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RatHuntConfig(1f, 0f));
        }
    }
}
