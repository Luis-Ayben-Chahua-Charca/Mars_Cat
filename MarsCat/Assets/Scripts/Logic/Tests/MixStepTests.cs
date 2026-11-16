using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class MixStepTests
    {
        private static ResourcePool Pool(float compost)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.Compost] = compost;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        // Lote de 1 kg; abono guía 20 % (0 a 30); bacterias guía 5 % (1 a 10).
        private static MixConfig Config()
        {
            return new MixConfig(1f, 20f, 0f, 30f, 5f, 1f, 10f, 0.1f);
        }

        private static MixStep Step(float compost, float bacteria, ResourcePool pool)
        {
            var step = new MixStep(Config(), pool);
            step.SetCompost(compost);
            step.SetBacteria(bacteria);
            return step;
        }

        [Test]
        public void Evaluate_AtTheGuideScoresOne()
        {
            StepScore score = Step(20f, 5f, Pool(10f)).Evaluate();
            Assert.That(score.InRange, Is.True);
            Assert.That(score.Score, Is.EqualTo(1f));
        }

        [Test]
        public void Evaluate_ZeroCompostIsInsideTheRangeButScoresZero()
        {
            StepScore score = Step(0f, 5f, Pool(10f)).Evaluate();
            Assert.That(score.InRange, Is.True);
            Assert.That(score.Score, Is.Zero);
        }

        [Test]
        public void Evaluate_TakesTheWeakestOfCompostAndBacteria()
        {
            Assert.That(Step(10f, 5f, Pool(10f)).Evaluate().Score, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(Step(20f, 7.25f, Pool(10f)).Evaluate().Score, Is.EqualTo(0.55f).Within(1e-5f));
            Assert.That(Step(10f, 7.5f, Pool(10f)).Evaluate().Score, Is.EqualTo(0.5f).Within(1e-5f));
        }

        [Test]
        public void Evaluate_TooMuchCompostBurnsThePlantsAndNamesTheInput()
        {
            StepScore score = Step(31f, 5f, Pool(10f)).Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo("prep.mix.compost.too_high"));
        }

        [TestCase(0.5f, "prep.mix.bacteria.too_low")]
        [TestCase(11f, "prep.mix.bacteria.too_high")]
        public void Evaluate_BacteriaOutOfRangeNamesTheInput(float dose, string key)
        {
            StepScore score = Step(20f, dose, Pool(10f)).Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo(key));
        }

        [Test]
        public void Evaluate_DefaultValuesAreOutOfRangeBecauseBacteriaStartAtZero()
        {
            var step = new MixStep(Config(), Pool(10f));
            Assert.That(step.Evaluate().InRange, Is.False);
        }

        [Test]
        public void Charge_TakesCompostAsPercentOfTheBatch()
        {
            ResourcePool pool = Pool(1f);
            MixStep step = Step(20f, 5f, pool);
            Assert.That(step.CompostKg, Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(step.CanCharge(), Is.True);
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(1f));
            Assert.That(step.TryCharge(), Is.True);
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(0.8f).Within(1e-6f));
        }

        [Test]
        public void Charge_WithoutEnoughCompostDoesNotTakeAnything()
        {
            ResourcePool pool = Pool(0.1f);
            MixStep step = Step(20f, 5f, pool);
            Assert.That(step.CanCharge(), Is.False);
            Assert.That(step.TryCharge(), Is.False);
            Assert.That(pool.Get(ResourceId.Compost), Is.EqualTo(0.1f));
        }

        [Test]
        public void Charge_ZeroCompostNeedsNoCompost()
        {
            MixStep step = Step(0f, 5f, Pool(0f));
            Assert.That(step.CanCharge(), Is.True);
            Assert.That(step.TryCharge(), Is.True);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Setters_RejectInvalidValues(float value)
        {
            var step = new MixStep(Config(), Pool(1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => step.SetCompost(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => step.SetBacteria(value));
        }

        [Test]
        public void Constructor_RejectsNullsAndHasTheMixId()
        {
            Assert.Throws<ArgumentNullException>(() => new MixStep(null, Pool(1f)));
            Assert.Throws<ArgumentNullException>(() => new MixStep(Config(), null));
            Assert.That(new MixStep(Config(), Pool(1f)).Id, Is.EqualTo("mix"));
        }
    }
}
