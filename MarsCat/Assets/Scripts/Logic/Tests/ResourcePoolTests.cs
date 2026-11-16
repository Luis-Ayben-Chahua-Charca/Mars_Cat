// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class ResourcePoolTests
    {
        private static ResourcePool CreatePool(float initial = 3f, float capacity = 10f)
        {
            return new ResourcePool(new ResourceConfig(
                ResourceConfigTests.FiniteValues(initial), ResourceConfigTests.FiniteValues(capacity)));
        }

        [Test]
        public void Add_CapsEachResourceIndependentlyAndReturnsAcceptedAmount()
        {
            var pool = CreatePool();
            Assert.That(pool.Add(ResourceId.Food, 9f), Is.EqualTo(7f));
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(10f));
            Assert.That(pool.Add(ResourceId.Food, 1f), Is.Zero);
            Assert.That(pool.Add(ResourceId.Water, 9f), Is.EqualTo(7f));
            Assert.That(pool.Get(ResourceId.Metal), Is.EqualTo(3f));
            Assert.That(pool.Capacity(ResourceId.Electricity), Is.EqualTo(10f));
        }

        [Test]
        public void TryConsume_IsAllOrNothingAndCanEmptyResource()
        {
            var pool = CreatePool();
            Assert.That(pool.TryConsume(ResourceId.Food, 4f), Is.False);
            Assert.That(pool.Get(ResourceId.Food), Is.EqualTo(3f));
            Assert.That(pool.TryConsume(ResourceId.Food, 3f), Is.True);
            Assert.That(pool.Get(ResourceId.Food), Is.Zero);
            Assert.That(pool.TryConsume(ResourceId.Food, 1f), Is.False);
        }

        [TestCase(ResourceId.Bacteria)]
        [TestCase(ResourceId.Regolith)]
        public void InfiniteResource_AlwaysAcceptsAndNeverRunsOut(ResourceId id)
        {
            var pool = CreatePool();
            int events = 0;
            pool.OnChanged += (resource, amount) => events++;
            Assert.That(pool.IsInfinite(id), Is.True);
            Assert.That(pool.Capacity(id), Is.EqualTo(float.PositiveInfinity));
            Assert.That(pool.Add(id, float.MaxValue), Is.EqualTo(float.MaxValue));
            for (int i = 0; i < 3; i++)
                Assert.That(pool.TryConsume(id, float.MaxValue), Is.True);
            Assert.That(pool.Get(id), Is.EqualTo(float.PositiveInfinity));
            Assert.That(events, Is.Zero);
        }

        [Test]
        public void OnChanged_ReportsUpdatedStateOnlyWhenAmountChanges()
        {
            var pool = CreatePool();
            int events = 0;
            pool.OnChanged += (id, amount) =>
            {
                events++;
                Assert.That(id, Is.EqualTo(ResourceId.Food));
                Assert.That(pool.Get(id), Is.EqualTo(amount));
            };
            pool.Add(ResourceId.Food, 20f);
            pool.Add(ResourceId.Food, 1f);
            pool.Add(ResourceId.Food, 0f);
            pool.TryConsume(ResourceId.Food, 11f);
            pool.TryConsume(ResourceId.Food, 0f);
            pool.TryConsume(ResourceId.Food, 10f);
            Assert.That(events, Is.EqualTo(2));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Mutations_RejectInvalidAmountsForEveryResource(float invalid)
        {
            var pool = CreatePool();
            foreach (ResourceId id in Enum.GetValues(typeof(ResourceId)))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => pool.Add(id, invalid));
                Assert.Throws<ArgumentOutOfRangeException>(() => pool.TryConsume(id, invalid));
            }
        }

        [Test]
        public void PublicOperations_RejectUnknownIdsAndNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourcePool(null));
            var pool = CreatePool();
            var invalid = (ResourceId)999;
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Get(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Capacity(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.IsInfinite(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Add(invalid, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.TryConsume(invalid, 1f));
        }

        [Test]
        public void Pools_HaveIndependentStateAndHandleExtremeCapacity()
        {
            var config = new ResourceConfig(ResourceConfigTests.FiniteValues(0f),
                ResourceConfigTests.FiniteValues(float.MaxValue));
            var first = new ResourcePool(config);
            var second = new ResourcePool(config);
            Assert.That(first.Add(ResourceId.Water, float.MaxValue), Is.EqualTo(float.MaxValue));
            Assert.That(first.Add(ResourceId.Water, float.MaxValue), Is.Zero);
            Assert.That(second.Get(ResourceId.Water), Is.Zero);
            Assert.That(first.IsInfinite(ResourceId.Water), Is.False);
        }
    }
}
