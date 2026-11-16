// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Data;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BuildingByproductTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";

        private static BuildingConfig Config(IDictionary<ResourceId, float> byproducts)
        {
            // Mismos valores que BuildingConfigTests.Config(): 20 por sol, sol de 10 s, dura 4 s.
            return new BuildingConfig("farm", 2f, 3f, 2f, ResourceId.Forage, 20f, 4f, 10f,
                EnergyPriority.None, 10f, byproducts);
        }

        private static Building RunningBuilding(ResourcePool pool, BuildingConfig config, float yield)
        {
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            var building = new Building(config, pool, grid);
            Assert.That(building.StartRepair(), Is.True);
            building.Tick(2f);
            building.ApplyPreparation(PreparationResult.Ok(yield));
            building.SetWorkerPresent(true);
            grid.Tick(1f, false);
            return building;
        }

        private static ResourcePool Pool(float fiberCapacity = 100f)
        {
            var capacities = ResourceConfigTests.FiniteValues(100f);
            capacities[ResourceId.Fiber] = fiberCapacity;
            return new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(10f), capacities));
        }

        [Test]
        public void Constructor_WithoutByproducts_HasNone()
        {
            Assert.That(BuildingConfigTests.Config().ByproductRatesPerSol.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_ConvertsByproductRatesPerSolToPerSecond()
        {
            var config = Config(new Dictionary<ResourceId, float> { { ResourceId.Fiber, 10f } });
            Assert.That(config.ByproductRatesPerSol[ResourceId.Fiber], Is.EqualTo(10f));
            Assert.That(config.ByproductRatesPerSecond[ResourceId.Fiber], Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void Constructor_RejectsProducedResourceOrNegativeRateAsByproduct()
        {
            Assert.Throws<ArgumentException>(() => Config(new Dictionary<ResourceId, float> { { ResourceId.Forage, 1f } }));
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(new Dictionary<ResourceId, float> { { ResourceId.Fiber, -1f } }));
        }

        [Test]
        public void Running_DeliversByproductsWithTheSameYield()
        {
            var pool = Pool();
            var config = Config(new Dictionary<ResourceId, float>
            {
                { ResourceId.Fiber, 10f },
                { ResourceId.CropResidue, 5f },
            });
            Building building = RunningBuilding(pool, config, 1.5f);

            building.Tick(1f);

            Assert.That(pool.Get(ResourceId.Forage), Is.EqualTo(13f).Within(1e-4f));
            Assert.That(pool.Get(ResourceId.Fiber), Is.EqualTo(11.5f).Within(1e-4f));
            Assert.That(pool.Get(ResourceId.CropResidue), Is.EqualTo(10.75f).Within(1e-4f));
        }

        [Test]
        public void FullPool_KeepsByproductExcessQueued()
        {
            var pool = Pool(fiberCapacity: 11f);
            var config = Config(new Dictionary<ResourceId, float> { { ResourceId.Fiber, 10f } });
            Building building = RunningBuilding(pool, config, 1.5f);

            building.Tick(1f);
            Assert.That(pool.Get(ResourceId.Fiber), Is.EqualTo(11f).Within(1e-4f));

            Assert.That(pool.TryConsume(ResourceId.Fiber, 5f), Is.True);
            building.Tick(1f);
            // 0,5 en cola del tick anterior + 1,5 del nuevo.
            Assert.That(pool.Get(ResourceId.Fiber), Is.EqualTo(8f).Within(1e-4f));
        }

        [Test]
        public void FromBalance_ReadsOptionalByproductKeys()
        {
            BalanceData data = BalanceLoader.Parse(Header +
                "clock.day_seconds;6;s;diseno\nclock.night_seconds;2;s;diseno\nclock.night_time_scale;2;factor;diseno\n" +
                "building.farm.energy_priority;0;p;diseno\nbuilding.farm.repair_metal_cost;1;u;diseno\n" +
                "building.farm.repair_debris_cost;1;u;diseno\nbuilding.farm.repair_seconds;1;s;diseno\n" +
                "building.farm.production_rate_per_sol;10;kg;diseno\nbuilding.farm.run_seconds;10;s;diseno\n" +
                "building.farm.energy_rate_per_sol;1;kWh;diseno\n" +
                "building.farm.byproduct.fiber_per_sol;0.5;kg;diseno\n" +
                "building.farm.byproduct.crop_residue_per_sol;0.2;kg;diseno\n");

            BuildingConfig config = BuildingConfig.FromBalance(data, "farm", ResourceId.Forage);

            Assert.That(config.ByproductRatesPerSol.Count, Is.EqualTo(2));
            Assert.That(config.ByproductRatesPerSol[ResourceId.Fiber], Is.EqualTo(0.5f));
            Assert.That(config.ByproductRatesPerSol[ResourceId.CropResidue], Is.EqualTo(0.2f));
            Assert.That(config.ByproductRatesPerSol.ContainsKey(ResourceId.Water), Is.False);
        }
    }
}
