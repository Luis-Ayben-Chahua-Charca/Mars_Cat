using System;
using System.Collections.Generic;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BuildingConfigTests
    {
        internal static BuildingConfig Config(float metal = 2f, float debris = 3f,
            float repair = 2f, float rate = 20f, float duration = 4f,
            float energy = 10f, float sol = 10f, string id = "farm")
        {
            return new BuildingConfig(id, metal, debris, repair, ResourceId.Food,
                rate, duration, energy, EnergyPriority.None, sol);
        }

        [Test]
        public void Constructor_PreservesCostsDurationsAndConvertsProductionRate()
        {
            var config = Config();
            Assert.That(config.Id, Is.EqualTo("farm"));
            Assert.That(config.RepairMetalCost, Is.EqualTo(2f));
            Assert.That(config.RepairDebrisCost, Is.EqualTo(3f));
            Assert.That(config.RepairSeconds, Is.EqualTo(2f));
            Assert.That(config.ProducedResource, Is.EqualTo(ResourceId.Food));
            Assert.That(config.ProductionRatePerSecond, Is.EqualTo(2f));
            Assert.That(config.RunSeconds, Is.EqualTo(4f));
            Assert.That(config.EnergyRatePerSol, Is.EqualTo(10f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidNumbers(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(metal: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(debris: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(repair: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(rate: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(duration: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(energy: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(sol: invalid));
        }

        [Test]
        public void Constructor_ValidatesIdsEnumsAndZeroDurations()
        {
            Assert.Throws<ArgumentException>(() => Config(id: " "));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(duration: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(sol: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingConfig("x", 0f, 0f, 0f,
                (ResourceId)99, 0f, 1f, 0f, EnergyPriority.None, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildingConfig("x", 0f, 0f, 0f,
                ResourceId.Food, 0f, 1f, 0f, (EnergyPriority)99, 1f));
            Assert.That(Config(metal: 0f, debris: 0f, repair: 0f, rate: 0f, energy: 0f).RepairSeconds, Is.Zero);
        }

        [Test]
        public void FromBalance_ReadsBuildingValuesAndConvertsUsingGameSol()
        {
            var data = BalanceLoader.Parse(Csv());
            var config = BuildingConfig.FromBalance(data, "farm", ResourceId.Water);
            Assert.That(config.ProductionRatePerSecond, Is.EqualTo(2f));
            Assert.That(config.RepairMetalCost, Is.EqualTo(2f));
            Assert.That(config.RepairDebrisCost, Is.EqualTo(3f));
            Assert.That(config.RepairSeconds, Is.EqualTo(4f));
            Assert.That(config.RunSeconds, Is.EqualTo(5f));
            Assert.That(config.EnergyRatePerSol, Is.EqualTo(60f));
            Assert.That(config.EnergyPriority, Is.EqualTo(EnergyPriority.High));
            Assert.That(config.ProducedResource, Is.EqualTo(ResourceId.Water));
        }

        [Test]
        public void FromBalance_RejectsMissingKeysAndFractionalPriority()
        {
            Assert.Throws<ArgumentNullException>(() => BuildingConfig.FromBalance(null, "farm", ResourceId.Food));
            var data = BalanceLoader.Parse(Csv());
            Assert.Throws<ArgumentException>(() => BuildingConfig.FromBalance(data, " ", ResourceId.Food));
            Assert.That(Assert.Throws<KeyNotFoundException>(() =>
                BuildingConfig.FromBalance(data, "missing", ResourceId.Food)).Message,
                Does.Contain("building.missing.energy_priority"));
            data = BalanceLoader.Parse(Csv().Replace("energy_priority;1", "energy_priority;1.5"));
            Assert.Throws<ArgumentOutOfRangeException>(() => BuildingConfig.FromBalance(data, "farm", ResourceId.Food));
        }

        private static string Csv()
        {
            return "clave;valor;unidad;fuente\nclock.day_seconds;12;s;diseno\n" +
                "clock.night_seconds;4;s;diseno\nclock.night_time_scale;2;factor;diseno\n" +
                "building.farm.repair_metal_cost;2;u;diseno\nbuilding.farm.repair_debris_cost;3;u;diseno\n" +
                "building.farm.repair_seconds;4;s;diseno\nbuilding.farm.production_rate_per_sol;40;u;diseno\n" +
                "building.farm.run_seconds;5;s;diseno\nbuilding.farm.energy_rate_per_sol;60;u;diseno\n" +
                "building.farm.energy_priority;1;grupo;diseno";
        }
    }
}
