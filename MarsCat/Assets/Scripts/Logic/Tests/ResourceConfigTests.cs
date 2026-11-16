// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class ResourceConfigTests
    {
        internal static Dictionary<ResourceId, float> FiniteValues(float value)
        {
            var values = new Dictionary<ResourceId, float>();
            foreach (ResourceId id in Enum.GetValues(typeof(ResourceId)))
            {
                if (id != ResourceId.Bacteria && id != ResourceId.Regolith)
                    values.Add(id, value);
            }
            return values;
        }

        [Test]
        public void Constructor_CopiesValuesAndSupportsZeroCapacity()
        {
            var initial = FiniteValues(0f);
            var capacities = FiniteValues(0f);
            var config = new ResourceConfig(initial, capacities);
            initial[ResourceId.Food] = 5f;
            capacities[ResourceId.Food] = 10f;

            Assert.That(config.GetInitialAmount(ResourceId.Food), Is.Zero);
            Assert.That(config.GetCapacity(ResourceId.Food), Is.Zero);
            Assert.That(config.GetInitialAmount(ResourceId.Bacteria), Is.EqualTo(float.PositiveInfinity));
            Assert.That(config.GetCapacity(ResourceId.Regolith), Is.EqualTo(float.PositiveInfinity));
        }

        [Test]
        public void Constructor_RejectsNullMissingAndOverCapacityValues()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceConfig(null, FiniteValues(10f)));
            Assert.Throws<ArgumentNullException>(() => new ResourceConfig(FiniteValues(0f), null));
            var missing = FiniteValues(0f);
            missing.Remove(ResourceId.Water);
            Assert.Throws<ArgumentException>(() => new ResourceConfig(missing, FiniteValues(10f)));
            Assert.Throws<ArgumentException>(() => new ResourceConfig(FiniteValues(0f), missing));
            Assert.Throws<ArgumentException>(() => new ResourceConfig(FiniteValues(11f), FiniteValues(10f)));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidValues(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceConfig(FiniteValues(invalid), FiniteValues(10f)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceConfig(FiniteValues(0f), FiniteValues(invalid)));
        }

        [Test]
        public void ConstructorAndQueries_RejectUnknownIds()
        {
            var initial = FiniteValues(0f);
            initial.Add((ResourceId)999, 0f);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceConfig(initial, FiniteValues(10f)));
            var config = new ResourceConfig(FiniteValues(0f), FiniteValues(10f));
            Assert.Throws<ArgumentOutOfRangeException>(() => config.GetInitialAmount((ResourceId)999));
            Assert.Throws<ArgumentOutOfRangeException>(() => config.GetCapacity((ResourceId)999));
            Assert.Throws<ArgumentOutOfRangeException>(() => config.IsInfinite((ResourceId)999));
        }
    }
}
