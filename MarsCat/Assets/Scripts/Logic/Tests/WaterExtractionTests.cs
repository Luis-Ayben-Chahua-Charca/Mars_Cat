using System;
using MarsCat.Logic.Data;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Water;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class WaterExtractionTests
    {
        private sealed class HalvingTreatment : IWaterTreatment
        {
            public IceBatch Treat(IceBatch batch)
            {
                return new IceBatch(batch.Liters / 2f, batch.Purity);
            }

            public bool IsSafe(IceBatch batch)
            {
                return true;
            }
        }

        private sealed class ToxicTreatment : IWaterTreatment
        {
            public IceBatch Treat(IceBatch batch)
            {
                return batch;
            }

            public bool IsSafe(IceBatch batch)
            {
                return false;
            }
        }

        private const string VeinCsv =
            "clave;valor;unidad;fuente\n" +
            "minigame.water.vein.liters;5;L_hielo;derivado\n" +
            "minigame.water.vein.purity;0.6;fraccion;nasa\n";

        private ResourcePool _pool;
        private MeltingChamberSession _chamber;

        [SetUp]
        public void SetUp()
        {
            _pool = new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(0f),
                ResourceConfigTests.FiniteValues(100f)));
            _chamber = new MeltingChamberSession(new MeltingConfig(0.4f, 5f, 0.2f, 0.2f, 0f, 2f, 0.5f), _pool, 1);
        }

        private static IIceSource Vein()
        {
            return FixedVein.FromBalance(BalanceLoader.Parse(VeinCsv));
        }

        private void Play(float seconds, float input)
        {
            for (float elapsed = 0f; elapsed < seconds - 0.0001f; elapsed += 0.1f)
                _chamber.Tick(0.1f, input);
        }

        [Test]
        public void FixedVein_ReadsLitersAndPurityFromBalanceAndMissingKeyThrows()
        {
            IceBatch batch = Vein().Take();
            Assert.That(batch.Liters, Is.EqualTo(5f));
            Assert.That(batch.Purity, Is.EqualTo(0.6f));
            Assert.That(Vein().Take().Liters, Is.EqualTo(batch.Liters));
            var ex = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                FixedVein.FromBalance(BalanceLoader.Parse("clave;valor;unidad;fuente\n")));
            Assert.That(ex.Message, Does.Contain("minigame.water.vein.liters"));
        }

        [Test]
        public void IceBatch_InvalidValuesThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IceBatch(-1f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new IceBatch(1f, 1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new IceBatch(1f, -0.1f));
        }

        [Test]
        public void WellPlayedChamber_AddsLitersTimesPurityToThePool()
        {
            var extraction = new WaterExtraction(Vein(), _chamber, new NoTreatment(), _pool);
            float reported = -1f;
            extraction.OnWaterExtracted += liters => reported = liters;
            Play(10.4f, 0f);
            Assert.That(_pool.Get(ResourceId.Water), Is.EqualTo(3f).Within(0.0001f));
            Assert.That(reported, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void BoiledWater_AddsNothingAndReportsTheLoss()
        {
            var extraction = new WaterExtraction(Vein(), _chamber, new NoTreatment(), _pool);
            string lost = null;
            int extracted = 0;
            extraction.OnBatchLost += key => lost = key;
            extraction.OnWaterExtracted += liters => extracted++;
            Play(5.2f, 0f);
            Play(2.2f, -1f);
            Assert.That(_chamber.Phase, Is.EqualTo(MeltPhase.Lost));
            Assert.That(_pool.Get(ResourceId.Water), Is.Zero);
            Assert.That(lost, Is.EqualTo("prep.water.boiled"));
            Assert.That(extracted, Is.Zero);
        }

        [Test]
        public void SwappingTreatment_ChangesTheWaterButNotTheChamber()
        {
            var other = new MeltingChamberSession(new MeltingConfig(0.4f, 5f, 0.2f, 0.2f, 0f, 2f, 0.5f), _pool, 1);
            new WaterExtraction(Vein(), _chamber, new NoTreatment(), _pool);
            new WaterExtraction(Vein(), other, new HalvingTreatment(), _pool);
            for (int i = 0; i < 104; i++)
            {
                _chamber.Tick(0.1f, 0f);
                other.Tick(0.1f, 0f);
                Assert.That(other.Phase, Is.EqualTo(_chamber.Phase));
                Assert.That(other.Progress, Is.EqualTo(_chamber.Progress));
            }
            Assert.That(_pool.Get(ResourceId.Water), Is.EqualTo(3f + 1.5f).Within(0.0001f));
        }

        [Test]
        public void UnsafeTreatedWater_IsNotAdded()
        {
            var extraction = new WaterExtraction(Vein(), _chamber, new ToxicTreatment(), _pool);
            string lost = null;
            extraction.OnBatchLost += key => lost = key;
            Play(10.4f, 0f);
            Assert.That(_chamber.Phase, Is.EqualTo(MeltPhase.Done));
            Assert.That(_pool.Get(ResourceId.Water), Is.Zero);
            Assert.That(lost, Is.EqualTo("prep.water.unsafe"));
        }

        [Test]
        public void FullTank_ReportsOnlyWhatEntered()
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Water] = 9f;
            var capacities = ResourceConfigTests.FiniteValues(100f);
            capacities[ResourceId.Water] = 10f;
            var pool = new ResourcePool(new ResourceConfig(amounts, capacities));
            var chamber = new MeltingChamberSession(new MeltingConfig(0.4f, 5f, 0.2f, 0.2f, 0f, 2f, 0.5f), pool, 1);
            var extraction = new WaterExtraction(Vein(), chamber, new NoTreatment(), pool);
            float reported = -1f;
            extraction.OnWaterExtracted += liters => reported = liters;
            for (int i = 0; i < 104; i++)
                chamber.Tick(0.1f, 0f);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(10f));
            Assert.That(reported, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void NoTreatment_ReturnsTheSameBatchAndIsSafe()
        {
            var batch = new IceBatch(4f, 0.25f);
            var treatment = new NoTreatment();
            Assert.That(treatment.Treat(batch).Liters, Is.EqualTo(4f));
            Assert.That(treatment.Treat(batch).Purity, Is.EqualTo(0.25f));
            Assert.That(treatment.IsSafe(batch), Is.True);
        }

        [Test]
        public void Constructor_InvalidArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new WaterExtraction(null, _chamber, new NoTreatment(), _pool));
            Assert.Throws<ArgumentNullException>(() => new WaterExtraction(Vein(), null, new NoTreatment(), _pool));
            Assert.Throws<ArgumentNullException>(() => new WaterExtraction(Vein(), _chamber, null, _pool));
            Assert.Throws<ArgumentNullException>(() => new WaterExtraction(Vein(), _chamber, new NoTreatment(), null));
            Play(10.4f, 0f);
            Assert.Throws<InvalidOperationException>(() =>
                new WaterExtraction(Vein(), _chamber, new NoTreatment(), _pool));
        }
    }
}
