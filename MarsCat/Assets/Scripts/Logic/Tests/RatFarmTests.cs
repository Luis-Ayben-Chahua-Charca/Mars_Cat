// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Food;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class RatFarmTests
    {
        private const float Sol = 720f;

        // Valores de prueba: 4 raciones, 80 s por rata, 0,02 kg de forraje y 0,02 kg de cama por sol.
        internal static RatFarmConfig Config(float breedRate = 0.75f, int max = 40, int min = 15)
        {
            return new RatFarmConfig(4f, 80f, 0.02f, 0.02f, max, min, breedRate, Sol);
        }

        internal static ResourcePool Pool(float forage, float food = 0f, float capacity = 10000f)
        {
            var initial = ResourceConfigTests.FiniteValues(0f);
            initial[ResourceId.Forage] = forage;
            initial[ResourceId.Food] = food;
            return new ResourcePool(new ResourceConfig(initial, ResourceConfigTests.FiniteValues(capacity)));
        }

        [Test]
        public void AddAndRemoveRats_RespectLimitsAndNotify()
        {
            var farm = new RatFarm(Config(max: 10, min: 2), Pool(0f));
            var changes = new List<int>();
            farm.OnRatsChanged += changes.Add;
            Assert.That(farm.AddRats(7), Is.EqualTo(7));
            Assert.That(farm.AddRats(7), Is.EqualTo(3));
            Assert.That(farm.AddRats(1), Is.Zero);
            Assert.That(farm.Rats, Is.EqualTo(10));
            Assert.That(farm.RemoveRats(4), Is.EqualTo(4));
            Assert.That(farm.RemoveRats(20), Is.EqualTo(6));
            Assert.That(farm.Rats, Is.Zero);
            Assert.That(changes, Is.EqualTo(new List<int> { 7, 10, 6, 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => farm.AddRats(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => farm.RemoveRats(-1));
        }

        [Test]
        public void CanStart_RequiresMoreRatsThanBreedingStock()
        {
            var farm = new RatFarm(Config(), Pool(0f));
            farm.AddRats(15);
            Assert.That(farm.CanStart, Is.False);
            farm.AddRats(1);
            Assert.That(farm.CanStart, Is.True);
        }

        [Test]
        public void Tick_RejectsInvalidTime()
        {
            var farm = new RatFarm(Config(), Pool(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => farm.Tick(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => farm.Tick(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => farm.Tick(float.PositiveInfinity));
        }

        [Test]
        public void Constructor_RejectsNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new RatFarm(null, Pool(0f)));
            Assert.Throws<ArgumentNullException>(() => new RatFarm(Config(), null));
        }

        [Test]
        public void Tick_ConvertsOneRatEveryConvertSecondsWithoutDroppingBelowStock()
        {
            var pool = Pool(100f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            var produced = new List<float>();
            farm.OnRationsProduced += produced.Add;
            farm.AddRats(17);
            farm.SetWorkerPresent(true);
            farm.Tick(79f);
            Assert.That(farm.Rats, Is.EqualTo(17));
            farm.Tick(1f);
            Assert.That(farm.Rats, Is.EqualTo(16));
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(4f));
            farm.Tick(80f);
            Assert.That(farm.Rats, Is.EqualTo(15));
            farm.Tick(Sol);
            Assert.That(farm.Rats, Is.EqualTo(15));
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(8f));
            Assert.That(produced, Is.EqualTo(new List<float> { 4f, 4f }));
        }

        [Test]
        public void Tick_LargeStepConvertsSeveralRats()
        {
            var pool = Pool(100f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(20);
            farm.SetWorkerPresent(true);
            farm.Tick(240f);
            Assert.That(farm.Rats, Is.EqualTo(17));
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(12f));
        }

        [Test]
        public void Tick_WithoutWorkerDoesNotConvert()
        {
            var pool = Pool(100f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(20);
            farm.Tick(Sol);
            Assert.That(farm.Rats, Is.EqualTo(20));
            Assert.That(pool.Get(ResourceId.Food), Is.Zero);
        }

        [Test]
        public void Tick_WorkerLeavingKeepsProgress()
        {
            var pool = Pool(100f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(20);
            farm.SetWorkerPresent(true);
            farm.Tick(50f);
            farm.SetWorkerPresent(false);
            farm.Tick(500f);
            farm.SetWorkerPresent(true);
            farm.Tick(30f);
            Assert.That(farm.Rats, Is.EqualTo(19));
        }

        [Test]
        public void Tick_WithoutForageDoesNotConvertNorBreed()
        {
            var pool = Pool(0f);
            var farm = new RatFarm(Config(), pool);
            farm.AddRats(20);
            farm.SetWorkerPresent(true);
            farm.Tick(Sol * 2f);
            Assert.That(farm.Rats, Is.EqualTo(20));
            Assert.That(pool.Get(ResourceId.Food), Is.Zero);
            Assert.That(pool.Get(ResourceId.RatWaste), Is.Zero);
        }

        [Test]
        public void Tick_ResumesWhenForageArrives()
        {
            var pool = Pool(0f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(20);
            farm.SetWorkerPresent(true);
            farm.Tick(200f);
            Assert.That(farm.Rats, Is.EqualTo(20));
            pool.Add(ResourceId.Forage, 10f);
            farm.Tick(80f);
            Assert.That(farm.Rats, Is.EqualTo(19));
        }

        [Test]
        public void Tick_ConsumesForagePerRatPerSol()
        {
            var pool = Pool(10f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(10);
            farm.Tick(Sol);
            Assert.That(pool.Get(ResourceId.Forage), Is.EqualTo(10f - 10f * 0.02f).Within(1e-4f));
        }

        [Test]
        public void Tick_PartialForageSlowsTheFarmProportionally()
        {
            // Necesitan 0,2 kg por sol y solo hay 0,1: la mitad del tiempo cuenta.
            var pool = Pool(0.1f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(10);
            farm.Tick(Sol);
            Assert.That(pool.Get(ResourceId.Forage), Is.Zero);
            Assert.That(pool.Get(ResourceId.RatWaste), Is.EqualTo(0.1f).Within(1e-4f));
        }

        [Test]
        public void Tick_ProducesRatWastePerRatPerSol()
        {
            var pool = Pool(10f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(10);
            farm.Tick(Sol);
            Assert.That(pool.Get(ResourceId.RatWaste), Is.EqualTo(0.2f).Within(1e-4f));
        }

        [Test]
        public void Tick_WithZeroBreedRateRatsDoNotGrow()
        {
            var farm = new RatFarm(Config(breedRate: 0f), Pool(100f));
            farm.AddRats(10);
            farm.Tick(Sol * 5f);
            Assert.That(farm.Rats, Is.EqualTo(10));
        }

        [Test]
        public void Tick_LogisticGrowthFromTenReachesFifteenInAboutPointEightSol()
        {
            var farm = new RatFarm(Config(), Pool(100f));
            farm.AddRats(10);
            farm.Tick(Sol * 0.75f);
            Assert.That(farm.Rats, Is.LessThan(15));
            farm.Tick(Sol * 0.1f);
            Assert.That(farm.Rats, Is.EqualTo(15));
        }

        [Test]
        public void Tick_GrowthIsIndependentOfTickSize()
        {
            var coarse = new RatFarm(Config(), Pool(100f));
            var fine = new RatFarm(Config(), Pool(100f));
            coarse.AddRats(10);
            fine.AddRats(10);
            coarse.Tick(Sol);
            for (int i = 0; i < 720; i++)
                fine.Tick(1f);
            Assert.That(fine.Rats, Is.EqualTo(coarse.Rats));
            Assert.That(coarse.Rats, Is.GreaterThan(15));
        }

        [Test]
        public void Tick_NoBirthsAtTheCap()
        {
            var farm = new RatFarm(Config(max: 12, min: 5), Pool(100f));
            farm.AddRats(12);
            farm.Tick(Sol * 10f);
            Assert.That(farm.Rats, Is.EqualTo(12));
        }

        [Test]
        public void Tick_GrowthStopsAtTheCapWithoutExceedingIt()
        {
            var farm = new RatFarm(Config(max: 12, min: 5), Pool(1000f));
            farm.AddRats(10);
            farm.Tick(Sol * 50f);
            Assert.That(farm.Rats, Is.LessThanOrEqualTo(12));
            Assert.That(farm.Rats, Is.GreaterThanOrEqualTo(11));
        }

        [Test]
        public void Tick_FullDepotPausesConversion()
        {
            var pool = Pool(5f, food: 10f, capacity: 10f);
            var farm = new RatFarm(Config(breedRate: 0f), pool);
            farm.AddRats(20);
            farm.SetWorkerPresent(true);
            farm.Tick(Sol);
            Assert.That(farm.Rats, Is.EqualTo(20));
        }

        [Test]
        public void Tick_ZeroTimeChangesNothing()
        {
            var pool = Pool(10f);
            var farm = new RatFarm(Config(), pool);
            farm.AddRats(20);
            farm.Tick(0f);
            Assert.That(farm.Rats, Is.EqualTo(20));
            Assert.That(pool.Get(ResourceId.Forage), Is.EqualTo(10f));
        }
    }
}
