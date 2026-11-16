using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class WashStepTests
    {
        private static ResourcePool Pool(float water)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Water] = water;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        // Lote de 0,25 kg, perclorato inicial 0,5 %, umbral 0,1 %: 1 L deja justo el umbral.
        private static WashConfig Config()
        {
            return new WashConfig(0.25f, 0.5f, 0.1f, 1f, 1f, 2f, 0.1f);
        }

        [TestCase(0f, 0.5f)]
        [TestCase(1f, 0.1f)]
        [TestCase(2f, 0.0555556f)]
        public void PerchloratePercent_FollowsTheEquilibriumModel(float liters, float expected)
        {
            var step = new WashStep(Config(), Pool(10f));
            step.SetWater(liters);
            Assert.That(step.PerchloratePercent, Is.EqualTo(expected).Within(1e-5f));
        }

        [Test]
        public void Evaluate_LessWaterThanNeededLeavesPerchlorateAboveThresholdAndDoesNotStart()
        {
            var step = new WashStep(Config(), Pool(10f));
            Assert.That(step.Evaluate().InRange, Is.False);
            step.SetWater(0.9f);
            StepScore score = step.Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo("prep.wash.too_low"));
        }

        [Test]
        public void Evaluate_ExactGuideScoresOneAndExtraWaterLowersTheScore()
        {
            var step = new WashStep(Config(), Pool(10f));
            step.SetWater(1f);
            Assert.That(step.Evaluate().InRange, Is.True);
            Assert.That(step.Evaluate().Score, Is.EqualTo(1f));
            step.SetWater(1.5f);
            Assert.That(step.Evaluate().Score, Is.EqualTo(0.5f).Within(1e-5f));
            step.SetWater(2f);
            Assert.That(step.Evaluate().InRange, Is.True);
            Assert.That(step.Evaluate().Score, Is.Zero);
        }

        [Test]
        public void Evaluate_TooMuchWaterIsOutOfRange()
        {
            var step = new WashStep(Config(), Pool(10f));
            step.SetWater(2.5f);
            Assert.That(step.Evaluate().InRange, Is.False);
            Assert.That(step.Evaluate().ReasonKey, Is.EqualTo("prep.wash.too_high"));
        }

        [Test]
        public void Evaluate_RangeBelowTheThresholdLiterStillBlocksPerchlorateAboveThreshold()
        {
            // Si el balance bajara el mínimo por debajo de lo necesario, el umbral sigue mandando.
            var config = new WashConfig(0.25f, 0.5f, 0.1f, 1f, 0.5f, 2f, 0.1f);
            var step = new WashStep(config, Pool(10f));
            step.SetWater(0.6f);
            Assert.That(step.Evaluate().InRange, Is.False);
            Assert.That(step.Evaluate().ReasonKey, Is.EqualTo("prep.wash.too_low"));
        }

        [Test]
        public void Charge_TakesTheWaterOnlyWhenCharged()
        {
            ResourcePool pool = Pool(3f);
            var step = new WashStep(Config(), pool);
            step.SetWater(1.5f);
            Assert.That(step.CanCharge(), Is.True);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(3f));
            Assert.That(step.TryCharge(), Is.True);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(1.5f).Within(1e-5f));
        }

        [Test]
        public void Charge_WithoutEnoughWaterDoesNotTakeAnything()
        {
            ResourcePool pool = Pool(0.5f);
            var step = new WashStep(Config(), pool);
            step.SetWater(1f);
            Assert.That(step.CanCharge(), Is.False);
            Assert.That(step.TryCharge(), Is.False);
            Assert.That(pool.Get(ResourceId.Water), Is.EqualTo(0.5f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void SetWater_RejectsInvalidLiters(float liters)
        {
            var step = new WashStep(Config(), Pool(1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => step.SetWater(liters));
        }

        [Test]
        public void Constructor_RejectsNullsAndHasTheWashId()
        {
            Assert.Throws<ArgumentNullException>(() => new WashStep(null, Pool(1f)));
            Assert.Throws<ArgumentNullException>(() => new WashStep(Config(), null));
            Assert.That(new WashStep(Config(), Pool(1f)).Id, Is.EqualTo("wash"));
        }
    }
}
