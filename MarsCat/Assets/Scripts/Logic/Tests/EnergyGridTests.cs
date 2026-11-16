// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Energy;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class EnergyGridTests
    {
        private static ResourcePool Pool(float electricity = 0f, float capacity = 100f)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Electricity] = electricity;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(capacity)));
        }

        [Test]
        public void Solar_ChargesBatteryByYieldOnlyDuringDayAndRespectsCapacity()
        {
            var pool = Pool(0f, 8f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterProducer("solar", 20f, true);
            grid.SetProducerYield("solar", 0.5f);
            grid.Tick(3f, false);
            Assert.That(grid.SolarOutput, Is.EqualTo(1f));
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(3f));
            grid.Tick(3f, true);
            Assert.That(grid.SolarOutput, Is.Zero);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(3f));
            grid.Tick(20f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(8f));
        }

        [Test]
        public void Night_ConsumesHeatingBeforeOtherLoadsAndAllowsNonSolarProduction()
        {
            var pool = Pool(3f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 20f), pool);
            grid.RegisterProducer("reactor", 10f, false);
            grid.RegisterConsumer("moxie", 30f, EnergyPriority.Max);
            grid.Tick(1f, true);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(2f));
            Assert.That(grid.IsPowered("moxie"), Is.False);
            Assert.That(grid.HasPower, Is.False);
        }

        [Test]
        public void Generation_SuppliesLoadsBeforeApplyingBatteryCapacity()
        {
            var pool = Pool(5f, 5f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterProducer("solar", 100f, true);
            grid.RegisterConsumer("load", 100f, EnergyPriority.High);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("load"), Is.True);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(5f));
        }

        [Test]
        public void Blackout_ShedsNoneThenHighThenMaxAndRestoresInReverseOrder()
        {
            var pool = Pool(6f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterConsumer("farm", 20f, EnergyPriority.None);
            grid.RegisterConsumer("water", 20f, EnergyPriority.High);
            grid.RegisterConsumer("moxie", 20f, EnergyPriority.Max);
            int blackouts = 0;
            int restored = 0;
            grid.OnBlackout += () => blackouts++;
            grid.OnPowerRestored += () => restored++;
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("farm"), Is.True);
            pool.Add(ResourceId.Electricity, 4f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("farm"), Is.False);
            Assert.That(grid.IsPowered("water"), Is.True);
            Assert.That(grid.IsPowered("moxie"), Is.True);
            pool.Add(ResourceId.Electricity, 2f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("water"), Is.False);
            Assert.That(grid.IsPowered("moxie"), Is.True);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("moxie"), Is.False);
            pool.Add(ResourceId.Electricity, 2f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("moxie"), Is.True);
            Assert.That(grid.IsPowered("water"), Is.False);
            pool.Add(ResourceId.Electricity, 4f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("water"), Is.True);
            Assert.That(grid.IsPowered("farm"), Is.False);
            pool.Add(ResourceId.Electricity, 6f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("farm"), Is.True);
            Assert.That(grid.HasPower, Is.True);
            Assert.That(blackouts, Is.EqualTo(1));
            Assert.That(restored, Is.EqualTo(1));
        }

        [Test]
        public void EqualPriority_PreservesFirstRegistrationAndDoesNotSkipExpensiveLoad()
        {
            var pool = Pool(2f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterConsumer("first", 20f, EnergyPriority.None);
            grid.RegisterConsumer("last", 10f, EnergyPriority.None);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("first"), Is.True);
            Assert.That(grid.IsPowered("last"), Is.False);
            pool.Add(ResourceId.Electricity, 1f);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("first"), Is.False);
            Assert.That(grid.IsPowered("last"), Is.False);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(1f));
        }

        [Test]
        public void InactiveConsumer_DoesNotConsumeOrRemainPowered()
        {
            var pool = Pool(5f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterConsumer("load", 10f, EnergyPriority.None);
            grid.Tick(1f, false);
            grid.SetActive("load", false);
            Assert.That(grid.IsPowered("load"), Is.False);
            grid.Tick(1f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(4f));
            grid.SetActive("load", true);
            grid.Tick(1f, false);
            Assert.That(grid.IsPowered("load"), Is.True);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(3f));
        }

        [Test]
        public void Heating_IsNightOnlyAndBlackoutDoesNotOverdrawBattery()
        {
            var pool = Pool(3f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 20f), pool);
            grid.Tick(1f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(3f));
            grid.Tick(1f, true);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(1f));
            grid.Tick(1f, true);
            Assert.That(grid.HasPower, Is.False);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(1f));
        }

        [Test]
        public void PublicMethods_RejectDuplicateUnknownAndEmptyIds()
        {
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), Pool());
            grid.RegisterProducer("solar", 10f, true);
            grid.RegisterConsumer("load", 10f, EnergyPriority.High);
            Assert.Throws<ArgumentException>(() => grid.RegisterProducer("solar", 1f, true));
            Assert.Throws<ArgumentException>(() => grid.RegisterConsumer("solar", 1f, EnergyPriority.High));
            Assert.Throws<ArgumentException>(() => grid.RegisterProducer("load", 1f, false));
            Assert.Throws<KeyNotFoundException>(() => grid.SetProducerYield("missing", 1f));
            Assert.Throws<KeyNotFoundException>(() => grid.SetActive("missing", true));
            Assert.Throws<KeyNotFoundException>(() => grid.IsPowered("missing"));
            Assert.Throws<ArgumentException>(() => grid.RegisterProducer(" ", 1f, true));
            Assert.Throws<ArgumentException>(() => grid.RegisterConsumer(null, 1f, EnergyPriority.None));
            Assert.Throws<ArgumentException>(() => grid.IsPowered(""));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.RegisterConsumer("bad", 1f, (EnergyPriority)99));
            Assert.Throws<ArgumentNullException>(() => new EnergyGrid(null, Pool()));
            Assert.Throws<ArgumentNullException>(() => new EnergyGrid(new EnergyConfig(1f, 0f), null));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void PublicMethods_RejectInvalidAmounts(float invalid)
        {
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), Pool());
            grid.RegisterProducer("solar", 10f, true);
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Tick(invalid, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetProducerYield("solar", invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.RegisterProducer("bad", invalid, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.RegisterConsumer("bad", invalid, EnergyPriority.None));
        }

        [Test]
        public void ZeroTick_DoesNotConsumeProduceOrEmitEvents()
        {
            var pool = Pool(1f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 10f), pool);
            grid.RegisterProducer("solar", 100f, true);
            int events = 0;
            grid.OnBlackout += () => events++;
            grid.OnPowerRestored += () => events++;
            grid.Tick(0f, true);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(1f));
            Assert.That(events, Is.Zero);
        }

        [Test]
        public void ResourceEvent_ReentrantTickDoesNotDoubleChargeBattery()
        {
            var pool = Pool(5f);
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterConsumer("load", 10f, EnergyPriority.None);
            pool.OnChanged += (id, amount) => grid.Tick(1f, false);
            grid.Tick(1f, false);
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(4f));
        }

        [Test]
        public void ProducerYield_ZeroStopsGenerationAndMultiplePanelsAddTheirOutput()
        {
            var pool = Pool();
            var grid = new EnergyGrid(new EnergyConfig(10f, 0f), pool);
            grid.RegisterProducer("first", 10f, true);
            grid.RegisterProducer("second", 20f, true);
            grid.Tick(1f, false);
            Assert.That(grid.SolarOutput, Is.EqualTo(3f));
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(3f));
            grid.SetProducerYield("second", 0f);
            grid.Tick(1f, false);
            Assert.That(grid.SolarOutput, Is.EqualTo(1f));
            Assert.That(pool.Get(ResourceId.Electricity), Is.EqualTo(4f));
        }
    }
}
